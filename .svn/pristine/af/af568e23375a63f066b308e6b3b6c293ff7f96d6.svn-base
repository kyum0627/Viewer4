# SelectionManager 개선 제안

## ?? 현재 문제점

### 1. **과도한 책임 (God Object)**
```csharp
public class SelectionManager
{
    // ? 선택 관리
    private readonly Dictionary<PickedGeometry, (Vector4 Color, SelectTo Mode)> _pickedList;
    
    // ? 시각화 데이터
    public List<GLVertex> InstancedVectors = [];
    public List<BasicInstance> InstancedBoxes = [];
    
    // ? 선택 옵션
    public SelectionOptions Selection = new SelectionOptions();
    
    // ? 피킹 로직
    public PickHelper Pick = new PickHelper();
    
    // ? 렌더링 업데이트
    _rendererManager.Highlight(...);
}
```

**위반하는 원칙:**
- 단일 책임 원칙 (SRP): 선택 관리, 시각화, 렌더링 업데이트를 모두 담당
- 캡슐화: public 필드로 내부 상태 노출

### 2. **직접적인 렌더링 의존성**
```csharp
private void AppendPickedList(...)
{
    // ? SelectionManager가 직접 렌더링 업데이트
    _rendererManager.Highlight(inst, itemToAdd.ModelID, itemToAdd.GeometryID, Selection.DoWhat, color);
}
```

**문제점:**
- SelectionManager가 렌더링 레이어에 강하게 결합
- 렌더링 방식 변경 시 SelectionManager도 수정 필요

### 3. **이벤트 시스템 부재**
```csharp
public void UpdatePickedList(int modelid, List<int> toUpdate)
{
    // 선택 변경 후 알림 없음
    // 외부에서 상태 변화를 감지할 방법 없음
}
```

**문제점:**
- UI나 다른 컴포넌트가 선택 변경을 감지할 수 없음
- Polling 방식으로 상태 확인 필요

### 4. **public 필드 노출**
```csharp
public List<GLVertex> InstancedVectors = [];         // ? public 필드
public List<BasicInstance> InstancedBoxes = [];      // ? public 필드
public SelectionOptions Selection = new();           // ? public 필드
public PickHelper Pick = new PickHelper();           // ? public 필드
```

**문제점:**
- 외부에서 직접 수정 가능 → 데이터 무결성 위험
- 변경 감지 불가능
- 캡슐화 위반

---

## ? 개선 방안

### 1. **책임 분리 (Separation of Concerns)**

#### A. SelectionManager (핵심 선택 로직)
```csharp
/// <summary>
/// 3D 객체 선택 상태 관리 - 순수한 선택 로직만 담당
/// </summary>
public class SelectionManager
{
    private readonly Dictionary<PickedGeometry, SelectionInfo> _selections = new();
    
    // 이벤트를 통한 알림
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler? SelectionCleared;
    
    // 읽기 전용 컬렉션 노출
    public IReadOnlyDictionary<PickedGeometry, SelectionInfo> Selections => _selections;
    
    // 선택 관리 메서드
    public void Select(PickedGeometry item, SelectionInfo info)
    {
        if (_selections.TryAdd(item, info))
        {
            OnSelectionChanged(new SelectionChangedEventArgs(item, true));
        }
    }
    
    public bool Deselect(PickedGeometry item)
    {
        if (_selections.Remove(item))
        {
            OnSelectionChanged(new SelectionChangedEventArgs(item, false));
            return true;
        }
        return false;
    }
    
    public void ClearAll()
    {
        _selections.Clear();
        OnSelectionCleared();
    }
    
    protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }
    
    protected virtual void OnSelectionCleared()
    {
        SelectionCleared?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// 선택 정보 (색상, 모드)
/// </summary>
public record SelectionInfo(Vector4 Color, SelectTo Mode);

/// <summary>
/// 선택 변경 이벤트 인자
/// </summary>
public class SelectionChangedEventArgs : EventArgs
{
    public PickedGeometry Geometry { get; }
    public bool IsSelected { get; }
    
    public SelectionChangedEventArgs(PickedGeometry geometry, bool isSelected)
    {
        Geometry = geometry;
        IsSelected = isSelected;
    }
}
```

