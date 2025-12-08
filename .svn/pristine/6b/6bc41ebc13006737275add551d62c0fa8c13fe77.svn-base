using System;
using OpenTK.Mathematics;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Distance
{
    class DistancePoint2OOBB2
    {
        Vector2 point;
        OOBB2 box;

        public DistancePoint2OOBB2(Vector2 PointIn, OOBB2 boxIn)
        {
            point = PointIn;
            box = boxIn;
        }

        public Result2f Compute()
        {
            Result2f result = new();
            Vector2 diff = point - box.center;

            float sqrDistance = 0;
            float delta;
            Vector2 closest = Vector2.Zero;
            int i;
            for (i = 0; i < 2; ++i)
            {
                closest[i] = Vector2.Dot(diff, box.Axis(i));
                if (closest[i] < -box.extent[i])
                {
                    delta = closest[i] + box.extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = -box.extent[i];
                }
                else if (closest[i] > box.extent[i])
                {
                    delta = closest[i] - box.extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = box.extent[i];
                }
            }

            Vector2 BoxClosest = box.center;
            for (i = 0; i < 2; ++i)
            {
                BoxClosest += closest[i] * box.Axis(i);
            }
            result.closest[0] = BoxClosest;
            result.sqrDistance = sqrDistance;
            result.distance = (float)Math.Sqrt(sqrDistance);
            return result;
        }
    }
}
