using MathNet.Numerics.LinearAlgebra;

namespace IGX.Geometry.ConvexHull
{
    public abstract class ApprQuery<T, R>
    {
        protected Matrix<double> CovarianceMatrix { get; set; } = Matrix<double>.Build.Dense(0, 0); // 기본값 설정
        protected Vector<double> Mean { get; set; } = Vector<double>.Build.Dense(0); // 기본값 설정
        public abstract void Fit(T[] points);
        public R GetParameters() => ComputeBoundingBox();
        protected void ComputeMeanAndCovariance(T[] points)
        {
            Mean = ComputeMean(points); // 평균 계산
            CovarianceMatrix = ComputeCovarianceMatrix(points, Mean); // 공분산 행렬 계산
        }
        protected abstract Vector<double> ComputeMean(T[] points);
        protected abstract Matrix<double> ComputeCovarianceMatrix(T[] points, Vector<double> mean);
        public abstract R ComputeBoundingBox();
        public abstract T ToVector(Vector<double> vec);
    }
}
