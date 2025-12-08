# GLView 및 Pass 클래스 의존성 개선 제안

## ?? 현재 문제점

### 1. **GLView가 IgxViewAPI 전체를 보유**

```csharp
public partial class GLView : UserControl
{
    private readonly IgxViewAPI _apis;  // ? 전체 API 객체
    
    // 실제로 사용하는 것:
    private List<BasicInstance> _boxesToDraw => _apis.SelectionManager.InstancedBoxes;
    private List<GLVertex> _vectorsToDraw => _apis.SelectionManager.InstancedVectors;
    private Vector4 _boxColor => _apis.SceneParameter.BoxColor;
    
    _apis.Drawing.Normals = !_apis.Drawing.Normals;
    _apis.Shading.Mode = newMode;
}
```

**문제점:**
- GLView가 필요 이상의 책임과 접근 권한을 가짐
- 변경 이유가 많아짐 (SRP 위반)
- 테스트가 어려움 (전체 IgxViewAPI Mock 필요)

### 2. **Pass 클래스들도 과도한 의존성**

```csharp
// ForwardPass
_forwardPass.Initialize(_camera, _apis);  // ? 전체 API

// FBOPass - 이미 개선됨 ?
_gBufferPass.Initialize(
    dataProvider: _apis,
    settings: _apis);

// PostFBOPass - 이미 개선됨 ?
_postFBOPass.Initialize(
    sourceGBuffer: _gBufferPass.Gbuffer!,
    lighting: _apis,
    settings: _apis);
```

---

## ? 개선 방안

### 1. **GLView 리팩토링 - 구체적 의존성 주입**

#### A. 새로운 생성자

```csharp
public partial class GLView : UserControl
{
    private readonly IMyCamera _camera;
    
    // ? 구체적인 의존성만 주입
    private readonly IRenderSettings _renderSettings;
    private readonly AuxillaryDrawSetting _drawingSettings;
    private readonly SelectionManager _selectionManager;
    private readonly SceneParameters _sceneParameters;
    
    // 이벤트 구독을 위한 참조 (읽기 전용)
    private readonly ShaderParameters _shading;
    
    public GLView(
        IMyCamera camera,
        IRenderSettings renderSettings,
        AuxillaryDrawSetting drawingSettings,
        SelectionManager selectionManager,
        SceneParameters sceneParameters,
        ShaderParameters shading,
        GLControl? shared = null)
    {
        InitializeComponent();
        
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _renderSettings = renderSettings ?? throw new ArgumentNullException(nameof(renderSettings));
        _drawingSettings = drawingSettings ?? throw new ArgumentNullException(nameof(drawingSettings));
        _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));
        _sceneParameters = sceneParameters ?? throw new ArgumentNullException(nameof(sceneParameters));
        _shading = shading ?? throw new ArgumentNullException(nameof(shading));
        
        // 이벤트 구독
        _shading.ModeChanged += OnShadeModeChanged;
        _drawingSettings.SettingChanged += OnDrawingSettingChanged;
        
        SetStrategyBasedOnShadeMode();
        SetupGLControl();
        
        this.KeyDown += GLView_KeyDown;
    }
    
    // ? IgxViewAPI를 받는 편의 생성자 (하위 호환성)
    public GLView(IgxViewAPI api, MyCamera? camera = null, GLControl? shared = null)
        : this(
            camera ?? (MyCamera)api.SceneParameter.Camera,
            api,                        // IRenderSettings
            api.Drawing,
            api.SelectionManager,
            api.SceneParameter,
            api.Shading,
            shared)
    {
    }
}
```

#### B. 속성 업데이트

```csharp
// Before
private List<BasicInstance> _boxesToDraw => _apis.SelectionManager.InstancedBoxes;
private List<GLVertex> _vectorsToDraw => _apis.SelectionManager.InstancedVectors;
private Vector4 _boxColor => _apis.SceneParameter.BoxColor;
private Vector4 _vectorColor => _apis.SceneParameter.VectorColor;

// After
private List<BasicInstance> _boxesToDraw => _selectionManager.InstancedBoxes;
private List<GLVertex> _vectorsToDraw => _selectionManager.InstancedVectors;
private Vector4 _boxColor => _sceneParameters.BoxColor;
private Vector4 _vectorColor => _sceneParameters.VectorColor;
```

#### C. 메서드 업데이트

