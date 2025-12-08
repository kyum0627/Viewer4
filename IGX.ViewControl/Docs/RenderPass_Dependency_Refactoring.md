# RenderPass 의존성 개선 제안

## ?? 현재 문제점

### 1. God Object Anti-Pattern
```csharp
// 현재: IgxViewAPI를 통째로 전달
public void Initialize(object? context, object? color = null)
{
    if (context is IgxViewAPI apis)
    {
        _apis = apis;  // 너무 많은 책임을 가진 객체
    }
}

// FBOPass에서 실제로 사용하는 것:
- _apis.RenderManager.AllDrawBuffers  // 렌더링할 버퍼 목록
- _apis.Shading.Mode                   // 쉐이딩 모드
```

### 2. 불명확한 의존성
```csharp
// ForwardPass가 실제로 필요한 것:
- 렌더링할 DrawBuffer 목록
- 조명 정보
- 배경색

// 하지만 IgxViewAPI 전체를 받음
private IgxViewAPI _apis;
```

### 3. 초기화 메서드의 모호함
```csharp
// object? 타입으로 받아서 타입 체크 필요
public void Initialize(object? context1, object? context2)
{
    if (context1 is IMyCamera camera) { ... }
    else if (context1 is IgxViewAPI apis) { ... }
    else if (context1 is List<BasicInstance> list) { ... }
}
```

---

## ? 개선 방안

### 1. 구체적인 컨텍스트 인터페이스 정의

```csharp
/// <summary>
/// 렌더링 Pass가 필요로 하는 렌더링 데이터 제공자
/// </summary>
public interface IRenderDataProvider
{
    IReadOnlyList<IDrawBuffer> DrawBuffers { get; }
}

/// <summary>
/// 렌더링 Pass가 필요로 하는 설정 정보 제공자
/// </summary>
public interface IRenderSettings
{
    ShadeMode ShadeMode { get; }
    Vector4 BackgroundColor { get; }
    bool DisplayEdges { get; }
    float EdgeThickness { get; }
    Vector3 EdgeColor { get; }
}

/// <summary>
/// 조명 정보 제공자
/// </summary>
public interface ILightingProvider
{
    Vector3 Direction { get; }
    Vector4 Color { get; }
}
```

### 2. Pass별 구체적인 의존성 주입

#### A. FBOPass (G-Buffer Pass)
```csharp
public class FBOPass : IRenderPass
{
    private IRenderDataProvider? _dataProvider;
    private IRenderSettings? _settings;
    
    // 명확한 초기화
    public void Initialize(IRenderDataProvider dataProvider, IRenderSettings settings)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        PassShader = ShaderManager.Create(
            "GBuffer", 
            ShaderSource.gBufferVtx, 
            ShaderSource.gBufferFrg, 
            false, 
            true);
        Gbuffer = new FrameBufferObject(1, 1);
    }
    
    public void Execute()
    {
        // 명확한 의존성 사용
        var drawBuffers = _dataProvider.DrawBuffers;
        var shadeMode = _settings.ShadeMode;
        
        // ... 렌더링 로직
    }
}
```

#### B. ForwardPass
```csharp
public class ForwardPass : IRenderPass
{
    private IRenderDataProvider? _dataProvider;
    private ILightingProvider? _lighting;
    private IRenderSettings? _settings;
    
    public void Initialize(
        IRenderDataProvider dataProvider, 
        ILightingProvider lighting,
        IRenderSettings settings)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _lighting = lighting ?? throw new ArgumentNullException(nameof(lighting));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        PassShader = ShaderManager.Create(
            "Forward", 
            ShaderSource.forwardVtx, 
            ShaderSource.forwardFrg, 
            false, 
            true);
    }
    
    public void Execute()
    {
        GLUtil.SetClearColor(_settings.BackgroundColor);
        
        using (PassShader.Use(_camera, _lighting))
        {
            foreach (var buffer in _dataProvider.DrawBuffers)
            {
                buffer.Execute();
            }
        }
    }
}
```

#### C. PostFBOPass
```csharp
public class PostFBOPass : IRenderPass
{
    private ILightingProvider? _lighting;
    private IRenderSettings? _settings;
    private FrameBufferObject? _sourceGBuffer;
    
    public void Initialize(
        FrameBufferObject sourceGBuffer,
        ILightingProvider lighting,
        IRenderSettings settings)
    {
        _sourceGBuffer = sourceGBuffer ?? throw new ArgumentNullException(nameof(sourceGBuffer));
        _lighting = lighting ?? throw new ArgumentNullException(nameof(lighting));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        _outlinePostShader = ShaderManager.Create(
            "OutLine", 
            ShaderSource.quadVtx, 
            ShaderSource.outlinePostFrg, 
            false, 
            true);
        _quad = new Quad();
        _quad.InitializeQuad();
    }
}
```

### 3. IgxViewAPI에 Provider 구현

```csharp
public class IgxViewAPI : IRenderDataProvider, IRenderSettings, ILightingProvider
{
    // IRenderDataProvider 구현
    public IReadOnlyList<IDrawBuffer> DrawBuffers => RenderManager.AllDrawBuffers;
    
    // IRenderSettings 구현
    public ShadeMode ShadeMode => Shading.Mode;
    public Vector4 BackgroundColor => BackGroundColor;
    public bool DisplayEdges => Shading.DisplayEdge;
    public float EdgeThickness => Shading.EdgeThickness;
    public Vector3 EdgeColor => Shading.EdgeColor;
    
    // ILightingProvider 구현
    Vector3 ILightingProvider.Direction => Lighting.Direction;
    Vector4 ILightingProvider.Color => Lighting.Color;
    
    // 기존 속성들...
    public RenderManager RenderManager { get; }
    public ShadingInfo Shading { get; }
    public LightingInfo Lighting { get; }
    // ...
}
```

