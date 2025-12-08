using IGX.Geometry.DataStructure;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    public class ShaderParameters
    {
        private ShadeMode _mode = ShadeMode.Phong;

        public event EventHandler<EventArgs>? ModeChanged;

        public ShadeMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnModeChanged();
                }
            }
        }

        public bool DisplayEdge = true;
        public Vector3 EdgeColor = MyColor.ToBuffer(Color.Black).Xyz;
        public float EdgeThickness = 1f;
        public string FragShaderFile = "all_frag.glsl";
        public string VertexShaderFile = "uni_vert.glsl";

        protected virtual void OnModeChanged()
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}