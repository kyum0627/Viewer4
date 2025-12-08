using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.GLDataStructure
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttribute : Attribute
    {
        public int Location { get; set; }
        public int Size { get; set; }
        public All Type { get; set; } = All.Float;
        public bool Normalized { get; set; } = false;
        public int Divisor { get; }
    }
}
