using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class SphericalDish : PrimitiveBase
    {
        public override bool IsTransformable => false;

        public float Radius;
        public float Height;
        public float arc;
        public float shift_z;
        public float scale_z = 1.0f;
        public SphericalDish(Matrix4 matrix, AABB3 bBoxLocal, float baseRadius = 1f, float height = 1f)
        {
            Radius = baseRadius;
            Height = height;
            TransformComp.Trans = matrix;
        }

        public Matrix4 MakeInstancingMatrix()
        {
            return Matrix4.CreateScale(new Vector3(Radius, Radius, Radius)) * Transform;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.SphericalDish;
    }
}