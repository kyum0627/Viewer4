# Buffer와 Render 디렉토리 역할 분석 및 재구조화 계획

## ?? 현재 구조 분석

### Buffer 디렉토리 클래스들

#### 1. Geometry 클래스들 (Draw*Geometry)
```
Buffer/
├── DrawElementGeometry.cs      - 인덱스 기반 렌더링 데이터
├── DrawArraysGeometry.cs       - 배열 기반 렌더링 데이터
├── DrawInstanceGeometry.cs     - 인스턴싱 렌더링 데이터
├── DrawIndirectGeometry.cs     - 간접 렌더링 데이터
└── DrawSSBObaseGeometry.cs     - SSBO 기반 렌더링 데이터
```

**현재 역할:**
- GPU 버퍼 소유 (VBO, IBO 등)
- 데이터 업로드 관리
- VAO 설정 포함

**문제점:**
- ?? 렌더링 로직 포함 (Execute 메서드)
- ?? Shader 의존성 있음
- ?? GL 드로우 콜 직접 호출

#### 2. Buffer 관리 클래스들
```
Buffer/
├── GLBuffer.cs              - GL 버퍼 추상화
├── ElementBuffer.cs         - 인덱스 버퍼
├── VertexBuffer.cs          - 정점 버퍼
├── InstanceBuffer.cs        - 인스턴스 버퍼
├── IndirectCommandBuffer.cs - 간접 명령 버퍼
├── ShaderStorageBuffer.cs   - SSBO
├── UniformBuffer.cs         - UBO
└── TextureBuffer.cs         - 텍스처 버퍼
```

**역할:** ? 올바름
- GPU 버퍼 생성/삭제
- 데이터 업로드
- 바인딩 관리

#### 3. GL 상태 관리 클래스들
```
Buffer/
├── GLUtil.cs               - GL 상태 유틸리티
├── GLStateSnapshot.cs      - GL 상태 캡처
├── GLStateScope.cs         - GL 상태 자동 복원
├── GLResourceManager.cs    - GL 리소스 삭제 관리
└── VertexArrayObject.cs    - VAO 관리
```

**역할:** ? 올바름
- GL 상태 관리
- 리소스 생명주기

---

### Render 디렉토리 클래스들

#### 1. 렌더링 실행 클래스들
```
Render/
├── DrawRenderer.cs              - 통합 렌더러 (Strategy 패턴)
├── DrawRendererFactory.cs       - 렌더러 팩토리
├── IDrawStrategy.cs             - 렌더링 전략 인터페이스
└── Strategies/
    ├── ElementDrawStrategy.cs   - 인덱스 렌더링 전략
    ├── ArraysDrawStrategy.cs    - 배열 렌더링 전략
    ├── InstanceDrawStrategy.cs  - 인스턴싱 전략
    ├── IndirectDrawStrategy.cs  - 간접 렌더링 전략
    └── SSBODrawStrategy.cs      - SSBO 렌더링 전략
```

**현재 역할:**
- 렌더링 전략 구현
- GL 드로우 콜 실행
- Shader 관리

**문제점:**
- ?? Geometry 클래스와 역할 중복
- ?? 이중 구조 (Geometry + Strategy)

#### 2. 렌더링 파이프라인 클래스들
```
Render/
├── RenderPipeline.cs       - 렌더링 파이프라인
├── RenderPassBase.cs       - 렌더 패스 기본 클래스
├── ForwardPass.cs          - Forward 렌더링
├── FBOPass.cs              - Deferred 렌더링
└── Auxilliary/
    ├── BackgroundPass.cs   - 배경
    ├── CoordinatePass.cs   - 좌표축
    └── ...
```

**역할:** ? 올바름
- 렌더링 순서 관리
- 패스 실행

---

## ? 문제점 정리

### 1. 역할 중복

| 기능 | Buffer/Draw*Geometry | Render/Strategy | 문제 |
|------|----------------------|-----------------|------|
| **버퍼 관리** | ? 소유 | ? 참조 | - |
| **VAO 설정** | ? 포함 | ? 사용 | - |
| **데이터 업로드** | ? 담당 | ? 없음 | - |
| **GL 드로우 콜** | ?? 포함 | ?? 포함 | ? 중복 |
| **렌더링 로직** | ?? Execute | ?? Draw | ? 중복 |
| **Shader 관리** | ?? 의존 | ?? 의존 | ? 중복 |

### 2. 책임 분리 위반 (SRP)

**DrawElementGeometry (현재):**
```csharp
public class DrawElementGeometry<VTX>
{
    // ? 데이터 책임
    private VertexBuffer<VTX> _vbo;
    private ElementBuffer<uint> _ibo;
    
    // ?? 렌더링 책임 (SRP 위반!)
    public void Execute(Shader shader)
    {
        // GL 드로우 콜
        GL.DrawElements(...);
    }
}
```