#### B. SelectionVisualizer (시각화 전담)
```csharp
/// <summary>
/// 선택된 객체의 시각화 데이터 생성 (BoundingBox, Normal Vectors)
/// </summary>
public class SelectionVisualizer
{
    private readonly Model3dDataManager _modelManager;
    private readonly List<BasicInstance> _boxInstances = new();
    private readonly List<GLVertex> _vectorInstances = new();
    
    public IReadOnlyList<BasicInstance> BoxInstances => _boxInstances;
    public IReadOnlyList<GLVertex> VectorInstances => _vectorInstances;
    
    public SelectionVisualizer(Model3dDataManager modelManager)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
    }
    
    /// <summary>
    /// 선택 목록으로부터 시각화 데이터 생성
    /// </summary>
    public void UpdateVisualization(
        IReadOnlyDictionary<PickedGeometry, SelectionInfo> selections, 
        bool useOOBB = true)
    {
        _boxInstances.Clear();
        _vectorInstances.Clear();
        
        foreach (var (pickedGeo, selectionInfo) in selections)
        {
            if (!IsValidGeometry(pickedGeo))
                continue;
            
            // BoundingBox 생성
            var box = CreateBoundingBox(pickedGeo, useOOBB, selectionInfo.Color);
            if (box.HasValue)
                _boxInstances.Add(box.Value);
            
            // Normal Vector 생성
            var vector = CreateNormalVector(pickedGeo);
            if (vector.HasValue)
                _vectorInstances.Add(vector.Value);
        }
    }
    
    private bool IsValidGeometry(PickedGeometry geo)
    {
        return geo.ModelID >= 0 && 
               geo.ModelID < _modelManager.Models.Count &&
               geo.GeometryID >= 0 &&
               geo.GeometryID < _modelManager.Models[geo.ModelID].Geometries.Count;
    }
    
    private BasicInstance? CreateBoundingBox(PickedGeometry geo, bool useOOBB, Vector4 color)
    {
        var geometry = _modelManager.Models[geo.ModelID].Geometries[geo.GeometryID];
        Matrix4 boxMatrix = useOOBB ? geometry.Oobb.ToMatrix() : geometry.Aabb.ToMatrix();
        
        return new BasicInstance
        {
            Model = boxMatrix,
            Color = color
        };
    }
    
    private GLVertex? CreateNormalVector(PickedGeometry geo)
    {
        var geometry = _modelManager.Models[geo.ModelID].Geometries[geo.GeometryID];
        
        Vector3 position = geometry.Oobb.center;
        Vector3 normal = geometry.Oobb.axisZ;
        
        if (geometry.GeometryType == ParaPrimType.FacetVolume && 
            geometry is FacetVolume vol)
        {
            position = vol.Volume.Surfaces[0].Median.Position;
            normal = vol.Volume.Surfaces[0].Median.Normal;
        }
        
        return new GLVertex(position, normal);
    }
}
```

#### C. SelectionRenderer (렌더링 전담)
```csharp
/// <summary>
/// 선택 상태를 렌더링 시스템에 반영
/// </summary>
public class SelectionRenderer
{
    private readonly Model3dDataManager _modelManager;
    private readonly Model3dBufferManager _bufferManager;
    
    public SelectionRenderer(
        Model3dDataManager modelManager, 
        Model3dBufferManager bufferManager)
    {
        _modelManager = modelManager;
        _bufferManager = bufferManager;
    }
    
    /// <summary>
    /// 선택 하이라이트 적용
    /// </summary>
    public void ApplyHighlight(PickedGeometry geo, SelectionInfo info)
    {
        var instance = ModelQuery.GetMeshInstance(
            _modelManager.Models[geo.ModelID], 
            geo.GeometryID);
        
        _bufferManager.Highlight(
            instance, 
            geo.ModelID, 
            geo.GeometryID, 
            info.Mode, 
            info.Color);
    }
    
    /// <summary>
    /// 선택 하이라이트 제거
    /// </summary>
    public void RevertHighlight(PickedGeometry geo)
    {
        var instance = ModelQuery.GetMeshInstance(
            _modelManager.Models[geo.ModelID], 
            geo.GeometryID);
        
        if (_bufferManager._geometryBuffers[geo.ModelID] is IInstancedGeometryBuffer<MeshInstanceGL> buffer)
        {
            buffer.UpdateSingleInstance(geo.GeometryID, instance);
        }
    }
    
    /// <summary>
    /// 모든 하이라이트 제거
    /// </summary>
    public void RevertAll(IEnumerable<PickedGeometry> geometries)
    {
        foreach (var geo in geometries)
        {
            RevertHighlight(geo);
        }
    }
}
```

### 2. **통합 Facade 클래스**

