using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Intersect
{
    public class IntersectRay3OOBB3
    {
        Ray3f ray;
        OOBB3 box;

        IntersectionResult3 result;

        //public int quantity = 0;
        //public IntersectionResult Result = IntersectionResult.NOTCOMPUTED;
        //public IntersectionType type = IntersectionType.EMPTY;

        public Ray3f Ray
        {
            get { return ray; }
            set
            {
                ray = value;
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

        public bool IsSimpleIntersection
        {
            get { return result.status == IntersectionResult.INTERSECT && result.type == IntersectionType.POINT; }
        }

        public float RayParam0;
        public float RayParam1;
        public Vector3 Point0 = Vector3.Zero;
        public Vector3 Point1 = Vector3.Zero;

        public IntersectRay3OOBB3(Ray3f r, OOBB3 b, float eps = MathUtil.ZeroTolerance)
        {
            ray = r;
            box = b;
            Compute(eps);
        }

        public bool Compute(float eps = MathUtil.ZeroTolerance)
        {
            return Find(eps);
            //return this;
        }

        public bool Find(float eps = MathUtil.ZeroTolerance)
        {
            if (result.status != IntersectionResult.NOTCOMPUTED)
            {
                return result.status == IntersectionResult.INTERSECT;
            }

            RayParam0 = 0.0f;
            RayParam1 = float.MaxValue;

            DoClip(true, eps);

            result.status = result.type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT : IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        public bool Test()
        {
            return Intersects(ref ray, ref box);
        }

        public static bool Intersects(ref Ray3f ray, ref OOBB3 box, float eps = MathUtil.ZeroTolerance)
        {
            Vector3 WdU = Vector3.Zero;
            Vector3 AWdU = Vector3.Zero;
            Vector3 DdU = Vector3.Zero;
            Vector3 ADdU = Vector3.Zero;
            Vector3 AWxDdU = Vector3.Zero;
            float RHS;

            Vector3 diff = ray.origin - box.center;
            Vector3 extent = box.extent + new Vector3(eps, eps, eps);

            WdU[0] = Vector3.Dot(ray.direction, box.axisX);
            AWdU[0] = Math.Abs(WdU[0]);
            DdU[0] = Vector3.Dot(diff, box.axisX);
            ADdU[0] = Math.Abs(DdU[0]);
            if (ADdU[0] > extent.X && DdU[0] * WdU[0] >= 0)
            {
                return false;
            }

            WdU[1] = Vector3.Dot(ray.direction, box.axisY);
            AWdU[1] = Math.Abs(WdU[1]);
            DdU[1] = Vector3.Dot(diff, box.axisY);
            ADdU[1] = Math.Abs(DdU[1]);
            if (ADdU[1] > extent.Y && DdU[1] * WdU[1] >= 0)
            {
                return false;
            }

            WdU[2] = Vector3.Dot(ray.direction, box.axisZ);
            AWdU[2] = Math.Abs(WdU[2]);
            DdU[2] = Vector3.Dot(diff, box.axisZ);
            ADdU[2] = Math.Abs(DdU[2]);
            if (ADdU[2] > extent.Z && DdU[2] * WdU[2] >= 0)
            {
                return false;
            }

            Vector3 WxD = ray.direction.Cross(diff);

            AWxDdU[0] = Math.Abs(Vector3.Dot(WxD, box.axisX));
            RHS = (extent.Y * AWdU[2]) + (extent.Z * AWdU[1]);
            if (AWxDdU[0] > RHS)
            {
                return false;
            }

            AWxDdU[1] = Math.Abs(Vector3.Dot(WxD, box.axisY));
            RHS = (extent.X * AWdU[2]) + (extent.Z * AWdU[0]);
            if (AWxDdU[1] > RHS)
            {
                return false;
            }

            AWxDdU[2] = Math.Abs(Vector3.Dot(WxD, box.axisZ));
            RHS = (extent.X * AWdU[1]) + (extent.Y * AWdU[0]);
            return AWxDdU[2] <= RHS;
        }

        public bool DoClip(bool solid = true, float eps = MathUtil.ZeroTolerance)
        {
            Vector3 ori = ray.origin;
            Vector3 dir = ray.direction;

            float t0 = result.parameter1[0];
            float t1 = result.parameter1[1];

            Vector3 dif = ori - box.center;
            Vector3 bor = new(Vector3.Dot(dif, box.axisX), Vector3.Dot(dif, box.axisY), Vector3.Dot(dif, box.axisZ));
            Vector3 bdr = new(Vector3.Dot(dir, box.axisX), Vector3.Dot(dir, box.axisY), Vector3.Dot(dir, box.axisZ));

            float saveT0 = t0;
            float saveT1 = t1;

            bool notAllClipped =
                Clip(+bdr.X, -bor.X - box.extent.X, ref t0, ref t1, eps) &&
                Clip(-bdr.X, +bor.X - box.extent.X, ref t0, ref t1, eps) &&
                Clip(+bdr.Y, -bor.Y - box.extent.Y, ref t0, ref t1, eps) &&
                Clip(-bdr.Y, +bor.Y - box.extent.Y, ref t0, ref t1, eps) &&
                Clip(+bdr.Z, -bor.Z - box.extent.Z, ref t0, ref t1, eps) &&
                Clip(-bdr.Z, +bor.Z - box.extent.Z, ref t0, ref t1, eps);

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

        static public bool Clip(float denom, float numer, ref float t0, ref float t1, float eps = MathUtil.ZeroTolerance)
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
    }
}
