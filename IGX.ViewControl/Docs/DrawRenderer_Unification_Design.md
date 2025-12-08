# Draw 렌더러 통합 설계안

## ?? 목표

4개의 Default*Draw 클래스를 전략 패턴을 사용하여 통합

---

## ?? 새로운 구조

### 1. **IDrawStrategy** - 렌더링 전략 인터페이스

```csharp
/// <summary>
/// OpenGL 드로우 호출 전략 인터페이스
/// 각 렌더링 방식(Indirect, Instanced, SSBO, Elements)에 대한 구체적 구현 제공
/// </summary>
public interface IDrawStrategy
{
    /// <summary>
    /// 전체 렌더링 실행
    /// </summary>
    void ExecuteDraw(PrimitiveType primType);
    
    /// <summary>
    /// 범위 지정 렌더링 실행
    /// </summary>
    void ExecuteDrawRange(PrimitiveType primType, int offset, int count);
    
    /// <summary>
    /// 지오메트리 바인딩
    /// </summary>
    void Bind();
    
    /// <summary>
    /// 지오메트리 언바인딩
    /// </summary>
    void Unbind();
    
    /// <summary>
    /// 렌더링 가능 여부 (데이터 검증)
    /// </summary>
    bool CanDraw { get; }
}
```

### 2. **DrawRenderer** - 통합 렌더러 (Template Method Pattern)

```csharp
/// <summary>
/// 통합 드로우 렌더러
/// 전략 패턴을 사용하여 다양한 렌더링 방식 지원
/// </summary>
public class DrawRenderer : IDrawBuffer
{
    private readonly IDrawStrategy _strategy;
    
    public Shader? Shader { get; set; }
    public PrimitiveType PrimType { get; set; } = PrimitiveType.Triangles;

    public DrawRenderer(IDrawStrategy strategy, Shader? shader = null)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        Shader = shader;
    }

    #region IDrawBuffer 구현
    
    public void Execute()
    {
        if (!_strategy.CanDraw) return;

        ExecuteDrawCommon(
            drawAction: () => _strategy.ExecuteDraw(PrimType),
            overrideShader: null);
    }

    public void ExecuteRange(int offset, int count)
    {
        if (count <= 0 || !_strategy.CanDraw) return;

        ExecuteDrawCommon(
            drawAction: () => _strategy.ExecuteDrawRange(PrimType, offset, count),
            overrideShader: null);
    }

    public void ExecuteWithShader(Shader? shader)
    {
        ArgumentNullException.ThrowIfNull(shader);
        
        if (!_strategy.CanDraw) return;

        ExecuteDrawCommon(
            drawAction: () => _strategy.ExecuteDraw(PrimType),
            overrideShader: shader);
    }
    
    #endregion

    #region 공통 실행 패턴
    
    private void ExecuteDrawCommon(Action drawAction, Shader? overrideShader)
    {
        GLUtil.EnsureContextActive();

        try
        {
            BindResources(overrideShader);
            drawAction();
        }
        finally
        {
            _strategy.Unbind();
        }
    }

    private void BindResources(Shader? overrideShader)
    {
        _strategy.Bind();
        
        Shader? activeShader = overrideShader ?? Shader;
        activeShader?.Use();
    }
    
    #endregion
}
```

### 3. **구체적 전략 구현**

#### A. IndirectDrawStrategy
```csharp
/// <summary>
/// 간접 드로우 전략 (MultiDrawElementsIndirect)
/// </summary>
internal class IndirectDrawStrategy : IDrawStrategy
{
    private readonly DrawIndirectGeometry<VTX, IDX, NST> _geometry;
    private static readonly int _commandSize = Marshal.SizeOf<IndirectCommandData>();

    public IndirectDrawStrategy(DrawIndirectGeometry<VTX, IDX, NST> geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public bool CanDraw => _geometry.CommandCount > 0;

    public void ExecuteDraw(PrimitiveType primType)
    {
        if (!GLUtil.IsContextActive()) return;

        GL.MultiDrawElementsIndirect(
            primType,
            _geometry.ElementType,
            IntPtr.Zero,
            _geometry.CommandCount,
            _commandSize);
    }

    public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
    {
        if (!GLUtil.IsContextActive()) return;

        IntPtr byteOffset = (nint)(offset * _commandSize);
        
        GL.MultiDrawElementsIndirect(
            primType,
            _geometry.ElementType,
            byteOffset,
            count,
            _commandSize);
    }

    public void Bind() => _geometry.Bind();
    public void Unbind() => _geometry.Unbind();
}
```

