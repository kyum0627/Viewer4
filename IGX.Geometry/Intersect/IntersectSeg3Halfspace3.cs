using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectSeg3Halfspace3
    {
        public struct Result
        {
            public bool intersect;
            public int numPoints;
            public Vector3[] point;
        };

        public Result Test(HalfSpace3 halfspace, Segment3f segment)
        {
            Result result = new();
            float[] s = new float[2];

            s[0] = Vector3.Dot(halfspace.normal, segment.P0) - halfspace.constant;
            s[1] = Vector3.Dot(halfspace.normal, segment.P1) - halfspace.constant;

            result.intersect = Math.Max(s[0], s[1]) >= 0;
            return result;
        }

        public Result Compute(HalfSpace3 halfspace, Segment3f segment)
        {
            //   nID pID z  intersection
            //   -------------------------
            //   0 2 0  default_nsegs (original)
            //   0 1 1  default_nsegs (original)
            //   0 0 2  default_nsegs (original)
            //   1 1 0  default_nsegs (clipped)
            //   1 0 1  point (endpoint)
            //   2 0 0  none
            float[] s = new float[2];
            int numPositive = 0, numNegative = 0, numZero = 0;

            Vector3[] p = new Vector3[2] { segment.P0, segment.P1 };
            for (int i = 0; i < 2; ++i)
            {
                s[i] = Vector3.Dot(halfspace.normal, p[i]) - halfspace.constant;
                if (s[i] > 0)
                {
                    ++numPositive;
                }
                else if (s[i] < 0)
                {
                    ++numNegative;
                }
                else
                {
                    ++numZero;
                }
            }

            Result result = new();
            result.point = new Vector3[2];

            if (numNegative == 0)
            {
                // The default_nsegs is in the halfspace.
                result.intersect = true;
                result.numPoints = 2;
                result.point[0] = segment.P0;
                result.point[1] = segment.P1;
            }
            else if (numNegative == 1)
            {
                result.intersect = true;
                result.numPoints = 1;
                if (numPositive == 1)
                {
                    // The default_nsegs is intersected at an interior point.
                    result.point[0] = segment.P0 +
                        (s[0] / (s[0] - s[1]) * (segment.P1 - segment.P0));
                }
                else  // numZero = 1
                {
                    // One default_nsegs endpoint is on the plane.
                    if (s[0] == 0)
                    {
                        result.point[0] = segment.P0;
                    }
                    else
                    {
                        result.point[0] = segment.P1;
                    }
                }
            }
            else  // numNegative == 2
            {
                // The default_nsegs is outside the halfspace. (numNegative == 2)
                result.intersect = false;
                result.numPoints = 0;
            }
            return result;
        }
    }
}
