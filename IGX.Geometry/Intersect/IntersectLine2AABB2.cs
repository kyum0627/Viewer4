using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    class IntersectLine2AABB2
    {
        Line2f line;
        AABB2 box;

        public IntersectionResult2 result;

        public IntersectLine2AABB2(Line2f l, AABB2 b)
        {
            line = l;
            box = b;
        }

        public bool Compute(float epsilonD = MathUtil.Epsilon, float epsilonI = MathUtil.ZeroTolerance)
        {
            // line direction이 단위 벡터가 아니면 계산 준비가 안된 것으로 return
            if (!line.Direction.IsNormalized())
            {
                result.type = IntersectionType.EMPTY;
                result.status = IntersectionResult.INVALID;
                return false;
            }

            result.parameter1[0] = -float.MaxValue; // 충분히 긴 선
            result.parameter1[1] = float.MaxValue; // 충분히 긴 선

            DoQuery(true, epsilonD, epsilonI);

            result.status = result.type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT : IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        private bool DoQuery(bool solid = true, float epsilonD = MathUtil.Epsilon, float epsilonI = MathUtil.ZeroTolerance)
        {
            // 무한 직선의 t 값은 interval (-infinity,+infinity).
            //   0, no intersection
            //   1, 점 교차 (t0 = 점의 line parameter)
            //   2, 선분 교차 (interval = [t0,t1])
            Vector2 origin = line.Point;
            Vector2 direction = line.Direction;

            Vector2 bo = line.Point - box.Center;
            Vector2 extent = box.Extents;

            float t0 = result.parameter1[0];
            float t1 = result.parameter1[1];

            float saveT0 = t0;
            float saveT1 = t1;

            bool isClipped =
                Clip(+direction.X, -bo.X - extent.X, ref t0, ref t1, epsilonI) &&
                Clip(-direction.X, +bo.X - extent.X, ref t0, ref t1, epsilonI) &&
                Clip(+direction.Y, -bo.Y - extent.Y, ref t0, ref t1, epsilonI) &&
                Clip(-direction.Y, +bo.Y - extent.Y, ref t0, ref t1, epsilonI);

            if (isClipped && (solid || t0 != saveT0 || t1 != saveT1))
            {
                if (t1 > t0)
                {
                    result.type = IntersectionType.SEGMENT;
                    result.quantity = 2;
                    result.parameter1[0] = t0;
                    result.parameter1[1] = t1;
                    result.points = new List<Vector2>
                    {
                        origin + (t0 * direction),
                        origin + (t1 * direction)
                    };
                    result.status = IntersectionResult.INTERSECT;
                }
                else
                {
                    result.type = IntersectionType.POINT;
                    result.quantity = 1;
                    result.parameter1[0] = t0;
                    result.points = new List<Vector2>
                    {
                        origin + (t0 * direction)
                    };
                    result.status = IntersectionResult.INTERSECT;
                }
            }
            else
            {
                result.quantity = 0;
                result.type = IntersectionType.EMPTY;
                result.status = IntersectionResult.NOTINTERSECT;
            }

            return result.type != IntersectionType.EMPTY;
        }

        private static bool Clip(float denom, float numer, ref float t0, ref float t1, float eps = MathUtil.ZeroTolerance)
        {
            if (denom > 0)
            {
                if (numer - (denom * t1) > eps)
                {
                    return false;
                }

                if (numer > denom * t0)
                {
                    t0 = numer / denom;
                }

                return true;
            }
            else if (denom < 0)
            {
                if (numer - (denom * t0) > eps)
                {
                    return false;
                }

                if (numer > denom * t1)
                {
                    t1 = numer / denom;
                }

                return true;
            }
            else
            {
                return numer <= 0;
            }
        }
    }
}