```csharp
/// <summary>
/// 선택 시스템의 Facade - 외부 인터페이스 제공
/// </summary>
public class SelectionSystem
{
    private readonly SelectionManager _manager;
    private readonly SelectionVisualizer _visualizer;
    private readonly SelectionRenderer _renderer;
    
    // 외부에서 접근할 속성
    public SelectionOptions Options { get; } = new();
    public PickHelper PickHelper { get; } = new();
    
    // 읽기 전용 선택 목록
    public IReadOnlyDictionary<PickedGeometry, SelectionInfo> Selections => _manager.Selections;
    
    // 시각화 데이터 (Pass에서 사용)
    public IReadOnlyList<BasicInstance> SelectionBoxes => _visualizer.BoxInstances;
    public IReadOnlyList<GLVertex> SelectionVectors => _visualizer.VectorInstances;
    
    // 이벤트
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => _manager.SelectionChanged += value;
        remove => _manager.SelectionChanged -= value;
    }
    
    public SelectionSystem(
        Model3dDataManager modelManager,
        Model3dBufferManager bufferManager)
    {
        _manager = new SelectionManager();
        _visualizer = new SelectionVisualizer(modelManager);
        _renderer = new SelectionRenderer(modelManager, bufferManager);
        
        // 선택 변경 시 자동으로 렌더링 및 시각화 업데이트
        _manager.SelectionChanged += OnSelectionChanged;
        _manager.SelectionCleared += OnSelectionCleared;
    }
    
    /// <summary>
    /// 선택 업데이트 (기존 UpdatePickedList 대체)
    /// </summary>
    public void UpdateSelection(int modelId, List<int> geometryIds)
    {
        bool exists = PickHelper.ContainsPicked(modelId, geometryIds, _manager.Selections);
        
        foreach (var geoId in geometryIds)
        {
            var geo = new PickedGeometry(modelId, geoId);
            
            if (exists)
            {
                _manager.Deselect(geo);
            }
            else
            {
                var color = Options.Color;
                if (Options.DoWhat == SelectTo.Transparent)
                    color.W = 0;
                
                var info = new SelectionInfo(color, Options.DoWhat);
                _manager.Select(geo, info);
            }
        }
    }
    
    /// <summary>
    /// 모든 선택 해제
    /// </summary>
    public void ClearAll()
    {
        _renderer.RevertAll(_manager.Selections.Keys);
        _manager.ClearAll();
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.IsSelected)
        {
            // 선택 시 하이라이트 적용
            var info = _manager.Selections[e.Geometry];
            _renderer.ApplyHighlight(e.Geometry, info);
        }
        else
        {
            // 선택 해제 시 하이라이트 제거
            _renderer.RevertHighlight(e.Geometry);
        }
        
        // 시각화 데이터 업데이트
        _visualizer.UpdateVisualization(_manager.Selections, Options.UseOOBB);
    }
    
    private void OnSelectionCleared(object? sender, EventArgs e)
    {
        _visualizer.UpdateVisualization(_manager.Selections, Options.UseOOBB);
    }
}
```

### 3. **IgxViewAPI 업데이트**

```csharp
public class IgxViewAPI : IRenderDataProvider, IRenderSettings, ILightingProvider
{
    // Before
    // public SelectionManager SelectionManager { get; }
    
    // After
    public SelectionSystem Selection { get; }
    
    public IgxViewAPI(...)
    {
        // ...
        Selection = new SelectionSystem(modelManager, renderManager);
    }
    
    // Auxiliary Pass 초기화
    public void InitializeRenderPipeline(IMyCamera camera)
    {
        var auxiliaryPasses = StandardRenderPassFactory.CreateStandardAuxiliaryPasses(
            camera,
            () => Selection.SelectionBoxes.ToList(),    // IReadOnlyList → List
            () => Selection.SelectionVectors.ToList(),  // IReadOnlyList → List
            SceneParameter,
            SceneParameter.BoxColor,
            SceneParameter.VectorColor);
        
        // ...
    }
}
```

---

## ?? 비교표

| 항목 | Before | After |
|------|--------|-------|
| **책임** | 선택+시각화+렌더링 혼재 | 각각 분리된 클래스 |
| **결합도** | 높음 (렌더링 직접 의존) | 낮음 (이벤트 기반) |
| **캡슐화** | public 필드 노출 | 속성 + 읽기전용 |
| **이벤트** | 없음 | SelectionChanged 이벤트 |
| **테스트** | 어려움 | 각 클래스 독립 테스트 |
| **유지보수** | 하나의 큰 클래스 | 명확한 책임 분리 |

---

## ?? 개선 효과

### 1. **단일 책임 원칙 (SRP) 준수**
- `SelectionManager`: 선택 상태만 관리
- `SelectionVisualizer`: 시각화 데이터 생성
- `SelectionRenderer`: 렌더링 업데이트

