using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.ConvexHull
{
    /// <summary>
    /// 2D mVertexPool 를 둘러 싸는 최소 bounding box
    /// https://www.geometrictools.com/Documentation/MinimumAreaRectangle.pdf
    /// </summary>
    public class MinimumAreaBox
    {
        public OOBB2 MinBox = OOBB2.Empty;

        public MinimumAreaBox(List<Vector2> points, float tolerrance = 1e-5f)
        {
            Vector2[] mPoints;
            mPoints = ConvexHull2.Compute(points).ToArray(); // 주어진 점들에 대한 볼록 다각형의 볼록 껍질 계산
            int mNumPoints = mPoints.Length;
            IntrinsicsVector2 info = new(mNumPoints, mPoints, 1e-5f); // 점들이 epsilon 근방에 있는지 확인하기 위해 정보 객체 생성
            int dimension = info.Dimension;// 다각형의 차원 확인 (0, 1, 2)

            if (dimension == 0)
            {
                // 모든 점들이 epsilon 근방에 있어 같은 점으로 간주됨
                MinBox.center = mPoints[0];
                MinBox.axisX = Vector2.UnitX;
                MinBox.axisY = Vector2.UnitY;
                MinBox.extent.X = 0;
                MinBox.extent.Y = 0;
            }

            if (dimension == 1)
            {// 동일 직선상에 있는 ControlPoints 들이 있음
                const float tmin = 0;
                const float tmax = 0;
                MinBox.extent[0] = 0.5f * (tmax - tmin);
                MinBox.extent[1] = 0;
            }
            MinBox = MinAreaBoxOfConvexPolygon(mPoints.ToList());
        }
        public OOBB2 MinAreaBoxOfConvexPolygon(List<Vector2> points)
        {
            if (points == null || points.Count == 0)
            {
                return OOBB2.Empty;
            }

            int n = points.Count;
            if (n == 1)
            {
                return new OOBB2
                {
                    center = points[0],
                    axisX = Vector2.UnitX,
                    axisY = Vector2.UnitY,
                    extent = Vector2.Zero
                };
            }
            OOBB2 minBox = new()
            {
                extent = new Vector2(float.MaxValue, float.MaxValue)
            };
            float minArea = float.MaxValue;
            for (int i = 0; i < n; ++i)
            {
                Vector2 origin = points[i];
                Vector2 edge = points[(i + 1) % n] - points[i];
                Vector2 axisX = Vector2.Normalize(edge);
                Vector2 axisY = -axisX.Perpendicular();

                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;

                for (int j = 0; j < n; j++)
                {
                    Vector2 D = points[j] - origin;
                    float projX = Vector2.Dot(D, axisX);
                    float projY = Vector2.Dot(D, axisY);

                    if (projX < minX)
                    {
                        minX = projX;
                    }

                    if (projX > maxX)
                    {
                        maxX = projX;
                    }

                    if (projY < minY)
                    {
                        minY = projY;
                    }

                    if (projY > maxY)
                    {
                        maxY = projY;
                    }
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