### 3. 이중 구조의 비효율성

```
사용자 코드:
    ↓
DrawRenderer (Render/)
    ↓
IDrawStrategy (Render/)
    ↓
Draw*Geometry (Buffer/)  ← 여기서 실제 드로우 콜
    ↓
GL.DrawElements/Arrays/...
```

**문제:**
- 불필요한 래퍼
- 복잡한 호출 체인
- 유지보수 어려움

---

## ? 개선 방안

### 원칙: **명확한 책임 분리**

```
Buffer 디렉토리:
- 버퍼 생성/관리
- 데이터 업로드
- VAO 설정
- ? 렌더링 로직 없음

Render 디렉토리:
- 렌더링 실행
- GL 드로우 콜
- Shader 관리
- 렌더링 파이프라인
```

---

## ?? 재구조화 계획

### 1단계: Draw*Geometry 클래스 정리

#### Before:
```csharp
// Buffer/DrawElementGeometry.cs
public class DrawElementGeometry<VTX>
{
    private VertexBuffer<VTX> _vbo;
    private ElementBuffer<uint> _ibo;
    private VAO _vao;
    
    // ?? 렌더링 로직 (SRP 위반)
    public void Execute(Shader shader)
    {
        _vao.Bind();
        GL.DrawElements(...);
    }
}
```

#### After:
```csharp
// Buffer/ElementGeometry.cs (이름 변경)
public class ElementGeometry<VTX> : IGeometry
{
    private VertexBuffer<VTX> _vbo;
    private ElementBuffer<uint> _ibo;
    private VAO _vao;
    
    // ? 데이터 책임만
    public void Bind() => _vao.Bind();
    public void Unbind() => _vao.Unbind();
    
    // 렌더링 정보 제공 (실행은 안 함)
    public int IndexCount => _ibo.Count;
    public PrimitiveType PrimitiveType { get; set; }
}
```

### 2단계: Strategy 클래스 통합

#### Before (이중 구조):
```
DrawRenderer
    → ElementDrawStrategy
        → DrawElementGeometry
            → GL.DrawElements()
```

#### After (단순 구조):
```
DrawRenderer
    → ElementGeometry
    → GL.DrawElements() (DrawRenderer 내부)
```

### 3단계: 클래스 이름 재정의

| Before (Buffer/) | After (Buffer/) | 역할 |
|------------------|-----------------|------|
| DrawElementGeometry | ElementGeometry | 인덱스 기반 데이터 |
| DrawArraysGeometry | ArraysGeometry | 배열 기반 데이터 |
| DrawInstanceGeometry | InstanceGeometry | 인스턴싱 데이터 |
| DrawIndirectGeometry | IndirectGeometry | 간접 렌더링 데이터 |
| DrawSSBObaseGeometry | SSBOGeometry | SSBO 기반 데이터 |

**이유:**
- "Draw" 제거 → 렌더링이 아닌 데이터 역할 강조
- 간결한 이름
- 역할 명확화

### 4단계: 인터페이스 정의

```csharp
// Buffer/IGeometry.cs
public interface IGeometry : IDisposable
{
    /// <summary>
    /// VAO 바인딩
    /// </summary>
    void Bind();
    
    /// <summary>
    /// VAO 언바인딩
    /// </summary>
    void Unbind();
    
    /// <summary>
    /// 프리미티브 타입
    /// </summary>
    PrimitiveType PrimitiveType { get; }
    
    /// <summary>
    /// 유효성 검사
    /// </summary>
    bool IsValid { get; }
}

// Buffer/IIndexedGeometry.cs
public interface IIndexedGeometry : IGeometry
{
    /// <summary>
    /// 인덱스 수
    /// </summary>
    int IndexCount { get; }
    
    /// <summary>
    /// 인덱스 타입
    /// </summary>
    DrawElementsType IndexType { get; }
}

// Buffer/IInstancedGeometry.cs
public interface IInstancedGeometry : IGeometry
{
    /// <summary>
    /// 인스턴스 수
    /// </summary>
    int InstanceCount { get; }
}
```

---

## ?? 최종 구조

### Buffer 디렉토리 (데이터 계층)

