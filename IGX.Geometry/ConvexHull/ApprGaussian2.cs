using System;
using System.Linq;
using IGX.Geometry.Common;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Mathematics;

namespace IGX.Geometry.ConvexHull
{
    public class ApprGaussian2 : ApprQuery<Vector2, OOBB2>
    {
        private readonly double tolerance = 1e-5;  // 고유값 비교 허용 오차

        // Fit 메서드 구현
        public override void Fit(Vector2[] points)
        {
            ComputeMeanAndCovariance(points); // 평균과 공분산 행렬 계산
            ShapeType shape = GetShape(); // 도형 형태 판별
        }

        // Vector2 포인트 배열로부터 평균 벡터를 계산
        protected override Vector<double> ComputeMean(Vector2[] points)
        {// 평균 X, Y를 계산
            float meanX = points.Average(p => p.X); // X축 평균
            float meanY = points.Average(p => p.Y); // Y축 평균
            return Vector<double>.Build.DenseOfArray(new double[] { meanX, meanY });
        }

        // Vector2 포인트 배열로부터 공분산 행렬을 계산
        protected override Matrix<double> ComputeCovarianceMatrix(Vector2[] points, Vector<double> mean)
        {
            Matrix<double> covarianceMatrix = Matrix<double>.Build.Dense(2, 2);

            // 각 포인트에 대해 공분산 계산
            foreach (Vector2 point in points)
            {
                Vector<double> centeredPoint = Vector<double>.Build.DenseOfArray(new double[] { point.X, point.Y }) - mean;
                covarianceMatrix[0, 0] += centeredPoint[0] * centeredPoint[0];
                covarianceMatrix[0, 1] += centeredPoint[0] * centeredPoint[1];
                covarianceMatrix[1, 0] += centeredPoint[1] * centeredPoint[0];
                covarianceMatrix[1, 1] += centeredPoint[1] * centeredPoint[1];
            }

            covarianceMatrix /= points.Length - 1; // 공분산 행렬 정규화
            return covarianceMatrix;
        }

        // 공분산 행렬을 기반으로 도형 형태를 결정
        public ShapeType GetShape()
        {
            MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> eigen = CovarianceMatrix.Evd(); // 공분산 행렬의 고유값 분해
            double[] eigenValues = eigen.EigenValues.Select(e => e.Magnitude).ToArray(); // 고유값 배열
            //var tolerance = 1e-5; // 오차 허용 범위

            if (eigenValues.Length < 2)
            {
                return ShapeType.Dimension0; // 고유값이 2개 미만일 경우 점으로 간주
            }

            double maxEval = eigenValues.Max(); // 최대 고유값
            double minEval = eigenValues.Min(); // 최소 고유값
            System.Collections.Generic.List<double> distinctEigenValues = eigenValues.Distinct().ToList(); // 고유값의 고유 값들

            // 고유값의 차이가 허용 오차 이내인지 확인
            if (Math.Abs(maxEval - minEval) < tolerance && distinctEigenValues.Count == 1)
            {
                return ShapeType.Dimension0;
            }
            else if (Math.Abs(maxEval - minEval) < tolerance && distinctEigenValues.Count == 2)
            {
                return ShapeType.Dimension1;
            }
            else
            {
                return ShapeType.Dimension3; // 2D에서는 면적이 아닌 영역이기 때문에 Volume을 사용
            }
        }

        // 공분산 행렬을 기반으로 OOBB2 계산
        public override OOBB2 ComputeBoundingBox()
        {
            MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> eigen = CovarianceMatrix.Evd(); // 공분산 행렬의 고유값 분해
            double[] eigenValues = eigen.EigenValues.Select(e => e.Magnitude).ToArray(); // 고유값 배열
            Matrix<double> eigenVectors = eigen.EigenVectors; // 고유벡터

            // 고유벡터를 OpenTK.Vector2로 변환
            //var axisX = eigenVectors.Column(0).ToVector();
            //var axisY = eigenVectors.Column(1).ToVector();
            Vector2 axisX = ToVector(eigenVectors.Column(0));
            Vector2 axisY = ToVector(eigenVectors.Column(1));
            // 고유값을 이용해 축의 크기 계산
            double extentX = Math.Sqrt(eigenValues.Max());
            double extentY = Math.Sqrt(eigenValues.Min());

            OOBB2 oOBB2 = new()
            {
                center = ToVector(Mean), // 중심점
                axisX = axisX, // X축
                axisY = axisY, // Y축
                extent = new Vector2((float)extentX, (float)extentY) // 크기
            };
            return oOBB2;
        }
        // Vector<double>를 OpenTK.Vector2로 변환
        public override Vector2 ToVector(Vector<double> vec)
        {
            return vec.Count != 2
                ? throw new ArgumentException("Vector must have exactly 2 elements to convert to Vector2.")
                : new Vector2((float)vec[0], (float)vec[1]);
        }
    }
}
