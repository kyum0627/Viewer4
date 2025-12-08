//using IGX.Geometry.Curves;
//using OpenTK.Mathematics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MathNet.Numerics.LinearAlgebra;

//namespace IGX.Geometry.Curves
//{
//    public class LeastSquaresFitCurve : CurveBase
//    {
//        public Vector3[] ControlPoints { get; private set; }  // 입력 점들
//        private int _segments;
//        private int _degree; // 근사 다항식 차수
//        public LeastSquaresFitCurve(IEnumerable<Vector3> dataPoints, int segments = 50)
//        {
//            if (!dataPoints.Any()) throw new ArgumentException("데이터 점 필요");
//            ControlPoints = dataPoints.ToArray();
//            GenerateVertices();
//        }

//        public override void GenerateVertices()
//        {
//            // 단순 1차 least squares (직선 근사)
//            Vertices.Clear();
//            int n = ControlPoints.Length;
//            float sumX = ControlPoints.Sum(p => p.X);
//            float sumY = ControlPoints.Sum(p => p.Y);
//            float sumXY = ControlPoints.Sum(p => p.X * p.Y);
//            float sumXX = ControlPoints.Sum(p => p.X * p.X);

//            float a = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
//            float b = (sumY - a * sumX) / n;

//            float minX = ControlPoints.Min(p => p.X);
//            float maxX = ControlPoints.Max(p => p.X);

//            int segments = 50;
//            for (int i = 0; i <= segments; i++)
//            {
//                float x = minX + (maxX - minX) / segments * i;
//                float y = a * x + b;
//                Vertices.Add(new Vector3(x, y, 0));
//            }

//            UpdateCumulativeLengths();
//            UpdateNormals();
//        }

//        public override void UpdateControlPoints(IEnumerable<Vector3> newControls)
//        {
//            ControlPoints = newControls.ToArray();
//            GenerateVertices();
//        }
//    }
//}
