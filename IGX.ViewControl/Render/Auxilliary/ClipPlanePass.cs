using IGX.ViewControl.Render.ClipPlane;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render.Auxilliary
{
    /// <summary>
    /// 클리핑 평면 렌더링 Pass - RenderPassBase를 상속받아 클리핑 영역 표시
    /// </summary>
    internal class ClipPlanePass : RenderPassBase
    {
        public override string Name => "ClipPlanePass";

        private ClipPlaneSystem? _clipSystem;
        private SceneParameters? _sceneParams;
        private Vector4 _color = Vector4.One;

        public override void Initialize(object? context1 = null, object? context2 = null)
        {
            base.Initialize(context1, context2);

            if (context1 is not SceneParameters ctx) return;
            _sceneParams = ctx;

            if (ctx.Camera is IMyCamera cam)
            {
                SetCamera(cam);
                // Camera가 설정된 후 ClipPlaneSystem 생성
                _clipSystem = new ClipPlaneSystem(cam);
                _clipSystem.SetEnabled(false); // 초기에는 비활성화
            }

            if (context2 is Vector4 c)
            {
                _color = c;
            }

            // Pass 자체도 초기에는 비활성화
            Enabled = false;
        }

        public override void Execute()
        {
            if (!Enabled)
            {
                return;
            }

            if (_clipSystem == null)
            {
                System.Diagnostics.Debug.WriteLine("ClipPlanePass: _clipSystem is null");
                return;
            }

            if (_sceneParams == null)
            {
                System.Diagnostics.Debug.WriteLine("ClipPlanePass: _sceneParams is null");
                return;
            }

            if (Camera == null)
            {
                System.Diagnostics.Debug.WriteLine("ClipPlanePass: Camera is null");
                return;
            }

            // ClipBox 렌더링
            System.Diagnostics.Debug.WriteLine($"ClipPlanePass: Drawing ClipBox (Enabled={Enabled}, SystemEnabled={_clipSystem.Enabled})");
            _clipSystem.ResetBox(_sceneParams.TotalBoundingBox);
            _clipSystem.Draw(_color);
        }

        /// <summary>
        /// GLView에서 ClipPlaneSystem에 접근할 수 있도록 제공
        /// </summary>
        public ClipPlaneSystem? GetClipPlaneSystem() => _clipSystem;

        /// <summary>
        /// ClipBox 모드 활성화/비활성화 - ClipPlaneSystem과 Pass 동기화
        /// </summary>
        public void SetClipBoxMode(bool enabled)
        {
            Enabled = enabled;
            _clipSystem?.SetEnabled(enabled);
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _clipSystem?.Dispose();
            base.Dispose();
        }
    }
}