```csharp
// Before
_apis.Drawing.Normals = !_apis.Drawing.Normals;
_apis.Shading.Mode = newMode;
_camera.Fit(_apis.SceneParameter.TotalBoundingBox);

// After
_drawingSettings.Normals = !_drawingSettings.Normals;
_shading.Mode = newMode;
_camera.Fit(_sceneParameters.TotalBoundingBox);
```

#### D. Pass 초기화 업데이트

```csharp
private void InitializeRenderPasses()
{
    // ? ForwardPass도 구체적 의존성 주입으로 변경 필요
    _forwardPass = new ForwardPass();
    _forwardPass.Initialize(_camera, _renderSettings, /* 필요한 것만 */);

    // ? 이미 개선됨
    _gBufferPass = new FBOPass();
    _gBufferPass.Initialize(
        dataProvider: _renderSettings as IRenderDataProvider,
        settings: _renderSettings);

    _postFBOPass = new PostFBOPass();
    _postFBOPass.Initialize(
        sourceGBuffer: _gBufferPass.Gbuffer!,
        lighting: _renderSettings as ILightingProvider,
        settings: _renderSettings);

    // Auxiliary passes
    _backgroundPass = new BackgroundPass();
    _backgroundPass.Initialize(_camera);

    _coordinatePass = new CoordinatePass();
    _coordinatePass.Initialize(_camera);

    _objectboxPass = new ObjectBoxPass();
    _objectboxPass.Initialize(_boxesToDraw, _boxColor);

    _normalvectorPass = new NormalVectorsPass();
    _normalvectorPass.Initialize(_vectorsToDraw, _vectorColor);

    _clipPlanePass = new ClipPlanePass();
    _clipPlanePass.Initialize(_sceneParameters, _boxColor);
}
```

### 2. **ForwardPass 리팩토링**

현재 `ForwardPass`도 `IgxViewAPI` 전체를 받고 있습니다:

```csharp
// Before
public class ForwardPass : IRenderPass
{
    private IgxViewAPI _apis;  // ?
    
    public void Initialize(object? context = null, object? temp = null)
    {
        if (context is IMyCamera camera) { _camera = camera; }
        if (temp is IgxViewAPI mesh) { _apis = mesh; }
        // ...
    }
}
```

#### 개선안:

```csharp
/// <summary>
/// Forward 렌더링 패스 - 구체적 의존성 주입
/// </summary>
public class ForwardPass : IRenderPass
{
    private IMyCamera? _camera;
    private IRenderDataProvider? _dataProvider;
    private ILightingProvider? _lighting;
    private IRenderSettings? _settings;
    private Shader? _forwardShader;
    
    // ? 명확한 초기화 메서드
    public void Initialize(
        IMyCamera camera,
        IRenderDataProvider dataProvider,
        ILightingProvider lighting,
        IRenderSettings settings)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        _lighting = lighting ?? throw new ArgumentNullException(nameof(lighting));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        
        try
        {
            PassShader = ShaderManager.Create("Forward", ShaderSource.forwardVtx, ShaderSource.forwardFrg, false, true);
            _forwardShader = PassShader;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ErrorCheck initializing ForwardPass: {ex.Message}");
            Dispose();
            throw;
        }
    }
    
    // ? 하위 호환성을 위한 Obsolete 메서드
    [Obsolete("Use Initialize(IMyCamera, IRenderDataProvider, ILightingProvider, IRenderSettings) instead")]
    public void Initialize(object? context = null, object? temp = null)
    {
        if (context is IMyCamera camera && 
            temp is IgxViewAPI apis)
        {
            Initialize(camera, apis, apis, apis);
        }
        else
        {
            throw new ArgumentException("Invalid initialization parameters");
        }
    }
    
    public void Execute()
    {
        if (_isDisposed || PassShader == null || !Enabled || _camera == null || 
            _dataProvider == null || _lighting == null || _settings == null) 
            return;
        
        // ? 명확한 의존성 사용
        GLUtil.SetClearColor(_settings.BackgroundColor);
        GLUtil.ClearDrawingBuffer();
        
        GLUtil.SetDepthTest(true);
        GLUtil.SetDepthMask(true);
        GLUtil.DepthFunc(DepthFunction.Less);
        GLUtil.SetCullFace(true, TriangleFace.Back);

        using (PassShader.Use(_camera, _lighting))
        {
            PassShader.SetUniformIfExist("uView", _viewMatrix);
            PassShader.SetUniformIfExist("uProjection", _projectionMatrix);
            PassShader.SetUniformIfExist("uCameraPosition", _camera.Position);
            PassShader.SetUniformIfExist("shadeMode", (int)_settings.ShadeMode);

            // ? 명확한 의존성 사용
            foreach (var bufferManager in _dataProvider.DrawBuffers)
            {
                if (bufferManager != null)
                {
                    bufferManager.Execute();
                }
            }
        }
    }
}
```

