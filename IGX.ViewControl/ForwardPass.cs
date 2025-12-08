using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl
{
    /// <summary>
    /// Forward Rendering Pass
    /// 전통적인 Forward 렌더링 방식으로 불투명/투명 객체 렌더링
    /// 
    /// 의존성:
    /// - IRenderPassContext: 렌더링 데이터, 설정, 조명 제공
    /// - IMyCamera: 카메라 정보
    /// - Shader: Forward 쉐이더
    /// </summary>
    public class ForwardPass : IRenderPass
    {
        #region Properties
        
        public string Name => "ForwardPass";
        public int Order { get; set; } = 100;
        public bool Enabled { get; set; } = true;
        
        public Shader? PassShader { get; set; }
        
        public bool ClearColor { get; set; } = true;
        public bool ClearDepth { get; set; } = true;
        public Vector4 ClearColorValue { get; set; }
        
        #endregion

        #region Fields
        
        /// <summary>
        /// 렌더 패스 컨텍스트 (데이터, 설정, 조명 제공)
        /// </summary>
        private readonly IRenderPassContext _context;
        
        /// <summary>
        /// 카메라
        /// </summary>
        private IMyCamera? _camera;
        
        /// <summary>
        /// View 행렬 캐시
        /// </summary>
        private Matrix4 _viewMatrix;
        
        /// <summary>
        /// Projection 행렬 캐시
        /// </summary>
        private Matrix4 _projectionMatrix;
        
        /// <summary>
        /// Dispose 상태
        /// </summary>
        private bool _isDisposed;
        
        /// <summary>
        /// Dispose 락
        /// </summary>
        private readonly object _disposeLock = new();
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// ForwardPass 생성
        /// </summary>
        /// <param name="context">렌더 패스 컨텍스트</param>
        public ForwardPass(IRenderPassContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), 
                "렌더 패스 컨텍스트가 null입니다.");
            
            Debug.WriteLine("[ForwardPass] 생성됨");
        }
        
        #endregion

        #region IRenderPass Implementation
        
        /// <summary>
        /// 파이프라인 추가 시 호출
        /// </summary>
        public void OnAddedToPipeline(IRenderPipeline pipeline)
        {
            pipeline?.OnAddedToPipeline(this);
            Debug.WriteLine("[ForwardPass] 파이프라인에 추가됨");
        }

        /// <summary>
        /// 파이프라인 제거 시 호출
        /// </summary>
        public void OnRemovedFromPipeline(IRenderPipeline pipeline)
        {
            pipeline?.OnRemovedFromPipeline(this);
            Debug.WriteLine("[ForwardPass] 파이프라인에서 제거됨");
        }

        /// <summary>
        /// 카메라 설정
        /// </summary>
        public void SetCamera(IMyCamera camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera), 
                "카메라가 null입니다.");
            
            Debug.WriteLine("[ForwardPass] 카메라 설정됨");
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(object? context = null, object? temp = null)
        {
            // context로 카메라 주입
            if (context is IMyCamera camera)
            {
                SetCamera(camera);
            }

            try
            {
                // Forward 쉐이더 생성
                PassShader = ShaderManager.Create(
                    "Forward", 
                    ShaderSource.forwardVtx, 
                    ShaderSource.forwardFrg, 
                    false, 
                    true);
                
                // 배경색 설정
                ClearColorValue = _context.Settings.BackgroundColor;
                
                Debug.WriteLine("[ForwardPass] 초기화 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForwardPass] 초기화 실패: {ex.Message}");
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 카메라 Uniform 설정
        /// </summary>
        public void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            _viewMatrix = view;
            _projectionMatrix = projection;
        }

        /// <summary>
        /// 렌더링 실행
        /// </summary>
        public void Execute()
        {
            if (_isDisposed || PassShader == null || !Enabled || _camera == null)
            {
                Debug.WriteLine("[ForwardPass] 실행 건너뜀 (Disposed, Shader 없음, 비활성화, 또는 카메라 없음)");
                return;
            }

            // 화면 클리어
            SetupClearState();
            
            // 렌더링 상태 설정
            SetupRenderState();

            // 쉐이더 바인딩 및 렌더링
            using (PassShader.Use(_camera, _context.LightingProvider.Light))
            {
                SetupShaderUniforms();
                
                // 불투명 객체 렌더링
                RenderOpaqueObjects();

                // 하이라이트 객체 렌더링 (향후 구현)
                RenderHighlightObjects();

                // 투명 객체 렌더링
                SetupTransparentRenderState();
                RenderTransparentObjects();
                
                // 블렌딩 비활성화
                GLUtil.SetBlending(false);
            }

            Debug.WriteLine("[ForwardPass] 실행 완료");
        }

        /// <summary>
        /// 리사이즈
        /// </summary>
        public void Resize(int width, int height)
        {
            // Frame Buffer를 사용하지 않으므로 무의미
            Debug.WriteLine($"[ForwardPass] Resize 호출됨: {width}x{height} (무시됨)");
        }

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed) return;
                
                PassShader?.Dispose();
                PassShader = null;
                
                _isDisposed = true;
                Debug.WriteLine("[ForwardPass] Dispose 완료");
            }
        }
        
        #endregion

        #region Rendering Setup
        
        /// <summary>
        /// 화면 클리어 설정
        /// </summary>
        private void SetupClearState()
        {
            if (ClearColor)
            {
                GLUtil.SetClearColor(_context.Settings.BackgroundColor);
            }
            
            if (ClearColor || ClearDepth)
            {
                GLUtil.ClearDrawingBuffer();
            }
        }

        /// <summary>
        /// 렌더링 상태 설정 (Depth, Culling 등)
        /// </summary>
        private void SetupRenderState()
        {
            // Depth Test 활성화
            GLUtil.SetDepthTest(true);
            GLUtil.SetDepthMask(true);
            GLUtil.DepthFunc(DepthFunction.Less);
            
            // Backface Culling 활성화
            GLUtil.SetCullFace(true, TriangleFace.Back);
        }

        /// <summary>
        /// 쉐이더 Uniform 설정
        /// </summary>
        private void SetupShaderUniforms()
        {
            if (PassShader == null || _camera == null) return;

            PassShader.SetUniformIfExist("uView", _viewMatrix);
            PassShader.SetUniformIfExist("uProjection", _projectionMatrix);
            PassShader.SetUniformIfExist("uCameraPosition", _camera.Position);
            
            // 조명 정보는 Shader.Use(camera, lighting)에서 자동 설정됨
        }

        /// <summary>
        /// 투명 객체 렌더링 상태 설정
        /// </summary>
        private void SetupTransparentRenderState()
        {
            // 양면 렌더링 (투명도는 양면을 그려야 함)
            GLUtil.SetCullFace(false, TriangleFace.Back);
            
            // 블렌딩 활성화
            GLUtil.SetBlending(true);
        }
        
        #endregion

        #region Rendering Methods
        
        /// <summary>
        /// 불투명 객체 렌더링
        /// </summary>
        private void RenderOpaqueObjects()
        {
            var drawBuffers = _context.DataProvider.DrawBuffers;
            
            foreach (var buffer in drawBuffers)
            {
                if (buffer != null)
                {
                    buffer.Execute();
                }
            }
            
            Debug.WriteLine($"[ForwardPass] 불투명 객체 렌더링: {drawBuffers.Count}개");
        }

        /// <summary>
        /// 하이라이트 객체 렌더링 (향후 구현)
        /// </summary>
        private void RenderHighlightObjects()
        {
            // TODO: SelectionManager로부터 하이라이트 객체 가져와서 렌더링
            // 별도의 쉐이더 또는 Stencil Buffer 사용
        }

        /// <summary>
        /// 투명 객체 렌더링 (향후 구현)
        /// </summary>
        private void RenderTransparentObjects()
        {
            // TODO: 투명 객체를 거리순으로 정렬 후 렌더링
            // 뒤에서 앞으로 (Back-to-Front) 정렬 필요
        }
        
        #endregion

        #region Not Implemented (Legacy Interface)
        
        /// <summary>
        /// Pass 리소스 생성 (미구현 - 향후 필요시 구현)
        /// </summary>
        public void CreatePassResources()
        {
            throw new NotImplementedException("CreatePassResources는 아직 구현되지 않았습니다.");
        }

        /// <summary>
        /// Pass 리소스 파괴 (미구현 - 향후 필요시 구현)
        /// </summary>
        public void DestroyPassResources()
        {
            throw new NotImplementedException("DestroyPassResources는 아직 구현되지 않았습니다.");
        }

        /// <summary>
        /// Pass 시작 (미구현 - 향후 필요시 구현)
        /// </summary>
        public void BeginPass()
        {
            throw new NotImplementedException("BeginPass는 아직 구현되지 않았습니다.");
        }

        /// <summary>
        /// Draw (미구현 - Execute 사용)
        /// </summary>
        public void Draw()
        {
            throw new NotImplementedException("Draw는 아직 구현되지 않았습니다. Execute()를 사용하세요.");
        }

        /// <summary>
        /// Pass 종료 (미구현 - 향후 필요시 구현)
        /// </summary>
        public void EndPass()
        {
            throw new NotImplementedException("EndPass는 아직 구현되지 않았습니다.");
        }
        
        #endregion
    }

    /// <summary>
    /// 렌더링 파이프라인 인터페이스
    /// </summary>
    public interface IRenderPipeline
    {
        /// <summary>
        /// Pass가 파이프라인에 추가될 때 호출
        /// </summary>
        void OnAddedToPipeline(IRenderPass renderPass);

        /// <summary>
        /// 파이프라인에 Pass 추가
        /// </summary>
        void AddRenderPass(IRenderPass renderPass);

        /// <summary>
        /// Pass가 파이프라인에서 제거될 때 호출
        /// </summary>
        void OnRemovedFromPipeline(IRenderPass renderPass);
    }
}