#### B. InstanceDrawStrategy
```csharp
/// <summary>
/// 인스턴스드 드로우 전략 (DrawElementsInstanced)
/// </summary>
internal class InstanceDrawStrategy : IDrawStrategy
{
    private readonly DrawInstanceGeometry<VTX, IDX, NST> _geometry;
    private static readonly int _indexSize = Marshal.SizeOf<IDX>();

    public InstanceDrawStrategy(DrawInstanceGeometry<VTX, IDX, NST> geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public bool CanDraw => _geometry.IndexCount > 0 && _geometry.InstanceCount > 0;

    public void ExecuteDraw(PrimitiveType primType)
    {
        if (!GLUtil.IsContextActive()) return;

        GL.DrawElementsInstanced(
            primType,
            _geometry.IndexCount,
            _geometry.ElementType,
            IntPtr.Zero,
            _geometry.InstanceCount);
    }

    public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
    {
        if (!GLUtil.IsContextActive()) return;

        IntPtr byteOffset = (IntPtr)(offset * _indexSize);

        GL.DrawElementsInstanced(
            primType,
            count,
            _geometry.ElementType,
            byteOffset,
            _geometry.InstanceCount);
    }

    public void Bind() => _geometry.Bind();
    public void Unbind() => _geometry.Unbind();
}
```

#### C. SSBODrawStrategy
```csharp
/// <summary>
/// SSBO 기반 인스턴스드 드로우 전략
/// </summary>
internal class SSBODrawStrategy : IDrawStrategy
{
    private readonly DrawSSBObaseGeometry<VTX, NST> _geometry;

    public SSBODrawStrategy(DrawSSBObaseGeometry<VTX, NST> geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public bool CanDraw => _geometry.IndexCount > 0 && _geometry.InstanceCount > 0;

    public void ExecuteDraw(PrimitiveType primType)
    {
        if (!GLUtil.IsContextActive()) return;

        GL.DrawElementsInstancedBaseInstance(
            primType,
            _geometry.IndexCount,
            _geometry.ElementType,
            IntPtr.Zero,
            _geometry.InstanceCount,
            0);
    }

    public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
    {
        // SSBO는 범위 렌더링을 전체 렌더링으로 폴백
        ExecuteDraw(primType);
    }

    public void Bind() => _geometry.Bind();
    public void Unbind() => _geometry.Unbind();
}
```

#### D. ElementDrawStrategy
```csharp
/// <summary>
/// 인덱스 기반 드로우 전략 (DrawElements)
/// </summary>
internal class ElementDrawStrategy : IDrawStrategy
{
    private readonly DrawElementGeometry<VTX> _geometry;
    private static readonly int _indexSize = Marshal.SizeOf<uint>();

    public ElementDrawStrategy(DrawElementGeometry<VTX> geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public bool CanDraw => _geometry.IndexCount > 0;

    public void ExecuteDraw(PrimitiveType primType)
    {
        if (!GLUtil.IsContextActive()) return;

        GL.DrawElements(
            primType,
            _geometry.IndexCount,
            _geometry.ElementType,
            IntPtr.Zero);
    }

    public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
    {
        if (!GLUtil.IsContextActive()) return;

        IntPtr byteOffset = (nint)(offset * _indexSize);

        GL.DrawElements(
            primType,
            count,
            _geometry.ElementType,
            byteOffset);
    }

    public void Bind() => _geometry.Bind();
    public void Unbind() => _geometry.Unbind();
}
```

---

## ?? 사용법

### Before (현재)
```csharp
// 4개의 다른 클래스 사용
var indirectRenderer = new DefaultIndirectDraw(indirectGeometry);
var instanceRenderer = new DefaultInstanceDraw(instanceGeometry);
var ssboRenderer = new DefaultSSBODraw(ssboGeometry);
var elementRenderer = new DefaultElementGeometryDraw(elementGeometry);

indirectRenderer.Execute();
instanceRenderer.Execute();
ssboRenderer.Execute();
elementRenderer.Execute();
```

