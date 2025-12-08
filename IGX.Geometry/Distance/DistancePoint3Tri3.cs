using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistancePoint3Triangle3
    {
        Vector3 point;
        Triangle3f triangle;

        public DistancePoint3Triangle3(Vector3 PointIn, Triangle3f TriangleIn)
        {
            point = PointIn;
            triangle = TriangleIn;
        }

        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 diff = triangle.V0 - point;
            Vector3 e0 = triangle.V1 - triangle.V0;
            Vector3 e1 = triangle.V2 - triangle.V0;
            float a00 = e0.LengthSquared;
            float a01 = e0.Dot(e1);
            float a11 = e1.LengthSquared;
            float b0 = diff.Dot(e0);
            float b1 = diff.Dot(e1);
            float c = diff.LengthSquared;
            float det = Math.Abs((a00 * a11) - (a01 * a01));
            float s = (a01 * b1) - (a11 * b0);
            float t = (a01 * b0) - (a00 * b1);
            float sqrDistance;

            if (s + t <= det)
            {
                if (s < 0)
                {
                    if (t < 0)  // region 4
                    {
                        if (b0 < 0)
                        {
                            t = 0;
                            if (-b0 >= a00)
                            {
                                s = 1;
                                sqrDistance = a00 + (2 * b0) + c;
                            }
                            else
                            {
                                s = -b0 / a00;
                                sqrDistance = (b0 * s) + c;
                            }
                        }
                        else
                        {
                            s = 0;
                            if (b1 >= 0)
                            {
                                t = 0;
                                sqrDistance = c;
                            }
                            else if (-b1 >= a11)
                            {
                                t = 1;
                                sqrDistance = a11 + (2 * b1) + c;
                            }
                            else
                            {
                                t = -b1 / a11;
                                sqrDistance = (b1 * t) + c;
                            }
                        }
                    }
                    else  // region 3
                    {
                        s = 0;
                        if (b1 >= 0)
                        {
                            t = 0;
                            sqrDistance = c;
                        }
                        else if (-b1 >= a11)
                        {
                            t = 1;
                            sqrDistance = a11 + (2 * b1) + c;
                        }
                        else
                        {
                            t = -b1 / a11;
                            sqrDistance = (b1 * t) + c;
                        }
                    }
                }
                else if (t < 0)  // region 5
                {
                    t = 0;
                    if (b0 >= 0)
                    {
                        s = 0;
                        sqrDistance = c;
                    }
                    else if (-b0 >= a00)
                    {
                        s = 1;
                        sqrDistance = a00 + (2 * b0) + c;
                    }
                    else
                    {
                        s = -b0 / a00;
                        sqrDistance = (b0 * s) + c;
                    }
                }
                else  // region 0
                {
                    float invDet = 1 / det;
                    s *= invDet;
                    t *= invDet;
                    sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
                                  (t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
                }
            }
            else
            {
                float tmp0, tmp1, numer, denom;

                if (s < 0)  // region 2
                {
                    tmp0 = a01 + b0;
                    tmp1 = a11 + b1;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = a00 - (2 * a01) + a11;
                        if (numer >= denom)
                        {
                            s = 1;
                            t = 0;
                            sqrDistance = a00 + (2 * b0) + c;
                        }
                        else
                        {
                            s = numer / denom;
                            t = 1 - s;
                            sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
                                          (t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
                        }
                    }
                    else
                    {
                        s = 0;
                        if (tmp1 <= 0)
                        {
                            t = 1;
                            sqrDistance = a11 + (2 * b1) + c;
                        }
                        else if (b1 >= 0)
                        {
                            t = 0;
                            sqrDistance = c;
                        }
                        else
                        {
                            t = -b1 / a11;
                            sqrDistance = (b1 * t) + c;
                        }
                    }
                }
                else if (t < 0)  // region 6
                {
                    tmp0 = a01 + b1;
                    tmp1 = a00 + b0;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = a00 - (2 * a01) + a11;
                        if (numer >= denom)
                        {
                            t = 1;
                            s = 0;
                            sqrDistance = a11 + (2 * b1) + c;
                        }
                        else
                        {
                            t = numer / denom;
                            s = 1 - t;
                            sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
                                          (t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
                        }
                    }
                    else
                    {
                        t = 0;
                        if (tmp1 <= 0)
                        {
                            s = 1;
                            sqrDistance = a00 + (2 * b0) + c;
                        }
                        else if (b0 >= 0)
                        {
                            s = 0;
                            sqrDistance = c;
                        }
                        else
                        {
                            s = -b0 / a00;
                            sqrDistance = (b0 * s) + c;
                        }
                    }
                }
                else  // region 1
                {
                    numer = a11 + b1 - a01 - b0;
                    if (numer <= 0)
                    {
                        s = 0;
                        t = 1;
                        sqrDistance = a11 + (2 * b1) + c;
                    }
                    else
                    {
                        denom = a00 - (2 * a01) + a11;
                        if (numer >= denom)
                        {
                            s = 1;
                            t = 0;
                            sqrDistance = a00 + (2 * b0) + c;
                        }
                        else
                        {
                            s = numer / denom;
                            t = 1 - s;
                            sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
                                          (t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
                        }
                    }
                }
            }
            result.sqrDistance = sqrDistance;
            result.distance = (float)Math.Sqrt(sqrDistance);
            result.closest[0] = triangle.V0 + (s * e0) + (t * e1);
            result.parameter = new float[3] { 1 - s - t, s, t };

            return result;
        }
    }
}
