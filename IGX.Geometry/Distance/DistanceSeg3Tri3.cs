using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    class DistanceSeg3Triangle3
    {
        Segment3f segment;
        Triangle3f triangle;

        public DistanceSeg3Triangle3(Segment3f SegmentIn, Triangle3f TriangleIn)
        {
            triangle = TriangleIn;
            segment = SegmentIn;
        }

        public Result3f Compute()
        {
            Result3f result = new();

            Line3f line = new(segment.P0, segment.P1);//.Direction);
            DistanceLine3Triangle3 ltQuery = new(line, triangle);
            Result3f res = ltQuery.Compute();

            result.parameter[0] = res.parameter[0];

            if (result.parameter[0] >= -segment.Extent)
            {
                if (result.parameter[0] <= segment.Extent)
                {
                    result.distance = res.distance;
                    result.sqrDistance = res.sqrDistance;
                    result.closest[0] = res.closest[0];
                    result.closest[1] = res.closest[1];
                    result.parameter2 = res.parameter2;
                }
                else
                {
                    result.closest[0] = segment.P1;
                    DistancePoint3Triangle3 ptQuery = new(result.closest[0], triangle);
                    Result3f ptq = ptQuery.Compute();
                    result.distance = ptq.distance;
                    result.sqrDistance = ptq.sqrDistance;
                    result.closest[0] = ptq.closest[0];
                    result.parameter[0] = segment.Extent;
                    result.parameter2 = ptq.parameter;
                }
            }
            else
            {
                result.closest[0] = segment.P0;
                DistancePoint3Triangle3 ptQuery = new(result.closest[0], triangle);
                Result3f ptq = ptQuery.Compute();
                result.distance = ptq.distance;
                result.sqrDistance = ptq.sqrDistance;
                result.closest[1] = ptq.closest[0];
                result.parameter[0] = -segment.Extent;
                result.parameter2 = ptq.parameter2;
            }

            return result;
        }
    }
}
