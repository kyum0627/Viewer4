using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Strategies;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 간접 렌더링을 위한 지오메트리 버퍼
    /// 여러 드로우 커맨드를 한 번에 처리 (MultiDrawElementsIndirect)
    /// commands가 없으면 단일 드로우로 폴백
    /// </summary>
    public class DrawIndirectGeometry<VTX, IDX, NST> : IModelGeometryBuffer<NST>, IInstancedGeometryBuffer<NST>
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        public int VAO => _instancedGeometry.VAO;
        public int VBO => _instancedGeometry.VBO;
        public int IBO => _instancedGeometry.IBO;
        public int VertexCount => _instancedGeometry.VertexCount;
        public int IndexCount => _instancedGeometry.IndexCount;
        public int CommandCount => _indirectCommandsGpuBuffer?.Count ?? 1;
        public int InstanceCount => _instancedGeometry.InstanceCount;

        private DrawInstanceGeometry<VTX, IDX, NST> _instancedGeometry;
        private readonly IndirectCommandBuffer? _indirectCommandsGpuBuffer;
        private readonly bool _useIndirectDraw;

        public IReadOnlyList<IndirectCommandData>? OriginalCommands { get; }
        public DrawElementsType ElementType => _instancedGeometry.ElementType;
        private bool _isDisposed = false;
        private IDrawBuffer _renderer;
        
        public IDrawBuffer Renderer
        {
            get => _renderer;
            set => _renderer = value;
        }
        
        public DrawInstanceGeometry<VTX, IDX, NST> BaseGeometry => _instancedGeometry;

        public int GpuMemoryBytes => 
            _instancedGeometry.GpuMemoryBytes + 
            (_indirectCommandsGpuBuffer != null ? CommandCount * Marshal.SizeOf<IndirectCommandData>() : 0);

        /// <summary>
        /// 간접 드로우를 사용하는 생성자
        /// </summary>
        public DrawIndirectGeometry(
            ReadOnlySpan<VTX> vertices,
            ReadOnlySpan<uint> indices,
            ReadOnlySpan<NST> instances,
            ReadOnlySpan<IndirectCommandData> commands,
            Shader? defaultShader = null,
            BufferUsageHint vertexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint indexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint commandHint = BufferUsageHint.StaticDraw,
            BufferUsageHint instanceHint = BufferUsageHint.DynamicDraw)
        {
            GLUtil.EnsureContextActive();
            
            if (vertices.IsEmpty) throw new ArgumentException("정점 데이터가 비어있습니다.", nameof(vertices));
            if (indices.IsEmpty) throw new ArgumentException("인덱스 데이터가 비어있습니다.", nameof(indices));
            
            // instances가 비어있으면 팩토리로 기본 인스턴스 생성
            ReadOnlySpan<NST> actualInstances = instances;
            if (instances.IsEmpty)
            {
                var defaultInstance = InstanceFactory.CreateDefault<NST>();
                actualInstances = new ReadOnlySpan<NST>(new[] { defaultInstance });
            }

            _instancedGeometry = new DrawInstanceGeometry<VTX, IDX, NST>(
                vertices, indices, actualInstances, null, vertexHint, indexHint, instanceHint);

            if (!commands.IsEmpty)
            {
                _useIndirectDraw = true;
                OriginalCommands = commands.ToArray();
                _indirectCommandsGpuBuffer = new IndirectCommandBuffer(commands.ToArray(), commandHint);
                
                // 새로운 통합 렌더러 사용 (간접 드로우)
                var strategy = new IndirectDrawStrategy<VTX, IDX, NST>(this);
                _renderer = new DrawRenderer(strategy, defaultShader);
            }
            else
            {
                _useIndirectDraw = false;
                OriginalCommands = null;
                _indirectCommandsGpuBuffer = null;
                
                // 기본 인스턴스 렌더러 사용
                _renderer = _instancedGeometry.Renderer;
                
                // DrawRenderer는 생성자에서 shader를 설정했으므로 추가 설정 불필요
            }
        }

        /// <summary>
        /// 단순 인스턴스드 렌더링을 위한 생성자 (간접 드로우 없음)
        /// </summary>
        public DrawIndirectGeometry(
            ReadOnlySpan<VTX> vertices,
            ReadOnlySpan<uint> indices,
            ReadOnlySpan<NST> instances,
            Shader? defaultShader = null,
            BufferUsageHint vertexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint indexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint instanceHint = BufferUsageHint.DynamicDraw)
            : this(vertices, indices, instances, ReadOnlySpan<IndirectCommandData>.Empty, 
                   defaultShader, vertexHint, indexHint, BufferUsageHint.StaticDraw, instanceHint)
        {
        }
        
        /// <summary>
        /// 지오메트리 렌더링
        /// </summary>
        public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            EnsureNotDisposed();
            Renderer.PrimType = primitiveType;
            Renderer.Execute();
        }
        
        /// <summary>
        /// 간접 드로우 사용 여부
        /// </summary>
        public bool UseIndirectDraw => _useIndirectDraw;
        
        /// <summary>
        /// 커맨드 인덱스 유효성 검사
        /// </summary>
        public bool CommandExists(int commandIndex)
        {
            if (!_useIndirectDraw || OriginalCommands == null) 
                return false;
            
            return commandIndex >= 0 && commandIndex < OriginalCommands.Count;
        }

        /// <summary>
        /// 커맨드 데이터 가져오기
        /// </summary>
        public IndirectCommandData GetCommand(int commandIndex)
        {
            if (!_useIndirectDraw || OriginalCommands == null)
                throw new InvalidOperationException("간접 드로우가 활성화되지 않았습니다.");
            
            if (!CommandExists(commandIndex))
                throw new ArgumentOutOfRangeException(nameof(commandIndex), "커맨드 인덱스가 범위를 벗어났습니다.");
            
            return OriginalCommands[commandIndex];
        }

        #region 인스턴스 관리
        
        /// <summary>
        /// 인스턴스 추가
        /// </summary>
        public void AddInstance(NST instance) => _instancedGeometry.AddInstance(instance);
        
        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        public void RemoveInstance(int index) => _instancedGeometry.RemoveInstance(index);
        
        /// <summary>
        /// 인스턴스 범위 업데이트
        /// </summary>
        public void UpdateInstanceRange(ReadOnlySpan<NST> instances) => 
            _instancedGeometry.UpdateInstanceRange(instances, 0);
        
        /// <summary>
        /// 모든 인스턴스 업데이트
        /// </summary>
        public void UpdateInstances(ReadOnlySpan<NST> instances) => 
            _instancedGeometry.UpdateInstances(instances);
        
        /// <summary>
        /// 인스턴스 변환 행렬 업데이트
        /// </summary>
        public void UpdateInstanceTransform(int index, Matrix4 transform)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= _instancedGeometry.InstanceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "인스턴스 인덱스가 범위를 벗어났습니다.");
            }

            NST instanceData = _instancedGeometry.GetLocalInstanceData(index);

            if (instanceData is MeshInstanceGL meshInstance)
            {
                meshInstance.Model = transform;
                _instancedGeometry.UpdateSingleInstance(index, (NST)(object)meshInstance);
            }
            else
            {
                throw new InvalidOperationException("NST 타입이 MeshInstanceGL이어야 합니다.");
            }
        }
        
        /// <summary>
        /// 인스턴스 데이터를 GPU로 플러시
        /// </summary>
        public void FlushInstanceData() => _instancedGeometry.FlushInstanceData();
        
        /// <summary>
        /// 인스턴스 버퍼 크기 조정
        /// </summary>
        public void ResizeInstanceBuffer(int newSize) => _instancedGeometry.ResizeInstanceBuffer(newSize);
        
        #endregion

        #region 정점/인덱스 업데이트
        
        /// <summary>
        /// 정점 데이터 업데이트
        /// </summary>
        public void UpdateVertices(ReadOnlySpan<VTX> vertices) => 
            _instancedGeometry.UpdateVertices(0, vertices);
        
        /// <summary>
        /// 정점 데이터 범위 업데이트
        /// </summary>
        public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> vertices) => 
            _instancedGeometry.UpdateVertices(startIndex, vertices);
        
        /// <summary>
        /// 단일 정점 업데이트
        /// </summary>
        public void UpdateSingleVertex(int vertexIndex, VTX vertex) => 
            _instancedGeometry.UpdateSingleVertex(vertexIndex, vertex);
        
        /// <summary>
        /// 인덱스 데이터 업데이트
        /// </summary>
        public void UpdateIndices(ReadOnlySpan<uint> indices) => 
            _instancedGeometry.UpdateIndices(0, indices);
        
        #endregion

        #region 커맨드 업데이트
        
        /// <summary>
        /// 드로우 커맨드 업데이트 (간접 드로우 전용)
        /// </summary>
        public void UpdateCommands(ReadOnlySpan<IndirectCommandData> commands)
        {
            if (!_useIndirectDraw || _indirectCommandsGpuBuffer == null)
                throw new InvalidOperationException("간접 드로우가 활성화되지 않았습니다.");
            
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            _indirectCommandsGpuBuffer.SyncToGpuAll(commands);
        }

        /// <summary>
        /// 커맨드 버퍼 크기 조정 (간접 드로우 전용)
        /// </summary>
        public void ResizeCommandBuffer(int newSize)
        {
            if (!_useIndirectDraw || _indirectCommandsGpuBuffer == null)
                throw new InvalidOperationException("간접 드로우가 활성화되지 않았습니다.");
            
            EnsureNotDisposed();
            
            if (newSize < 0) 
                throw new ArgumentException("버퍼 크기는 0 이상이어야 합니다.", nameof(newSize));
            
            GLUtil.EnsureContextActive();
            _indirectCommandsGpuBuffer.ReallocateGpuBuffer(newSize);
        }

        #endregion
        
        #region 바인딩
        
        /// <summary>
        /// VAO 및 버퍼 바인딩
        /// </summary>
        public void Bind()
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            _instancedGeometry.Bind();
            _indirectCommandsGpuBuffer?.Bind();
        }

        /// <summary>
        /// VAO 및 버퍼 언바인딩
        /// </summary>
        public void Unbind()
        {
            _indirectCommandsGpuBuffer?.Unbind();
            _instancedGeometry.Unbind();
        }

        #endregion
        
        #region 리소스 해제

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _instancedGeometry?.Dispose();
                _indirectCommandsGpuBuffer?.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DrawIndirectGeometry()
        {
            Dispose(false);
        }

        #endregion

        #region 헬퍼 메서드
        
        /// <summary>
        /// Disposed 상태 확인
        /// </summary>
        private void EnsureNotDisposed()
        {
            if (_isDisposed) 
                throw new ObjectDisposedException(GetType().Name, "객체가 이미 해제되었습니다.");
        }

        /// <summary>
        /// 단일 인스턴스 업데이트
        /// </summary>
        public void UpdateSingleInstance(int baseInstanceIndex, NST instanceData) => 
            _instancedGeometry.UpdateSingleInstance(baseInstanceIndex, instanceData);

        /// <summary>
        /// 로컬 인스턴스 데이터 가져오기
        /// </summary>
        public object GetLocalInstanceData(int baseInstanceIndex) =>
            _instancedGeometry.GetLocalInstanceData(baseInstanceIndex);

        /// <summary>
        /// 데이터 업로드 (미구현)
        /// </summary>
        public void Upload(float[] vertices, int[] indices)
        {
            throw new NotImplementedException("Upload는 아직 구현되지 않았습니다.");
        }
        
        #endregion
    }
}