### 3. **PickHelper 리팩토링**

`PickHelper`도 `IgxViewAPI` 전체를 받고 있습니다:

```csharp
// Before
public static void PickUnPick(Keys keyboard, MouseEventArgs e, GLControl glControl, IgxViewAPI vc, bool bMouseMove)
{
    var models = vc.ModelManager.Models.ToArray();
    var byPart = vc.SelectionManager.Selection.ByPart;
    var selection = vc.SelectionManager;
    var camera = vc.SceneParameter.Camera;
    // ...
}
```

#### 개선안:

```csharp
/// <summary>
/// 피킹 컨텍스트 - PickHelper에 필요한 데이터만 제공
/// </summary>
public interface IPickContext
{
    IReadOnlyList<Model3D> Models { get; }
    SelectionManager SelectionManager { get; }
    IMyCamera Camera { get; }
}

/// <summary>
/// IgxViewAPI에서 IPickContext 구현
/// </summary>
public class IgxViewAPI : IRenderDataProvider, IRenderSettings, ILightingProvider, IPickContext
{
    // ...existing code...
    
    #region IPickContext 구현
    
    IReadOnlyList<Model3D> IPickContext.Models => ModelManager.Models;
    SelectionManager IPickContext.SelectionManager => SelectionManager;
    IMyCamera IPickContext.Camera => SceneParameter.Camera;
    
    #endregion
}

/// <summary>
/// PickHelper - 구체적 의존성 사용
/// </summary>
public static class PickHelper
{
    public static void PickUnPick(
        Keys keyboard, 
        MouseEventArgs e, 
        GLControl glControl, 
        IPickContext context,  // ? 구체적 인터페이스
        bool bMouseMove)
    {
        var models = context.Models.ToArray();
        var byPart = context.SelectionManager.Selection.ByPart;
        var selection = context.SelectionManager;
        var camera = context.Camera;
        
        Matrix4 invVP = camera.InvVP;
        
        // ...existing logic...
    }
}

// GLView에서 사용
private void OnMouseUpForPicking(object? sender, MouseEventArgs e)
{
    OnMouseUp(sender, e);
    if (!_mouseDragged && !_isClipBoxMode)
    {
        _glControl.MakeCurrent();
        
        // ? IPickContext 생성 또는 IgxViewAPI를 IPickContext로 캐스팅
        PickHelper.PickUnPick(Keys.None, e, _glControl, _apis, false);
    }
    _mouseDragged = false;
    _glControl.Invalidate();
}
```

---

## ?? 비교표

| 클래스 | Before | After |
|--------|--------|-------|
| **GLView** | `IgxViewAPI _apis` (전체) | 구체적 인터페이스 5개 |
| **ForwardPass** | `IgxViewAPI _apis` | `IRenderDataProvider`, `ILightingProvider`, `IRenderSettings` |
| **FBOPass** | ? 이미 개선됨 | `IRenderDataProvider`, `IRenderSettings` |
| **PostFBOPass** | ? 이미 개선됨 | `FrameBufferObject`, `ILightingProvider`, `IRenderSettings` |
| **PickHelper** | `IgxViewAPI vc` | `IPickContext` |

---

## ?? 개선 효과

### 1. **단일 책임 원칙 (SRP) 준수**
```csharp
// Before: GLView가 너무 많은 것을 알고 있음
_apis.ModelManager.Clear();
_apis.RenderManager.ClearRenderResources();
_apis.SelectionManager.Clear();

// After: GLView는 자신의 책임에만 집중
_drawingSettings.Normals = !_drawingSettings.Normals;
```

### 2. **테스트 용이성**
```csharp
// Before: IgxViewAPI 전체를 Mock해야 함
var mockApis = new Mock<IgxViewAPI>();
mockApis.Setup(a => a.SelectionManager).Returns(mockSelectionManager);
mockApis.Setup(a => a.SceneParameter).Returns(mockSceneParam);
// ... 수십 개의 Setup

// After: 필요한 것만 Mock
var mockSettings = new Mock<IRenderSettings>();
var mockDrawing = new Mock<AuxillaryDrawSetting>();
var glView = new GLView(camera, mockSettings.Object, mockDrawing.Object, ...);
```

