using System;
using OpenTK.Mathematics;
using System.Linq;
using System.Collections.Generic;
namespace IGX.Geometry.Distance
{
    public class PointPair
    {
        public float Distance; // 두 점 사이의 거리
        public Vector2 Point1; // 첫 번째 점
        public Vector2 Point2; // 두 번째 점

        public PointPair(float distance, Vector2 point1, Vector2 point2)
        {
            Distance = distance;
            Point1 = point1;
            Point2 = point2;
        }
    }

    public class ClosestPair
    {
        private static float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        // 반복문을 사용하여 최단 점 쌍을 찾는 함수
        public static PointPair FindClosestPair(Vector2[] points)
        {
            // 점들을 X좌표 기준으로 정렬
            points = points.OrderBy(p => p.X).ToArray();

            Stack<(int left, int right)> stack = new();
            stack.Push((0, points.Length - 1));

            PointPair closestPair = new(float.MaxValue, default, default);

            while (stack.Count > 0)
            {
                (int left, int right) = stack.Pop();
                if (right - left <= 2) // 점이 2개 또는 3개일 경우 브루트포스로 계산
                {
                    PointPair pair = BruteForceClosestPair(points, left, right);
                    if (pair.Distance < closestPair.Distance)
                    {
                        closestPair = pair;
                    }

                    continue;
                }

                int mid = left + ((right - left) / 2);

                // 왼쪽과 오른쪽 부분을 각각 처리하기 위해 스택에 추가
                stack.Push((left, mid));
                stack.Push((mid + 1, right));

                // 두 영역에서 최소 거리 계산 후, 중간 영역도 고려
                PointPair leftPair = FindClosestPairInRange(points, left, mid);
                PointPair rightPair = FindClosestPairInRange(points, mid + 1, right);

                // 최소 거리 찾기
                PointPair minPair = leftPair.Distance < rightPair.Distance ? leftPair : rightPair;
                PointPair? midPair = FindMidRange(points, left, right, mid, minPair.Distance);
                if (midPair != null)
                {
                    closestPair = minPair.Distance < midPair.Distance ? minPair : midPair;
                }
                else
                {
                    closestPair = minPair;
                }
            }

            return closestPair;
        }

        private static PointPair BruteForceClosestPair(Vector2[] points, int left, int right)
        {
            float min = float.MaxValue;
            Vector2 p1 = default, p2 = default;

            for (int i = left; i < right; i++)
            {
                for (int j = i + 1; j <= right; j++)
                {
                    float dist = Distance(points[i], points[j]);
                    if (dist < min)
                    {
                        min = dist;
                        p1 = points[i];
                        p2 = points[j];
                    }
                }
            }

            return new PointPair(min, p1, p2);
        }

        private static PointPair FindClosestPairInRange(Vector2[] points, int left, int right)
        {
            return BruteForceClosestPair(points, left, right);
        }

        private static PointPair? FindMidRange(Vector2[] points, int left, int right, int mid, float minDistance)
        {
            List<Vector2> strip = points
                .Where(p => Math.Abs(p.X - points[mid].X) < minDistance)
                .OrderBy(p => p.Y)
                .ToList();

            PointPair? closestPair = null;

            for (int i = 0; i < strip.Count; i++)
            {
                for (int j = i + 1; j < strip.Count && strip[j].Y - strip[i].Y < minDistance; j++)
                {
                    float distance = Distance(strip[i], strip[j]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPair = new PointPair(minDistance, strip[i], strip[j]);
                    }
                }
            }

            return closestPair;
        }
    }
}