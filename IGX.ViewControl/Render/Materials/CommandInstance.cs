using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render.Materials
{
    public struct CommandInstance
    {
        public PrimitiveType PrimitiveType;
        public int IndexCount;
        public nint IndexOffset;
        public int InstanceCount;

        public CommandInstance(PrimitiveType primitive, int count, nint offset, int instances)
        {
            PrimitiveType = primitive;
            IndexCount = count;
            IndexOffset = offset;
            InstanceCount = instances;
        }
    }
}