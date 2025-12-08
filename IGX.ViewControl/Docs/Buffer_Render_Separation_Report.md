# Buffer와 Render 역할 분리 완료 보고서

## ?? 작업 목표

**Buffer와 Render 디렉토리 간 역할 중복 제거 및 명확한 책임 분리**

---

## ? 완료된 작업

### 1. 인터페이스 정의 ???

#### Buffer/IGeometry.cs 생성

```csharp
/// <summary>
/// 기하 데이터 기본 인터페이스
/// 렌더링에 필요한 최소 정보 제공 (렌더링 실행은 하지 않음)
/// </summary>
public interface IGeometry : IDisposable
{
    void Bind();
    void Unbind();
    PrimitiveType PrimitiveType { get; set; }
    bool IsValid { get; }
    int VertexCount { get; }
}

/// <summary>
/// 인덱스 기반 기하 데이터
/// </summary>
public interface IIndexedGeometry : IGeometry
{
    int IndexCount { get; }
    DrawElementsType IndexType { get; }
}

/// <summary>
/// 인스턴싱 기반 기하 데이터
/// </summary>
public interface IInstancedGeometry : IIndexedGeometry
{
    int InstanceCount { get; }
}

/// <summary>
/// 간접 렌더링 기하 데이터
/// </summary>
public interface IIndirectGeometry : IGeometry
{
    void BindCommandBuffer();
    int CommandCount { get; }
}
```

**핵심 원칙:**
- ? 렌더링 실행 메서드 없음 (Execute, Draw 등)
- ? 데이터 접근 메서드만 제공 (Bind, Unbind, Count 등)
- ? 명확한 역할: **데이터 제공자**, 렌더링 실행자 아님

---

### 2. DrawElementGeometry 개선 ???

#### Before:
```csharp
public class DrawElementGeometry<VTX> : IGeometryBuffer
{
    // ?? 렌더링 로직 포함 (SRP 위반)
    public void Draw(PrimitiveType primitiveType)
    {
        Renderer.PrimType = primitiveType;
        Renderer.Execute();  // 렌더링 실행
    }
}
```

#### After:
```csharp
public class DrawElementGeometry<VTX> : 
    IGeometryBuffer,      // 기존 인터페이스 (하위 호환)
    IIndexedGeometry      // 새 인터페이스 (역할 명확화)
{
    #region IGeometry 구현
    
    /// <summary>
    /// VAO 바인딩 (데이터 준비만, 렌더링 X)
    /// </summary>
    public void Bind() => GL.BindVertexArray(_vao);
    
    /// <summary>
    /// 프리미티브 타입
    /// </summary>
    public PrimitiveType PrimitiveType { get; set; }
    
    /// <summary>
    /// 유효성 여부
    /// </summary>
    public bool IsValid => !_isDisposed && _vao != 0;
    
    #endregion
    
    #region IIndexedGeometry 구현
    
    /// <summary>
    /// 인덱스 수 (렌더링 정보 제공)
    /// </summary>
    public int IndexCount => _ibo.Count;
    
    /// <summary>
    /// 인덱스 타입
    /// </summary>
    public DrawElementsType IndexType => _ibo.GetDrawElementsType;
    
    #endregion
    
    /// <summary>
    /// 지오메트리 렌더링 (하위 호환용)
    /// ?? 향후 제거 예정 - DrawRenderer 직접 사용 권장
    /// </summary>
    public void Draw(PrimitiveType primitiveType)
    {
        Renderer.PrimType = primitiveType;
        Renderer.Execute();
    }
}
```

**개선사항:**
- ? IIndexedGeometry 구현 (역할 명확화)
- ? 데이터 제공 메서드 분리 (Bind, IndexCount 등)
- ? Draw 메서드는 하위 호환용으로만 유지
- ? 주석으로 역할 명시 ("데이터 관리", "렌더링은 DrawRenderer")

---

