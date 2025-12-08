using System;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    class DistanceTriangle3Triangle3
    {
        Triangle3f tri0;
        Triangle3f tri1;

        public DistanceTriangle3Triangle3(Triangle3f tri0, Triangle3f tri1)
        {
            this.tri0 = tri0;
            this.tri1 = tri1;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            float sqrDist = float.MaxValue, sqrDistTmp;
            Segment3f edge = new();
            float ratio;
            int i0, i1;
            for (i0 = 2, i1 = 0; i1 < 3; i0 = i1++)
            {
                edge.UpdateFromEndpoints(tri0[i0], tri0[i1]);

                DistanceSeg3Triangle3 queryST = new(edge, tri1);
                Result3f res = queryST.Compute();
                sqrDistTmp = res.distance;
                if (sqrDistTmp < sqrDist)
                {
                    result.closest[0] = res.closest[0];
                    result.closest[1] = res.closest[1];
                    sqrDist = sqrDistTmp;

                    ratio = res.parameter[0] / edge.Extent;
                    result.parameter = new float[3];
                    result.parameter[i0] = 0.5f * (1 - ratio);
                    result.parameter[i1] = 1 - result.parameter[i0];
                    result.parameter[3 - i0 - i1] = 0;
                    result.parameter2 = res.parameter2;

                    if (sqrDist <= MathUtil.ZeroTolerance)
                    {
                        result.sqrDistance = 0;
                        return result;
                    }
                }
            }

            for (i0 = 2, i1 = 0; i1 < 3; i0 = i1++)
            {
                edge.UpdateFromEndpoints(tri1[i0], tri1[i1]);

                DistanceSeg3Triangle3 queryST = new(edge, tri0);
                Result3f res = queryST.Compute();
                sqrDistTmp = res.sqrDistance;
                if (sqrDistTmp < sqrDist)
                {
                    result.closest[0] = res.closest[0];
                    result.closest[1] = res.closest[1];
                    sqrDist = sqrDistTmp;

                    ratio = res.parameter[0] / edge.Extent;
                    result.parameter2 = new float[3];
                    result.parameter2[i0] = 0.5f * (1 - ratio);
                    result.parameter2[i1] = 1 - result.parameter2[i0];
                    result.parameter2[3 - i0 - i1] = 0;
                    result.parameter = res.parameter;

                    if (sqrDist <= MathUtil.ZeroTolerance)
                    {
                        result.sqrDistance = 0;
                        return result;
                    }
                }
            }

            result.sqrDistance = sqrDist;
            result.distance = (float)Math.Sqrt(result.sqrDistance);
            return result;
        }
    }
}