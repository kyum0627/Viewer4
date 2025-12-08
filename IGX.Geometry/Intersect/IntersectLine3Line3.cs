using System;
using OpenTK.Mathematics;
//using IGX.Geometry.Operations.MinimumBox;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine3Line3
    {
        Line3f line1;
        Line3f line2;

        public IntersectionResult3 result;

        public IntersectLine3Line3(Line3f s1, Line3f s2)
        {
            line1 = s1;
            line2 = s2;
        }

        public IntersectLine3Line3(Vector3 p0, Vector3 d0, Vector3 p1, Vector3 d1)
        {
            line1 = new Line3f(p0, Vector3.Normalize(d0));
            line2 = new Line3f(p1, Vector3.Normalize(d1));
        }

        public IntersectionType GetIntersectionType(ref Interval s, float eps = MathUtil.Epsilon)
        {
            Vector3 cr = line1.direction.Cross(line2.direction);
            float sgn = Vector3.Dot(cr, cr);

            if (sgn > eps)
            { // Skew or intersect
                if (IsSkew(line1.origin, line1.direction, line2.origin, line2.direction, eps))
                {
                    result.status = IntersectionResult.NOTINTERSECT;
                    return IntersectionType.EMPTY;
                }
                result.status = IntersectionResult.INTERSECT;
                result.type = IntersectionType.POINT;
                return IntersectionType.POINT;
            }
            else
            { // parallel
                float dist = Vector3.Dot(line1.direction, line2.origin - line1.origin);
                if (Math.Abs(dist) < eps)
                {// Colinear
                    result.status = IntersectionResult.INTERSECT;
                    return IntersectionType.LINE;
                }
                else
                {// parallel, not intersect
                    return IntersectionType.EMPTY;
                }
            }
        }

        private bool IsSkew(Vector3 p0, Vector3 d0, Vector3 p1, Vector3 d1, float eps = MathUtil.Epsilon)
        {
            Vector3 t2 = Vector3.Normalize(line1.direction.Cross(line2.direction));
            return Vector3.Dot(t2, t2) > eps && (Math.Abs(Vector3.Dot(t2, p0)) > eps || Math.Abs(Vector3.Dot(t2, p1)) > eps);
        }
    }
}