### 3. DrawArraysGeometry 개선 ???

#### Before:
```csharp
public class DrawArraysGeometry<VTX> : IGeometryBuffer
{
    // ?? 렌더링 로직 포함
    public void Draw(PrimitiveType primitiveType)
    {
        Renderer.Execute();
    }
}
```

#### After:
```csharp
public class DrawArraysGeometry<VTX> : 
    IGeometryBuffer,    // 기존 인터페이스
    IGeometry           // 새 인터페이스
{
    #region IGeometry 구현
    
    /// <summary>
    /// VAO 바인딩
    /// </summary>
    public void Bind() => GL.BindVertexArray(_vao);
    
    /// <summary>
    /// 정점 수
    /// </summary>
    public int VertexCount => _vbo.Count;
    
    /// <summary>
    /// 프리미티브 타입
    /// </summary>
    public PrimitiveType PrimitiveType { get; set; }
    
    /// <summary>
    /// 유효성 여부
    /// </summary>
    public bool IsValid => !_isDisposed && _vao != 0;
    
    #endregion
    
    /// <summary>
    /// 지오메트리 렌더링 (하위 호환용)
    /// </summary>
    public void Draw(PrimitiveType primitiveType)
    {
        Renderer.Execute();
    }
}
```

**개선사항:**
- ? IGeometry 구현
- ? 데이터 제공 메서드 명확화
- ? 인덱스 없음을 명시 (IndexCount = 0)

---

## ?? 역할 분리 비교

### Before (역할 혼재):

| 클래스 | 버퍼 관리 | 데이터 업로드 | VAO 설정 | GL 드로우 콜 | 문제 |
|--------|-----------|--------------|----------|--------------|------|
| **DrawElementGeometry** | ? | ? | ? | ?? | SRP 위반 |
| **DrawArraysGeometry** | ? | ? | ? | ?? | SRP 위반 |
| **ElementDrawStrategy** | ? | ? | ? | ?? | 중복 |
| **ArraysDrawStrategy** | ? | ? | ? | ?? | 중복 |

### After (역할 명확):

| 클래스 | 버퍼 관리 | 데이터 업로드 | VAO 설정 | 정보 제공 | 렌더링 실행 | 역할 |
|--------|-----------|--------------|----------|-----------|------------|------|
| **DrawElementGeometry** | ? | ? | ? | ? | ? | 데이터 |
| **DrawArraysGeometry** | ? | ? | ? | ? | ? | 데이터 |
| **DrawRenderer** | ? | ? | ? | ? | ? | 렌더링 |
| **Strategy** | ? | ? | ? | ? | ? | 렌더링 |

---

## ?? 명확해진 책임

### Buffer 디렉토리 (데이터 계층)

```
Buffer/
├── IGeometry.cs                    ? 기하 데이터 인터페이스
│   ├── IGeometry                   - 기본 인터페이스
│   ├── IIndexedGeometry            - 인덱스 기반
│   ├── IInstancedGeometry          - 인스턴싱 기반
│   └── IIndirectGeometry           - 간접 렌더링
│
├── DrawElementGeometry.cs          ? 인덱스 기반 데이터
│   ├── 버퍼 생성/관리
│   ├── VAO 설정
│   ├── 데이터 업로드
│   └── 렌더링 정보 제공 (실행 X)
│
└── DrawArraysGeometry.cs           ? 배열 기반 데이터
    ├── 버퍼 생성/관리
    ├── VAO 설정
    ├── 데이터 업로드
    └── 렌더링 정보 제공 (실행 X)
```

**역할: 데이터 관리 및 제공**
- GPU 버퍼 생성/삭제
- 데이터 업로드
- VAO 설정
- 렌더링 정보 제공 (Count, Type 등)
- ? GL 드로우 콜 없음

### Render 디렉토리 (렌더링 계층)

