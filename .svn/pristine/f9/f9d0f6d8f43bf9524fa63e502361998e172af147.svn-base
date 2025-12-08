using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl
{
    /// <summary>
    /// FBO 후처리 패스: G-Buffer(FBO)의 출력을 스크린에 렌더링
    /// 구체적인 의존성 주입으로 명확한 책임 분리
    /// </summary>
    public class PostFBOPass : IRenderPass
    {
        public string Name => "PostFBOPass";
        public int Order { get; set; } = 20;
        public bool Enabled { get; set; } = true;
        public Shader? PassShader { get; set; }

        public bool ClearColor { get; set; } = false;
        public bool ClearDepth { get; set; } = false;
        public Vector4 ClearColorValue { get; set; }

        // 구체적인 의존성
        private ILightingProvider? _lighting;
        private IRenderSettings? _settings;
        private FrameBufferObject? _sourceGBuffer;
        private IMyCamera? _camera;
        private Shader? _outlinePostShader;
        private Quad? _quad;
        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;
        private bool _isDisposed;
        private readonly object _disposeLock = new object();

        public PostFBOPass()
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
        public void Initialize(
            FrameBufferObject sourceGBuffer,
            ILightingProvider lighting,
            IRenderSettings settings)
        {
            _sourceGBuffer = sourceGBuffer ?? throw new ArgumentNullException(nameof(sourceGBuffer));
            _lighting = lighting ?? throw new ArgumentNullException(nameof(lighting));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            try
            {
                _outlinePostShader = ShaderManager.Create("OutLine", ShaderSource.quadVtx, ShaderSource.outlinePostFrg, false, true);
                PassShader = _outlinePostShader;
                
                _quad = new Quad();
                _quad.InitializeQuad();
                
                Debug.WriteLine($"[PostFBOPass] Initialized with concrete dependencies");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ErrorCheck initializing PostFBOPass: {ex.Message}");
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 하위 호환성을 위한 기존 Initialize 메서드
        /// </summary>
        [Obsolete("Use Initialize(FrameBufferObject, ILightingProvider, IRenderSettings) instead")]
        public void Initialize(object? apis, object? gbuffer = null)
        {
            if (gbuffer is FrameBufferObject fbo && 
                apis is ILightingProvider lighting && 
                apis is IRenderSettings settings)
            {
                Initialize(fbo, lighting, settings);
            }
            else if (apis is IgxViewAPI igxApi && gbuffer is FrameBufferObject fbo2)
            {
                Initialize(fbo2, igxApi, igxApi);
            }
            else
            {
                throw new ArgumentException("Invalid initialization parameters");
            }
        }

        public void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            _viewMatrix = view;
            _projectionMatrix = projection;
        }

        public void Execute()
        {
            if (_isDisposed || !Enabled || _camera == null || 
                _lighting == null || _settings == null || _sourceGBuffer == null) 
                return;

            if (_sourceGBuffer == null || !_sourceGBuffer.IsValid) return;

            // OpenGL 상태 저장
            int[] savedViewport = new int[4];
            GL.GetInteger(GetPName.Viewport, savedViewport);
            int savedDepthTest = GL.IsEnabled(EnableCap.DepthTest) ? 1 : 0;
            int savedCullFace = GL.IsEnabled(EnableCap.CullFace) ? 1 : 0;
            int savedBlend = GL.IsEnabled(EnableCap.Blend) ? 1 : 0;

            // G-Buffer의 깊이 버퍼를 기본 프레임버퍼로 복사 (보조 렌더링을 위해)
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _sourceGBuffer.Handle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(
                0, 0, _camera.ViewportWidth, _camera.ViewportHeight, 
                0, 0, _camera.ViewportWidth, _camera.ViewportHeight, 
                ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // OpenGL 상태 설정
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Viewport(0, 0, _camera.ViewportWidth, _camera.ViewportHeight);

            using (_outlinePostShader!.Use())
            {
                _sourceGBuffer.Position?.Use(TextureUnit.Texture0, _outlinePostShader, "gPosition");
                _sourceGBuffer.Normal?.Use(TextureUnit.Texture1, _outlinePostShader, "gNormal");
                _sourceGBuffer.Color?.Use(TextureUnit.Texture2, _outlinePostShader, "gAlbedo");
                _sourceGBuffer.ObjectID?.Use(TextureUnit.Texture3, _outlinePostShader, "gObjectID");
                _sourceGBuffer.DepthStencil?.Use(TextureUnit.Texture4, _outlinePostShader, "gDepthStencil");

                _outlinePostShader.SetUniformIfExist("uScreenSize", new Vector2(_camera.ViewportWidth, _camera.ViewportHeight));
                _outlinePostShader.SetUniformIfExist("uViewPosition", _camera.Position);
                
                // 명확한 의존성 사용
                _outlinePostShader.SetUniformIfExist("uLightDirection", _lighting.Direction);
                _outlinePostShader.SetUniformIfExist("uLightColor", _lighting.Color);
                _outlinePostShader.SetUniformIfExist("uAmbientStrength", RendererConstants.AmbientStrength);
                _outlinePostShader.SetUniformIfExist("uSpecularStrength", RendererConstants.SpecularStrength);
                _outlinePostShader.SetUniformIfExist("uShininess", RendererConstants.Shininess);
                _outlinePostShader.SetUniformIfExist("uEdgelineColor", _settings.EdgeColor);
                _outlinePostShader.SetUniformIfExist("uEdgelineThickness", _settings.EdgeThickness);
                _outlinePostShader.SetUniformIfExist("uDrawEdge", _settings.DisplayEdges ? 1 : 0);

                _quad!.DrawQuad();
            }

            // OpenGL 상태 복원
            GL.Viewport(savedViewport[0], savedViewport[1], savedViewport[2], savedViewport[3]);
            if (savedDepthTest == 1) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (savedCullFace == 1) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (savedBlend == 1) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
        }

        public void Resize(int width, int height)
        {
            if (_isDisposed) return;
            GL.Viewport(0, 0, width, height);
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_isDisposed) return;

                _outlinePostShader?.Dispose();
                _quad?.Dispose();

                _outlinePostShader = null;
                PassShader = null;
                _quad = null;
                _isDisposed = true;
            }
        }
    }
}