### After (통합)
```csharp
// 통일된 인터페이스
var indirectRenderer = new DrawRenderer(new IndirectDrawStrategy(indirectGeometry));
var instanceRenderer = new DrawRenderer(new InstanceDrawStrategy(instanceGeometry));
var ssboRenderer = new DrawRenderer(new SSBODrawStrategy(ssboGeometry));
var elementRenderer = new DrawRenderer(new ElementDrawStrategy(elementGeometry));

// 모두 동일한 방식
indirectRenderer.Execute();
instanceRenderer.Execute();
ssboRenderer.Execute();
elementRenderer.Execute();

// 또는 Factory 사용
var renderer = DrawRendererFactory.Create(geometry);
renderer.Execute();
```

---

## ?? Factory Pattern 추가

```csharp
/// <summary>
/// 지오메트리 타입에 따라 적절한 렌더러 생성
/// </summary>
public static class DrawRendererFactory
{
    public static IDrawBuffer Create<VTX, IDX, NST>(
        DrawIndirectGeometry<VTX, IDX, NST> geometry, 
        Shader? shader = null)
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        var strategy = new IndirectDrawStrategy(geometry);
        return new DrawRenderer(strategy, shader);
    }

    public static IDrawBuffer Create<VTX, IDX, NST>(
        DrawInstanceGeometry<VTX, IDX, NST> geometry, 
        Shader? shader = null)
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        var strategy = new InstanceDrawStrategy(geometry);
        return new DrawRenderer(strategy, shader);
    }

    public static IDrawBuffer Create<VTX, NST>(
        DrawSSBObaseGeometry<VTX, NST> geometry, 
        Shader? shader = null)
        where VTX : unmanaged
        where NST : unmanaged
    {
        var strategy = new SSBODrawStrategy(geometry);
        return new DrawRenderer(strategy, shader);
    }

    public static IDrawBuffer Create<VTX>(
        DrawElementGeometry<VTX> geometry, 
        Shader? shader = null)
        where VTX : unmanaged
    {
        var strategy = new ElementDrawStrategy(geometry);
        return new DrawRenderer(strategy, shader);
    }
}
```

---

## ?? 비교표

| 항목 | Before (4개 클래스) | After (통합) |
|------|-------------------|------------|
| **코드 중복** | 높음 (80% 중복) | ? 낮음 (공통 로직 1곳) |
| **유지보수** | 어려움 (4곳 수정) | ? 쉬움 (1곳 수정) |
| **확장성** | 낮음 (새 클래스 필요) | ? 높음 (새 Strategy 추가) |
| **테스트** | 어려움 (4개 테스트) | ? 쉬움 (Strategy Mock) |
| **일관성** | 보통 (각자 구현) | ? 높음 (통일된 패턴) |

---

## ? 장점

1. **코드 중복 제거**: 공통 로직이 DrawRenderer에 한 번만 존재
2. **전략 교체 용이**: 런타임에 전략 변경 가능
3. **테스트 용이**: IDrawStrategy를 Mock하여 단위 테스트
4. **확장 용이**: 새로운 렌더링 방식 추가가 간단
5. **일관성**: 모든 렌더러가 동일한 인터페이스

---

## ?? 마이그레이션 전략

### Phase 1: 새 구조 추가 (병행)
- IDrawStrategy 인터페이스 생성
- DrawRenderer 클래스 생성
- 4개 Strategy 클래스 생성
- Factory 추가

### Phase 2: 점진적 전환
- 새 코드는 DrawRenderer 사용
- 기존 코드는 Default*Draw 유지 (하위 호환)

### Phase 3: 완전 전환
- 모든 코드를 DrawRenderer로 전환
- Default*Draw 클래스에 [Obsolete] 표시
- 문서 업데이트

### Phase 4: 정리
- Default*Draw 클래스 제거
- 최종 테스트

---

이 통합 설계를 적용하면 코드가 훨씬 깔끔하고 유지보수하기 쉬워집니다! ??
