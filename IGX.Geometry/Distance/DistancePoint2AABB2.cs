using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint2AABB2
    {
        Vector2 point;
        AABB2 box;

        public DistancePoint2AABB2(Vector2 point, AABB2 box)
        {
            this.point = point;
            this.box = box;
        }

        public Result2f Compute()
        {
            Result2f result;

            Vector2 boxCenter = new();
            Vector2 boxExtent = new();

            box.GetCenteredForm(ref boxCenter, ref boxExtent);
            Vector2 closest = point - boxCenter;

            result = new Result2f
            {
                sqrDistance = 0f
            };

            for (int i = 0; i < 2; ++i)
            {
                if (point[i] < -boxExtent[i])
                {
                    float delta = point[i] + boxExtent[i];
                    result.sqrDistance += delta * delta;
                    point[i] = -boxExtent[i];
                }
                else if (point[i] > boxExtent[i])
                {
                    float delta = point[i] - boxExtent[i];
                    result.sqrDistance += delta * delta;
                    point[i] = boxExtent[i];
                }
            }
            result.distance = (float)Math.Sqrt(result.sqrDistance);
            result.closest[0] = boxCenter + closest;
            return result;
        }
    }
}