### 2. **이벤트 기반 아키텍처**
```csharp
// UI에서 선택 변경 감지
selection.SelectionChanged += (s, e) => 
{
    UpdatePropertiesPanel(e.Geometry);
    LogSelection(e.IsSelected ? "Selected" : "Deselected", e.Geometry);
};
```

### 3. **캡슐화 강화**
```csharp
// Before: 외부에서 직접 수정 가능
selectionManager.InstancedBoxes.Clear(); // ?

// After: 읽기 전용
IReadOnlyList<BasicInstance> boxes = selection.SelectionBoxes; // ?
```

### 4. **테스트 용이성**
```csharp
// SelectionManager 단위 테스트
[Test]
public void Select_NewItem_FiresEvent()
{
    var manager = new SelectionManager();
    bool eventFired = false;
    manager.SelectionChanged += (s, e) => eventFired = true;
    
    var geo = new PickedGeometry(0, 0);
    var info = new SelectionInfo(Vector4.One, SelectTo.Highlight);
    
    manager.Select(geo, info);
    
    Assert.IsTrue(eventFired);
    Assert.Contains(geo, manager.Selections.Keys);
}
```

### 5. **확장성**
```csharp
// 새로운 시각화 전략 추가
public class WireframeSelectionVisualizer : SelectionVisualizer
{
    protected override BasicInstance? CreateBoundingBox(...)
    {
        // Wireframe 전용 시각화
    }
}
```

---

## ?? 마이그레이션 단계

### Phase 1: 새 클래스 추가
1. `SelectionInfo` record 추가
2. `SelectionChangedEventArgs` 추가
3. `SelectionManager` (새 버전) 추가
4. `SelectionVisualizer` 추가
5. `SelectionRenderer` 추가
6. `SelectionSystem` Facade 추가

### Phase 2: 기존 코드와 병행
```csharp
public class IgxViewAPI
{
    [Obsolete("Use Selection instead")]
    public SelectionManager SelectionManager { get; }  // 기존
    
    public SelectionSystem Selection { get; }           // 신규
}
```

### Phase 3: 마이그레이션
- 모든 `SelectionManager` 참조를 `Selection`으로 변경
- 테스트 및 검증

### Phase 4: 정리
- `SelectionManager` (구버전) 제거
- Obsolete 속성 제거

---

## ?? 추가 개선 아이디어

### 1. **Command Pattern 적용**
```csharp
public interface ISelectionCommand
{
    void Execute();
    void Undo();
}

public class SelectCommand : ISelectionCommand
{
    private readonly SelectionManager _manager;
    private readonly PickedGeometry _geometry;
    private readonly SelectionInfo _info;
    
    public void Execute() => _manager.Select(_geometry, _info);
    public void Undo() => _manager.Deselect(_geometry);
}

// Undo/Redo 구현
public class SelectionHistory
{
    private readonly Stack<ISelectionCommand> _undoStack = new();
    private readonly Stack<ISelectionCommand> _redoStack = new();
    
    public void Execute(ISelectionCommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }
    
    public void Undo()
    {
        if (_undoStack.TryPop(out var command))
        {
            command.Undo();
            _redoStack.Push(command);
        }
    }
    
    public void Redo()
    {
        if (_redoStack.TryPop(out var command))
        {
            command.Execute();
            _undoStack.Push(command);
        }
    }
}
```

### 2. **Query Object Pattern**
```csharp
public class SelectionQuery
{
    private readonly SelectionManager _manager;
    
    public IEnumerable<PickedGeometry> GetByModel(int modelId) =>
        _manager.Selections.Keys.Where(g => g.ModelID == modelId);
    
    public IEnumerable<PickedGeometry> GetByColor(Vector4 color) =>
        _manager.Selections.Where(kvp => kvp.Value.Color == color)
                          .Select(kvp => kvp.Key);
    
    public int Count => _manager.Selections.Count;
    public bool IsSelected(PickedGeometry geo) => _manager.Selections.ContainsKey(geo);
}
```

### 3. **Strategy Pattern for Visualization**
```csharp
public interface ISelectionVisualizationStrategy
{
    List<BasicInstance> CreateBoxes(IReadOnlyDictionary<PickedGeometry, SelectionInfo> selections);
    List<GLVertex> CreateVectors(IReadOnlyDictionary<PickedGeometry, SelectionInfo> selections);
}

public class OOBBVisualizationStrategy : ISelectionVisualizationStrategy { }
public class AABBVisualizationStrategy : ISelectionVisualizationStrategy { }
public class WireframeVisualizationStrategy : ISelectionVisualizationStrategy { }
```

---

이 개선안을 적용하면 코드가 훨씬 더 모듈화되고, 테스트 가능하며, 확장 가능한 구조가 됩니다! ??
