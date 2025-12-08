using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    public class MockDrawCommandManager : IDrawCommandManager
    {
        private readonly Dictionary<int, IDrawBuffer> _renderers = new();
        private readonly Dictionary<int, Matrix4> _transformUpdates = new();
        public MockDrawCommandManager()
        {
            _renderers.Add(1, new MockBufferRenderer());
            _renderers.Add(2, new MockBufferRenderer());
        }
        public CommandAllocationInfo FindOrRegister(PrimitiveBase primitive, IAssembly assembly)
        {
            Console.WriteLine("Mock: Finding or registering geometry. Returning default allocation info.");
            return CommandAllocationInfo.Default;
        }
        public void UpdateInstanceTransform(int baseInstanceIndex, Matrix4 transform)
        {
            _transformUpdates[baseInstanceIndex] = transform;
        }
        public void FlushInstanceTransforms()
        {
            if (_transformUpdates.Count > 0)
            {
                Console.WriteLine($"Mock: Flushing {_transformUpdates.Count} instance transforms to GPU buffer (SSBO/VBO Update).");
                _transformUpdates.Clear(); // 전송 완료 후 초기화
            }
        }
        public void PrepareVisibleDrawCommands(IReadOnlyList<int> visibleDrawCommandIndices)
        {
            Console.WriteLine($"Mock: Preparing {visibleDrawCommandIndices.Count} unique Execute FaceDrawCommands for execution.");
        }
        public void ExecuteIndirectDraws()
        {
            Console.WriteLine("Mock: Executing all prepared Execute FaceDrawCommands via MultiDrawElementsIndirect.");
        }

        public IDrawBuffer? GetRendererForBaseInstance(int baseInstance) => _renderers.GetValueOrDefault(baseInstance);
        public void UnregisterGeometry(int drawCommandIndex) => Console.WriteLine($"Mock: Unregistering geometry for command index {drawCommandIndex}. Freeing resources.");
        public AABB3 GetLocalBoundingBox(int drawCommandIndex) => AABB3.UnitBox;

        public void UpdateInstanceStatus(int baseInstanceIndex, bool isVisible, bool isSelected, bool isHighlighted, float opacity = 1)
        {
            throw new NotImplementedException();
        }
    }
}