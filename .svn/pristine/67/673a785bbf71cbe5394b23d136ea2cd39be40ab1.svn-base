using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class Cylinder : PrimitiveBase
    {
        public override bool IsTransformable => true;

        public float Radius;
        public float Height;

        public Cylinder(Matrix4 matrix, AABB3 bBoxLocal, float radius = 0.5f, float height = 1f)
        {
            Radius = radius;
            Height = height;
            TransformComp.Scale = bBoxLocal.Extents;
            TransformComp.Trans = matrix;
        }

        public Cylinder(Vector3 from, Vector3 to, float radius)
        {
            Matrix4 matrix = Matrix4.Identity;
            matrix = matrix.CalculateTRmatrix(from, to);
            TransformComp.Trans = matrix;
            Radius = radius;
            Height = (to - from).Length;
            TransformComp.Scale = new Vector3(radius, radius, Height / 2);
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.Cylinder;
    }
}