using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Render
{
    public class MeshedModel
    {
        private readonly List<GLVertex> _vertices;
        private readonly List<uint> _triIndices;
        private readonly List<uint> _edgeIndices;
        private readonly List<MeshInstanceGL> _instInstances;
        private readonly List<BasicInstance> _matColInstances;
        private readonly List<IndirectCommandData> _faceCommands;
        private readonly List<IndirectCommandData> _edgeCommands;

        public int ModelId { get; }
        public ReadOnlySpan<GLVertex> Vertices => CollectionsMarshal.AsSpan(_vertices);
        public ReadOnlySpan<uint> TriIndices => CollectionsMarshal.AsSpan(_triIndices);
        public ReadOnlySpan<uint> EdgeIndices => CollectionsMarshal.AsSpan(_edgeIndices);
        public ReadOnlySpan<MeshInstanceGL> InstInstances => CollectionsMarshal.AsSpan(_instInstances);
        public ReadOnlySpan<BasicInstance> MatColInstances => CollectionsMarshal.AsSpan(_matColInstances);
        public ReadOnlySpan<IndirectCommandData> FaceDrawCommands => CollectionsMarshal.AsSpan(_faceCommands);
        public ReadOnlySpan<IndirectCommandData> EdgeDrawCommands => CollectionsMarshal.AsSpan(_edgeCommands);
        public AABB3 ModelAabb { get; set; } = AABB3.Empty;

        private bool _disposed;

        public MeshedModel(int modelId, int initialVertexCapacity = 1000000, int initialIndexCapacity = 3000000)
        {
            ModelId = modelId;
            _vertices = new (initialVertexCapacity);
            _triIndices = new (initialIndexCapacity);
            _edgeIndices = new (initialIndexCapacity);
            _instInstances = new (10000);
            _matColInstances = new (10000);
            _faceCommands = new (10000);
            _edgeCommands = new (10000);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVertices(IEnumerable<GLVertex> vertices) => _vertices.AddRange(vertices);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTriIndices(IEnumerable<uint> face_Indices) => _triIndices.AddRange(face_Indices);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEdgeIndices(IEnumerable<uint> edgeIndices) => _edgeIndices.AddRange(edgeIndices);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFaceDrawCommand(IndirectCommandData command) => _faceCommands.Add(command);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEdgeDrawCommand(IndirectCommandData command) => _edgeCommands.Add(command);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInstance(GeometryInstance instanceData, string grandType) => _instInstances.Add(new(instanceData, grandType));

        public GlBufferCreationArrays ToArrays()
        {
            GLVertex[] vArr = ArrayPool<GLVertex>.Shared.Rent(_vertices.Count);
            uint[] triArr = ArrayPool<uint>.Shared.Rent(_triIndices.Count);
            uint[] edgeArr = ArrayPool<uint>.Shared.Rent(_edgeIndices.Count);
            MeshInstanceGL[] instArr = ArrayPool<MeshInstanceGL>.Shared.Rent(_instInstances.Count);
            IndirectCommandData[] cmdArr = ArrayPool<IndirectCommandData>.Shared.Rent(_faceCommands.Count);
            IndirectCommandData[] edgeCmdArr = ArrayPool<IndirectCommandData>.Shared.Rent(_edgeCommands.Count);

            try
            {
                _vertices.CopyTo(vArr);
                _triIndices.CopyTo(triArr);
                _edgeIndices.CopyTo(edgeArr);
                _instInstances.CopyTo(instArr);
                _faceCommands.CopyTo(cmdArr);
                _edgeCommands.CopyTo(edgeCmdArr);

                return new GlBufferCreationArrays(
                    ModelId,
                    vArr[.._vertices.Count].ToArray(),
                    triArr[.._triIndices.Count].ToArray(),
                    edgeArr[.._edgeIndices.Count].ToArray(),
                    instArr[.._instInstances.Count].ToArray(),
                    cmdArr[.._faceCommands.Count].ToArray(),
                    edgeCmdArr[.._edgeCommands.Count].ToArray()
                );
            }
            finally
            {
                ArrayPool<GLVertex>.Shared.Return(vArr);
                ArrayPool<uint>.Shared.Return(triArr);
                ArrayPool<uint>.Shared.Return(edgeArr);
                ArrayPool<MeshInstanceGL>.Shared.Return(instArr);
                ArrayPool<IndirectCommandData>.Shared.Return(cmdArr);
                ArrayPool<IndirectCommandData>.Shared.Return(edgeCmdArr);
            }
        }

        public record GlBufferCreationArrays(
            int ModelId,
            GLVertex[] Vertices,
            uint[] TriIndices,
            uint[] EdgeIndices,
            MeshInstanceGL[] InstInstances,
            IndirectCommandData[] Commands,
            IndirectCommandData[] EdgeCommands)
        {
            public AABB3 ModelAabb { get; init; } = AABB3.Empty;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _vertices.Clear();
            _triIndices.Clear();
            _edgeIndices.Clear();
            _instInstances.Clear();
            _matColInstances.Clear();
            _faceCommands.Clear();
            _edgeCommands.Clear();
            _disposed = true;
        }
    }
}
