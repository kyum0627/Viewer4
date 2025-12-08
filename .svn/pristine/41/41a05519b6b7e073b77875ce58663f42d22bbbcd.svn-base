using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint3Seg3
    {
        Vector3 point;
        Segment3f segment;

        public DistancePoint3Seg3(Vector3 v, Segment3f seg)
        {
            point = v;
            segment = seg;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 direction = segment.P1 - segment.P0;
            Vector3 diff = point - segment.P1;
            float t = direction.Dot(diff);
            if (t >= 0f)
            {
                result.parameter[0] = 1f;
                result.closest[0] = segment.P1;
            }
            else
            {
                diff = point - segment.P0;
                t = direction.Dot(diff);
                if (t <= 0f)
                {
                    result.parameter[0] = 0f;
                    result.closest[0] = segment.P0;
                }
                else
                {
                    float sqLength = direction.Dot(direction);
                    if (sqLength > 0f)
                    {
                        t /= sqLength;
                        result.parameter[0] = t;
                        result.closest[0] = segment.P0 + (t * direction);
                    }
                    else
                    {
                        result.parameter[0] = 0f;
                        result.closest[0] = segment.P0;
                    }
                }
            }

            diff = point - result.closest[0];
            result.sqrDistance = diff.Dot(diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);

            return result;
        }
    }
}