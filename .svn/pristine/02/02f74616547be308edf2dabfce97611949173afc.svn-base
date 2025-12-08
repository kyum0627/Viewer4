using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    /// <summary>
    /// 3차원 삼각형 간의 교차를 테스트하고 교차 영역을 계산하는 유틸리티.
    /// 이 클래스는 분리 축 정리(SAT)와 3D 클리핑 알고리즘을 사용.
    /// </summary>
    public static class IntersectTriangle3Triangle3
    {
        private const float Epsilon = 1e-6f; // 부동 소수점 비교를 위한 작은 값 (오차 허용 범위)

        ///// <summary>
        ///// 두 삼각형이 겹치는지 여부를 분리 축 분리(SAT)를 사용하여 빠르게 판별.
        ///// </summary>
        public static bool Test(Triangle3f t0, Triangle3f t1, float tolerrance = Epsilon)
        {
            return Triangle3f.IsCollide(t0, t1, Epsilon);
        }

        /// <summary>
        /// 두 삼각형의 교차 영역을 계산.
        /// </summary>
        public static IntersectionResult3 Compute(Triangle3f t0, Triangle3f t1, float tolerrance = Epsilon)
        {
            var result = new IntersectionResult3();
            if (!Test(t0, t1, tolerrance)) // Test 함수로 교차 여부 먼저 판별
            {
                result.status = IntersectionResult.NOTINTERSECT;
                return result;
            }

            // 1. t0 평면을 기준으로 t1의 점 위치 질의
            Plane3f plane0 = t0.Plane;
            QueryTriangle3Position(t1, plane0, out float[] dist1, out int[] sign1, out int pos1, out int neg1, out int zero1); // t1의 점들이 t0 평면의 어느 쪽에 있는지 확인

            if (zero1 == 3) // t1의 모든 점이 t0 평면에 있는 경우 (동일 평면)
            {
                return GetCoplanarIntersection(t0, t1); // 동일 평면 교차 계산
            }
            else
            {
                return GetNonCoplanarIntersection(t0, t1); // 다른 평면 교차 계산
            }
        }

        /// <summary>
        /// 두 삼각형이 평행하지 않을 때 교차 영역을 계산.
        /// </summary>
        private static IntersectionResult3 GetNonCoplanarIntersection(Triangle3f t0, Triangle3f t1)
        {
            var result = new IntersectionResult3();

            // t0의 변들을 기준으로 t1을 클리핑
            var clippedPoints = new List<Vector3> { t1[0], t1[1], t1[2] }; // 클리핑할 다각형(t1)

            for (int i0 = 0, i1 = 2; i0 < 3; i1 = i0++)
            {
                Vector3 normal = Vector3.Cross(t0[i1] - t0[i0], t0.Normal); // 클리핑 평면의 법선
                float constant = Vector3.Dot(normal, t0[i1]); // 평면 상수 (Ax+By+Cz=D의 D)

                clippedPoints = ClipConvexPolygonAgainstLine(clippedPoints, normal, constant); // 클리핑 수행
            }

            // 교차 결과 설정
            result.status = IntersectionResult.INTERSECT;
            result.quantity = clippedPoints.Count;
            result.points = clippedPoints;
            result.type = clippedPoints.Count == 0 ? IntersectionType.EMPTY :
                          clippedPoints.Count == 1 ? IntersectionType.POINT :
                          clippedPoints.Count == 2 ? IntersectionType.SEGMENT : IntersectionType.POLYGON;

            return result;
        }

        /// <summary>
        /// 볼록 다각형을 주어진 선에 대해 클리핑하여 새로운 볼록 다각형을 반환.
        /// </summary>
        private static List<Vector3> ClipConvexPolygonAgainstLine(List<Vector3> inputPoints, Vector3 normal, float constant)
        {
            var outputPoints = new List<Vector3>();

            if (inputPoints.Count == 0)
            {
                return outputPoints;
            }

            Vector3 prevPoint = inputPoints[inputPoints.Count - 1];
            float prevDist = Vector3.Dot(normal, prevPoint) - constant; // 이전 점이 평면에서 얼마나 떨어져 있는지 (부호 포함)

            for (int i = 0; i < inputPoints.Count; i++)
            {
                Vector3 currentPoint = inputPoints[i];
                float currentDist = Vector3.Dot(normal, currentPoint) - constant; // 현재 점이 평면에서 얼마나 떨어져 있는지

                if (currentDist >= -Epsilon) // 현재 점이 평면의 안쪽에 있거나 경계선에 있으면
                {
                    if (prevDist < -Epsilon) // 이전 점이 평면 바깥에 있었다면
                    {
                        // 교차점을 계산하여 추가
                        float t = prevDist / (prevDist - currentDist);
                        outputPoints.Add(prevPoint + t * (currentPoint - prevPoint));
                    }
                    outputPoints.Add(currentPoint); // 현재 점을 추가
                }
                else if (prevDist >= -Epsilon) // 현재 점은 바깥에 있고 이전 점은 안쪽에 있었다면
                {
                    // 교차점을 계산하여 추가
                    float t = prevDist / (prevDist - currentDist);
                    outputPoints.Add(prevPoint + t * (currentPoint - prevPoint));
                }

                prevPoint = currentPoint;
                prevDist = currentDist;
            }
            return outputPoints;
        }

        /// <summary>
        /// 두 삼각형의 교차 영역을 계산.
        /// </summary>
        private static IntersectionResult3 GetCoplanarIntersection(Triangle3f t0, Triangle3f t1)
        {
            var result = new IntersectionResult3();

            // 1. 투영 평면 선택: 가장 큰 노멀 성분을 기준으로 투영 축을 결정
            Vector3 N = t0.Normal;
            int ignoreAxis = 0;
            float absX = Math.Abs(N.X);
            float absY = Math.Abs(N.Y);
            if (absY > absX) { ignoreAxis = 1; absX = absY; }
            if (Math.Abs(N.Z) > absX) { ignoreAxis = 2; }

            // 2. 3D 삼각형을 2D 평면으로 투영
            Triangle2 projT0 = ProjectTo2D(t0, ignoreAxis);
            Triangle2 projT1 = ProjectTo2D(t1, ignoreAxis);

            // 3. 2D 삼각형 교차 계산
            var tr2tr2Result = IntersectTriangle2Triangle2.GetIntersectionResult(projT0, projT1);

            // 4. 교차 결과가 없으면 종료
            if (tr2tr2Result.status == IntersectionResult.NOTINTERSECT)
            {
                result.status = IntersectionResult.NOTINTERSECT;
                return result;
            }

            // 5. 2D 교차점을 3D로 다시 변환
            result.status = IntersectionResult.INTERSECT;
            result.type = tr2tr2Result.type;
            result.quantity = tr2tr2Result.quantity;
            result.points = new List<Vector3>();

            float constant = t0.Plane.distance;

            for (int i = 0; i < tr2tr2Result.quantity; i++)
            {
                Vector2 p2D = tr2tr2Result.points[i];
                Vector3 p3D = Vector3.Zero;

                switch (ignoreAxis)
                {
                    case 0: // X-축 무시 (YZ 평면)
                        p3D.Y = p2D.X;
                        p3D.Z = p2D.Y;
                        if (Math.Abs(N.X) > MathUtil.Epsilon)
                        {
                            p3D.X = (constant - (N.Y * p3D.Y) - (N.Z * p3D.Z)) / N.X;
                        }
                        break;
                    case 1: // Y-축 무시 (ZX 평면)
                        p3D.Z = p2D.X;
                        p3D.X = p2D.Y;
                        if (Math.Abs(N.Y) > MathUtil.Epsilon)
                        {
                            p3D.Y = (constant - (N.Z * p3D.Z) - (N.X * p3D.X)) / N.Y;
                        }
                        break;
                    case 2: // Z-축 무시 (XY 평면)
                    default:
                        p3D.X = p2D.X;
                        p3D.Y = p2D.Y;
                        if (Math.Abs(N.Z) > MathUtil.Epsilon)
                        {
                            p3D.Z = (constant - (N.X * p3D.X) - (N.Y * p3D.Y)) / N.Z;
                        }
                        break;
                }
                result.points.Add(p3D);
            }
            return result;
        }

        /// <summary>
        /// 3차원 삼각형을 주어진 축을 무시하고 2차원 평면에 투영.
        /// </summary>
        private static Triangle2 ProjectTo2D(Triangle3f tri, int ignoreAxis)
        {
            var projectedTri = new Triangle2();
            for (int i = 0; i < 3; i++)
            {
                switch (ignoreAxis)
                {
                    case 0: projectedTri[i] = tri[i].Yz(); break; // YZ 평면으로 투영
                    case 1: projectedTri[i] = tri[i].Zx(); break; // ZX 평면으로 투영
                    case 2: default: projectedTri[i] = tri[i].Xy(); break; // XY 평면으로 투영
                }
            }
            return projectedTri;
        }

        /// <summary>
        /// 삼각형을 구성하는 점이 plane 기준 어느쪽에 놓이는지 확인
        /// </summary>
        private static void QueryTriangle3Position(Triangle3f triangle, Plane3f plane,
            out float[] distance, out int[] sign,
            out int positive, out int negative, out int zero)
        {
            positive = 0;
            negative = 0;
            zero = 0;
            distance = new float[3];
            sign = new int[3];

            for (int i = 0; i < 3; ++i)
            {
                distance[i] = plane.GetDistanceToPoint(triangle[i]); // 점과 평면 사이의 거리 계산
                if (distance[i] > Epsilon)
                {
                    sign[i] = 1; positive++; // 평면의 양수 쪽에 위치
                }
                else if (distance[i] < -Epsilon)
                {
                    sign[i] = -1; negative++; // 평면의 음수 쪽에 위치
                }
                else
                {
                    distance[i] = 0;
                    sign[i] = 0; // 평면 위에 위치
                    zero++;
                }
            }
        }
    }
}