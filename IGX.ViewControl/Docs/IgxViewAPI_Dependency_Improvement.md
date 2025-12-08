# IgxViewAPI 의존성 주입 개선 가이드

## ?? 목표

`IgxViewAPI`가 God Object가 되는 것을 방지하고, 렌더 패스가 필요한 데이터만 주입받도록 개선

---

## ? 문제점 (Before)

### ForwardPass가 IgxViewAPI 전체를 받음:
```csharp
public class ForwardPass : IRenderPass
{
    private IgxViewAPI _apis;  // ? 모든 것에 접근 가능
    
    public void Initialize(object? context = null, object? temp = null)
    {
        if (temp is IgxViewAPI mesh)  // ? 변수명도 잘못됨
        {
            _apis = mesh;
        }
    }
    
    public void Execute()
    {
        // _apis.RenderManager ? 필요
        // _apis.Lighting ? 필요
        // _apis.BackGroundColor ? 필요
        // _apis.ModelManager ? 불필요
        // _apis.SelectionManager ? 불필요
        // _apis.Shading ? 직접 접근 불필요
    }
}
```

**문제점:**
1. ? ForwardPass가 필요 없는 데이터까지 접근 가능
2. ? 의존성이 명확하지 않음
3. ? 테스트 시 전체 IgxViewAPI를 Mock해야 함
4. ? 순환 의존성 위험

---

## ? 해결 방안 (After)

### 1. Provider 인터페이스 정의

```csharp
/// <summary>
/// 렌더링 데이터 제공
/// </summary>
public interface IRenderDataProvider
{
    IReadOnlyList<IDrawBuffer> DrawBuffers { get; }
}

/// <summary>
/// 렌더링 설정 제공
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
/// 조명 정보 제공
/// </summary>
public interface ILightingProvider
{
    Vector3 Direction { get; }
    Vector4 Color { get; }
    ILight Light { get; }  // Shader.Use 호환성
}

/// <summary>
/// 렌더 패스 컨텍스트 (필요한 것만 제공)
/// </summary>
public interface IRenderPassContext
{
    IRenderDataProvider DataProvider { get; }
    IRenderSettings Settings { get; }
    ILightingProvider LightingProvider { get; }
}
```

### 2. IgxViewAPI가 IRenderPassContext 구현

```csharp
public class IgxViewAPI : 
    IRenderDataProvider,      // DrawBuffers 제공
    IRenderSettings,           // 렌더링 설정 제공
    ILightingProvider,         // 조명 정보 제공
    IRenderPassContext         // 통합 컨텍스트
{
    #region IRenderDataProvider
    public IReadOnlyList<IDrawBuffer> DrawBuffers => RenderManager.AllDrawBuffers;
    #endregion

    #region IRenderSettings
    public ShadeMode ShadeMode => Shading.Mode;
    public Vector4 BackgroundColor => SceneParameter.BackGroundColor;
    public bool DisplayEdges => Shading.DisplayEdge;
    public float EdgeThickness => Shading.EdgeThickness;
    public Vector3 EdgeColor => Shading.EdgeColor;
    #endregion

    #region ILightingProvider
    public Vector3 Direction => Lighting.Direction;
    public Vector4 Color => new Vector4(Lighting.Color, 1.0f);
    public ILight Light => Lighting;
    #endregion

    #region IRenderPassContext
    public IRenderDataProvider DataProvider => this;
    public IRenderSettings Settings => this;
    public ILightingProvider LightingProvider => this;
    #endregion
}
```

### 3. ForwardPass가 IRenderPassContext만 받음

```csharp
public class ForwardPass : IRenderPass
{
    /// <summary>
    /// 렌더 패스 컨텍스트 (필요한 데이터만)
    /// </summary>
    private readonly IRenderPassContext _context;  // ? 명확한 의존성
    
    public ForwardPass(IRenderPassContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public void Execute()
    {
        // ? 필요한 것만 접근
        var drawBuffers = _context.DataProvider.DrawBuffers;
        var backgroundColor = _context.Settings.BackgroundColor;
        var light = _context.LightingProvider.Light;
        
        // ? 불필요한 것은 접근 불가
        // _context.ModelManager  // 컴파일 에러!
        // _context.SelectionManager  // 컴파일 에러!
    }
}
```

---

## ?? 비교

