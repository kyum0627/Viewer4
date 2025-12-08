using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Strategies;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// SSBO 기반 인스턴스드 렌더링을 위한 지오메트리 버퍼
    /// 대용량 인스턴스 데이터를 Shader Storage Buffer Object로 관리
    /// instances가 비어있으면 기본 인스턴스 1개 자동 생성
    /// </summary>
    public class DrawSSBObaseGeometry<VTX, NST> : IGeometryBuffer
        where VTX : unmanaged
        where NST : unmanaged
    {
        public int VAO => _indexedBuffer.VAO;
        public int VBO => _indexedBuffer.VBO;
        public int IBO => _indexedBuffer.IBO;
        public int VertexCount => _indexedBuffer.VertexCount;
        public int IndexCount => _indexedBuffer.IndexCount;
        public int InstanceCount => _instanceSSBO.Count;
        public int CommandCount => 1;
        public DrawElementsType ElementType => _indexedBuffer.ElementType;
        
        public IDrawBuffer Renderer 
        { 
            get => _renderer; 
            set => _renderer = value; 
        }

        public int GpuMemoryBytes => 
            (_indexedBuffer.VertexCount * Unsafe.SizeOf<VTX>()) +
            (_indexedBuffer.IndexCount * sizeof(uint)) +
            (_instanceSSBO.Count * Unsafe.SizeOf<NST>());

        private readonly DrawElementGeometry<VTX> _indexedBuffer;
        public readonly ShaderStorageBuffer<NST> _instanceSSBO;
        public readonly int _instanceSSBOBindingIndex;
        private readonly object _lock = new();
        private IDrawBuffer _renderer;
        private bool _isDisposed = false;

        /// <summary>
        /// SSBO 기반 지오메트리 버퍼 생성
        /// instances가 비어있으면 팩토리로 기본 인스턴스 생성
        /// </summary>
        public DrawSSBObaseGeometry(
            ReadOnlySpan<VTX> vertices,
            ReadOnlySpan<uint> indices,
            ReadOnlySpan<NST> instances,
            int instanceSSBOBindingIndex,
            Shader? defaultShader = null,
            BufferUsageHint vertexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint indexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint instanceHint = BufferUsageHint.DynamicDraw)
        {
            GLUtil.EnsureContextActive();
            
            if (vertices.IsEmpty) 
                throw new ArgumentException("정점 데이터가 비어있습니다.", nameof(vertices));
            if (indices.IsEmpty) 
                throw new ArgumentException("인덱스 데이터가 비어있습니다.", nameof(indices));
            if (!IsContextActive()) 
                throw new InvalidOperationException("활성 GL 컨텍스트가 필요합니다.");

            _instanceSSBOBindingIndex = instanceSSBOBindingIndex;
            _indexedBuffer = new DrawElementGeometry<VTX>(vertices, indices, vertexHint, indexHint);

            // instances가 비어있으면 팩토리로 기본 인스턴스 생성
            ReadOnlySpan<NST> actualInstances = instances;
            if (instances.IsEmpty)
            {
                var defaultInstance = InstanceFactory.CreateDefault<NST>();
                actualInstances = new ReadOnlySpan<NST>(new[] { defaultInstance });
                System.Diagnostics.Debug.WriteLine($"[DrawSSBObaseGeometry] 인스턴스 없음 - 기본 인스턴스 1개 생성");
            }

            _indexedBuffer.Bind();
            _instanceSSBO = new ShaderStorageBuffer<NST>(actualInstances, instanceHint, keepCpuData: true);
            _instanceSSBO.BindBase(_instanceSSBOBindingIndex);
            
            // 새로운 통합 렌더러 사용
            var strategy = new SSBODrawStrategy<VTX, NST>(this);
            _renderer = new DrawRenderer(strategy, defaultShader);
            
            _indexedBuffer.Unbind();
            
            System.Diagnostics.Debug.WriteLine($"[DrawSSBObaseGeometry] 생성 완료: {VertexCount}개 정점, {IndexCount}개 인덱스, {InstanceCount}개 인스턴스");
        }

        /// <summary>
        /// 지오메트리 렌더링
        /// </summary>
        public void Draw(PrimitiveType primitiveType)
        {
            EnsureNotDisposed();
            _renderer.PrimType = primitiveType;
            _renderer.Execute();
        }
    
        #region 인스턴스 관리
        
        /// <summary>
        /// 인스턴스 추가
        /// </summary>
        public void AddInstance(NST instance)
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();

            lock (_lock)
            {
                _instanceSSBO.AppendToBuffer(MemoryMarshal.CreateReadOnlySpan(ref instance, 1));
            }
        }

        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        public void RemoveInstance(int index)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= _instanceSSBO.Count)
                throw new ArgumentOutOfRangeException(nameof(index), 
                    $"인덱스 {index}가 범위 [0, {_instanceSSBO.Count})를 벗어났습니다.");
            
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                _instanceSSBO.RemoveCpuAt(index);
            }
        }

        /// <summary>
        /// 인스턴스 범위 업데이트
        /// </summary>
        public void UpdateInstanceRange(ReadOnlySpan<NST> data, int offset = 0)
        {
            EnsureNotDisposed();
            if (data.IsEmpty) return;
            
            if (offset < 0 || offset + data.Length > _instanceSSBO.Count)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + data.Length})가 인스턴스 개수 {_instanceSSBO.Count}를 초과합니다.");
            
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                _instanceSSBO.UpdateInstances(offset, data);
            }
        }

        /// <summary>
        /// 단일 인스턴스 업데이트
        /// </summary>
        public void UpdateSingleInstance(int index, NST instance)
        {
            EnsureNotDisposed();
            UpdateInstanceRange(new NST[] { instance }.AsSpan(), index);
        }
        
        #endregion

        #region 정점/인덱스 업데이트
        
        /// <summary>
        /// 정점 범위 업데이트
        /// </summary>
        public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> newVertexData)
        {
            EnsureNotDisposed();
            if (newVertexData.IsEmpty) return;
            
            GLUtil.EnsureContextActive();
            _indexedBuffer.Bind();
            
            try
            {
                _indexedBuffer.UpdateVertices(startIndex, newVertexData);
            }
            finally
            {
                _indexedBuffer.Unbind();
            }
        }

        /// <summary>
        /// 인덱스 범위 업데이트
        /// </summary>
        public void UpdateIndices(int startIndex, ReadOnlySpan<uint> newIndices)
        {
            EnsureNotDisposed();
            if (newIndices.IsEmpty) return;
            
            GLUtil.EnsureContextActive();
            _indexedBuffer.Bind();
            
            try
            {
                _indexedBuffer.UpdateIndices(startIndex, newIndices);
            }
            finally
            {
                _indexedBuffer.Unbind();
            }
        }

        /// <summary>
        /// 단일 정점 업데이트
        /// </summary>
        public void UpdateSingleVertex(int vertexIndex, VTX newVertexData)
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();

            _indexedBuffer.Bind();
            
            try
            {
                _indexedBuffer.UpdateSingleVertex(vertexIndex, newVertexData);
            }
            finally
            {
                _indexedBuffer.Unbind();
            }
        }
        
        #endregion

        #region 바인딩
        
        /// <summary>
        /// VAO 및 SSBO 바인딩
        /// </summary>
        public void Bind()
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            _indexedBuffer.Bind();
            _instanceSSBO.BindBase(_instanceSSBOBindingIndex);
        }

        /// <summary>
        /// VAO 및 SSBO 언바인딩
        /// </summary>
        public void Unbind()
        {
            _instanceSSBO.UnbindBase(_instanceSSBOBindingIndex);
            _indexedBuffer.Unbind();
        }
        
        #endregion

        #region 리소스 해제
        
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            
            lock (_lock)
            {
                if (disposing)
                {
                    _indexedBuffer?.Dispose();
                    _instanceSSBO?.Dispose();
                    System.Diagnostics.Debug.WriteLine($"[DrawSSBObaseGeometry] 해제 완료");
                }
                
                if (GLUtil.IsContextActive())
                {
                    GLResourceManager.EnqueueForDeletion(VAO);
                }
                
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DrawSSBObaseGeometry() => Dispose(false);
        
        #endregion

        #region 헬퍼 메서드
        
        /// <summary>
        /// GL 컨텍스트 활성 상태 확인
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsContextActive()
        {
            try { GL.GetError(); return true; } 
            catch { return false; }
        }

        /// <summary>
        /// Disposed 상태 확인
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (_isDisposed) 
                throw new ObjectDisposedException(GetType().Name, "DrawSSBObaseGeometry가 이미 해제되었습니다.");
        }

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
