using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    class DistancePoint3Circle3
    {
        private Vector3 point;
        readonly Circle3 circle;

        public DistancePoint3Circle3(Vector3 PointIn, Circle3 circleIn)
        {
            point = PointIn;
            circle = circleIn;
        }

        public Result3f Compute()
        {
            Result3f result = new();

            // P-C 를 평면에 투영한 Q-C = P-C - Dot(N,P-C)*N.
            Vector3 PmC = point - circle.center;
            //Vector3 QmC = PmC - Vector3.Dot(circle.plane.norID, PmC) * circle.plane.norID;
            Vector3 QmC = PmC - (circle.plane.normal.Dot(PmC) * circle.plane.normal);

            float LengthQmC = QmC.Length;
            if (LengthQmC > MathUtil.Epsilon)
            { // 원 자체
                result.closest[0] = circle.center + (circle.radius * QmC / LengthQmC);
            }
            else
            {
                result.closest[0] = circle.center + (circle.radius * circle.axisX);
            }

            Vector3 diff = point - result.closest[0];
            float sqrDistance = diff.Dot(diff);

            if (sqrDistance < 0)
            {
                sqrDistance = 0;
            }
            result.sqrDistance = sqrDistance;
            result.distance = (float)Math.Sqrt(sqrDistance);
            return result;
        }
    }
}