| 항목 | Before | After |
|------|--------|-------|
| **의존성** | IgxViewAPI 전체 | IRenderPassContext만 |
| **접근 범위** | 모든 데이터 | 필요한 데이터만 |
| **테스트** | IgxViewAPI Mock 필요 | Context만 Mock |
| **순환 의존성** | 위험 높음 | 위험 낮음 |
| **명확성** | 불명확 | 명확 |

---

## ?? 사용법

### 1. ForwardPass 생성 (IgxViewAPI에서)

```csharp
// IgxViewAPI.InitializeRenderPipeline()
public void InitializeRenderPipeline(IMyCamera camera)
{
    // ? IgxViewAPI가 IRenderPassContext를 구현하므로
    // this를 그대로 전달
    var forwardPass = new ForwardPass(this);
    MainPipeline.AddPass(forwardPass);
    
    // 초기화
    MainPipeline.Initialize(camera);
}
```

### 2. 테스트용 Mock Context

```csharp
// 테스트에서는 간단한 Mock 사용
public class MockRenderPassContext : IRenderPassContext
{
    public IRenderDataProvider DataProvider { get; set; }
    public IRenderSettings Settings { get; set; }
    public ILightingProvider LightingProvider { get; set; }
}

[Test]
public void ForwardPass_Execute_RendersDrawBuffers()
{
    // Arrange
    var mockContext = new MockRenderPassContext
    {
        DataProvider = new MockDataProvider(),
        Settings = new MockSettings(),
        LightingProvider = new MockLighting()
    };
    
    var pass = new ForwardPass(mockContext);
    
    // Act
    pass.Execute();
    
    // Assert
    // ...
}
```

---

## ?? 남은 작업

### 1. GLView.cs 수정

```csharp
// Before
_forwardPass = new ForwardPass();

// After
_forwardPass = new ForwardPass(_apis);  // _apis가 IRenderPassContext 구현
```

### 2. StandardRenderPassFactory.cs 수정

```csharp
// Before
public static IRenderPass CreateForwardPass(IMyCamera camera, IgxViewAPI api)
{
    var pass = new ForwardPass();  // ?
    pass.Initialize(camera, api);
    return pass;
}

// After
public static IRenderPass CreateForwardPass(IMyCamera camera, IRenderPassContext context)
{
    var pass = new ForwardPass(context);  // ?
    pass.Initialize(camera);
    return pass;
}
```

### 3. 다른 Pass들도 동일하게 개선

- `FBOPass`
- `PostFBOPass`
- `BackgroundPass`
- `CoordinatePass`
- 등등...

---

## ? 장점

### 1. **명확한 의존성**
```csharp
// ? 생성자를 보면 필요한 것이 명확
public ForwardPass(IRenderPassContext context)
```

### 2. **테스트 용이성**
```csharp
// ? Context만 Mock하면 됨
var mockContext = new MockRenderPassContext();
var pass = new ForwardPass(mockContext);
```

### 3. **순환 의존성 방지**
```csharp
// ? Pass는 Context만 알고, Context는 데이터만 제공
ForwardPass → IRenderPassContext → Data
```

### 4. **확장성**
```csharp
// ? 새 Pass 추가 시 필요한 Provider만 추가
public interface IPostProcessingSettings { ... }
public class PostProcessPass(IRenderPassContext context, IPostProcessingSettings settings)
```

---

## ?? 패턴 이름

이 패턴은 다음과 같은 디자인 패턴의 조합입니다:

1. **Provider Pattern** - 데이터 제공 인터페이스
2. **Dependency Injection** - 생성자를 통한 주입
3. **Interface Segregation Principle (ISP)** - 필요한 인터페이스만 의존
4. **Facade Pattern** - IRenderPassContext가 여러 Provider를 통합

---

## ?? 요약

### Before:
```csharp
ForwardPass → IgxViewAPI (모든 것)
              ├── ModelManager ?
              ├── RenderManager ?
              ├── SelectionManager ?
              ├── Shading ?
              ├── Drawing ?
              ├── Lighting ?
              └── SceneParameter ?
```

### After:
```csharp
ForwardPass → IRenderPassContext
              ├── DataProvider (DrawBuffers만) ?
              ├── Settings (BackgroundColor 등) ?
              └── LightingProvider (Light만) ?
```

**결과:** 필요한 것만 명확하게 주입! ??
