using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render
{
    public class MockBufferRenderer : IDrawBuffer
    {
        public PrimitiveType PrimType { get; set; } = PrimitiveType.Triangles;

        public Shader? Shader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public void Execute()
        {
        }
        public void DrawInstanced(int instanceCount)
        {
        }

        public void ExecuteWithShader(Shader? shader)
        {
            ArgumentNullException.ThrowIfNull(shader);
        }
        public void ExecuteRange(int startIndex, int drawCount)
        {
            throw new NotImplementedException();
        }
    }
}