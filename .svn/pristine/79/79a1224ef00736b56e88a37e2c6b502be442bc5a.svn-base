using OpenTK.Mathematics;

//using IGX.Geometry.Operations.MinimumBox;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectSeg3Tirangle3
    {
        public Segment3f segment;
        public Triangle3f triangle;

        public IntersectionResult3 result;

        IntersectionResult3 Find()
        {
            Vector3 diff = segment.center - triangle.V0;
            Vector3 edge1 = triangle.V1 - triangle.V0;
            Vector3 edge2 = triangle.V2 - triangle.V0;
            Vector3 normal = edge1 * edge2;

            // Solve Q + t*D = b1*E1 + b2*E2 (Q = diff, D = default_nsegs Direction,
            // E1 = edge1, E2 = edge2, N = Cross(E1,E2)) by
            //   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
            //   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
            //   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
            float DdN = Vector3.Dot(segment.direction, normal);
            float sign;
            if (DdN > 0)
            {
                sign = 1;
            }
            else if (DdN < 0)
            {
                sign = -1;
                DdN = -DdN;
            }
            else
            {
                // no intersection
                result.status = IntersectionResult.NOTINTERSECT;
                return result;
            }

            float DdQxE2 = sign * segment.direction.DotAfterCross(diff, edge2);
            if (DdQxE2 >= 0)
            {
                float DdE1xQ = sign * segment.direction.DotAfterCross(edge1, diff);
                if (DdE1xQ >= 0)
                {
                    if (DdQxE2 + DdE1xQ <= DdN)
                    {
                        // Dimension1 intersects triangle, check default_nsegs
                        float QdN = -sign * Vector3.Dot(diff, normal);
                        float extDdN = segment.Extent * DdN;
                        if (-extDdN <= QdN && QdN <= extDdN)
                        {
                            // Segment intersects triangle.
                            result.status = IntersectionResult.INTERSECT;
                            float inv = 1 / DdN;
                            result.parameter1[0] = QdN * inv;
                            result.parameter2[1] = DdQxE2 * inv;
                            result.parameter2[2] = DdE1xQ * inv;
                            result.parameter2[0] = 1 - result.parameter2[1] - result.parameter2[2];
                            result.points[0] = segment.center + (result.parameter1[0] * segment.direction);
                            return result;
                        }
                        // else: |t| > extent, no intersection
                    }
                    // else: b1+b2 > 1, no intersection
                }
                // else: b2 < 0, no intersection
            }
            // else: b1 < 0, no intersection

            result.status = IntersectionResult.NOTINTERSECT;
            return result;
        }
    }
}
