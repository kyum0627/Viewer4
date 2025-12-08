using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Intersect
{
    public class IntersectSeg3OOBB3
    {
        Segment3f segment;
        OOBB3 box;
        bool solid = false;

        public int Quantity = 0;
        public IntersectionResult3 result;
        public IntersectionType Type = IntersectionType.EMPTY;

        public float SegmentParam0;
        public float SegmentParam1;
        public Vector3 Point0 = Vector3.Zero;
        public Vector3 Point1 = Vector3.Zero;

        public Segment3f Segment
        {
            get { return segment; }
            set
            {
                segment = value;
                result.status = IntersectionResult.NOTCOMPUTED;
            }
        }

        public OOBB3 Box
        {
            get { return box; }
            set
            {
                box = value;
                result.status = IntersectionResult.NOTCOMPUTED;
            }
        }

        // solid = false는 segment가 box에 완전히 포함된 경우 교차하지 않는 것으로 간주
        public bool Solid
        {
            get { return solid; }
            set
            {
                solid = value;
                result.status = IntersectionResult.NOTCOMPUTED;
            }
        }

        public bool IsSimpleIntersection
        {
            get { return result.status == IntersectionResult.INTERSECT && Type == IntersectionType.POINT; }
        }

        public IntersectSeg3OOBB3(Segment3f s, OOBB3 b, bool solidBox)
        {
            // solid = false는 segment가 box에 완전히 포함된 경우 교차하지 않는 것으로 간주
            segment = s;
            box = b;
            solid = solidBox;
        }

        public bool Compute()
        {
            return Find();
            //return this;
        }

        public bool Find()
        {
            if (result.status != IntersectionResult.NOTCOMPUTED)
            {
                return result.status == IntersectionResult.INTERSECT;
            }

            //if (default_nsegs.Direction.IsNormalized == false)
            //{
            //    type = IntersectionType.EMPTY;
            //    result.status = IntersectionResult.INVALID;
            //    return false;
            //}

            SegmentParam0 = -segment.Extent;
            SegmentParam1 = segment.Extent;
            DoClipping(ref SegmentParam0, ref SegmentParam1, segment.center, segment.direction, box,
                      ref Quantity, ref Point0, ref Point1, ref Type, solid);

            result.status = Type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT :
                IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        public bool Test(float tolerance = MathUtil.Epsilon)
        {
            Vector3 AWdU = Vector3.Zero;
            Vector3 ADdU = Vector3.Zero;
            Vector3 AWxDdU = Vector3.Zero;
            float RHS;

            Vector3 diff = segment.center - box.center;

            // 중심 연결선을 각 축에 투영하여 그 길이가 box의 extent + seg extent를 벗어나면 교차하지 않음

            AWdU[0] = Math.Abs(Vector3.Dot(segment.direction, box.axisX));
            ADdU[0] = Math.Abs(Vector3.Dot(diff, box.axisX));
            RHS = box.extent[0] + (segment.Extent * AWdU[0]);
            if (ADdU[0] > RHS)
            {
                return false;
            }

            AWdU[1] = Math.Abs(Vector3.Dot(segment.direction, box.axisY));
            ADdU[1] = Math.Abs(Vector3.Dot(diff, box.axisY));
            RHS = box.extent[1] + (segment.Extent * AWdU[1]);
            if (ADdU[1] > RHS)
            {
                return false;
            }

            AWdU[2] = Math.Abs(Vector3.Dot(segment.direction, box.axisZ));
            ADdU[2] = Math.Abs(Vector3.Dot(diff, box.axisZ));
            RHS = box.extent[2] + (segment.Extent * AWdU[2]);
            if (ADdU[2] > RHS)
            {
                return false;
            }

            // default_nsegs & diff 의 cross vector에 box의 edge들을 투영
            // 
            Vector3 WxD = segment.direction.Cross(diff);

            AWxDdU[0] = Math.Abs(Vector3.Dot(WxD, box.axisX));
            RHS = (box.extent[1] * AWdU[2]) + (box.extent[2] * AWdU[1]);
            if (AWxDdU[0] > RHS + tolerance)
            {
                return false;
            }

            AWxDdU[1] = Math.Abs(Vector3.Dot(WxD, box.axisY));
            RHS = (box.extent[0] * AWdU[2]) + (box.extent[2] * AWdU[0]);
            if (AWxDdU[1] > RHS + tolerance)
            {
                return false;
            }

            AWxDdU[2] = Math.Abs(Vector3.Dot(WxD, box.axisZ));
            RHS = (box.extent[0] * AWdU[1]) + (box.extent[1] * AWdU[0]);
            return AWxDdU[2] <= RHS + tolerance;
        }

        static public bool DoClipping(ref float t0, ref float t1,
                         Vector3 origin, Vector3 direction,
                         OOBB3 box, ref int quantity,
                         ref Vector3 point0, ref Vector3 point1,
                         ref IntersectionType intrType, bool solid)
        {
            // Convert linear component to box coordinates.
            Vector3 diff = origin - box.center;
            Vector3 BOrigin = new(
                Vector3.Dot(diff, box.axisX),
                Vector3.Dot(diff, box.axisY),
                Vector3.Dot(diff, box.axisZ)
            );
            Vector3 BDirection = new(
                Vector3.Dot(direction, box.axisX),
                Vector3.Dot(direction, box.axisY),
                Vector3.Dot(direction, box.axisZ)
            );

            float saveT0 = t0, saveT1 = t1;
            bool notAllClipped =
                Clip(+BDirection.X, -BOrigin.X - box.extent.X, ref t0, ref t1) &&
                Clip(-BDirection.X, +BOrigin.X - box.extent.X, ref t0, ref t1) &&
                Clip(+BDirection.Y, -BOrigin.Y - box.extent.Y, ref t0, ref t1) &&
                Clip(-BDirection.Y, +BOrigin.Y - box.extent.Y, ref t0, ref t1) &&
                Clip(+BDirection.Z, -BOrigin.Z - box.extent.Z, ref t0, ref t1) &&
                Clip(-BDirection.Z, +BOrigin.Z - box.extent.Z, ref t0, ref t1);

            if (notAllClipped && (solid || t0 != saveT0 || t1 != saveT1))
            {
                if (t1 > t0)
                {
                    intrType = IntersectionType.SEGMENT;
                    quantity = 2;
                    point0 = origin + (t0 * direction);
                    point1 = origin + (t1 * direction);
                }
                else
                {
                    intrType = IntersectionType.POINT;
                    quantity = 1;
                    point0 = origin + (t0 * direction);
                }
            }
            else
            {
                quantity = 0;
                intrType = IntersectionType.EMPTY;
            }

            return intrType != IntersectionType.EMPTY;
        }

        static public bool Clip(float denom, float numer, ref float t0, ref float t1)
        {
            if (denom > 0)
            {
                if (numer > denom * t1)
                {
                    return false;
                }
                if (numer > denom * t0)
                {
                    t0 = numer / denom;
                }
                return true;
            }
            else if (denom < 0)
            {
                if (numer > denom * t0)
                {
                    return false;
                }
                if (numer > denom * t1)
                {
                    t1 = numer / denom;
                }
                return true;
            }
            else
            {
                return numer <= 0;
            }
        }
    }
}
