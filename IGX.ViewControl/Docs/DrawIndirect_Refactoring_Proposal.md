# DrawIndirectGeometry와 DefaultIndirectDraw 개선 제안

## ?? 현재 문제점

### 1. **책임 혼재 (Mixed Responsibilities)**

```csharp
// DrawIndirectGeometry - 데이터 관리 + 렌더링 로직
public class DrawIndirectGeometry<VTX, IDX, NST>
{
    // ? 데이터 관리 (적절)
    public void AddInstance(NST instance)
    public void UpdateCommands(...)
    public void Bind()
    
    // ? 렌더링 결정 (DefaultIndirectDraw가 해야 할 일)
    public void Draw(PrimitiveType primitiveType)
    {
        Renderer.PrimType = primitiveType;
        Renderer.Execute();  // Renderer에 위임만 함
    }
}

// DefaultIndirectDraw - 렌더링 실행
internal class DefaultIndirectDraw<VTX, IDX, NST> : IDrawBuffer
{
    // ? 렌더링 로직 (적절)
    public void Execute()
    public void ExecuteRange(...)
    public void ExecuteWithShader(...)
    
    // ? Geometry에 직접 접근 (강한 결합)
    private readonly DrawIndirectGeometry<VTX, IDX, NST> _geometryBuffer;
    
    // ? 중복된 Execute 오버로드 (혼란)
    public void Execute()
    public void Execute(int startCommandIndex = 0, int drawCount = -1, int notused = -1)
}
```

### 2. **강한 결합 (Tight Coupling)**

```csharp
// DefaultIndirectDraw가 DrawIndirectGeometry의 내부 구조를 알고 있음
ExecuteDrawCommon(
    drawAction: () => ExecuteMultiDraw(0, commandCount),
    overrideShader: null);

_geometryBuffer.Bind();  // ? 직접 바인딩 제어
_geometryBuffer.CommandCount  // ? 직접 데이터 접근
_geometryBuffer.ElementType
```

### 3. **불명확한 인터페이스**

```csharp
// IDrawBuffer 인터페이스가 너무 다양한 시그니처 허용
public interface IDrawBuffer
{
    void Execute();
    void ExecuteRange(int offset, int count);
    void ExecuteWithShader(Shader? shader);
}

// DefaultIndirectDraw에서 추가 메서드
public void Execute(int startCommandIndex = 0, int drawCount = -1, int notused = -1)  // ? 표준이 아님
public void DrawSingleInstance(int commandIndex, int instanceOffset)  // ? 인터페이스 밖
```

---

## ? 개선 방안

### 1. **명확한 역할 분리 (Clear Separation of Concerns)**

#### A. DrawIndirectGeometry - 순수 데이터 컨테이너
```csharp
/// <summary>
/// 간접 렌더링을 위한 지오메트리 데이터 컨테이너
/// 렌더링 로직은 포함하지 않음 (데이터 관리만)
/// </summary>
public class DrawIndirectGeometry<VTX, IDX, NST> : IDisposable
{
    #region 데이터 속성 (읽기 전용)
    
    public int VAO { get; }
    public int VBO { get; }
    public int IBO { get; }
    public int VertexCount { get; }
    public int IndexCount { get; }
    public int CommandCount { get; }
    public int InstanceCount { get; }
    public DrawElementsType ElementType { get; }
    public bool UseIndirectDraw { get; }
    
    #endregion
    
    #region 데이터 관리 (적절)
    
    public void AddInstance(NST instance)
    public void UpdateInstances(ReadOnlySpan<NST> instances)
    public void UpdateCommands(ReadOnlySpan<IndirectCommandData> commands)
    public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> vertices)
    
    #endregion
    
    #region GL 상태 관리 (적절)
    
    public void Bind()
    public void Unbind()
    
    #endregion
    
    #region ? 제거할 메서드
    
    // public void Draw(PrimitiveType primitiveType)  // ? 제거! Renderer가 해야 함
    // public IDrawBuffer Renderer { get; set; }  // ? 제거! 외부에서 관리
    
    #endregion
}
```

