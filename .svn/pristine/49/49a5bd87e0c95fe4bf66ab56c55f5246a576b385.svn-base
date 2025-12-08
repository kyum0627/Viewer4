using System;
using System.Collections.Generic;
using System.Linq;
using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.ConvexHull
{
    public class RotatingCaliper2D
    {
        public OOBB2 CalculateOobb2(List<Vector2> convexHull)
        {
            if (convexHull == null || convexHull.Count < 3)
            {
                throw new ArgumentException("볼록 껍질에 최소 3개의 점이 필요.");
            }

            // 볼록 껍질 점들을 시계방향으로 정렬
            List<Vector2> sortedPoints = SortConvexHull(convexHull);

            // OOBB2 계산
            OOBB2 minBox = GetBoundingBox(sortedPoints);
            return minBox;
        }

        private List<Vector2> SortConvexHull(List<Vector2> convexHull)
        {
            Vector2 centroid = CalculateCentroid(convexHull);
            return convexHull.OrderBy(p => Math.Atan2(p.Y - centroid.Y, p.X - centroid.X)).ToList();
        }

        private Vector2 CalculateCentroid(List<Vector2> points)
        {
            float x = points.Average(p => p.X);
            float y = points.Average(p => p.Y);
            return new Vector2((float)x, (float)y);
        }

        private OOBB2 GetBoundingBox(List<Vector2> points)
        {
            OOBB2 minBox = new();
            float minArea = float.MaxValue;
            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                // 벡터 A, B를 선분의 방향으로 설정
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % n];
                Vector2 edge = p2 - p1;
                Vector2 normal = new(-edge.Y, edge.X);

                // 모든 점을 노멀 벡터를 기준으로 투영
                float[] projections = points.Select(p => Vector2.Dot(p, normal)).ToArray();
                float minProj = projections.Min();
                float maxProj = projections.Max();
                float width = maxProj - minProj;

                // 다른 방향 벡터를 기준으로 투영
                Vector2 otherNormal = new(edge.Y, -edge.X);
                projections = points.Select(p => Vector2.Dot(p, otherNormal)).ToArray();
                minProj = projections.Min();
                maxProj = projections.Max();
                float height = maxProj - minProj;

                // 면적 계산 및 최소 면적 비교
                float area = width * height;
                if (area < minArea)
                {
                    minArea = area;

                    // 회전 벡터 계산
                    float angle = (float)Math.Atan2(edge.Y, edge.X);
                    Matrix2 rotationMatrix = CreateRotationMatrix(angle);

                    // 회전 축 계산
                    Vector2 axisX = Transform(Vector2.UnitX, rotationMatrix);
                    Vector2 axisY = Transform(Vector2.UnitY, rotationMatrix);

                    minBox = new OOBB2
                    {
                        center = CalculateBoxCenter(p1, p2, width, height, edge),
                        extent = new Vector2(width / 2, height / 2),
                        axisX = axisX,
                        axisY = axisY
                    };
                }
            }

            return minBox;
        }

        // Vector2에 대해 회전 행렬을 적용하는 확장 메서드
        public Vector2 Transform(Vector2 vector, Matrix2 rotationMatrix)
        {
            // 회전 행렬을 사용하여 벡터를 변환.
            return new Vector2(
                (vector.X * rotationMatrix.M11) + (vector.Y * rotationMatrix.M21),
                (vector.X * rotationMatrix.M12) + (vector.Y * rotationMatrix.M22)
            );
        }

        private Matrix2 CreateRotationMatrix(float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new Matrix2(
                new Vector2(cos, -sin),
                new Vector2(sin, cos)
            );
        }

        private Vector2 CalculateBoxCenter(Vector2 p1, Vector2 p2, float width, float height, Vector2 edge)
        {
            Vector2 midPoint = (p1 + p2) / 2;
            return midPoint;
        }
    }
}
