using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class EllipticalDish : PrimitiveBase
    {
        public override bool IsTransformable => false;

        public float BaseRadius;
        public float Height;
        public float arc;
        public float shift_z;
        public float scale_z;
        public float startAngle;

        public EllipticalDish(Matrix4 matrix, AABB3 bBoxLocal, float baseradius = 0.5f, float height = 1f)
        {
            BaseRadius = baseradius;
            Height = height;
            TransformComp.Trans = matrix;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.EllipticalDish;
    }
}