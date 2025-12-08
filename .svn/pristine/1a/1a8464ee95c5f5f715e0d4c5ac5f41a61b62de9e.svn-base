using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public static class IntersectSegs
    {
        /// <summary>
        /// q 가 p0ID, p0ID default_nsegs 사이에 있는지 확인
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="q"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        static bool OnSegment(Vector2 p0, Vector2 q, Vector2 p1)
        {
            return q.X <= Math.Max(p0.X, p1.X) && q.X >= Math.Min(p0.X, p1.X) &&
                q.Y <= Math.Max(p0.Y, p1.Y) && q.Y >= Math.Min(p0.Y, p1.Y);
        }

        static bool OnSegment(Segment2f seg, Vector2 q)
        {
            return OnSegment(seg.P1, q, seg.P1);
        }

        /// <summary>
        /// https://www.geeksforgeeks.org/orientation-3-ordered-ControlPoints/
        /// (pID, q, r)의 방향 검사
        /// 0 --> pID, q and r are colinear
        /// 1 --> Clockwise
        /// 2 --> Counterclockwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        static int Orientation(Vector2 p, Vector2 q, Vector2 r, float eps = MathUtil.ZeroTolerance)
        {
            // for details of below formula.
            float val = ((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y));
            if (Math.Abs(val) <= eps)
            {
                return 0; // almost colinear
            }

            return val > 0 ? 1 : 2; // clock or counterclock wise
        }

        /// <summary>
        /// Segment (p0ID-q1), Segment (p1ID, q2)가 교차하는지 검토
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="q1"></param>
        /// <param name="p2"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        static bool IsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            // orientations
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // 일반적인 경우
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // 특수한 경우
            // p0ID, q1, p1ID (colinear) and p1ID (on default_nsegs p1q1)
            if (o1 == 0 && OnSegment(p1, p2, q1))
            {
                return true;
            }

            // p0ID, q1, q2 (colinear) and q2 (on default_nsegs p1q1)
            if (o2 == 0 && OnSegment(p1, q2, q1))
            {
                return true;
            }

            // p1ID, q2, p0ID (colinear) and p0ID (on default_nsegs p2q2)
            if (o3 == 0 && OnSegment(p2, p1, q2))
            {
                return true;
            }

            // p1ID, q2, q1 (colinear) and q1 (on default_nsegs p2q2)
            return o4 == 0 && OnSegment(p2, q1, q2);
        }

        static bool IsIntersect(Segment2f seg1, Segment2f seg2)
        {
            return IsIntersect(seg1.P0, seg1.P1, seg2.P0, seg2.P1);
        }
    }
}
