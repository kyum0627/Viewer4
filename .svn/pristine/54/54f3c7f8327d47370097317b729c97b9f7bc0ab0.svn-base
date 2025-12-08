using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint3Plane3
    {
        Vector3 point;
        Plane3f plane;

        public DistancePoint3Plane3(Vector3 p, Plane3f pln)
        {
            point = p;
            plane = pln;
        }

        public Result3f Compute()
        {
            Result3f result = new()
            {
                distance = plane.normal.Dot(point) - plane.distance
            };
            result.sqrDistance = Math.Abs(result.distance);
            result.closest[0] = point - (result.distance * plane.normal);

            return result;
        }
    }
}
