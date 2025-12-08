using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using System;

namespace IGX.Geometry.ConvexHull
{
    // https://www.geometrictools.com/Documentation/MinimumVolumeBox.pdf
    public class MinimumVolumeBox
    {
        public OOBB3 Oobb;

        public OOBB3 ComputeFlatBox(List<Vector3> points, Vector3 FaceNormal, float epsilon = float.Epsilon)
        {
            if (FaceNormal == Vector3.Zero)
            { // 삼각형이 아님, 직선
                return OOBB3.Empty;
            }
            // 나머지 두 축을 생성
            Vector3 U = Vector3.Zero;
            Vector3 V = Vector3.Zero;

            FaceNormal = FaceNormal.MakeUVnormalsFromW(out U, out V);

            // 3차원 점들을 삼각형이 놓인 평면에 투영

            Vector3[] points3 = new Vector3[points.Count];
            Vector2[] points2 = new Vector2[points.Count];
            for (int k = 0; k < points.Count; k++)
            {
                Vector3 diff = points[k] - points[0];
                points2[k] = new Vector2(Vector3.Dot(U, diff), Vector3.Dot(V, diff));
                points3[k] = new Vector3(Vector3.Dot(U, diff), Vector3.Dot(V, diff), 0f);
            }

            // Rotating Calipers 알고리즘에 의거 2d projection polygn을 둘러싸는 최소 박스를 계산
            MinimumAreaBox box2 = new(points2.ToList());
            //box2.Compute(points2.ToList());

            OOBB3 obb = OOBB3.Empty;
            // 계산 결과를 3D로 환원
            obb.extent[0] = box2.MinBox.extent[0] + epsilon;
            obb.extent[1] = box2.MinBox.extent[1] + epsilon;
            obb.extent[2] = epsilon;
            obb.axisX = (box2.MinBox.Axis(0).X * U) + (box2.MinBox.Axis(0).Y * V);
            obb.axisY = (box2.MinBox.Axis(1).X * U) + (box2.MinBox.Axis(1).Y * V);
            obb.axisZ = FaceNormal;
            obb.center = points[0] + (box2.MinBox.center.X * U) + (box2.MinBox.center.Y * V);

            return obb;
        }

        struct EdgeKey
        {
            public int v0;
            public int v1;
            public EdgeKey(int start, int end)
            {
                v0 = start;
                v1 = end;
            }
        }

