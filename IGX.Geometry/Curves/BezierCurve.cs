//using IGX.Geometry.Curves;
//using OpenTK.Mathematics;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace IGX.Geometry.Curves
//{
//    public class BezierCurve : CurveBase
//    {
//        public new Vector3[] ControlPoints { get; private set; }
//        private int _segments;

//        public BezierCurve(Vector3[] controlPoints, int segments = 50)
//        {
//            if (controlPoints.Length < 2)
//                throw new ArgumentException("최소 2개 이상의 제어점 필요.", nameof(controlPoints));

//            ControlPoints = controlPoints;
//            _segments = segments;
//            ControlPointIDs = Enumerable.Range(0, controlPoints.Length).ToList();
//            GenerateVertices();
//        }

//        public override void GenerateVertices()
//        {
//            Vertices = new List<Vector3>(_segments + 1);
//            for (int i = 0; i <= _segments; i++)
//            {
//                float t = i / (float)_segments;
//                Vertices[i] = DeCasteljau(t, ControlPoints);
//            }

//            UpdateCumulativeLengths();
//            UpdateBounds();
//        }

//        private static Vector3 DeCasteljau(float t, Vector3[] points)
//        {
//            Vector3[] temp = (Vector3[])points.Clone();
//            for (int i = 1; i < temp.Length; i++)
//            {
//                for (int j = 0; j < temp.Length - i; j++)
//                {
//                    temp[j] = (1 - t) * temp[j] + t * temp[j + 1];
//                }
//            }
//            return temp[0];
//        }

//        public override void UpdateControlPoints(IEnumerable<Vector3> newControlPoints)
//        {
//            if (newControlPoints.Count() < 2)
//                throw new ArgumentException("최소 2개 이상의 제어점 필요.", nameof(newControlPoints));

//            ControlPoints = newControlPoints.ToArray();
//            ControlPointIDs = Enumerable.Range(0, newControlPoints.Count()).ToList();
//            GenerateVertices();
//        }
//    }
//}