using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine3Seg3
    {
        Line3f line;
        Segment3f segment;

        public IntersectionResult3 result;

        public IntersectLine3Seg3(Line3f L, Segment3f S)
        {
            line = L;
            segment = S;
        }

        /// <summary>
        /// 라인과 세그먼트간 교차 계산
        /// 재확인 필요 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="line"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool Compute()
        {
            result = new IntersectionResult3();

            Vector3 segCenter = segment.center;
            Vector3 segDirection = segment.direction;
            float segExtent = segment.extent;

            Vector3 diff = line.origin - segCenter;
            float a01 = -Vector3.Dot(line.direction, segDirection);
            float b0 = Vector3.Dot(diff, line.direction);
            float s0, s1;

            if (Math.Abs(a01) < 1f)
            {
                // The line and default_nsegs are not parallel.
                float det = 1 - (a01 * a01);
                float extDet = segExtent * det;
                float b1 = -Vector3.Dot(diff, segDirection);
                s1 = (a01 * b0) - b1;

                if (s1 >= -extDet)
                {
                    if (s1 <= extDet)
                    {
                        s0 = ((a01 * b1) - b0) / det;
                        s1 /= det;
                    }
                    else
                    {
                        s1 = segExtent;
                        s0 = -((a01 * s1) + b0);
                    }
                }
                else
                {
                    s1 = -segExtent;
                    s0 = -((a01 * s1) + b0);
                }
                result.parameter1 = new Interval(s0, s1);
                result.points = new List<Vector3>
                {
                    line.origin + (s0 * line.direction),
                    segCenter + (s1 * segDirection)
                };
                result.quantity = 1;
            }
            else
            {
                s1 = 0f;
                s0 = -b0;
            }

            return result.status == IntersectionResult.INTERSECT;
        }
    }
}
