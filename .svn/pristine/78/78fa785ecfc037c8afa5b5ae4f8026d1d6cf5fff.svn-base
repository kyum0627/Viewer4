using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace IGX.Geometry.ConvexHull
{
    public class QuickHull2D
    {
        public List<Vector2> Compute(List<Vector2> points, float tolerance = 1e-6f)
        {
            if (points == null || points.Count < 3)
            {
                return new List<Vector2>(points!);
            }

            // 부동소수점 오차 제거 및 정렬
            List<Vector2> sorted = points
                .Select(p => new Vector2(
                    (float)Math.Round(p.X, 6),
                    (float)Math.Round(p.Y, 6)))
                .Distinct()
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToList();

            int n = sorted.Count;
            List<Vector2> hull = new();

            // Lower Hull
            for (int i = 0; i < n; i++)
            {
                while (hull.Count >= 2 &&
                    Cross(hull[^2], hull[^1], sorted[i]) <= tolerance)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(sorted[i]);
            }

            // Upper Hull
            int lowerCount = hull.Count;
            for (int i = n - 2; i >= 0; i--)
            {
                while (hull.Count > lowerCount &&
                    Cross(hull[^2], hull[^1], sorted[i]) <= tolerance)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(sorted[i]);
            }

            // 마지막 점 중복 제거
            if (hull.Count > 1)
            {
                hull.RemoveAt(hull.Count - 1);
            }

            return hull;
        }

        // 벡터 외적 (CCW 판단용)
        private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        {
            return ((a.X - o.X) * (b.Y - o.Y)) - ((a.Y - o.Y) * (b.X - o.X));
        }

        public List<Vector2> xCompute(List<Vector2> points)
        {
            if (points == null || points.Count < 3)
            {
                throw new ArgumentException("최소 3개의 점이 필요.");
            }

            // 중복된 점 제거 및 정렬
            List<Vector2> uniquePoints = points.Distinct().ToList();
            if (uniquePoints.Count < 3)
            {
                return uniquePoints;
            }

            // 세 점 - 삼각형의 Convex Hull
            if (points.Count == 3)
            {
                return new List<Vector2> { points[0], points[1], points[2] };
            }

            // 좌표를 x축 기준으로 정렬
            uniquePoints.Sort((p1, p2) => p1.X.CompareTo(p2.X));

            Vector2 minX = uniquePoints[0];
            Vector2 maxX = uniquePoints[^1];

            // 왼쪽과 오른쪽으로 분리
            List<Vector2> leftSet = uniquePoints.Where(p => IsLeftOfLine(minX, maxX, p)).ToList();
            List<Vector2> rightSet = uniquePoints.Where(p => IsRightOfLine(minX, maxX, p)).ToList();

            // Convex Hull 계산
            HashSet<Vector2> hull = new()
            {
                minX,
                maxX
            };

            hull.UnionWith(FindHull(minX, maxX, leftSet));
            hull.UnionWith(FindHull(maxX, minX, rightSet));

            return hull.ToList();
        }

        private List<Vector2> FindHull(Vector2 point1, Vector2 point2, List<Vector2> points)
        {
            List<Vector2> hull = new();

            if (points.Count == 0)
            {
                return hull;
            }

            // 가장 먼 점 찾기
            Vector2 farthestPoint = points
                .OrderByDescending(p => DistanceToLine(point1, point2, p))
                .First();

            // 두 개의 삼각형으로 나누기
            List<Vector2> leftOfFarthest = points.Where(p => IsLeftOfLine(point1, farthestPoint, p)).ToList();
            List<Vector2> rightOfFarthest = points.Where(p => IsRightOfLine(farthestPoint, point2, p)).ToList();

            hull.AddRange(FindHull(point1, farthestPoint, leftOfFarthest));
            hull.AddRange(FindHull(farthestPoint, point2, rightOfFarthest));

            return hull;
        }

        private bool IsLeftOfLine(Vector2 point1, Vector2 point2, Vector2 point)
        {
            // 선분 (point1, point2)에서 점 point까지의 거리 계산
            float crossProduct = ((point2.X - point1.X) * (point.Y - point1.Y)) -
                                 ((point2.Y - point1.Y) * (point.X - point1.X));
            return crossProduct > 0;
        }

        private bool IsRightOfLine(Vector2 point1, Vector2 point2, Vector2 point)
        {
            float crossProduct = ((point2.X - point1.X) * (point.Y - point1.Y)) -
                                 ((point2.Y - point1.Y) * (point.X - point1.X));
            return crossProduct < 0;
        }

        private float DistanceToLine(Vector2 point1, Vector2 point2, Vector2 point)
        {
            // 선분 (point1, point2)에서 점 point까지의 거리 계산
            float A = point2.Y - point1.Y;
            float B = point1.X - point2.X;
            float C = (point2.X * point1.Y) - (point1.X * point2.Y);
            return Math.Abs((A * point.X) + (B * point.Y) + C) / (float)Math.Sqrt((A * A) + (B * B));
        }
    }
}