        public MinimumVolumeBox(List<Vector3> inputpoints, float epsilon = 0f)
        {
            QuickHull3D hull = new();
            List<Vector3> hullVertices = new(); // output : convex hull을 구성하는 정점 리스트
            List<int> triangleindices = new(); // output : convexhull을 구성하는 삼각형들(각 삼각형에 데해 세 정점들의 index)
            List<Vector3> pointsnormals = new();
            bool isnotempty = hull.Compute(inputpoints, false, ref hullVertices, ref triangleindices, ref pointsnormals);

            if (!isnotempty)
            {
                return;
            }

            Vector3[] points3 = new Vector3[hullVertices.Count]; // 2D에 투영된 points의 좌표

            int hullQuantity = hullVertices.Count;
            float minVolume = float.MaxValue;

            Vector3 minp = Vector3.PositiveInfinity;
            Vector3 maxp = Vector3.NegativeInfinity;

            HashSet<Plane3f> planes = new();
            Dictionary<Plane3f, (OOBB3, AABB3)> obbs = new();

            AABB3 test = AABB3.Empty;

            for (int id = 0; id < triangleindices.Count; id += 3)
            {
                int v0 = triangleindices[id];
                int v1 = triangleindices[id + 1];
                int v2 = triangleindices[id + 2];

                Plane3f plane = new(hullVertices[v0], hullVertices[v1], hullVertices[v2]);
                if (planes.Contains(plane) || plane.normal == Vector3.Zero)
                {
                    continue;
                }

                planes.Add(plane);
                Vector3 axisW = plane.normal;
                Vector3 origin = hullVertices[v0];// plane.V0id;
                Vector3 axisU0 = (hullVertices[v1] - hullVertices[v0]).Normalized();
                Vector3 axisV0 = axisW.Cross(axisU0);

                List<Vector3> projected3D = new();
                List<float> heights = new();

                foreach (Vector3 p in hullVertices)
                {
                    Vector3 v = p - origin;
                    float d = Vector3.Dot(v, axisW);
                    Vector3 projected = p - (d * axisW);

                    Vector3 diff = projected - origin;
                    projected3D.Add(new Vector3(Vector3.Dot(diff, axisU0), Vector3.Dot(diff, axisV0), 0));
                    heights.Add(d);
                }
                float minH = heights.Min();
                float maxH = heights.Max();
                float height = maxH - minH;

                List<Vector2> point2d = projected3D
                    .Select(p => new Vector2((float)Math.Round(p.X, 6), (float)Math.Round(p.Y, 6))) // 부동소수 정리
                    .Distinct()
                    .OrderBy(p => p.X)
                    .ThenBy(p => p.Y)
                    .ToList();

                OOBB2 box2 = ComputeMinimumAreaBoxSafe(point2d);
                float volume = box2.Area * height;

                Matrix3 invm = new Matrix3(axisU0, axisV0, axisW).Inverted();

                if (volume <= 0)
                {
                    planes.Remove(plane);
                    continue;
                }

                if (volume < minVolume)
                {
                    // 중심 복원: 평면상 중심 + 높이 중간값
                    Vector3 baseCenter = new(box2.center.X, box2.center.Y, (minH + maxH) / 2f);
                    Vector3 center = (invm * baseCenter) + origin;
                    List<Vector3> p3 = projected3D.Select(p => (invm * p) + origin).ToList();
                    p3.AddRange(projected3D.Select(p => (invm * p) + origin).ToList());

                    minVolume = volume;
                    Oobb = OOBB3.Empty;
                    Oobb.center = center;
                    Oobb.axisX = (invm * new Vector3(box2.axisX.X, box2.axisX.Y, 0)).Normalized();
                    Oobb.axisY = (invm * new Vector3(box2.axisY.X, box2.axisY.Y, 0)).Normalized();
                    Oobb.axisZ = axisW;
                    Oobb.extent = new Vector3(box2.extent.X, box2.extent.Y, height / 2);

                    test = AABB3.Empty;
                    foreach (Vector3 t in Oobb.Vertices())
                    {
                        test.Contain(t);
                    }

                    obbs[plane] = (Oobb, test);
                }
            }
            Oobb.AxisOrder();
        }
        public OOBB2 ComputeMinimumAreaBoxSafe(List<Vector2> inputPoints, float tolerance = 1e-5f)
        {
            // 1. 중복 제거 및 정렬
            List<Vector2> distinctPoints = inputPoints
                .Distinct()
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToList();

            // 2. 점 개수가 너무 적은 경우 방어
            if (distinctPoints.Count < 2)
            {
                return new OOBB2
                {
                    center = distinctPoints.FirstOrDefault(),
                    axisX = Vector2.UnitX,
                    axisY = Vector2.UnitY,
                    extent = Vector2.Zero
                };
            }

            // 3. Convex Hull 계산
            List<Vector2> hull = ConvexHull2D(distinctPoints);
            // 4. Rotating Calipers 적용
            return MinAreaBoxOfConvexPolygon(hull.ToList());
        }

        public static List<Vector2> ConvexHull2D(List<Vector2> points, float tolerance = 1e-6f)
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

        // 5. 내부 회전 캘리퍼 알고리즘 
        private static OOBB2 MinAreaBoxOfConvexPolygon(List<Vector2> points)
        {
            int n = points.Count;
            OOBB2 minBox = new() { extent = new Vector2(float.MaxValue, float.MaxValue) };
            float minArea = float.MaxValue;

            for (int i = 0; i < n; ++i)
            {
                Vector2 origin = points[i];
                Vector2 edge = points[(i + 1) % n] - points[i];
                Vector2 axisX = Vector2.Normalize(edge);
                Vector2 axisY = -axisX.Perpendicular();

                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;

                foreach (Vector2 p in points)
                {
                    Vector2 d = p - origin;
                    float projX = Vector2.Dot(d, axisX);
                    float projY = Vector2.Dot(d, axisY);

                    minX = MathF.Min(minX, projX);
                    maxX = MathF.Max(maxX, projX);
                    minY = MathF.Min(minY, projY);
                    maxY = MathF.Max(maxY, projY);
                }

                float extentX = (maxX - minX) * 0.5f;
                float extentY = (maxY - minY) * 0.5f;
                float area = extentX * 2 * extentY * 2;

                if (area < minArea)
                {
                    minArea = area;
                    minBox.axisX = axisX;
                    minBox.axisY = axisY;
                    minBox.extent = new Vector2(extentX, extentY);
                    minBox.center = origin + (axisX * (minX + extentX)) + (axisY * (minY + extentY));
                }
            }

            return minBox;
        }
    }
}