### 3. **명확한 의존성**
```csharp
// Before: 무엇을 사용하는지 불명확
private IgxViewAPI _apis;

// After: 필요한 것이 명확
private IRenderSettings _renderSettings;
private AuxillaryDrawSetting _drawingSettings;
private SelectionManager _selectionManager;
private SceneParameters _sceneParameters;
```

### 4. **변경 영향 최소화**
```csharp
// IgxViewAPI 내부 구조 변경 시
// Before: GLView, 모든 Pass, PickHelper 등이 영향받음
// After: 인터페이스만 유지하면 구현 변경 가능
```

---

## ?? 마이그레이션 단계

### Phase 1: 인터페이스 추가
1. `IPickContext` 인터페이스 정의 ?
2. `IgxViewAPI`에 `IPickContext` 구현 ?

### Phase 2: GLView 리팩토링
1. 새로운 생성자 추가 (구체적 의존성)
2. 기존 생성자는 새 생성자 호출하도록 수정 (하위 호환)
3. 모든 `_apis.XXX` 참조를 구체적 필드로 변경
4. 테스트 및 검증

### Phase 3: ForwardPass 리팩토링
1. 새로운 `Initialize` 메서드 추가
2. 기존 `Initialize`는 `[Obsolete]` 표시
3. GLView에서 새 메서드 사용
4. 테스트 및 검증

### Phase 4: PickHelper 리팩토링
1. `IPickContext` 사용하도록 변경
2. 호출부 업데이트
3. 테스트 및 검증

### Phase 5: 정리
1. Obsolete 메서드 제거 (breaking change - 메이저 버전 업)
2. 기존 생성자 제거 고려

---

## ?? 추가 개선 아이디어

### 1. **Facade 패턴 적용**

GLView가 너무 많은 의존성을 주입받는다면:

```csharp
/// <summary>
/// GLView가 필요한 모든 의존성을 제공하는 Facade
/// </summary>
public interface IGLViewContext
{
    IMyCamera Camera { get; }
    IRenderSettings RenderSettings { get; }
    AuxillaryDrawSetting DrawingSettings { get; }
    SelectionManager SelectionManager { get; }
    SceneParameters SceneParameters { get; }
    ShaderParameters Shading { get; }
}

public class GLView : UserControl
{
    private readonly IGLViewContext _context;
    
    public GLView(IGLViewContext context, GLControl? shared = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        // ...
    }
}

// IgxViewAPI에서 구현
public class IgxViewAPI : IRenderDataProvider, IRenderSettings, ILightingProvider, 
                          IPickContext, IGLViewContext
{
    // ...
    
    #region IGLViewContext 구현
    
    IMyCamera IGLViewContext.Camera => SceneParameter.Camera;
    IRenderSettings IGLViewContext.RenderSettings => this;
    AuxillaryDrawSetting IGLViewContext.DrawingSettings => Drawing;
    SelectionManager IGLViewContext.SelectionManager => SelectionManager;
    SceneParameters IGLViewContext.SceneParameters => SceneParameter;
    ShaderParameters IGLViewContext.Shading => Shading;
    
    #endregion
}
```

### 2. **Builder 패턴**

복잡한 초기화를 단순화:

```csharp
public class GLViewBuilder
{
    private IMyCamera? _camera;
    private IRenderSettings? _renderSettings;
    // ... 다른 의존성
    
    public GLViewBuilder WithCamera(IMyCamera camera)
    {
        _camera = camera;
        return this;
    }
    
    public GLViewBuilder WithRenderSettings(IRenderSettings settings)
    {
        _renderSettings = settings;
        return this;
    }
    
    public GLView Build()
    {
        ValidateRequiredDependencies();
        return new GLView(_camera!, _renderSettings!, ...);
    }
}

// 사용
var glView = new GLViewBuilder()
    .WithCamera(camera)
    .WithRenderSettings(renderSettings)
    .WithDrawingSettings(drawingSettings)
    .Build();
```

---

이 개선안을 적용하면 코드가 훨씬 더 모듈화되고, 테스트 가능하며, 유지보수하기 쉬워집니다! ??
