using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint3Line3
    {
        Vector3 point;
        Line3f line;

        public DistancePoint3Line3(Vector3 p, Line3f l2)
        {
            point = p;
            line = l2;
            //result = new Result3f();
        }

        public Result3f Compute()
        {
            Result3f result = new();

            Vector3 diff = point - line.origin;
            result.parameter[0] = line.direction.Dot(diff);
            result.closest[0] = line.origin + (result.parameter[0] * line.direction);

            diff = point - result.closest[0];
            result.sqrDistance = diff.Dot(diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);

            return result;
        }
    }
}
