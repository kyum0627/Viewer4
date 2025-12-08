using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    public class SceneState
    {
        public IMyCamera Camera { get; }
        public List<IDrawBuffer> MainRenderers { get; }
        public List<IDrawBuffer> AuxiliaryRenderers { get; }
        public ILight Lights { get; }
        public Vector4 BackgroundColor { get; set; } = new Vector4(0.1f, 0.1f, 0.2f, 1.0f);
        public bool DrawAuxiliary { get; set; } = true;

        public SceneState(IMyCamera camera, List<IDrawBuffer> mainRenderers, List<IDrawBuffer> auxRenderers, ILight lights)
        {
            Camera = camera;
            MainRenderers = mainRenderers;
            AuxiliaryRenderers = auxRenderers;
            Lights = lights;
        }
    }
}