```
Buffer/
├── Core/
│   ├── GLBuffer.cs              ? 버퍼 기본 클래스
│   ├── VertexBuffer.cs          ? 정점 버퍼
│   ├── ElementBuffer.cs         ? 인덱스 버퍼
│   ├── InstanceBuffer.cs        ? 인스턴스 버퍼
│   └── ...
│
├── Geometry/                     ? 기하 데이터 (렌더링 로직 없음)
│   ├── IGeometry.cs             - 기본 인터페이스
│   ├── IIndexedGeometry.cs      - 인덱스 기반
│   ├── IInstancedGeometry.cs    - 인스턴싱 기반
│   ├── ElementGeometry.cs       - 인덱스 렌더링 데이터
│   ├── ArraysGeometry.cs        - 배열 렌더링 데이터
│   ├── InstanceGeometry.cs      - 인스턴싱 데이터
│   ├── IndirectGeometry.cs      - 간접 렌더링 데이터
│   └── SSBOGeometry.cs          - SSBO 데이터
│
└── State/
    ├── GLUtil.cs                ? GL 상태 유틸
    ├── GLStateSnapshot.cs       ? 상태 캡처
    ├── GLStateScope.cs          ? 자동 복원
    └── GLResourceManager.cs     ? 리소스 관리
```

### Render 디렉토리 (렌더링 계층)

```
Render/
├── Core/
│   ├── DrawRenderer.cs          ? 통합 렌더러
│   ├── DrawRendererFactory.cs   ? 팩토리
│   └── Shader.cs                ? 셰이더 관리
│
├── Pipeline/
│   ├── RenderPipeline.cs        ? 파이프라인
│   ├── RenderPassBase.cs        ? 패스 기본
│   ├── ForwardPass.cs           ? Forward 렌더링
│   └── FBOPass.cs               ? Deferred 렌더링
│
└── Strategies/                   ?? 제거 또는 단순화
    └── (DrawRenderer로 통합)
```

---

## ?? 마이그레이션 전략

### 1단계: 인터페이스 추가 (하위 호환 유지)
```csharp
// 기존 클래스에 인터페이스 구현
public class DrawElementGeometry<VTX> : IIndexedGeometry
{
    // 기존 메서드 유지
    public void Execute(Shader shader) { ... }
    
    // 새 인터페이스 구현
    public void Bind() => _vao.Bind();
    public int IndexCount => _ibo.Count;
}
```

### 2단계: DrawRenderer 개선
```csharp
public class DrawRenderer : IDrawBuffer
{
    private readonly IGeometry _geometry;
    
    public void Execute()
    {
        _geometry.Bind();
        
        // Geometry 타입에 따라 적절한 드로우 콜
        if (_geometry is IIndexedGeometry indexed)
        {
            GL.DrawElements(..., indexed.IndexCount, ...);
        }
        else if (_geometry is IInstancedGeometry instanced)
        {
            GL.DrawElementsInstanced(..., instanced.InstanceCount);
        }
        // ...
    }
}
```

### 3단계: 점진적 마이그레이션
1. 새 인터페이스 정의 ?
2. 기존 클래스에 인터페이스 구현 ?
3. DrawRenderer를 인터페이스 기반으로 수정 ?
4. Strategy 클래스 제거 ??
5. Execute 메서드를 Obsolete로 표시 ??
6. 이름 변경 (Draw* → *) ??

---

## ? 기대 효과

### Before:
```csharp
// 복잡한 이중 구조
var geometry = new DrawElementGeometry<GLVertex>(...);
var strategy = new ElementDrawStrategy<GLVertex>(geometry);
var renderer = new DrawRenderer(strategy, shader);
renderer.Execute();
```

### After:
```csharp
// 단순하고 명확한 구조
var geometry = new ElementGeometry<GLVertex>(...);
var renderer = new DrawRenderer(geometry, shader);
renderer.Execute();

// 또는 Factory 사용
var renderer = DrawRendererFactory.Create(geometry, shader);
renderer.Execute();
```

### 개선 효과:
- ? 역할 명확화 (데이터 vs 렌더링)
- ? 코드 중복 제거
- ? 유지보수 용이
- ? 성능 향상 (불필요한 래퍼 제거)
- ? 테스트 용이 (책임 분리)

---

## ?? 체크리스트

### 분석 완료 ?
- [x] Buffer 디렉토리 클래스 분석
- [x] Render 디렉토리 클래스 분석
- [x] 역할 중복 식별
- [x] SRP 위반 식별

### 재구조화 계획 ?
- [x] 인터페이스 설계
- [x] 클래스 이름 재정의
- [x] 디렉토리 구조 재정의
- [x] 마이그레이션 전략 수립

### 실행 대기 ?
- [ ] 인터페이스 추가
- [ ] DrawRenderer 개선
- [ ] Strategy 제거
- [ ] 이름 변경
- [ ] 테스트
- [ ] 문서 업데이트

---

## ?? 다음 단계

1. **인터페이스 정의** - IGeometry, IIndexedGeometry 등
2. **DrawRenderer 개선** - 인터페이스 기반으로 단순화
3. **점진적 마이그레이션** - 하위 호환 유지하며 개선
4. **테스트 및 검증** - 기능 정상 작동 확인
5. **문서 업데이트** - 새 구조 반영

---

이 계획에 따라 Buffer와 Render의 역할을 명확히 분리하고 중복을 제거합니다!
