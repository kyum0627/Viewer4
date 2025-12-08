using System.Linq;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.ConvexHull
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Convex_hull
    /// https://hns17.tistory.com/entry/%EC%95%8C%EA%B3%A0%EB%A6%AC%EC%A6%98-ConvexHull-Grahams-Scan
    /// </summary>
    public class ConvexHull2
    {
        public ConvexHull2() { }
        public static List<Vector2> Compute(List<Vector2> points, float tolerance = 1e-6f)
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
                while (hull.Count >= 2 && Cross(hull[^2], hull[^1], sorted[i]) <= tolerance)
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
    }
}
