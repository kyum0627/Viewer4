using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    // https://www.geometrictools.com/Downloads/Downloads.html

    public class DistancePoint2Line2
    {
        Vector2 point;
        Line2f line;

        public DistancePoint2Line2(Vector2 p, Line2f l2)
        {
            point = p;
            line = l2;
        }

        public Result2f Compute()
        {
            Result2f result = new();

            Vector2 diff = point - line.Point;
            result.parameter[0] = Vector2.Dot(line.Direction, diff);
            result.closest[0] = line.Point + (result.parameter[0] * line.Direction);

            diff = point - result.closest[0];
            result.sqrDistance = Vector2.Dot(diff, diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);

            return result;
        }
    }
}