### 4. GLView에서 Pass 초기화

```csharp
private void InitializeRenderPasses()
{
    // 명확한 의존성 전달
    _forwardPass = new ForwardPass();
    _forwardPass.Initialize(
        dataProvider: _apis,
        lighting: _apis,
        settings: _apis);
    
    _fboPass = new FBOPass();
    _fboPass.Initialize(
        dataProvider: _apis,
        settings: _apis);
    
    _postFBOPass = new PostFBOPass();
    _postFBOPass.Initialize(
        sourceGBuffer: _fboPass.Gbuffer!,
        lighting: _apis,
        settings: _apis);
    
    // Auxiliary passes
    _objectboxPass = new ObjectBoxPass();
    _objectboxPass.Initialize(
        instanceProvider: () => _apis.SelectionManager.InstancedBoxes,
        color: _apis.SceneParameter.BoxColor);
}
```

---

## ?? 개선 효과

### 1. 명확한 의존성
```csharp
// Before: 무엇을 사용하는지 불명확
private IgxViewAPI _apis;

// After: 명확하게 필요한 것만 선언
private IRenderDataProvider _dataProvider;
private IRenderSettings _settings;
private ILightingProvider _lighting;
```

### 2. 테스트 용이성
```csharp
// Mock 객체 생성이 쉬워짐
var mockDataProvider = new Mock<IRenderDataProvider>();
var mockSettings = new Mock<IRenderSettings>();

var pass = new FBOPass();
pass.Initialize(mockDataProvider.Object, mockSettings.Object);
```

### 3. 단일 책임 원칙 (SRP)
- 각 Provider는 특정 영역의 데이터만 제공
- Pass는 필요한 Provider만 의존

### 4. 인터페이스 분리 원칙 (ISP)
- 클라이언트(Pass)는 사용하지 않는 메서드에 의존하지 않음
- 작고 구체적인 인터페이스

---

## ?? 비교표

| 항목 | Before | After |
|------|--------|-------|
| **의존성** | `IgxViewAPI` (전체) | 구체적 인터페이스 (필요한 것만) |
| **초기화** | `object?` 타입 + 런타임 체크 | 강타입 + 컴파일 타임 체크 |
| **가독성** | 낮음 (무엇을 사용하는지 불명확) | 높음 (필드 타입으로 명확) |
| **테스트** | 어려움 (IgxViewAPI 전체 Mock) | 쉬움 (필요한 것만 Mock) |
| **결합도** | 높음 | 낮음 |

---

## ?? 마이그레이션 단계

### Phase 1: 인터페이스 정의
1. `IRenderDataProvider`, `IRenderSettings`, `ILightingProvider` 추가
2. `IgxViewAPI`에 인터페이스 구현

### Phase 2: Pass 리팩토링 (하나씩)
1. `FBOPass` 리팩토링
2. `ForwardPass` 리팩토링
3. `PostFBOPass` 리팩토링
4. Auxiliary Passes 리팩토링

### Phase 3: 초기화 코드 업데이트
1. `GLView.InitializeRenderPasses()` 수정
2. 기존 `object?` 기반 초기화 제거

### Phase 4: IRenderPass 인터페이스 업데이트
```csharp
public interface IRenderPass
{
    // Before: 모호한 초기화
    void Initialize(object? context1 = null, object? context2 = null);
    
    // After: 구체적 초기화는 각 Pass에서 정의
    // Initialize를 인터페이스에서 제거하거나
    // 또는 제네릭으로 변경
}
```

---

## ?? 추가 개선 아이디어

### 1. Factory Pattern 적용
```csharp
public class RenderPassFactory
{
    private readonly IgxViewAPI _apis;
    
    public ForwardPass CreateForwardPass()
    {
        var pass = new ForwardPass();
        pass.Initialize(_apis, _apis, _apis);
        return pass;
    }
    
    public FBOPass CreateFBOPass()
    {
        var pass = new FBOPass();
        pass.Initialize(_apis, _apis);
        return pass;
    }
}
```

### 2. Builder Pattern 적용
```csharp
public class RenderPassBuilder
{
    private IRenderDataProvider? _dataProvider;
    private IRenderSettings? _settings;
    private ILightingProvider? _lighting;
    
    public RenderPassBuilder WithDataProvider(IRenderDataProvider provider)
    {
        _dataProvider = provider;
        return this;
    }
    
    public RenderPassBuilder WithSettings(IRenderSettings settings)
    {
        _settings = settings;
        return this;
    }
    
    public ForwardPass BuildForwardPass()
    {
        var pass = new ForwardPass();
        pass.Initialize(_dataProvider!, _lighting!, _settings!);
        return pass;
    }
}

// 사용
var pass = new RenderPassBuilder()
    .WithDataProvider(_apis)
    .WithSettings(_apis)
    .WithLighting(_apis)
    .BuildForwardPass();
```

---

## ?? 설계 원칙 준수

? **SOLID 원칙**
- **S**ingle Responsibility: 각 Provider는 하나의 책임
- **O**pen/Closed: 새로운 Provider 추가 가능
- **L**iskov Substitution: Provider 구현체 교체 가능
- **I**nterface Segregation: 작고 구체적인 인터페이스
- **D**ependency Inversion: 구체적 클래스가 아닌 인터페이스에 의존

? **명확한 계약(Contract)**
- 각 Pass가 필요로 하는 것이 명확
- 컴파일 타임에 의존성 검증

? **낮은 결합도**
- Pass는 IgxViewAPI 전체가 아닌 필요한 Provider만 의존
- Provider 변경이 Pass에 영향을 주지 않음
