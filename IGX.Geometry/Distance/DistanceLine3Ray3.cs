using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    // https://www.geometrictools.com/Downloads/Downloads.html

    class DistanceLine3Ray3
    {
        Line3f line;
        Ray3f ray;

        public DistanceLine3Ray3(Line3f LineIn, Ray3f rayIn)
        {
            ray = rayIn;
            line = LineIn;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 df = line.origin - ray.origin;
            //float ld_Dot_rd = -Vector3.Dot(line.Direction, ray.Direction);
            //float df_Dot_ld = Vector3.Dot(df, line.Direction);
            float ld_Dot_rd = -line.direction.Dot(ray.direction);
            float df_Dot_ld = df.Dot(line.direction);
            float df_MagSqr = df.LengthSquared;
            float det = (float)Math.Abs(1.0 - (ld_Dot_rd * ld_Dot_rd));
            float df_Dot_rd, s0, s1, sqrDist;

            if (det >= MathUtil.ZeroTolerance)
            {
                //df_Dot_rd = -Vector3.Dot(df, ray.Direction);
                df_Dot_rd = -df.Dot(ray.direction);
                s1 = (ld_Dot_rd * df_Dot_ld) - df_Dot_rd;

                if (s1 >= 0)
                {
                    float invDet = 1 / det;
                    s0 = ((ld_Dot_rd * df_Dot_rd) - df_Dot_ld) * invDet;
                    s1 *= invDet;
                    sqrDist = (s0 * (s0 + (ld_Dot_rd * s1) + (2 * df_Dot_ld))) +
                        (s1 * ((ld_Dot_rd * s0) + s1 + (2 * df_Dot_rd))) + df_MagSqr;
                }
                else
                {
                    s0 = -df_Dot_ld;
                    s1 = 0;
                    sqrDist = (df_Dot_ld * s0) + df_MagSqr;
                }
            }
            else
            {
                s0 = -df_Dot_ld;
                s1 = 0;
                sqrDist = (df_Dot_ld * s0) + df_MagSqr;
            }

            result.closest[0] = line.origin + (s0 * line.direction);
            result.closest[1] = ray.origin + (s1 * ray.direction);
            result.parameter[0] = s0;
            result.parameter2[1] = s1;

            if (sqrDist < 0)
            {
                sqrDist = 0;
            }
            result.sqrDistance = sqrDist;
            result.distance = (float)Math.Sqrt(sqrDist);

            return result;
        }
    }
}
