using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint2Seg2
    {
        Vector2 point;
        Segment2f segment;

        public DistancePoint2Seg2(Vector2 v, Segment2f seg)
        {
            point = v;
            segment = seg;
        }

        public Result2f Compute()
        {
            Result2f result = new();

            Vector2 direction = segment.P1 - segment.P0;
            Vector2 diff = point - segment.P1;
            float t = Vector2.Dot(direction, diff);
            if (t >= 0f)
            {
                result.parameter[0] = 1f;
                result.closest[0] = segment.P1;
            }
            else
            {
                diff = point - segment.P0;
                t = Vector2.Dot(direction, diff);
                if (t <= 0f)
                {
                    result.parameter[0] = 0f;
                    result.closest[0] = segment.P0;
                }
                else
                {
                    float sqLength = Vector2.Dot(direction, direction);
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
            result.sqrDistance = Vector2.Dot(diff, diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);

            return result;
        }
    }
}
