using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public static class Vector4Extensions
    {
        public static float[] ToBuffer(this Vector4 vec)
        {
            return new float[] { vec.X, vec.Y, vec.Z, vec.W };
        }
        public static float Dot(this Vector4 v, Vector4 other)
        {
            return (v.X * other.X) + (v.Y * other.Y) + (v.Z * other.Z) + (v.W * other.W);
        }
        public static Vector4 Transform(this Vector4 vec, Matrix4 m)
        {
            return new Vector4(
                vec.Dot(m.Column0),
                vec.Dot(m.Column1),
                vec.Dot(m.Column2),
                vec.Dot(m.Column3)
            );
        }
    }
}