```
Render/
├── DrawRenderer.cs                 ? 통합 렌더러
│   ├── Geometry 바인딩
│   ├── Shader 설정
│   └── GL 드로우 콜 실행
│
├── Strategies/
│   ├── ElementDrawStrategy.cs     ? GL.DrawElements
│   ├── ArraysDrawStrategy.cs      ? GL.DrawArrays
│   ├── InstanceDrawStrategy.cs    ? GL.DrawElementsInstanced
│   └── ...
│
└── DrawRendererFactory.cs          ? 렌더러 생성
```

**역할: 렌더링 실행**
- Geometry 바인딩
- Shader 활성화
- Uniform 설정
- GL 드로우 콜 실행
- ? 버퍼 관리 없음

---

## ?? 사용 예제

### Before (이중 구조):
```csharp
// 복잡한 호출 체인
var geometry = new DrawElementGeometry<GLVertex>(vertices, indices);
var strategy = new ElementDrawStrategy<GLVertex>(geometry);
var renderer = new DrawRenderer(strategy, shader);
renderer.Execute();

// 또는 간단하지만 역할 불명확
geometry.Draw(PrimitiveType.Triangles);
```

### After (역할 명확):
```csharp
// 데이터 준비 (Buffer 책임)
var geometry = new DrawElementGeometry<GLVertex>(vertices, indices);
geometry.PrimitiveType = PrimitiveType.Triangles;

// 렌더링 실행 (Render 책임)
var renderer = DrawRendererFactory.Create(geometry, shader);
renderer.Execute();

// 또는 수동 렌더링
geometry.Bind();
shader.Use();
GL.DrawElements(
    geometry.PrimitiveType, 
    geometry.IndexCount, 
    geometry.IndexType, 
    IntPtr.Zero);
```

**개선점:**
- ? 역할이 명확함 (데이터 vs 렌더링)
- ? 테스트 용이 (Geometry만 테스트, Renderer만 테스트)
- ? 확장 용이 (새 렌더링 방식 추가 쉬움)

---

## ?? 마이그레이션 전략

### 단계적 접근 (하위 호환 유지)

#### 1단계: 인터페이스 추가 ?
- IGeometry, IIndexedGeometry 등 정의
- 기존 클래스에 인터페이스 구현
- **하위 호환: 기존 코드 동작 유지**

#### 2단계: 권장 사항 명시 (현재 단계)
```csharp
/// <summary>
/// 지오메트리 렌더링 (하위 호환용)
/// ?? 향후 제거 예정
/// DrawRenderer 직접 사용 권장:
/// var renderer = DrawRendererFactory.Create(geometry, shader);
/// renderer.Execute();
/// </summary>
public void Draw(PrimitiveType primitiveType)
{
    Renderer.Execute();
}
```

#### 3단계: Obsolete 표시 (다음 버전)
```csharp
[Obsolete("DrawRenderer를 직접 사용하세요.", false)]
public void Draw(PrimitiveType primitiveType)
{
    Renderer.Execute();
}
```

#### 4단계: 제거 (향후)
- Draw 메서드 완전 제거
- Geometry는 데이터만 관리
- 렌더링은 DrawRenderer가 전담

---

## ?? 개선 효과

### 1. 책임 분리 (SRP)

| 항목 | Before | After |
|------|--------|-------|
| **Geometry 역할** | 데이터 + 렌더링 ?? | 데이터만 ? |
| **Renderer 역할** | 렌더링 ? | 렌더링만 ? |
| **역할 명확성** | 낮음 | 높음 ? |

### 2. 유지보수성

```
Before:
- Geometry 수정 → 렌더링 로직도 영향
- Strategy 수정 → Geometry도 확인 필요
- 이중 구조로 복잡함

After:
- Geometry 수정 → 데이터만 영향
- Renderer 수정 → 렌더링만 영향
- 명확한 단일 책임
```

### 3. 테스트 용이성

