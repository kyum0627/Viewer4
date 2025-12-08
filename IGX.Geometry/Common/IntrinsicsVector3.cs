using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public static class IntrinsicsVector3
    { // 포인트들이 동일한 점, 직선, 평면 또는 부피에 있는지 판단
        public static int DeterminePointDimension(IEnumerable<Vector3> points, float tol = MathUtil.Epsilon) // 동일한 점으로 간주하기 위한 오차
        {
            List<Vector3> pointList = points.ToList();
            if (pointList.Count < 2)
            {
                return 0;
            }

            if (AllPointsAreSame(pointList))
            {
                return 0;
            }

            if (pointList.Count == 2)
            {
                return 1;  // 두 점으로 직선 정의
            }

            if (pointList.Count == 3)
            {
                return 2;  // 세 점으로 평면 정의
            }

            return DeterminePointDimensionFromMultiplePoints(pointList, tol); // 더 많은 점들로 차원 결정
        }
        private static bool AllPointsAreSame(List<Vector3> points)
        {  // 모든 포인트가 동일한지 검사
            Vector3 firstPoint = points[0];
            return points.All(p => p == firstPoint);
        }
        private static int DeterminePointDimensionFromMultiplePoints(List<Vector3> points, float tolerance = 1e-5f)
        { // 다수의 점으로 차원 결정
            Vector3 mean = ComputeMean(points);
            Matrix<double> covarianceMatrix = ComputeCovarianceMatrix(points, mean);
            MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> eigen = covarianceMatrix.Evd();
            double[] eigenValues = eigen.EigenValues.Select(e => e.Magnitude).ToArray();
            List<double> distinctEigenValues = eigenValues.Distinct().ToList();
            return eigenValues.Length < 3 || Math.Abs(eigenValues.Max() - eigenValues.Min()) <= tolerance
                ? distinctEigenValues.Count == 1 ? 0 :
                       distinctEigenValues.Count == 2 ? 1 : 2
                : 3;
        }
        private static Vector3 ComputeMean(IEnumerable<Vector3> points)
        { // 평균 계산
            float meanX = points.Average(p => p.X);
            float meanY = points.Average(p => p.Y);
            float meanZ = points.Average(p => p.Z);
            return new Vector3((float)meanX, (float)meanY, (float)meanZ);
        }

        private static Matrix<double> ComputeCovarianceMatrix(IEnumerable<Vector3> points, Vector3 mean)
        { // 공분산 행렬 계산
            Vector3[] pointsArray = points.ToArray();
            Matrix<double> covarianceMatrix = Matrix<double>.Build.Dense(3, 3);

            foreach (Vector3 point in pointsArray)
            {
                Vector3 centeredPoint = new(
                    point.X - mean.X,
                    point.Y - mean.Y,
                    point.Z - mean.Z
                );
                Vector3d centeredPointDouble = new(centeredPoint.X, centeredPoint.Y, centeredPoint.Z);  // double로 변환하여 공분산 행렬에 추가

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        covarianceMatrix[i, j] += centeredPointDouble[i] * centeredPointDouble[j];
                    }
                }
            }

            covarianceMatrix /= pointsArray.Length - 1; // 공분산 행렬 정규화
            return covarianceMatrix;
        }
        // 벡터를 벡터3으로 변환하는 확장 메서드
        private static Vector3 ToVector3(this Vector<double> vector)
        {
            return vector.Count != 3
                ? throw new InvalidOperationException("Vector must be of size 3.")
                : new Vector3((float)vector[0], (float)vector[1], (float)vector[2]);
        }
    }

    // 도형 유형을 정의하는 열거형
    public enum PointDimension
    {
        Point,   // 점
        Line,    // 선
        Plane,   // 평면
        Volume   // 부피
    }
}
