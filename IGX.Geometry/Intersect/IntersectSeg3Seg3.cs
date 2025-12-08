using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    /// <summary>
    /// 두 3D 선분 간의 교차 계산을 수행하는 클래스
    /// </summary>
    public class IntersectSeg3Seg3
    {
        // 세 점의 방향을 계산하는 함수
        static readonly Func<Vector3, Vector3, Vector3, Vector3> orient3D = (p, q, r) =>
        {
            return Vector3.Cross(q - p, r - q);
        };

        /// <summary>
        /// 두 선분 사이의 교차 계산 메서드
        /// </summary>
        /// <param name="seg1">첫 번째 선분</param>
        /// <param name="seg2">두 번째 선분</param>
        /// <param name="tolerance">수치적 허용 오차 (기본값: 1e-05f)</param>
        /// <returns>교차 결과를 담은 IntersectionResult3 객체</returns>
        public static IntersectionResult3 Compute(Segment3 seg1, Segment3 seg2, float tolerance = 1e-05f)
        {
            // 세그먼트 1의 시작점과 끝점
            Vector3 p1 = seg1.P0; // 세그먼트 1의 시작점
            Vector3 q1 = seg1.P1; // 세그먼트 1의 끝점

            // 세그먼트 2의 시작점과 끝점
            Vector3 p2 = seg2.P0; // 세그먼트 2의 시작점
            Vector3 q2 = seg2.P1; // 세그먼트 2의 끝점

            IntersectionResult3 result = new();

            // 두 세그먼트가 평행하지 않고 교차하는지 판별
            Vector3 dir1 = q1 - p1;
            Vector3 dir2 = q2 - p2;
            Vector3 cross1 = orient3D(p1, q1, p2);
            Vector3 cross2 = orient3D(p1, q1, q2);

            bool segmentsIntersect = Vector3.Dot(cross1, cross2) <= tolerance &&
                                     Vector3.Dot(orient3D(p2, q2, p1), orient3D(p2, q2, q1)) <= tolerance;

            if (segmentsIntersect)
            {
                // 교차점 계산
                Vector3 intersectionDir1 = orient3D(p2, q2, p1);
                float t1 = Vector3.Dot(intersectionDir1, p1 - p2) / Vector3.Dot(intersectionDir1, dir2);
                Vector3 intersection = p2 + (t1 * dir2);

                // 세그먼트 1에서의 매개변수 값 계산
                float t1Param = Vector3.Dot(intersection - p1, dir1) / Vector3.Dot(dir1, dir1);

                result.quantity = 1;
                result.type = IntersectionType.POINT;
                result.intersects = true;
                result.status = IntersectionResult.INTERSECT;
                result.parameter1[0] = t1Param;
                result.parameter1[1] = t1Param;
                result.parameter2[0] = Vector3.Dot(intersection - seg2.Center, seg2.Direction);
                result.parameter2[1] = Vector3.Dot(intersection - seg2.Center, seg2.Direction);
                result.points = new List<Vector3>
                {
                    intersection
                };
                return result;
            }

            // 선분이 평행한 경우 처리
            float cross = Vector3.Dot(Vector3.Cross(seg1.Direction, seg2.Direction), Vector3.Cross(p2 - p1, seg1.Direction)); // cross product
            if (Math.Abs(cross) < tolerance)
            {
                // 각 세그먼트의 매개변수 범위 계산
                Interval parameter1 = ComputeInterval(seg1, seg2.P0, seg2.P1);
                Interval parameter2 = ComputeInterval(seg2, seg1.P0, seg1.P1);

                result.quantity = int.MaxValue;
                result.type = IntersectionType.SEGMENT;
                result.intersects = true;
                result.status = IntersectionResult.INTERSECT;
                result.parameter1 = parameter1;
                result.parameter2 = parameter2;
                result.points = new List<Vector3>
                {
                    (parameter1.a * seg1.Direction) + seg1.Center,
                    (parameter1.b * seg1.Direction) + seg1.Center
                };
                return result;
            }

            // 교차점이 없는 경우
            result.quantity = 0;
            result.type = IntersectionType.NONE;
            result.intersects = false;
            result.status = IntersectionResult.NOTINTERSECT;
            return result;
        }

        // 두 세그먼트 간의 매개변수 범위 계산 메서드
        private static Interval ComputeInterval(Segment3 seg1, Vector3 p, Vector3 q)
        {
            float t1 = Vector3.Dot(seg1.Direction, p - seg1.Center);
            float t2 = Vector3.Dot(seg1.Direction, q - seg1.Center);
            return new Interval(Math.Min(t1, t2), Math.Max(t1, t2));
        }
    }

    /// <summary>
    /// 3D 세그먼트를 나타내는 클래스
    /// </summary>
    public class Segment3
    {
        public Vector3 P0 { get; private set; }
        public Vector3 P1 { get; private set; }

        public Vector3 Direction { get { return P1 - P0; } }
        public Vector3 Center { get { return (P0 + P1) / 2; } }

        public Segment3(Vector3 p0, Vector3 p1)
        {
            P0 = p0;
            P1 = p1;
        }
    }

    ///// <summary>
    ///// 실수 구간을 나타내는 클래스
    ///// </summary>
    //public class Interval
    //{
    //    public float a;
    //    public float b;

    //    public Interval(float a, float b)
    //    {
    //        this.a = a;
    //        this.b = b;
    //    }
    //}
}
