using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistanceLine3Line3
    {
        Line3f line0;
        Line3f line1;

        public DistanceLine3Line3(Line3f l1, Line3f l2)
        {
            line1 = l2;
            line0 = l1;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 df = line0.origin - line1.origin;
            //float ld_Dot_ld = -Vector3.Dot(line0.Direction, line1.Direction);
            float ld_Dot_ld = -line0.direction.Dot(line1.direction);
            //float df_Dot_ld = Vector3.Dot(df, line0.Direction);
            float df_Dot_ld = df.Dot(line0.direction);
            float df_MagSqr = df.LengthSquared;
            float det = (float)Math.Abs(1.0f - (ld_Dot_ld * ld_Dot_ld));
            float b1, s0, s1, sqrDist;

            if (det > MathUtil.ZeroTolerance)
            {
                // 평행하지 않음
                //b1 = -Vector3.Dot(df, line1.Direction);
                b1 = -df.Dot(line1.direction);
                float invDet = 1f / det;
                s0 = ((ld_Dot_ld * b1) - df_Dot_ld) * invDet;
                s1 = ((ld_Dot_ld * df_Dot_ld) - b1) * invDet;
                sqrDist = 0f;
            }
            else
            {   // parallel, 다른 가까운 점 선택
                s0 = -df_Dot_ld;
                s1 = 0f;
                sqrDist = (df_Dot_ld * s0) + df_MagSqr;

                // 수치계산 오류 보정
                if (sqrDist < 0f)
                {
                    sqrDist = 0f;
                }
            }

            result.parameter[0] = s0;
            result.parameter2[0] = s1;
            result.closest[0] = line0.origin + (s0 * line0.direction);
            result.closest[1] = line1.origin + (s1 * line1.direction);
            result.sqrDistance = sqrDist;
            result.distance = (float)Math.Sqrt(result.sqrDistance);

            return result;
        }
    }
}
