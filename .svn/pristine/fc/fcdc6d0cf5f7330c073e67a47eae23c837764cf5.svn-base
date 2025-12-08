//using IGX.Geometry.Curves;
//using OpenTK.Mathematics;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace IGX.Geometry.Curves
//{
//    public class ArcCurve : CurveBase
//    {
//        private Vector3 _center;
//        private float _radius;
//        private float _startAngle;
//        private float _endAngle;
//        private int _segments;
//        private bool _isClosed;

//        public ArcCurve(Vector3 center, float radius, float startAngle, float endAngle, int segments = 50, bool isClosed = false)
//        {
//            _center = center;
//            _radius = radius;
//            _startAngle = startAngle;
//            _endAngle = endAngle;
//            _segments = segments;
//            _isClosed = isClosed;
//            GenerateVertices();
//        }

//        public override void GenerateVertices()
//        {
//            Vertices.Clear();
//            int pointCount = _isClosed ? _segments + 1 : _segments;
//            float angleRange = _endAngle - _startAngle;

//            for (int i = 0; i < pointCount; i++)
//            {
//                float angle = _startAngle + angleRange / (pointCount - 1) * i;
//                Vertices.Add(new Vector3(
//                    _center.X + _radius * MathF.Cos(angle),
//                    _center.Y + _radius * MathF.Sin(angle),
//                    _center.Z
//                ));
//            }

//            UpdateCumulativeLengths();
//            UpdateNormals();
//        }

//        public override void UpdateControlPoints(IEnumerable<Vector3> newControls)
//        {
//            throw new NotImplementedException("ArcCurve는 제어점 배열 대신 매개변수로 업데이트해야 합니다.");
//        }
//    }
//}