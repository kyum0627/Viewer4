using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl
{
    /// <summary>
    /// FBO 렌더링 패스: 위치, 법선, 색상 등을 G-Buffer(FBO)에 기록.
    /// 구체적인 의존성 주입으로 명확한 책임 분리
    /// </summary>
    public class FBOPass : IRenderPass
    {
        public string Name => "FBOPass";
        public int Order { get; set; } = 10;
        public bool Enabled { get; set; } = true;
        public Shader? PassShader { get; set; }

        public bool ClearColor { get; set; } = true;
        public bool ClearDepth { get; set; } = true;
        public Vector4 ClearColorValue { get; set; }

        public FrameBufferObject? Gbuffer { get; private set; }

        // 구체적인 의존성
        private IRenderDataProvider? _dataProvider;
        private IRenderSettings? _settings;
        private IMyCamera? _camera;
        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;
        private bool _isDisposed;
        private readonly object _disposeLock = new object();

        public FBOPass()
        {
        }

        public void OnAddedToPipeline(IRenderPipeline pipeline)
        {
            pipeline.OnAddedToPipeline(this);
        }

        public void OnRemovedFromPipeline(IRenderPipeline pipeline)
        {
            pipeline.OnRemovedFromPipeline(this);
        }

        public void SetCamera(IMyCamera camera)
        {
            _camera = camera;
        }

        /// <summary>
        /// 명확한 의존성 주입으로 초기화
        /// </summary>
        public void Initialize(IRenderDataProvider dataProvider, IRenderSettings settings)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            PassShader = ShaderManager.Create("GBuffer", ShaderSource.gBufferVtx, ShaderSource.gBufferFrg, false, true);
            Gbuffer = new FrameBufferObject(1, 1);
            
            Debug.WriteLine($"[FBOPass] Initialized with concrete dependencies");
        }

        /// <summary>
        /// 하위 호환성을 위한 기존 Initialize 메서드
        /// </summary>
        [Obsolete("Use Initialize(IRenderDataProvider, IRenderSettings) instead")]
        public void Initialize(object? context, object? color = null)
        {
            if (context is IRenderDataProvider dataProvider && context is IRenderSettings settings)
            {
                Initialize(dataProvider, settings);
            }
            else if (context is IgxViewAPI apis)
            {
                Initialize(apis, apis);
            }
            else
            {
                throw new ArgumentException("Context must implement IRenderDataProvider and IRenderSettings");
            }
        }

        public void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            _viewMatrix = view;
            _projectionMatrix = projection;
        }

        public void Execute()
        {
            if (_isDisposed || Gbuffer == null || PassShader == null || !Enabled || 
                _camera == null || _dataProvider == null || _settings == null)
                return;

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.Disable(EnableCap.Blend);

            Gbuffer.Bind();

            if (ClearDepth)
            {
                GLUtil.ClearDrawingBuffer();
            }

            using (PassShader.Use())
            {
                PassShader.SetUniformIfExist("uView", _viewMatrix);
                PassShader.SetUniformIfExist("uProjection", _projectionMatrix);
                PassShader.SetUniformIfExist("shadeMode", (int)_settings.ShadeMode);

                // 명확한 의존성 사용
                foreach (var bufferManager in _dataProvider.DrawBuffers)
                {
                    if (bufferManager != null)
                    {
                        bufferManager.Execute();
                    }
                }
            }

            GL.DepthMask(false);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
        }

        public void Resize(int width, int height)
        {
            if (_isDisposed || Gbuffer == null || _settings == null) return;

            try
            {
                Gbuffer.Resize(width, height, _settings.ShadeMode == ShadeMode.Xray);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ErrorCheck resizing FBOPass: {ex.Message}");
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    return;
                }
                PassShader?.Dispose();
                Gbuffer?.Dispose();

                PassShader = null;
                Gbuffer = null;

                _isDisposed = true;
            }
        }
    }
}