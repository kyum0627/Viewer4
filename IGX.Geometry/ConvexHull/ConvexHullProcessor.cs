using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using System.Diagnostics;

namespace IGX.Geometry.ConvexHull
{
    public struct ConvexHullProcessor
    {
        public OOBB3 Oobb { get; private set; } // OOBB3를 저장할 속성

        public void ProcessVertices(List<Vector3> inputpoints, List<Vector3> inputnormals)
        {
            List<Vector3> normals = inputnormals.Distinct().ToList();
            List<Vector3> points = inputpoints.Distinct().ToList();
            Oobb = OOBB3.Empty;
            if (points.Count < 3)
            { // 점 또는 선, 면을 구성할 수 없음. 데이터 오류이므로 건너뜀
                Debug.WriteLine("점이 부족하여 처리를 건너뜀.");
                return;
            }
            int dimension = CheckDimension(points);
            if (normals.Count == 1 && dimension < 3)
            {  // 평면의 경우 처리
                if (dimension < 2)
                {
                    return; // 한 점 or 동일 직선
                }

                ProcessPlanarPoints(points, normals[0]); // 동일 평면
                return;
            }
            ProcessNonPlanarPoints(points);
        }

        private void ProcessPlanarPoints(List<Vector3> points, Vector3 normal)//, int dimension)
        {
            List<Vector2> transformedPoints = TransformTo2D(points, normal);// 2. 2D 변환
            List<Vector2> convexHull2D = Calculate2DConvexHull(transformedPoints); // 3. 2D Convex Hull 계산
            if (convexHull2D.Count > 2)
            { // 4. Rotating Caliper 알고리즘으로 OOBB2 계산
                OOBB2 oobb2 = CalculateOobb2(convexHull2D);
                Oobb = TransformOobb2ToOobb3(oobb2, normal);  // 5. 3D 변환// 결과를 Oobb에 저장
            }
        }

        public void ProcessNonPlanarPoints(List<Vector3> points)
        {// 1. Gaussian 분포 계산 및 차원 확인
            int dimension = IntrinsicsVector3.DeterminePointDimension(points);
            // 2. 최소 볼륨 박스 계산
            if (dimension == 3)
            { // 점들이 3D 부피를 형성하는 경우
                Oobb = CalculateOobb3(points); // 결과를 Oobb에 저장
            }
            else
            { // 점들이 3D 부피를 형성하지 않는 경우
                Oobb = OOBB3.Empty;
            }
        }

        private int CheckDimension(List<Vector3> points)
        {
            int dimension = IntrinsicsVector3.DeterminePointDimension(points);
            return dimension;
        }

        private List<Vector2> TransformTo2D(List<Vector3> points, Vector3 normal)
        { // 평면에 대한 2D 변환
            Vector3 origin = points[0];
            Vector3 axisX = new(1, 0, 0);
            Vector3 axisY = Vector3.Cross(normal, axisX);
            axisX = Vector3.Cross(axisY, normal);

            List<Vector2> transformedPoints = points.Select(p =>
            {
                Vector3 pRel = p - origin;
                float x = Vector3.Dot(pRel, axisX);
                float y = Vector3.Dot(pRel, axisY);
                return new Vector2(x, y);
            }).ToList();

            return transformedPoints;
        }

        private List<Vector2> Calculate2DConvexHull(List<Vector2> points)
        { // 2D Convex Hull 계산
            QuickHull2D quickHull2D = new();
            return quickHull2D.Compute(points);
        }

        private OOBB2 CalculateOobb2(List<Vector2> convexHull2D)
        { // Rotating Caliper 알고리즘으로 OOBB2 계산
            RotatingCaliper2D rotatingCaliper = new();
            return rotatingCaliper.CalculateOobb2(convexHull2D);
        }

        private OOBB3 TransformOobb2ToOobb3(OOBB2 oobb2, Vector3 normal)
        { // 2D OOBB를 3D OOBB로 변환
            float rotationAngle = CalculateAngle(oobb2.axisX, oobb2.axisY);
            Matrix2 rotationMatrix = CreateRotationMatrix(rotationAngle);
            Vector3 axisX = new(rotationMatrix.M11, rotationMatrix.M12, 0);
            Vector3 axisY = new(rotationMatrix.M21, rotationMatrix.M22, 0);
            Vector3 axisZ = normal.Normalized(); // Normalizing norID for consistency
            // 중심과 크기 계산
            Vector3 center = new(oobb2.center.X, oobb2.center.Y, 0);
            Vector3 extent = new(oobb2.extent.X, oobb2.extent.Y, 0);

            return new OOBB3
            {
                center = center,
                axisX = axisX,
                axisY = axisY,
                axisZ = axisZ,
                extent = extent
            };
        }
        private float CalculateAngle(Vector2 axisX, Vector2 axisY)
        {
            // axisX가 (1, 0)일 때, axisY에 대한 각도 계산
            float angle = (float)Math.Atan2(axisY.Y, axisY.X);
            return angle;
        }

        /// <summary>
        /// Creates a 2D rotation CombinedMatrix that rotates vectors by the given RotationAngles.
        /// </summary>
        /// <param name="angle">The RotationAngles to rotate, in radians.</param>
        /// <returns>A 2x2 CombinedMatrix representing the rotation.</returns>
        public Matrix2 CreateRotationMatrix(float angle)
        {
            float cosAngle = (float)Math.Cos(angle);
            float sinAngle = (float)Math.Sin(angle);

            return new Matrix2(
                cosAngle, -sinAngle,
                sinAngle, cosAngle
            );
        }

        public OOBB3 CalculateOobb3(List<Vector3> points)
        { // Convex Hull의 외곽 점을 사용하여 OOBB3를 계산.
            // 1. Convex Hull을 계산.
            QuickHull3D convexHull3D = new();
            List<Vector3> hullPoints = new();
            List<int> triangles = new();
            List<Vector3> normals = new();
            bool success = convexHull3D.Compute(points, false, ref hullPoints, ref triangles, ref normals);
            if (success)
            {// 2. Convex Hull의 점들을 사용하여 최소 볼륨 박스를 계산.
                ApprGaussian3 apprGaussian3 = new();
                apprGaussian3.Fit(hullPoints.ToArray());
                return apprGaussian3.GetParameters();
            }
            return OOBB3.Empty;
        }
    }
}
