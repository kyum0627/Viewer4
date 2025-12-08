using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class VolLine : PrimitiveBase
    {
        public override bool IsTransformable => true;
        public float A;
        public float B;

        public VolLine(Matrix4 matrix, AABB3 bBoxLocal, float start, float end)
        {
            A = start;
            B = end;
            TransformComp.Trans = matrix;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.VolLine;
    }
}