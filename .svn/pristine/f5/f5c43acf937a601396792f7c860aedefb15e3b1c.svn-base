using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public abstract class DIstanceCalculator<T1, T2, R>
    {
        public float distance { get; set; }

        protected T1 Geometry1;
        protected T2 Geometry2;

        public DIstanceCalculator(T1 geo1, T2 geo2)
        {
            Geometry1 = geo1;
            Geometry2 = geo2;
        }

        public abstract R Calculate();
    }

    public class DistancePoint2Segment : DIstanceCalculator<Vector2, Segment2f, Result2f>
    {
        public DistancePoint2Segment(Vector2 point, Segment2f segment) : base(point, segment) { }
        public override Result2f Calculate()
        {
            Vector2 point = Geometry1;
            Vector2 segmentStart = Geometry2.P0;
            Vector2 segmentEnd = Geometry2.P1;

            Vector2 segmentDirection = segmentEnd - segmentStart;
            float segmentLengthSquared = segmentDirection.LengthSquared;
            Result2f result = new();

            if (segmentLengthSquared == 0)
            {
                // 선분의 길이가 0인 경우, 점과 선분의 시작점 사이의 거리 반환
                result.closest = new Vector2[1] { segmentStart };
                result.distance = Vector2.Distance(point, segmentStart);
                return result;
            }

            Vector2 pointToStart = point - segmentStart;
            float projection = Vector2.Dot(pointToStart, segmentDirection) / segmentLengthSquared;

            if (projection < 0)
            {
                // 투영이 선분의 시작점보다 앞에 있는 경우, 선분의 시작점까지의 거리 반환
                result.closest = new Vector2[1] { segmentStart };
                result.distance = Vector2.Distance(point, segmentStart);
                return result;
            }
            if (projection > 1)
            {
                // 투영이 선분의 끝점보다 뒤에 있는 경우, 선분의 끝점까지의 거리 반환
                result.closest = new Vector2[1] { segmentEnd };
                result.distance = Vector2.Distance(point, segmentEnd);
                return result;
            }

            // 투영이 선분에 있는 경우, 투영된 점과 점 사이의 거리 반환
            Vector2 closestPoint = segmentStart + (segmentDirection * projection);
            result.closest = new Vector2[2] { point, closestPoint };
            result.distance = Vector2.Distance(point, closestPoint);
            return result;
        }
    }

    // 원과 점 간의 거리 계산(원 밖에 있는 경우 +, 내부에 있으면 -)
    // 원과 점 간의 거리 계산(원 밖에 있는 경우 +, 내부에 있으면 -)
    public class DistancePoint2Circle : DIstanceCalculator<Vector2, Circle2, Result2f>
    {
        public DistancePoint2Circle(Vector2 point, Circle2 circle) : base(point, circle) { }

        public override Result2f Calculate()
        {
            Vector2 point = Geometry1;    // 점의 위치
            Circle2 circle = Geometry2;  // 원의 정보

            // 원의 중심 좌표
            Vector2 center = new(circle.Center.X, circle.Center.Y);

            // 점과 원의 중심 사이의 거리
            float distanceToCenter = Vector2.Distance(point, center);

            Result2f result = new();

            if (distanceToCenter <= circle.Radius)
            {
                // 점이 원 내부에 있는 경우
                result.closest = new Vector2[1] { point }; // 점 자체가 가장 가까운 점
                result.distance = -distanceToCenter; // 거리를 음수로 표현
            }
            else
            {
                // 점이 원 외부에 있는 경우
                // 원의 경계와 점과의 최단 거리 지점 계산
                Vector2 direction = Vector2.Normalize(point - center); // 원의 중심에서 점을 향하는 방향 벡터
                Vector2 closestPointOnCircle = center + (direction * circle.Radius); // 원의 경계와 점과의 최단 거리 지점

                result.closest = new Vector2[2] { point, closestPointOnCircle };
                result.distance = distanceToCenter - circle.Radius; // 거리 계산
            }

            return result;
        }
    }

    // AABB와 점 간의 거리 계산
    public class DistancePoint2AABB : DIstanceCalculator<Vector2, AABB2, Result2f>
    {
        public DistancePoint2AABB(Vector2 point, AABB2 aabb) : base(point, aabb) { }

        public override Result2f Calculate()
        {
            Vector2 point = Geometry1;  // 점의 위치
            AABB2 aabb = Geometry2;     // AABB의 정보

            // AABB의 경계 좌표
            Vector2 min = new(aabb.Min.X, aabb.Min.Y);
            Vector2 max = new(aabb.Max.X, aabb.Max.Y);

            // 점과 AssemblyAABB 사이의 거리 계산
            float dx = Math.Max(Math.Max(min.X - point.X, 0), point.X - max.X);
            float dy = Math.Max(Math.Max(min.Y - point.Y, 0), point.Y - max.Y);
            float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));

            Result2f result = new();

            // 점이 AssemblyAABB 내부에 있는 경우
            if (point.X >= min.X && point.X <= max.X &&
                point.Y >= min.Y && point.Y <= max.Y)
            {
                // AABB의 면에 투영된 점 계산
                Vector2 projectedPoint = new(point.X, point.Y);

                // x축 방향 투영
                if (point.X < min.X)
                {
                    projectedPoint.X = min.X;
                }
                else if (point.X > max.X)
                {
                    projectedPoint.X = max.X;
                }

                // y축 방향 투영
                if (point.Y < min.Y)
                {
                    projectedPoint.Y = min.Y;
                }
                else if (point.Y > max.Y)
                {
                    projectedPoint.Y = max.Y;
                }

                result.closest = new Vector2[2] { point, projectedPoint };
                // 내부에 있는 경우 거리를 음수로 표현
                result.distance = -distance;
            }
            else
            {
                // AssemblyAABB 외부에 있는 경우, AABB의 경계와의 최단 거리 지점 계산
                Vector2 closestPoint = new(
                    MathHelper.Clamp(point.X, min.X, max.X),
                    MathHelper.Clamp(point.Y, min.Y, max.Y)
                );

                result.closest = new Vector2[1] { closestPoint };
                result.distance = distance;
            }

            return result;
        }
    }

    public class DistanceSegment2Seg2 : DIstanceCalculator<Segment2f, Segment2f, Result2f>
    {
        public DistanceSegment2Seg2(Segment2f segment1, Segment2f segment2) : base(segment1, segment2) { }

        public override Result2f Calculate()
        {
            Segment2f segment1 = Geometry1;
            Segment2f segment2 = Geometry2;

            Vector2 closest1, closest2;
            Result2f result = new();

            // 두 세그먼트의 방향 벡터
            Vector2 dir1 = segment1.P1 - segment1.P0;
            Vector2 dir2 = segment2.P1 - segment2.P0;

            // 두 세그먼트의 시작점
            Vector2 start1 = segment1.P0;
            Vector2 start2 = segment2.P0;

            // 두 세그먼트의 길이 제곱
            float lengthSquared1 = dir1.LengthSquared;
            float lengthSquared2 = dir2.LengthSquared;

            // 두 세그먼트의 방향 벡터들 간의 내적
            float dot = Vector2.Dot(dir1, dir2);

            // 두 세그먼트가 평행한 경우
            if (Math.Abs(1 - Math.Abs(dot * dot / (lengthSquared1 * lengthSquared2))) < float.Epsilon)
            {
                // 두 세그먼트의 시작점 사이의 거리 계산
                float distanceStarts = Vector2.Distance(start1, start2);

                // 세그먼트1에서 세그먼트2로의 수직 거리 계산
                float projection1 = Vector2.Dot(start2 - start1, dir1) / lengthSquared1;
                projection1 = Math.Max(0, Math.Min(1, projection1)); // 세그먼트1 내부에 투영
                closest1 = start1 + (projection1 * dir1);
                float distanceParallel = Vector2.Distance(start2, closest1);

                // 세그먼트2에서 세그먼트1로의 수직 거리 계산
                float projection2 = Vector2.Dot(start1 - start2, dir2) / lengthSquared2;
                projection2 = Math.Max(0, Math.Min(1, projection2)); // 세그먼트2 내부에 투영
                closest2 = start2 + (projection2 * dir2);

                // 더 짧은 거리를 선택하여 결과 저장
                if (distanceStarts <= distanceParallel)
                {
                    result.closest = new Vector2[] { start1, start2 };
                    result.distance = distanceStarts;
                }
                else
                {
                    result.closest = new Vector2[] { closest1, closest2 };
                    result.distance = distanceParallel;
                }
            }
            else
            {
                // 두 세그먼트가 평행하지 않고, 교차하지 않는 경우
                // 두 세그먼트의 끝점과 시작점 사이의 거리를 계산하여 가장 짧은 거리를 저장
                float[] distances = new float[]
                {
                    Vector2.Distance(start1, start2),
                    Vector2.Distance(start1, segment2.P1),
                    Vector2.Distance(segment1.P1, start2),
                    Vector2.Distance(segment1.P1, segment2.P1)
                };

                float minDistance = distances.Min();
                int minIndex = Array.IndexOf(distances, minDistance);

                switch (minIndex)
                {
                    case 0:
                        result.closest = new Vector2[] { start1, start2 };
                        result.distance = distances[0];
                        break;
                    case 1:
                        result.closest = new Vector2[] { start1, segment2.P1 };
                        result.distance = distances[1];
                        break;
                    case 2:
                        result.closest = new Vector2[] { segment1.P1, start2 };
                        result.distance = distances[2];
                        break;
                    case 3:
                        result.closest = new Vector2[] { segment1.P1, segment2.P1 };
                        result.distance = distances[3];
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
    }

    public class DistancePoint2Polygon : DIstanceCalculator<Vector2, List<Vector2>, Result2f>
    {
        public DistancePoint2Polygon(Vector2 point, List<Vector2> polygon) : base(point, polygon) { }

        public override Result2f Calculate()
        {
            List<Vector2> polygon = Geometry2;
            Vector2 point = Geometry1;

            // 다각형 정점 개수가 3 이하인 경우 간단히 처리
            if (polygon.Count <= 3)
            {
                return CalculateNaive(point, polygon);
            }

            // 다각형을 분할하여 왼쪽과 오른쪽 영역으로 나눔
            int mid = polygon.Count / 2;
            List<Vector2> leftPolygon = polygon.GetRange(0, mid);
            List<Vector2> rightPolygon = polygon.GetRange(mid, polygon.Count - mid);

            // 왼쪽과 오른쪽 영역에서 최단 거리 계산
            Result2f leftResult = new DistancePoint2Polygon(point, leftPolygon).Calculate();
            Result2f rightResult = new DistancePoint2Polygon(point, rightPolygon).Calculate();

            // 왼쪽과 오른쪽 영역에서 계산된 최단 거리 중 최소 값을 선택
            Result2f minResult = leftResult.distance < rightResult.distance ? leftResult : rightResult;

            // 분할선 근처의 점들을 대상으로 추가 계산
            Result2f stripResult = CalculateStrip(point, polygon, minResult.distance);

            // 최종 최단 거리 결정
            return stripResult.distance < minResult.distance ? stripResult : minResult;
        }

        // 다각형 정점 개수가 3 이하인 경우 간단한 방법으로 최단 거리 계산
        private Result2f CalculateNaive(Vector2 point, List<Vector2> polygon)
        {
            Result2f result = new();
            float minDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.Zero;

            foreach (Vector2 vertex in polygon)
            {
                float distance = Vector2.Distance(point, vertex);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = vertex;
                }
            }

            result.closest = new Vector2[] { closestPoint };
            result.distance = minDistance;
            return result;
        }

        // 분할선 근처의 점들을 대상으로 최단 거리 계산
        private Result2f CalculateStrip(Vector2 point, List<Vector2> polygon, float minDistance)
        {
            Result2f result = new();
            List<Vector2> strip = new();

            // 다각형 내에서 수직 거리가 minDistance 이내에 있는 점들을 strip에 추가
            foreach (Vector2 vertex in polygon)
            {
                if (Math.Abs(vertex.X - point.X) < minDistance)
                {
                    strip.Add(vertex);
                }
            }

            // strip 내 점들에 대해 최단 거리 계산
            for (int i = 0; i < strip.Count; i++)
            {
                for (int j = i + 1; j < strip.Count && strip[j].Y - strip[i].Y < minDistance; j++)
                {
                    float distance = Vector2.Distance(point, strip[j]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result.closest = new Vector2[] { strip[i], strip[j] };
                        result.distance = distance;
                    }
                }
            }
            return result;
        }
    }

    public class DistancePolygon2Polygon : DIstanceCalculator<List<Vector2>, List<Vector2>, Result2f>
    {
        public DistancePolygon2Polygon(List<Vector2> polygon1, List<Vector2> polygon2) : base(polygon1, polygon2) { }

        public override Result2f Calculate()
        {
            return CalculateClosestPoints(Geometry1, Geometry2);
        }

        private Result2f CalculateClosestPoints(List<Vector2> polygon1, List<Vector2> polygon2)
        {
            // Initialize result
            Result2f result = new();

            // If either polygon is empty, return an invalid result
            if (polygon1.Count == 0 || polygon2.Count == 0)
            {
                return result;
            }

            // Calculate closest ControlPoints between the two contour_vertex_indices
            CalculateClosestPointsRecursive(polygon1, polygon2, ref result);

            return result;
        }

        private void CalculateClosestPointsRecursive(List<Vector2> polygon1, List<Vector2> polygon2, ref Result2f result)
        {
            // 3점 이하이면 막 계산
            if (polygon1.Count <= 3 || polygon2.Count <= 3)
            {
                foreach (Vector2 point1 in polygon1)
                {
                    foreach (Vector2 point2 in polygon2)
                    {
                        float distance = Vector2.Distance(point1, point2);

                        if (distance < result.distance)
                        {
                            result.distance = distance;
                            result.closest = new Vector2[2] { point1, point2 };
                        }
                    }
                }
                return;
            }

            // Divide contour_vertex_indices 
            List<Vector2> left1, right1, left2, right2;
            SplitPolygons(polygon1, polygon2, out left1, out right1, out left2, out right2);

            // 재귀 호출
            CalculateClosestPointsRecursive(left1, left2, ref result);
            CalculateClosestPointsRecursive(right1, right2, ref result);
        }

        private void SplitPolygons(List<Vector2> polygon1, List<Vector2> polygon2,
                                   out List<Vector2> left1, out List<Vector2> right1,
                                   out List<Vector2> left2, out List<Vector2> right2)
        {
            // Split both contour_vertex_indices into left and right halves along their longest dimension
            left1 = new List<Vector2>();
            right1 = new List<Vector2>();
            left2 = new List<Vector2>();
            right2 = new List<Vector2>();

            // Find the longest dimension of polygon1 and polygon2
            Vector2 min1 = FindMinPoint(polygon1);
            Vector2 max1 = FindMaxPoint(polygon1);
            Vector2 min2 = FindMinPoint(polygon2);
            Vector2 max2 = FindMaxPoint(polygon2);

            // Determine the splitting line
            Vector2 direction1 = max1 - min1;
            Vector2 direction2 = max2 - min2;

            // Split polygon1
            foreach (Vector2 point in polygon1)
            {
                if (Vector2.Dot(point - min1, direction1) < Vector2.Dot(direction1, direction1) / 2)
                {
                    left1.Add(point);
                }
                else
                {
                    right1.Add(point);
                }
            }

            // Split polygon2
            foreach (Vector2 point in polygon2)
            {
                if (Vector2.Dot(point - min2, direction2) < Vector2.Dot(direction2, direction2) / 2)
                {
                    left2.Add(point);
                }
                else
                {
                    right2.Add(point);
                }
            }
        }

        private Vector2 FindMinPoint(List<Vector2> polygon)
        {
            Vector2 minPoint = polygon[0];
            foreach (Vector2 point in polygon)
            {
                if (point.X < minPoint.X || (point.X == minPoint.X && point.Y < minPoint.Y))
                {
                    minPoint = point;
                }
            }
            return minPoint;
        }

        private Vector2 FindMaxPoint(List<Vector2> polygon)
        {
            Vector2 maxPoint = polygon[0];
            foreach (Vector2 point in polygon)
            {
                if (point.X > maxPoint.X || (point.X == maxPoint.X && point.Y > maxPoint.Y))
                {
                    maxPoint = point;
                }
            }
            return maxPoint;
        }
    }
}
