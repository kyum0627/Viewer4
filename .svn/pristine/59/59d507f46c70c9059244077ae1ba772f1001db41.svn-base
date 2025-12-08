using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine3AABB3
    {
        Line3f line;
        AABB3 box;

        public IntersectionResult3 result;

        public IntersectLine3AABB3(Line3f l, AABB3 b)
        {
            line = l;
            box = b;
        }

        public bool Compute()
        {
            if (result.status != IntersectionResult.NOTCOMPUTED)
            {
                return result.status == IntersectionResult.INTERSECT;
            }

            //// line direction이 단위 벡터가 아니면 계산 준비가 안된 것으로 return
            //if (line.Direction.IsNormalized == false)
            //{
            //    result.type = IntersectionType.EMPTY;
            //    result.status = IntersectionResult.INVALID;
            //    return false;
            //}

            result.parameter1[0] = -float.MaxValue; // 충분히 긴 선
            result.parameter1[1] = float.MaxValue; // 충분히 긴 선
            DoQuery(true);

            result.status = result.type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT :
                IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        public bool Test()
        {
            Vector3 AWdU = Vector3.Zero;
            Vector3 AWxDdU = Vector3.Zero;
            float RHS;

            Vector3 diff = line.origin - box.Center;
            Vector3 WxD = line.direction * diff;

            Vector3 extent = box.Extents;

            AWdU[1] = Math.Abs(Vector3.Dot(line.direction, Vector3.UnitY));
            AWdU[2] = Math.Abs(Vector3.Dot(line.direction, Vector3.UnitZ));
            AWxDdU[0] = Math.Abs(Vector3.Dot(WxD, Vector3.UnitX));
            RHS = (extent.Y * AWdU[2]) + (extent.Z * AWdU[1]);
            if (AWxDdU[0] > RHS)
            {
                return false;
            }

            AWdU[0] = Math.Abs(Vector3.Dot(line.direction, Vector3.UnitX));
            AWxDdU[1] = Math.Abs(Vector3.Dot(WxD, Vector3.UnitY));
            RHS = (extent.X * AWdU[2]) + (extent.Z * AWdU[0]);
            if (AWxDdU[1] > RHS)
            {
                return false;
            }

            AWxDdU[2] = Math.Abs(Vector3.Dot(WxD, Vector3.UnitZ));
            RHS = (extent.X * AWdU[1]) + (extent.Y * AWdU[0]);
            return AWxDdU[2] <= RHS;
        }

        /// <summary>
        /// AssemblyAABB box와 line 간 겹치는 부분 검사
        /// </summary>
        /// <param name="t0">직선의  - 방향 최대 길이</param>
        /// <param name="t1">직선의  + 방향 최대 길이</param>
        /// <param name="origin">직선의 원점</param>
        /// <param name="direction">직선의 방향</param>
        /// <param name="box">교차 게산할 box </param>
        /// <param name="solid">box를 solid로 볼 것인지?</param>
        /// <param name="quantity">교차점의 수</param>
        /// <param name="point0">교차점 0</param>
        /// <param name="point1">교차점 1</param>
        /// <param name="intrType">교차유형</param>
        /// <returns></returns>
        private bool DoQuery(bool solid = true)
        {
            // 무한 직선의 t 값은 interval (-infinity,+infinity).
            //   0, no intersection
            //   1, 점 교차 (t0 = 점의 line parameter)
            //   2, 선분 교차 (interval = [t0,t1])
            Vector3 origin = line.origin;
            Vector3 direction = line.direction;

            Vector3 bo = origin - box.Center;
            Vector3 extent = box.Extents;

            float t0 = result.parameter1[0];
            float t1 = result.parameter1[1];

            float saveT0 = t0;
            float saveT1 = t1;

            bool isClipped =
                Clip(+direction.X, -bo.X - extent.X, ref t0, ref t1) &&
                Clip(-direction.X, +bo.X - extent.X, ref t0, ref t1) &&
                Clip(+direction.Y, -bo.Y - extent.Y, ref t0, ref t1) &&
                Clip(-direction.Y, +bo.Y - extent.Y, ref t0, ref t1) &&
                Clip(+direction.Z, -bo.Z - extent.Z, ref t0, ref t1) &&
                Clip(-direction.Z, +bo.Z - extent.Z, ref t0, ref t1);

            if (isClipped && (solid || t0 != saveT0 || t1 != saveT1))
            {
                if (t1 > t0)
                {
                    result.type = IntersectionType.SEGMENT;
                    result.quantity = 2;
                    result.points[0] = origin + (t0 * direction);
                    result.points[1] = origin + (t1 * direction);
                }
                else
                {
                    result.type = IntersectionType.POINT;
                    result.quantity = 1;
                    result.points[0] = origin + (t0 * direction);
                }
            }
            else
            {
                result.quantity = 0;
                result.type = IntersectionType.EMPTY;
            }

            return result.type != IntersectionType.EMPTY;
        }

        /// <summary>
        /// clip된 segment가 test plane과 교차하는지 검사
        /// </summary>
        /// <param name="denom"></param>
        /// <param name="numer"></param>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns>교차하면 'true'</returns>
        private static bool Clip(float denom, float numer, ref float t0, ref float t1)
        {
            if (denom > 0)
            {
                if (numer - (denom * t1) > MathUtil.ZeroTolerance)
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
                if (numer - (denom * t0) > MathUtil.ZeroTolerance)
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