using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    /// <summary>
    /// 두 2D 선분 간의 교차 계산을 수행하는 클래스
    /// </summary>
    public class IntersectSeg2Seg2
    {
        // 세 점의 방향을 계산하는 함수
        static readonly Func<Vector2, Vector2, Vector2, float> orient = (p, q, r) =>
        {
            return ((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y));
        };

        /// <summary>
        /// 두 선분 사이의 교차 계산 메서드
        /// </summary>
        /// <param name="seg1">첫 번째 선분</param>
        /// <param name="seg2">두 번째 선분</param>
        /// <param name="tolerance">수치적 허용 오차 (기본값: 1e-05f)</param>
        /// <returns>교차 결과를 담은 IntersectionResult2 객체</returns>
        public static IntersectionResult2 Compute(Segment2f seg1, Segment2f seg2, float tolerance = 1e-05f)
        {
            // 세그먼트 1의 시작점과 끝점
            Vector2 p1 = seg1.P0; // 세그먼트 1의 시작점
            Vector2 q1 = seg1.P1; // 세그먼트 1의 끝점

            // 세그먼트 2의 시작점과 끝점
            Vector2 p2 = seg2.P0; // 세그먼트 2의 시작점
            Vector2 q2 = seg2.P1; // 세그먼트 2의 끝점

            IntersectionResult2 result = new();

            // 두 세그먼트가 교차하는지 판별
            bool segmentsIntersect = orient(p1, q1, p2) * orient(p1, q1, q2) <= 0 &&
                                     orient(p2, q2, p1) * orient(p2, q2, q1) <= 0;

            if (segmentsIntersect)
            {
                // 교차점 계산
                float t1 = orient(p2, q2, p1) / orient(p2, q2, q1 - p1);
                Vector2 intersection = p1 + (t1 * (q1 - p1));

                // 세그먼트 1에서의 매개변수 값 계산
                float t1Param = (t1 - 0.5f) * seg1.extent; // centered form에서 parameter

                result.quantity = 1;
                result.type = IntersectionType.POINT;
                result.intersects = true;
                result.status = IntersectionResult.INTERSECT;
                result.parameter1.a = t1Param;
                result.parameter1.b = t1Param;
                result.parameter2.a = Vector2.Dot(seg2.direction, intersection - seg2.center);
                result.parameter2.b = Vector2.Dot(seg2.direction, intersection - seg2.center);
                result.points = new List<Vector2>
                {
                    intersection
                };
                return result;
            }

            // 선분이 평행한 경우 처리
            float cross = seg1.direction.CrossDot(seg2.direction); // cross product
            if (Math.Abs(cross) < tolerance)
            {
                // 선분이 평행한 경우 처리
                float t1Seg1P0 = Vector2.Dot(seg1.direction, seg2.P0 - seg1.center);
                float t1Seg1P1 = Vector2.Dot(seg1.direction, seg2.P1 - seg1.center);
                float t2Seg2P0 = Vector2.Dot(seg2.direction, seg1.P0 - seg2.center);
                float t2Seg2P1 = Vector2.Dot(seg2.direction, seg1.P1 - seg2.center);

                // 각 세그먼트의 매개변수 범위 계산
                Interval parameter1 = new(Math.Max(-seg1.extent, Math.Min(t1Seg1P0, t1Seg1P1)), Math.Min(seg1.extent, Math.Max(t1Seg1P0, t1Seg1P1)));
                Interval parameter2 = new(Math.Max(-seg2.extent, Math.Min(t2Seg2P0, t2Seg2P1)), Math.Min(seg2.extent, Math.Max(t2Seg2P0, t2Seg2P1)));

                result.quantity = int.MaxValue;
                result.type = IntersectionType.SEGMENT;
                result.intersects = true;
                result.status = IntersectionResult.INTERSECT;
                result.parameter1 = parameter1;
                result.parameter2 = parameter2;
                result.points = new List<Vector2>
                {
                    (parameter1.a * seg1.direction) + seg1.center,
                    (parameter1.b * seg1.direction) + seg1.center
                };
            }

            // 교차점이 없는 경우
            result.quantity = 0;
            result.type = IntersectionType.NONE;
            result.intersects = false;
            result.status = IntersectionResult.NOTINTERSECT;
            return result;
        }
    }
}
