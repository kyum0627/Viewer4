using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class RectangularTorus : PrimitiveBase
    {
        public override bool IsTransformable => false;

        public float Rinside;
        public float Routside;
        public float Height;
        public float Angle;

        public RectangularTorus(
            Matrix4 matrix,
            AABB3 bBoxLocal,
            float radiusInner,
            float radiusOuter,
            float height,
            float angle)
        {
            Rinside = radiusInner;
            Routside = radiusOuter;
            Height = height;
            Angle = angle;
            TransformComp.Trans = matrix;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.RectangularTorus;
    }
}