using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistanceRay2Seg2
    {
        private readonly Ray2f ray;
        private Segment2f segment;

        public DistanceRay2Seg2(Ray2f r, Segment2f s)
        {
            ray = r;
            segment = s;
        }

        /// <summary>
        /// Ray에서 segment까지 최단 거리
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public Result2f Compute()
        {
            Result2f result = new();

            Vector2 segCenter, segDirection;
            float segExtent;
            segCenter = segment.center;
            segDirection = segment.direction;
            segExtent = segment.extent;

            Vector2 diff = ray.origin - segCenter;
            float a01 = -Vector2.Dot(ray.direction, segDirection);
            float b0 = Vector2.Dot(diff, ray.direction);
            float s0, s1;

            if (Math.Abs(a01) < 1)
            {
                // The ray and default_nsegs are not parallel.
                float det = 1 - (a01 * a01);
                float extDet = segExtent * det;
                float b1 = -Vector2.Dot(diff, segDirection);
                s0 = (a01 * b1) - b0;
                s1 = (a01 * b0) - b1;

                if (s0 >= 0)
                {
                    if (s1 >= -extDet)
                    {
                        if (s1 <= extDet)  // region 0
                        {
                            // Minimum at interior ControlPoints of ray and default_nsegs.
                            s0 /= det;
                            s1 /= det;
                        }
                        else  // region 1
                        {
                            s1 = segExtent;
                            s0 = Math.Max(-((a01 * s1) + b0), 0);
                        }
                    }
                    else  // region 5
                    {
                        s1 = -segExtent;
                        s0 = Math.Max(-((a01 * s1) + b0), 0);
                    }
                }
                else
                {
                    if (s1 <= -extDet)  // region 4
                    {
                        s0 = -((-a01 * segExtent) + b0);
                        if (s0 > 0)
                        {
                            s1 = -segExtent;
                        }
                        else
                        {
                            s0 = 0;
                            s1 = -b1;
                            if (s1 < -segExtent)
                            {
                                s1 = -segExtent;
                            }
                            else if (s1 > segExtent)
                            {
                                s1 = segExtent;
                            }
                        }
                    }
                    else if (s1 <= extDet)  // region 3
                    {
                        s0 = 0;
                        s1 = -b1;
                        if (s1 < -segExtent)
                        {
                            s1 = -segExtent;
                        }
                        else if (s1 > segExtent)
                        {
                            s1 = segExtent;
                        }
                    }
                    else  // region 2
                    {
                        s0 = -((a01 * segExtent) + b0);
                        if (s0 > 0)
                        {
                            s1 = segExtent;
                        }
                        else
                        {
                            s0 = 0;
                            s1 = -b1;
                            if (s1 < -segExtent)
                            {
                                s1 = -segExtent;
                            }
                            else if (s1 > segExtent)
                            {
                                s1 = segExtent;
                            }
                        }
                    }
                }
            }
            else
            {
                // Ray and default_nsegs are parallel.
                if (a01 > 0)
                {
                    // Opposite Direction vectors.
                    s1 = -segExtent;
                }
                else
                {
                    // Same Direction vectors.
                    s1 = segExtent;
                }

                s0 = Math.Max(-((a01 * s1) + b0), 0);
            }

            result.parameter[0] = s0;
            result.parameter2[1] = s1;
            result.closest[0] = ray.origin + (s0 * ray.direction);
            result.closest[1] = segCenter + (s1 * segDirection);
            diff = result.closest[0] - result.closest[1];
            result.sqrDistance = Vector2.Dot(diff, diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);
            return result;
        }
    }
}
