using IGX.ViewControl.Render;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    /// <summary>
    /// IRenderPass의 공통 구현을 제공하는 추상 베이스 클래스
    /// 각 Pass는 이를 상속받아 특화 로직만 구현
    /// </summary>
    public abstract class RenderPassBase : IRenderPass
    {
        public abstract string Name { get; }
        public virtual int Order { get; set; }
        public virtual bool Enabled { get; set; } = true;
        
        public virtual Shader? PassShader { get; set; }
        
        public virtual bool ClearColor { get; set; }
        public virtual bool ClearDepth { get; set; }
        public virtual Vector4 ClearColorValue { get; set; }

        protected IMyCamera? Camera { get; private set; }
        protected IRenderPipeline? Pipeline { get; private set; }

        public virtual void OnAddedToPipeline(IRenderPipeline pipeline)
        {
            Pipeline = pipeline;
            pipeline.OnAddedToPipeline(this);
        }

        public virtual void OnRemovedFromPipeline(IRenderPipeline pipeline)
        {
            Pipeline = null;
            pipeline.OnRemovedFromPipeline(this);
        }

        public virtual void SetCamera(IMyCamera camera)
        {
            Camera = camera;
        }

        public virtual void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            if (PassShader == null) return;

            PassShader.SetUniformIfExist("uView", view);
            PassShader.SetUniformIfExist("uProjection", projection);
            
            if (Camera != null)
            {
                PassShader.SetUniformIfExist("uCameraPosition", Camera.Position);
            }
        }

        public virtual void Initialize(object? context1 = null, object? context2 = null)
        {
            // 기본 구현: 파생 클래스에서 Override
        }

        public abstract void Execute();

        public virtual void Resize(int width, int height)
        {
            // 기본 구현: 파생 클래스에서 필요시 Override
        }

        protected bool _disposed;

        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            PassShader?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