#### B. IndirectDrawStrategy - 렌더링 전략
```csharp
/// <summary>
/// 간접 드로우 렌더링 전략
/// DrawIndirectGeometry의 데이터를 사용하여 렌더링 수행
/// </summary>
public class IndirectDrawStrategy<VTX, IDX, NST> : IDrawBuffer
    where VTX : unmanaged
    where IDX : unmanaged
    where NST : unmanaged
{
    private readonly DrawIndirectGeometry<VTX, IDX, NST> _geometry;
    private readonly DrawCommandExecutor _executor;
    
    public Shader? Shader { get; set; }
    public PrimitiveType PrimType { get; set; } = PrimitiveType.Triangles;

    public IndirectDrawStrategy(DrawIndirectGeometry<VTX, IDX, NST> geometry)
    {
        _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        _executor = new DrawCommandExecutor();
    }

    #region IDrawBuffer 구현
    
    public void Execute()
    {
        if (_geometry.CommandCount <= 0) return;
        
        _executor.ExecuteIndirectDraw(
            geometry: _geometry,
            primType: PrimType,
            shader: Shader,
            startCommand: 0,
            commandCount: _geometry.CommandCount);
    }

    public void ExecuteRange(int offset, int count)
    {
        if (count <= 0) return;
        
        ValidateRange(offset, count);
        
        _executor.ExecuteIndirectDraw(
            geometry: _geometry,
            primType: PrimType,
            shader: Shader,
            startCommand: offset,
            commandCount: count);
    }

    public void ExecuteWithShader(Shader? shader)
    {
        ArgumentNullException.ThrowIfNull(shader);
        
        if (_geometry.CommandCount <= 0) return;
        
        _executor.ExecuteIndirectDraw(
            geometry: _geometry,
            primType: PrimType,
            shader: shader,  // 오버라이드
            startCommand: 0,
            commandCount: _geometry.CommandCount);
    }
    
    #endregion
    
    #region 특화 메서드
    
    /// <summary>
    /// 단일 인스턴스만 렌더링
    /// </summary>
    public void ExecuteSingleInstance(int commandIndex, int instanceOffset)
    {
        if (!_geometry.CommandExists(commandIndex))
            throw new ArgumentOutOfRangeException(nameof(commandIndex), 
                "커맨드 인덱스가 범위를 벗어났습니다.");
        
        _executor.ExecuteSingleInstance(
            geometry: _geometry,
            primType: PrimType,
            shader: Shader,
            commandIndex: commandIndex,
            instanceOffset: instanceOffset);
    }
    
    #endregion
    
    private void ValidateRange(int offset, int count)
    {
        if (offset < 0 || offset + count > _geometry.CommandCount)
            throw new ArgumentOutOfRangeException(nameof(offset), 
                $"범위 [{offset}, {offset + count})가 커맨드 개수 {_geometry.CommandCount}를 초과합니다.");
    }
}
```

#### C. DrawCommandExecutor - 실제 GL 호출 캡슐화
```csharp
/// <summary>
/// OpenGL 드로우 커맨드 실행 헬퍼
/// GL 호출을 캡슐화하여 테스트 가능하게 만듦
/// </summary>
internal class DrawCommandExecutor
{
    private static readonly int _commandSize = Marshal.SizeOf<IndirectCommandData>();
    
    /// <summary>
    /// MultiDrawElementsIndirect 실행
    /// </summary>
    public void ExecuteIndirectDraw<VTX, IDX, NST>(
        DrawIndirectGeometry<VTX, IDX, NST> geometry,
        PrimitiveType primType,
        Shader? shader,
        int startCommand,
        int commandCount)
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        if (!GLUtil.IsContextActive()) return;
        
        try
        {
            // 바인딩
            geometry.Bind();
            shader?.Use();
            
            // GL 호출
            IntPtr byteOffset = (nint)(startCommand * _commandSize);
            GL.MultiDrawElementsIndirect(
                primType,
                geometry.ElementType,
                byteOffset,
                commandCount,
                _commandSize);
        }
        finally
        {
            geometry.Unbind();
        }
    }
    
    /// <summary>
    /// DrawElementsInstancedBaseVertexBaseInstance 실행 (단일 인스턴스)
    /// </summary>
    public void ExecuteSingleInstance<VTX, IDX, NST>(
        DrawIndirectGeometry<VTX, IDX, NST> geometry,
        PrimitiveType primType,
        Shader? shader,
        int commandIndex,
        int instanceOffset)
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        if (!GLUtil.IsContextActive()) return;
        
        var cmd = geometry.GetCommand(commandIndex);
        
        try
        {
            geometry.Bind();
            shader?.Use();
            
            GL.DrawElementsInstancedBaseVertexBaseInstance(
                primType,
                (int)cmd.count,
                geometry.ElementType,
                IntPtr.Zero,
                1,  // instanceCount = 1
                (int)cmd.baseVertex,
                (int)(cmd.baseInstance + instanceOffset));
        }
        finally
        {
            geometry.Unbind();
        }
    }
}
```

