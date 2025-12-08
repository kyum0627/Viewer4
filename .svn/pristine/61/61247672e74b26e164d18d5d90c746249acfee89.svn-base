using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    public struct CommandAllocationInfo
    {
        public int DrawCommandIndex;
        public int BaseInstance;
        public int BufferIndex;
        public static CommandAllocationInfo Default => new CommandAllocationInfo
        {
            DrawCommandIndex = 0,
            BaseInstance = 0,
            BufferIndex = 0
        };
    }

    public class Model3dBufferManager : IDrawCommandManager
    {
        private readonly Model3dDataManager _modelDataManager;
        public List<IGeometryBuffer> _geometryBuffers = new();

        public IReadOnlyList<IDrawBuffer> AllDrawBuffers => GetDrawBuffersReadOnly();

        private readonly Dictionary<int, CommandAllocationInfo> _geometryAllocationMap = new();
        private readonly List<int> _currentVisibleDrawCommands = [];

        private readonly object _lockObject = new();
        public Model3dBufferManager(Model3dDataManager modelDataManager)
        {
            _modelDataManager = modelDataManager ?? throw new ArgumentNullException(nameof(modelDataManager), "ModelManager는 null일 수 없음.");
        }
        public AABB3 GetLocalBoundingBox(int drawCommandIndex)
        {
            return _modelDataManager.Models[drawCommandIndex].ModelAABB;
        }
        private IReadOnlyList<IDrawBuffer> GetDrawBuffersReadOnly()
        {
            lock (_lockObject)
            {
                return _geometryBuffers.Select(g => g.Renderer).ToList().AsReadOnly();

            }
        }
        public void ClearRenderResources()
        {
            lock (_lockObject)
            {
                _geometryBuffers.Clear();
                _geometryAllocationMap.Clear();
            }
        }
        public CommandAllocationInfo FindOrRegister(PrimitiveBase primitive, IAssembly assembly)
        {
            lock (_lockObject)
            {
                if (_geometryAllocationMap.TryGetValue(primitive.GeometryID, out var allocation))
                {
                    return allocation;
                }
                int modelID = assembly.ModelID;

                if (modelID >= 0 && modelID < _geometryBuffers.Count)
                {
                    int baseInstanceIndex = primitive.GeometryID;
                    int commandIndex = baseInstanceIndex;

                    allocation = new CommandAllocationInfo
                    {
                        DrawCommandIndex = commandIndex,
                        BaseInstance = baseInstanceIndex,
                        BufferIndex = modelID
                    };

                    _geometryAllocationMap[primitive.GeometryID] = allocation;
                    Console.WriteLine($"Manager: Registered GeometryID {primitive.GeometryID} -> Buffer {modelID}, Command {commandIndex}");
                    return allocation;
                }
                throw new KeyNotFoundException($"ModelID {modelID} is out of range for GlGeometryBuffers or IAssembly lacks ModelID.");
            }
        }
        public IDrawBuffer? GetRendererForBaseInstance(int baseInstanceIndex)
        {
            lock (_lockObject)
            {
                var allocation = _geometryAllocationMap.Values
                    .FirstOrDefault(a => a.BaseInstance == baseInstanceIndex);
                if (allocation.BufferIndex >= 0 && allocation.BufferIndex < _geometryBuffers.Count)
                {
                    return _geometryBuffers[allocation.BufferIndex].Renderer;
                }
                return null;
            }
        }
        public void FlushInstanceTransforms()
        {
            lock (_lockObject)
            {
                Console.WriteLine("Manager: Starting global Flush of instance buffers to GPU.");
                foreach (var geometry in _geometryBuffers)
                {
                    if (geometry is IInstancedGeometryBuffer<MeshInstanceGL> instancedBuffer)
                    {
                        instancedBuffer.FlushInstanceData();
                    }
                }
            }
        }
        public void PrepareVisibleDrawCommands(IReadOnlyList<int> visibleDrawCommandIndices)
        {
            lock (_lockObject)
            {
                Console.WriteLine($"Manager: Preparing {visibleDrawCommandIndices.Count} Execute FaceDrawCommands for execution (Setting Instance CommandCount > 0).");
                _currentVisibleDrawCommands.Clear();
                _currentVisibleDrawCommands.AddRange(visibleDrawCommandIndices);
            }
        }

        public void UpdateInstanceTransform(int baseInstanceIndex, Matrix4 transform)
        {
            lock (_lockObject)
            {
                var allocation = _geometryAllocationMap.Values.FirstOrDefault(a => a.BaseInstance == baseInstanceIndex);
                if (allocation.BufferIndex >= 0 && allocation.BufferIndex < _geometryBuffers.Count)
                {
                    if (_geometryBuffers is IInstancedGeometryBuffer<MeshInstanceGL> inst)
                    {
                        inst.UpdateInstanceTransform(baseInstanceIndex, transform);
                    }
                }
                else
                {
                }
            }
        }
        public void UpdateInstanceStatus(
            int baseInstanceIndex,
            bool isVisible,
            bool isSelected,
            bool isHighlighted,
            float opacity = 1.0f)
        {
            lock (_lockObject)
            {
                var allocation = _geometryAllocationMap.Values.FirstOrDefault(a => a.BaseInstance == baseInstanceIndex);

                if (allocation.BufferIndex >= 0 && allocation.BufferIndex < _geometryBuffers.Count)
                {
                    var targetBuffer = _geometryBuffers[allocation.BufferIndex];
                    if (targetBuffer is IInstancedGeometryBuffer<MeshInstanceGL> inst)
                    {
                        MeshInstanceGL instanceData = (MeshInstanceGL)inst.GetLocalInstanceData(baseInstanceIndex);
                        instanceData.SelectionMode =
                            isHighlighted ? (int)SelectTo.ColorChange :
                            isSelected ? (int)SelectTo.Select :
                            (int)SelectTo.None;
                        inst.UpdateSingleInstance(baseInstanceIndex, instanceData);
                    }
                }
                else
                {
                    // Debug.WriteLine("Warning: Failed to find buffer for BaseInstance in UpdateInstanceStatus.");
                }
            }
        }
        public void ExecuteIndirectDraws()
        {
            lock (_lockObject)
            {
                if (_currentVisibleDrawCommands.Count == 0)
                {
                    Console.WriteLine("Manager: No visible commands to draw.");
                    return;
                }

                Console.WriteLine($"Manager: Executing MultiDrawElementsIndirect for {_geometryBuffers.Count} Geometry groups.");
            }
        }
        public void UnregisterGeometry(int drawCommandIndex)
        {
            lock (_lockObject)
            {
                var geometryID = _geometryAllocationMap.FirstOrDefault(x => x.Value.DrawCommandIndex == drawCommandIndex).Key;

                if (geometryID != 0) // Key가 존재하면
                {
                    _geometryAllocationMap.Remove(geometryID);
                    Console.WriteLine($"Manager: Unregistered Geometry associated with DrawCommandIndex {drawCommandIndex}.");

                }
            }
        }
        public void Highlight(MeshInstanceGL inst, int modelID, int geometryID, SelectTo selectTo, Vector4? color = null, int? layer = null)
        {
            lock (_lockObject)
            {
                if (modelID < 0 || modelID >= _geometryBuffers.Count) return;
                if (color.HasValue) { inst.Color = color.Value; }
                if (layer.HasValue) { inst.Layer = layer.Value; }
                inst.SelectionMode = (int)selectTo;
                if (_geometryBuffers[modelID] is IInstancedGeometryBuffer<MeshInstanceGL> meshBuffer)
                {
                    meshBuffer.UpdateSingleInstance(geometryID, inst);
                }
            }
        }
        public void MakeModel3dBuffer(List<MeshedModel> model3dBufferData)
        {
            lock (_lockObject)
            {
                ClearRenderResources();
                foreach (var bufferData in model3dBufferData)
                {
                    IGeometryBuffer glbuffer = new DrawIndirectGeometry<GLVertex, uint, MeshInstanceGL>(
                        bufferData.Vertices.ToArray(),
                        bufferData.TriIndices.ToArray(),
                        bufferData.InstInstances.ToArray(),
                        bufferData.FaceDrawCommands.ToArray());
                    _geometryBuffers.Add(glbuffer);
                }
            }
        }
    }
}