//using IGX.Geometry.Curves;
//using OpenTK.Mathematics;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace IGX.Geometry.Curves
//{
//    public class BSplineCurve : CurveBase
//    {
//        public int Degree { get; private set; }
//        private int _segments;

//        public BSplineCurve(IEnumerable<Vector3> controlPoints, int degree = 3, int segments = 50)
//        {
//            if (controlPoints.Count() < degree + 1)
//                throw new ArgumentException("BSpline은 Degree + 1 이상의 제어점 필요");

//            ControlPoints = controlPoints.ToList();
//            Degree = degree;
//            _segments = segments;
//            GenerateVertices();
//        }

//        public override void GenerateVertices()
//        {
//            Vertices.Clear();
//            int n = ControlPoints.Count - 1;
//            int m = n + Degree + 1;

//            // Uniform knot vector
//            List<float> knots = new();
//            for (int i = 0; i <= m; i++) knots.Add(i / (float)m);

//            for (int i = 0; i <= _segments; i++)
//            {
//                float t = i / (float)_segments;
//                Vertices.Add(CoxDeBoor(t, Degree, ControlPoints.ToArray(), knots.ToArray()));
//            }

//            UpdateCumulativeLengths();
//            UpdateNormals();
//        }

//        private Vector3 CoxDeBoor(float t, int k, Vector3[] P, float[] U)
//        {
//            int n = P.Length - 1;
//            if (k == 0)
//            {
//                for (int i = 0; i <= n; i++)
//                {
//                    if (t >= U[i] && t < U[i + 1]) return P[i];
//                }
//                return P[^1];
//            }

//            Vector3 sum = Vector3.Zero;
//            for (int i = 0; i <= n - k; i++)
//            {
//                float denom1 = U[i + k] - U[i];
//                float denom2 = U[i + k + 1] - U[i + 1];
//                float alpha1 = denom1 == 0 ? 0 : (t - U[i]) / denom1;
//                float alpha2 = denom2 == 0 ? 0 : (U[i + k + 1] - t) / denom2;
//                sum += alpha1 * CoxDeBoor(t, k - 1, new Vector3[] { P[i] }, new float[] { U[i], U[i + k] })
//                     + alpha2 * CoxDeBoor(t, k - 1, new Vector3[] { P[i + 1] }, new float[] { U[i + 1], U[i + k + 1] });
//            }
//            return sum;
//        }

//        public override void UpdateControlPoints(IEnumerable<Vector3> newControls)
//        {
//            ControlPoints = newControls.ToList();
//            GenerateVertices();
//        }
//    }
//}
