using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class Sphere : PrimitiveBase
    {
        public override bool IsTransformable => true;

        public float Radius;
        public float arc;
        public float shift_z;
        public float scale_z;
        public float startAngle;

        public Sphere(Matrix4 matrix, AABB3 bBoxLocal, float diameter = 2f)
        {
            Radius = diameter * 0.5f;
            shift_z = 0f;
            scale_z = 1f;
            TransformComp.Trans = matrix;
        }

        //public Matrix4 Trans { get => base.Trans; set => base.Trans = value; }

        public Matrix4 InstMatrix
        {
            get
            {
                return Matrix4.CreateScale(Radius) * base.Transform;
            }
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.Sphere;
    }
}