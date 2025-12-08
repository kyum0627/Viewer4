using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class Snout : PrimitiveBase
    {
        public override bool IsTransformable => false;

        public float Rtop;
        public float Rbottom;
        public float Height;
        public float Xoffset;
        public float Yoffset;
        public float XbottomShear;
        public float YbottomShear;
        public float XtopShear;
        public float YtopShear;

        public Snout(Matrix4 matrix,
            AABB3 bBoxLocal,
            float radiusBottom = 1f,
            float radiusTop = 1f,
            float height = 1f,
            float offsetX = 0f,
            float offsetY = 0f,
            float bottomShearX = 0f,
            float bottomShearY = 0f,
            float topShearX = 0f,
            float topShearY = 0f)
        {
            Rbottom = radiusBottom;
            Rtop = radiusTop;
            Height = height;

            Xoffset = offsetX;
            Yoffset = offsetY;
            XbottomShear = bottomShearX;
            YbottomShear = bottomShearY;
            XtopShear = topShearX;
            YtopShear = topShearY;
            TransformComp.Trans = matrix;
        }

        public Snout(Vector3 bottom, Vector3 top,
            float bottomRadius,
            float topradius = 0,
            float offsetX = 0f,
            float offsetY = 0f,
            float bottomShearX = 0f,
            float bottomShearY = 0f,
            float topShearX = 0f,
            float topShearY = 0f)
        {
            TransformComp.Trans = Transform.CalculateTRmatrix(bottom, top);
            Rbottom = bottomRadius;
            Rtop = topradius;
            Height = (top - bottom).Length;

            Xoffset = offsetX;
            Yoffset = offsetY;
            XbottomShear = bottomShearX;
            YbottomShear = bottomShearY;
            XtopShear = topShearX;
            YtopShear = topShearY;
        }

        public override ParaPrimType GetParametricType() => ParaPrimType.Snout;
    }
}