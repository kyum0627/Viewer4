using System.Collections.Generic;
using OpenTK.Mathematics;
//using IGX.Geometry.Operations.MinimumBox;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    /// <summary>
    /// 서로 다른 평면상의 두 3D Polygon 간 교차 계산
    /// </summary>
    public class IntersectPolygon3Polygon3
    {
        readonly Polygon3 p1;
        readonly Polygon3 p2;
        public List<Segment3f> result;

        public IntersectPolygon3Polygon3(Polygon3 t0, Polygon3 t1)
        {
            p1 = t0;
            p2 = t1;
            result = new List<Segment3f>();
        }

        public bool Compute()
        {
            Plane3f plane1 = p1.plane;
            Plane3f plane2 = p2.plane;

            Intersections.PlanePlane(plane1, plane2, out Line3f intline);
            Segment3f intseg = new(intline.origin, intline.direction, 10000000);

            int ig = plane1.normal.MaxLengthCoordinate();
            Segment2f intseg2d = intseg.Get2dSegment(ig);
            Polygon2 poly2d = p1.Get2dPolygon(ig);

            List<Segment2f> clipped2d1 = Intersections.ClipSegmentWithPolygon(intseg2d, poly2d, MathUtil.ZeroTolerance);

            ig = plane2.normal.MaxLengthCoordinate();
            poly2d = p2.Get2dPolygon(ig);

            List<Segment2f> clipped2d2 = Intersections.ClipSegmentWithPolygon(intseg2d, poly2d, MathUtil.ZeroTolerance);

            foreach (Segment2f s1 in clipped2d1)
            {
                foreach (Segment2f s2 in clipped2d2)
                {
                    Segment2f intersected = Intersections.SegmentSegment(s1, s2);
                    if (intersected.Length > 0)
                    {
                        Vector3 point0 = intersected.P0.To3D(plane1, ig);
                        Vector3 point1 = intersected.P1.To3D(plane1, ig);
                        result.Add(new Segment3f(point0, point1));
                    }
                }
            }

            return result.Count > 1;
        }
    }
}
