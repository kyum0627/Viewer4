using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class CircularTorus : PrimitiveBase
    {
        public override bool IsTransformable { get; } = false;

        public float Offset;
        public float Radius;
        public float Angle;

        public CircularTorus(Matrix4 matrix, AABB3 bBoxLocal, float offset, float radius, float angle)
        {
            Offset = offset;
            Radius = radius;
            Angle = angle;
            TransformComp.Trans = matrix;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.CircularTorus;
    }
}