using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Strategies;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 인덱스 기반 지오메트리 렌더링 버퍼
    /// 인스턴싱 없이 단일 메시 렌더링 (DrawElements)
    /// 
    /// 역할 분리:
    /// - 버퍼 관리 및 데이터 업로드 (이 클래스)
    /// - 렌더링 실행 (DrawRenderer via Renderer 속성)
    /// </summary>
    public class DrawElementGeometry<VTX> : IGeometryBuffer, IIndexedGeometry
        where VTX : unmanaged
    {
        private readonly int _vao;
        private readonly VertexBuffer<VTX> _vbo;
        private readonly ElementBuffer _ibo;
        private bool _isDisposed;
        private PrimitiveType _primitiveType = PrimitiveType.Triangles;

        #region IGeometryBuffer 구현 (기존 인터페이스)
        
        public int VAO => _vao;
        public int VBO => _vbo.Handle;
        public int IBO => _ibo.Handle;
        public int CommandCount => 1;
        public DrawElementsType ElementType => _ibo.GetDrawElementsType;
        
        private IDrawBuffer _rendererBacking;
        public IDrawBuffer Renderer
        {
            get => _rendererBacking;
            set => _rendererBacking = value;
        }

        /// <summary>
        /// GPU 메모리 사용량 (바이트)
        /// </summary>
        public int GpuMemoryBytes => 
            (VertexCount * Unsafe.SizeOf<VTX>()) + 
            (IndexCount * sizeof(uint));
        
        #endregion

        #region IGeometry 구현 (새 인터페이스)
        
        /// <summary>
        /// 정점 수
        /// </summary>
        public int VertexCount => _vbo.Count;
        
        /// <summary>
        /// 프리미티브 타입
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get => _primitiveType;
            set => _primitiveType = value;
        }
        
        /// <summary>
        /// 유효성 여부
        /// </summary>
        public bool IsValid => !_isDisposed && _vao != 0 && _vbo.Handle != 0 && _ibo.Handle != 0;
        
        #endregion

        #region IIndexedGeometry 구현
        
        /// <summary>
        /// 인덱스 수
        /// </summary>
        public int IndexCount => _ibo.Count;
        
        /// <summary>
        /// 인덱스 타입
        /// </summary>
        public DrawElementsType IndexType => _ibo.GetDrawElementsType;
        
        #endregion

        /// <summary>
        /// 인덱스 기반 지오메트리 버퍼 생성
        /// </summary>
        public DrawElementGeometry(
            ReadOnlySpan<VTX> vertices,
            ReadOnlySpan<uint> indices,
            BufferUsageHint vertexHint = BufferUsageHint.DynamicDraw,
            BufferUsageHint indexHint = BufferUsageHint.DynamicDraw,
            Shader? defaultShader = null)
        {
            GLUtil.EnsureContextActive();
            
            if (vertices.IsEmpty) 
                throw new ArgumentException("정점 데이터가 비어있습니다.", nameof(vertices));
            if (indices.IsEmpty) 
                throw new ArgumentException("인덱스 데이터가 비어있습니다.", nameof(indices));
            
            _vao = GL.GenVertexArray();

            try
            {
                GL.BindVertexArray(_vao);
                _vbo = new VertexBuffer<VTX>(vertices, vertexHint);
                _vbo.Bind();
                _vbo.SetAttributes();

                _ibo = new ElementBuffer(indices, indexHint, keepCpuData: true);
                _ibo.Bind();

                GL.BindVertexArray(0);

                // 통합 렌더러 사용
                var strategy = new ElementDrawStrategy<VTX>(this);
                _rendererBacking = new DrawRenderer(strategy, defaultShader);
                
                System.Diagnostics.Debug.WriteLine($"[DrawElementGeometry] 생성: {VertexCount}개 정점, {IndexCount}개 인덱스");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DrawElementGeometry] 초기화 실패: {ex.Message}");
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// 지오메트리 렌더링 (하위 호환용)
        /// </summary>
        public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            EnsureNotDisposed();
            Renderer.PrimType = primitiveType;
            Renderer.Execute();
        }
        
        #region 바인딩 (IGeometry 구현)
        
        /// <summary>
        /// VAO 바인딩
        /// </summary>
        public void Bind()
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            GL.BindVertexArray(_vao);
        }

        /// <summary>
        /// VAO 언바인딩
        /// </summary>
        public void Unbind() => GL.BindVertexArray(0);
        
        #endregion

        #region 정점/인덱스 업데이트
        
        /// <summary>
        /// 단일 정점 업데이트
        /// </summary>
        public void UpdateSingleVertex(int vertexIndex, VTX newVertexData)
        {
            EnsureNotDisposed();
            _vbo.UpdateElement(vertexIndex, newVertexData);
        }

        /// <summary>
        /// 정점 범위 업데이트
        /// </summary>
        public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> newVertexData)
        {
            EnsureNotDisposed();
            _vbo.UpdateVertices(startIndex, newVertexData);
        }

        /// <summary>
        /// 인덱스 범위 업데이트
        /// </summary>
        public void UpdateIndices(int startIndex, ReadOnlySpan<uint> indices)
        {
            EnsureNotDisposed();
            _ibo.UpdateIndices(startIndex, indices);
        }
        
        #endregion

        #region 리소스 해제
        
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            
            if (disposing)
            {
                _vbo?.Dispose();
                _ibo?.Dispose();
                System.Diagnostics.Debug.WriteLine($"[DrawElementGeometry] 해제 완료");
            }
            
            if (GLUtil.IsContextActive())
            {
                try
                {
                    GL.DeleteVertexArray(_vao);
                }
                catch
                {
                    GLResourceManager.EnqueueForDeletion(_vao);
                }
            }
            else
            {
                GLResourceManager.EnqueueForDeletion(_vao);
            }
            
            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DrawElementGeometry() => Dispose(false);
        
        #endregion

        #region 헬퍼 메서드
        
        /// <summary>
        /// Disposed 상태 확인
        /// </summary>
        private void EnsureNotDisposed()
        {
            if (_isDisposed) 
                throw new ObjectDisposedException(GetType().Name, "DrawElementGeometry가 이미 해제되었습니다.");
        }

        public override string ToString() =>
            $"DrawElementGeometry<{typeof(VTX).Name}> VAO={_vao}, 정점={VertexCount}, 인덱스={IndexCount}";

        /// <summary>
        /// 데이터 업로드 (배열 버전 - 하위 호환용)
        /// </summary>
        [Obsolete("UpdateVertices 및 UpdateIndices 메서드를 직접 사용하세요.")]
        public void Upload(float[] vertices, int[] indices)
        {
            throw new NotImplementedException(
                "Upload 메서드는 타입 변환이 필요합니다. " +
                "UpdateVertices 및 UpdateIndices 메서드를 직접 사용하세요.");
        }
        
        #endregion
    }
}
