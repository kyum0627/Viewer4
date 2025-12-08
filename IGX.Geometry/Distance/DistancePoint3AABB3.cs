using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint3AABB3
    {
        Vector3 point;
        AABB3 box;

        public DistancePoint3AABB3(Vector3 point, AABB3 box)
        {
            this.point = point;
            this.box = box;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 boxCenter = new();
            Vector3 boxExtent = new();

            box.GetCenteredForm(ref boxCenter, ref boxExtent);
            Vector3 closest = point - boxCenter;

            result.sqrDistance = 0f;
            for (int i = 0; i < 3; ++i)
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