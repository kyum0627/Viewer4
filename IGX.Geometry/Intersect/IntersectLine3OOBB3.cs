using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine3OOBB3
    {
        Line3f line;
        OOBB3 box;

        public IntersectionResult3 result;

        public IntersectLine3OOBB3(Line3f l, OOBB3 b, float eps = MathUtil.ZeroTolerance)
        {
            line = l;
            box = b;
        }

        public bool Compute(float eps = MathUtil.ZeroTolerance)
        {
            //if (line.Direction.IsNormalized == false)
            //{
            //    result.type = IntersectionType.EMPTY;
            //    result.status = IntersectionResult.INVALID;
            //    return false;
            //}

            result.parameter1[0] = -float.MaxValue;
            result.parameter1[0] = float.MaxValue;
            Clip(true, eps);

            result.status = result.type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT :
                IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        public bool Test()
        {
            Vector3 AWdU = Vector3.Zero;
            Vector3 AWxDdU = Vector3.Zero;
            float RHS;

            Vector3 df = line.origin - box.center;
            Vector3 Dxdf = line.direction.Cross(df);

            AWdU[1] = Math.Abs(Vector3.Dot(line.direction, box.axisY));
            AWdU[2] = Math.Abs(Vector3.Dot(line.direction, box.axisZ));
            AWxDdU[0] = Math.Abs(Vector3.Dot(Dxdf, box.axisX));
            RHS = (box.extent.Y * AWdU[2]) + (box.extent.Z * AWdU[1]);
            if (AWxDdU[0] > RHS)
            {
                return false;
            }

            AWdU[0] = Math.Abs(Vector3.Dot(line.direction, box.axisX));
            AWxDdU[1] = Math.Abs(Vector3.Dot(Dxdf, box.axisY));
            RHS = (box.extent.X * AWdU[2]) + (box.extent.Z * AWdU[0]);
            if (AWxDdU[1] > RHS)
            {
                return false;
            }

            AWxDdU[2] = Math.Abs(Vector3.Dot(Dxdf, box.axisZ));
            RHS = (box.extent.X * AWdU[1]) + (box.extent.Y * AWdU[0]);
            return AWxDdU[2] <= RHS;
        }

        private bool Clip(bool solid = true, float eps = MathUtil.ZeroTolerance)
        {
            Vector3 ori = line.origin;
            Vector3 dir = line.direction;

            float t0 = result.parameter1[0];
            float t1 = result.parameter1[1];

            Vector3 dif = ori - box.center;
            Vector3 bor = new(Vector3.Dot(dif, box.axisX), Vector3.Dot(dif, box.axisY), Vector3.Dot(dif, box.axisZ));
            Vector3 bdr = new(Vector3.Dot(dir, box.axisX), Vector3.Dot(dir, box.axisY), Vector3.Dot(dir, box.axisZ));

            float saveT0 = t0;
            float saveT1 = t1;

            bool notAllClipped =
                SubClip(+bdr.X, -bor.X - box.extent.X, ref t0, ref t1, eps) &&
                SubClip(-bdr.X, +bor.X - box.extent.X, ref t0, ref t1, eps) &&
                SubClip(+bdr.Y, -bor.Y - box.extent.Y, ref t0, ref t1, eps) &&
                SubClip(-bdr.Y, +bor.Y - box.extent.Y, ref t0, ref t1, eps) &&
                SubClip(+bdr.Z, -bor.Z - box.extent.Z, ref t0, ref t1, eps) &&
                SubClip(-bdr.Z, +bor.Z - box.extent.Z, ref t0, ref t1, eps);

            if (notAllClipped && (solid || t0 != saveT0 || t1 != saveT1))
            {
                if (t1 > t0)
                {
                    result.type = IntersectionType.SEGMENT;
                    result.quantity = 2;
                    result.points[0] = ori + (t0 * dir);
                    result.points[1] = ori + (t1 * dir);
                }
                else
                {
                    result.type = IntersectionType.POINT;
                    result.quantity = 1;
                    result.points[0] = ori + (t0 * dir);
                }
            }
            else
            {
                result.quantity = 0;
                result.type = IntersectionType.EMPTY;
            }

            return result.type != IntersectionType.EMPTY;
        }

        private static bool SubClip(float denom, float numer, ref float t0, ref float t1, float eps = MathUtil.ZeroTolerance)
        {
            if (denom > 0)
            {
                if (numer - (denom * t1) > MathUtil.ZeroTolerance)
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
                if (numer - (denom * t0) > MathUtil.ZeroTolerance)
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

        //Compute2(Line3f line, OOBB3 box)
        //{
        //    // Model the line to the oriented-box coordinate system.
        //    Vector3 diff = line.Point - box.center;
        //    Vector3 lineOrigin = new Vector3 (diff.Dot(box.axisX), diff.Dot(box.axisY), diff.Dot(box.axisZ));
        //    Vector3 lineDirection = new Vector3 (Vector3.Dot(line.Direction, box.axisX), Vector3.Dot(line.Direction, box.axisY), Vector3.Dot(line.Direction, box.axisZ));

        //    result;
        //    DoQuery(lineOrigin, lineDirection, box.extent, ref result);
        //    for (int i = 0; i<result.numPoints; ++i)
        //    {
        //        result.point[i] =
        //        line.Point + result.lineParameter[i] * line.Direction;
        //    }
        //    return result;
        //}

    }
}
