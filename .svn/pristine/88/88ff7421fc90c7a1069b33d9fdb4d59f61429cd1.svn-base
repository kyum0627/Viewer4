using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    /// <summary>
    /// 2차원 삼각형 간의 교차를 테스트하고 교차 영역을 계산하는 유틸리티.
    /// 이 클래스는 분리 축 정리(SAT)와 Sutherland-Hodgman 클리핑 알고리즘을 사용.
    /// </summary>
    public static class IntersectTriangle2Triangle2
    {
        private const float Epsilon = 1e-6f;

        /// <summary>
        /// 두 삼각형이 겹치는지 여부를 분리 축 정리(SAT)를 사용하여 빠르게 판별.
        /// </summary>
        /// <param name="t0">첫 번째 삼각형</param>
        /// <param name="t1">두 번째 삼각형</param>
        /// <returns>두 삼각형이 교차하면 true, 아니면 false를 반환.</returns>
        public static bool Test(Triangle2 t0, Triangle2 t1)
        {
            Vector2 direction = Vector2.Zero;

            // t0의 변들을 분리 축으로 사용하여 t1과의 분리 여부를 검사.
            for (int i0 = 0, i1 = 2; i0 < 3; i1 = i0++)
            {
                // 변에 수직인 방향 벡터(perpendicular vector)를 계산.
                direction.X = t0[i0].Y - t0[i1].Y;
                direction.Y = t0[i1].X - t0[i0].X;

                // t1의 모든 정점이 t0의 변에 의해 한쪽으로 분리되었는지 확인.
                if (WhichSide(t1, t0[i1], direction) > 0)
                {
                    // 완전히 분리되었다면 교차하지 않습니다.
                    return false;
                }
            }

            // t1의 변들을 분리 축으로 사용하여 t0과의 분리 여부를 검사.
            for (int i0 = 0, i1 = 2; i0 < 3; i1 = i0++)
            {
                // 변에 수직인 방향 벡터를 계산.
                direction.X = t1[i0].Y - t1[i1].Y;
                direction.Y = t1[i1].X - t1[i0].X;

                // t0의 모든 정점이 t1의 변에 의해 한쪽으로 분리되었는지 확인.
                if (WhichSide(t0, t1[i1], direction) > 0)
                {
                    // 완전히 분리되었다면 교차하지 않습니다.
                    return false;
                }
            }

            // 모든 분리 축 테스트를 통과하면 교차.
            return true;
        }

        /// <summary>
        /// 두 삼각형의 교차 영역을 계산.
        /// 교차하지 않는다면 IntersectionResult.NOTINTERSECT 상태를 반환.
        /// </summary>
        /// <param name="t0">첫 번째 삼각형</param>
        /// <param name="t1">두 번째 삼각형</param>
        /// <returns>교차 결과를 담은 IntersectionResult2 객체를 반환.</returns>
        public static IntersectionResult2 GetIntersectionResult(Triangle2 t0, Triangle2 t1)
        {
            var result = new IntersectionResult2();
            if (!Test(t0, t1))
            {
                result.status = IntersectionResult.NOTINTERSECT;
                return result;
            }

            // t1을 t0의 세 변으로 클리핑하여 교차 다각형을 구함.
            List<Vector2> intersectionPoints = new List<Vector2> { t1[0], t1[1], t1[2] };

            for (int i0 = 0, i1 = 2; i0 < 3; i1 = i0++)
            {
                // 클리핑 선의 법선과 상수를 계산.
                Vector2 normal = new(t0[i1].Y - t0[i0].Y, t0[i0].X - t0[i1].X);
                float constant = Vector2.Dot(normal, t0[i1]);

                intersectionPoints = ClipConvexPolygonAgainstLine(intersectionPoints, normal, constant);

                if (intersectionPoints.Count == 0)
                {
                    // 클리핑 후 교차점이 없으면 빈 결과를 반환.
                    result.status = IntersectionResult.NOTINTERSECT;
                    result.type = IntersectionType.EMPTY;
                    return result;
                }
            }

            // 최종 교차 영역의 정점 개수에 따라 교차 유형을 설정.
            result.status = IntersectionResult.INTERSECT;
            result.quantity = intersectionPoints.Count;
            result.points = intersectionPoints;

            if (intersectionPoints.Count == 1)
            {
                result.type = IntersectionType.POINT;
            }
            else if (intersectionPoints.Count == 2)
            {
                result.type = IntersectionType.SEGMENT;
            }
            else
            {
                result.type = IntersectionType.POLYGON;
            }

            return result;
        }

        /// <summary>
        /// 주어진 다각형의 모든 정점이 P를 지나는 선(방향 D)의 한쪽에 있는지 판별.
        /// 부동 소수점 오차를 고려.
        /// </summary>
        /// <param name="triangle">판별할 삼각형</param>
        /// <param name="pointOnLine">선 위의 한 점</param>
        /// <param name="direction">선의 방향 벡터</param>
        /// <returns>
        /// 모든 점이 양의 쪽에 있으면 1, 음의 쪽에 있으면 -1, 양쪽 모두에 있거나 선 위에 있으면 0을 반환.
        /// </returns>
        private static int WhichSide(Triangle2 triangle, Vector2 pointOnLine, Vector2 direction)
        {
            int positiveCount = 0;
            int negativeCount = 0;

            for (int i = 0; i < 3; ++i)
            {
                float dotResult = Vector2.Dot(direction, triangle[i] - pointOnLine);

                if (dotResult > Epsilon)
                {
                    positiveCount++;
                }
                else if (dotResult < -Epsilon)
                {
                    negativeCount++;
                }

                // 이미 양/음이 섞였다면 즉시 0을 반환하여 분리되지 않았음.
                if (positiveCount > 0 && negativeCount > 0)
                {
                    return 0;
                }
            }

            // 모든 점이 한쪽에 있거나 선 위에 있음.
            return positiveCount > 0 ? 1 : (negativeCount > 0 ? -1 : 0);
        }

        /// <summary>
        /// 볼록 다각형을 주어진 선에 대해 클리핑하여 새로운 볼록 다각형을 반환.
        /// Sutherland-Hodgman 클리핑 알고리즘을 사용.
        /// </summary>
        /// <param name="inputPoints">클리핑할 다각형의 정점 리스트</param>
        /// <param name="normal">클리핑 선의 법선 벡터</param>
        /// <param name="constant">클리핑 선의 상수</param>
        /// <returns>클리핑된 새로운 다각형의 정점 리스트</returns>
        private static List<Vector2> ClipConvexPolygonAgainstLine(List<Vector2> inputPoints, Vector2 normal, float constant)
        {
            if (inputPoints.Count == 0)
            {
                return new List<Vector2>();
            }

            var outputPoints = new List<Vector2>();
            Vector2 p1 = inputPoints[inputPoints.Count - 1]; // 마지막 점을 첫 번째 점으로 설정

            for (int i = 0; i < inputPoints.Count; i++)
            {
                Vector2 p2 = inputPoints[i];
                float d1 = Vector2.Dot(normal, p1) - constant;
                float d2 = Vector2.Dot(normal, p2) - constant;

                if (d2 >= -Epsilon) // p2가 클리핑 선의 안쪽에 있는 경우 (경계 포함)
                {
                    if (d1 < -Epsilon) // p1은 바깥쪽에 있고, p2는 안쪽에 있는 경우
                    {
                        // 교차점을 계산하여 추가.
                        float t = d1 / (d1 - d2);
                        outputPoints.Add(p1 + t * (p2 - p1));
                    }
                    outputPoints.Add(p2); // p2를 추가.
                }
                else if (d1 >= -Epsilon) // p2는 바깥쪽에 있고, p1은 안쪽에 있는 경우
                {
                    // 교차점을 계산하여 추가.
                    float t = d1 / (d1 - d2);
                    outputPoints.Add(p1 + t * (p2 - p1));
                }
                p1 = p2; // 다음 반복을 위해 현재 점을 이전 점으로 설정.
            }
            return outputPoints;
        }
    }
}