---

## ?? 개선 후 사용법

### Before (현재)
```csharp
// ? Geometry와 Renderer가 혼재
var geometry = new DrawIndirectGeometry<GLVertex, uint, MeshInstanceGL>(
    vertices, indices, instances, commands);

geometry.Draw(PrimitiveType.Triangles);  // Geometry가 렌더링?

// 또는
geometry.Renderer.PrimType = PrimitiveType.Triangles;
geometry.Renderer.Execute();
```

### After (개선)
```csharp
// ? 명확한 역할 분리
var geometry = new DrawIndirectGeometry<GLVertex, uint, MeshInstanceGL>(
    vertices, indices, instances, commands);

// 렌더링 전략 선택
var renderer = new IndirectDrawStrategy<GLVertex, uint, MeshInstanceGL>(geometry);
renderer.Shader = myShader;
renderer.PrimType = PrimitiveType.Triangles;

// 렌더링 실행
renderer.Execute();  // 전체 렌더링
renderer.ExecuteRange(0, 10);  // 부분 렌더링
renderer.ExecuteSingleInstance(5, 0);  // 단일 인스턴스

// 데이터 업데이트는 Geometry를 통해
geometry.UpdateInstances(newInstances);
geometry.UpdateCommands(newCommands);
```

---

## ?? 비교표

| 항목 | Before (현재) | After (개선) |
|------|--------------|-------------|
| **역할 분리** | 혼재 (데이터 + 렌더링) | ? 명확 (Geometry = 데이터, Strategy = 렌더링) |
| **결합도** | 높음 (상호 참조) | ? 낮음 (단방향 의존성) |
| **테스트** | 어려움 (GL 호출 내장) | ? 쉬움 (Executor Mock 가능) |
| **확장성** | 낮음 (클래스 수정 필요) | ? 높음 (새 Strategy 추가) |
| **가독성** | 낮음 (책임 불명확) | ? 높음 (명확한 이름) |

---

## ?? 추가 개선 아이디어

### 1. **Strategy Factory**
```csharp
public static class DrawStrategyFactory
{
    public static IDrawBuffer CreateStrategy<VTX, IDX, NST>(
        DrawIndirectGeometry<VTX, IDX, NST> geometry)
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        if (geometry.UseIndirectDraw)
        {
            return new IndirectDrawStrategy<VTX, IDX, NST>(geometry);
        }
        else
        {
            return new InstanceDrawStrategy<VTX, IDX, NST>(geometry.BaseGeometry);
        }
    }
}
```

### 2. **Command Pattern**
```csharp
public interface IDrawCommand
{
    void Execute();
}

public class IndirectDrawCommand : IDrawCommand
{
    private readonly IndirectDrawStrategy _strategy;
    private readonly int _startCommand;
    private readonly int _commandCount;
    
    public void Execute() => _strategy.ExecuteRange(_startCommand, _commandCount);
}
```

### 3. **Builder Pattern**
```csharp
var renderer = new IndirectDrawBuilder<GLVertex, uint, MeshInstanceGL>(geometry)
    .WithShader(myShader)
    .WithPrimitiveType(PrimitiveType.Triangles)
    .WithRange(0, 10)
    .Build();

renderer.Execute();
```

---

이 개선안을 적용하면:
- ? 단일 책임 원칙 (SRP) 준수
- ? 개방-폐쇄 원칙 (OCP) 준수
- ? 의존성 역전 원칙 (DIP) 준수
- ? 테스트 용이성 향상
- ? 확장성 향상

코드가 훨씬 더 깔끔하고 유지보수하기 쉬워집니다! ??