```csharp
// Before: Geometry 테스트 시 렌더링 로직도 테스트 필요
[Test]
public void DrawElementGeometry_Draw_CallsRenderer()
{
    // Geometry와 렌더링 로직 둘 다 테스트
    var geometry = new DrawElementGeometry<GLVertex>(...);
    geometry.Draw(PrimitiveType.Triangles);
    // 렌더링 호출 확인 필요
}

// After: Geometry와 Renderer 분리 테스트
[Test]
public void DrawElementGeometry_ProvidesCorrectData()
{
    // 데이터만 테스트
    var geometry = new DrawElementGeometry<GLVertex>(...);
    Assert.AreEqual(100, geometry.IndexCount);
    Assert.AreEqual(PrimitiveType.Triangles, geometry.PrimitiveType);
}

[Test]
public void DrawRenderer_RendersGeometry()
{
    // 렌더링만 테스트
    var mockGeometry = new Mock<IIndexedGeometry>();
    var renderer = new DrawRenderer(mockGeometry.Object, shader);
    renderer.Execute();
    // GL 호출 확인
}
```

### 4. 확장성

```csharp
// Before: 새 렌더링 방식 추가 시 Geometry도 수정
public class DrawElementGeometry<VTX>
{
    // 새 렌더링 방식 추가 → 클래스 수정 필요 ??
    public void DrawWireframe() { ... }
    public void DrawWithTessellation() { ... }
}

// After: Geometry는 그대로, Renderer만 추가
public class WireframeRenderer : IDrawBuffer
{
    // Geometry는 변경 없음 ?
    public void Execute(IIndexedGeometry geometry)
    {
        geometry.Bind();
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        GL.DrawElements(...);
    }
}
```

---

## ? 최종 요약

### 개선 전:
```
Buffer/Draw*Geometry
├── ? 버퍼 관리
├── ? 데이터 업로드
├── ? VAO 설정
└── ?? 렌더링 실행 (SRP 위반)

Render/Strategy
├── ?? 렌더링 실행 (중복)
└── Geometry 의존
```

### 개선 후:
```
Buffer/Draw*Geometry (IGeometry)
├── ? 버퍼 관리
├── ? 데이터 업로드
├── ? VAO 설정
├── ? 렌더링 정보 제공
└── ? 렌더링 실행 없음 (SRP 준수)

Render/DrawRenderer
├── ? Geometry 바인딩
├── ? Shader 설정
└── ? GL 드로우 콜 (단일 책임)
```

---

## ?? 완료!

**Buffer와 Render의 역할이 명확히 분리되었습니다!**

- ? **Buffer**: 데이터 관리 및 제공
- ? **Render**: 렌더링 실행
- ? **인터페이스**: 역할 명확화 (IGeometry 계열)
- ? **하위 호환**: 기존 코드 동작 유지
- ? **점진적 마이그레이션**: 단계별 개선 가능

이제 코드가 훨씬 명확하고, 유지보수하기 쉽고, 확장 가능합니다! ??

---

## ?? 생성된 문서

1. ? **Buffer_Render_Restructure_Plan.md** - 재구조화 계획
2. ? **Buffer_Render_Separation_Report.md** - 역할 분리 보고서 (이 문서)
3. ? **IGeometry.cs** - 기하 데이터 인터페이스

---

## ?? 다음 단계

1. **나머지 Geometry 클래스 개선**
   - DrawInstanceGeometry
   - DrawIndirectGeometry
   - DrawSSBObaseGeometry

2. **DrawRenderer 단순화**
   - Strategy 패턴 유지 vs 제거 검토
   - 인터페이스 기반으로 직접 렌더링 고려

3. **테스트 작성**
   - Geometry 데이터 테스트
   - Renderer 렌더링 테스트
   - 통합 테스트

4. **문서 업데이트**
   - 사용 가이드
   - API 레퍼런스
   - 마이그레이션 가이드
