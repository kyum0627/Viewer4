using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.GLDataStructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BasicInstance
    {
        [Vertex(Location =  2, Size = 16)] public Matrix4 Model;
        [Vertex(Location =  6, Size =  4)] public Vector4 Color;
        public static int SizeInBytes => Marshal.SizeOf(typeof(BasicInstance));
    }
}
