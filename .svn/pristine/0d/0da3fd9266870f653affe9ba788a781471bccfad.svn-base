using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    class DistanceSeg2Seg2
    {
        Segment2f segment0;
        Segment2f segment1;

        public DistanceSeg2Seg2(Segment2f Segment1, Segment2f Segment2)
        {
            segment1 = Segment2;
            segment0 = Segment1;
        }

        public DistanceSeg2Seg2(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1)
        {
            segment1 = new Segment2f(q0, q1);
            segment0 = new Segment2f(p0, p1);
        }

        public Result2f Compute()
        {
            Result2f result = new();
            Vector2 diff = segment0.center - segment1.center;
            float a01 = -Vector2.Dot(segment0.direction, segment1.direction);
            float b0 = Vector2.Dot(diff, segment0.direction);
            float b1 = -Vector2.Dot(diff, segment1.direction);
            float c = diff.LengthSquared;
            float det = Math.Abs(1f - (a01 * a01));
            float s0, s1, sqrDist, extDet0, extDet1, tmpS0, tmpS1;

            if (det >= MathUtil.ZeroTolerance)
            { // 평행하지 않음
                s0 = (a01 * b1) - b0;
                s1 = (a01 * b0) - b1;
                extDet0 = segment0.extent * det;
                extDet1 = segment1.extent * det;

                if (s0 >= -extDet0)
                {
                    if (s0 <= extDet0)
                    {
                        if (s1 >= -extDet1)
                        {
                            if (s1 <= extDet1)  // region 0 (interior)
                            {
                                float invDet = 1 / det;
                                s0 *= invDet;
                                s1 *= invDet;
                                sqrDist = 0;
                            }
                            else  // region 3 (side)
                            {
                                s1 = segment1.extent;
                                tmpS0 = -((a01 * s1) + b0);
                                if (tmpS0 < -segment0.extent)
                                {
                                    s0 = -segment0.extent;
                                    sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                              (s1 * (s1 + (2 * b1))) + c;
                                }
                                else if (tmpS0 <= segment0.extent)
                                {
                                    s0 = tmpS0;
                                    sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                                }
                                else
                                {
                                    s0 = segment0.extent;
                                    sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                              (s1 * (s1 + (2 * b1))) + c;
                                }
                            }
                        }
                        else  // region 7 (side)
                        {
                            s1 = -segment1.extent;
                            tmpS0 = -((a01 * s1) + b0);
                            if (tmpS0 < -segment0.extent)
                            {
                                s0 = -segment0.extent;
                                sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                          (s1 * (s1 + (2 * b1))) + c;
                            }
                            else if (tmpS0 <= segment0.extent)
                            {
                                s0 = tmpS0;
                                sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                            }
                            else
                            {
                                s0 = segment0.extent;
                                sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                          (s1 * (s1 + (2 * b1))) + c;
                            }
                        }
                    }
                    else
                    {
                        if (s1 >= -extDet1)
                        {
                            if (s1 <= extDet1)  // region 1 (side)
                            {
                                s0 = segment0.extent;
                                tmpS1 = -((a01 * s0) + b1);
                                if (tmpS1 < -segment1.extent)
                                {
                                    s1 = -segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                                else if (tmpS1 <= segment1.extent)
                                {
                                    s1 = tmpS1;
                                    sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                                }
                                else
                                {
                                    s1 = segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                            }
                            else  // region 2 (corner)
                            {
                                s1 = segment1.extent;
                                tmpS0 = -((a01 * s1) + b0);
                                if (tmpS0 < -segment0.extent)
                                {
                                    s0 = -segment0.extent;
                                    sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                              (s1 * (s1 + (2 * b1))) + c;
                                }
                                else if (tmpS0 <= segment0.extent)
                                {
                                    s0 = tmpS0;
                                    sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                                }
                                else
                                {
                                    s0 = segment0.extent;
                                    tmpS1 = -((a01 * s0) + b1);
                                    if (tmpS1 < -segment1.extent)
                                    {
                                        s1 = -segment1.extent;
                                        sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                                  (s0 * (s0 + (2 * b0))) + c;
                                    }
                                    else if (tmpS1 <= segment1.extent)
                                    {
                                        s1 = tmpS1;
                                        sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                                    }
                                    else
                                    {
                                        s1 = segment1.extent;
                                        sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                                  (s0 * (s0 + (2 * b0))) + c;
                                    }
                                }
                            }
                        }
                        else  // region 8 (corner)
                        {
                            s1 = -segment1.extent;
                            tmpS0 = -((a01 * s1) + b0);
                            if (tmpS0 < -segment0.extent)
                            {
                                s0 = -segment0.extent;
                                sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                    (s1 * (s1 + (2 * b1))) + c;
                            }
                            else if (tmpS0 <= segment0.extent)
                            {
                                s0 = tmpS0;
                                sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                            }
                            else
                            {
                                s0 = segment0.extent;
                                tmpS1 = -((a01 * s0) + b1);
                                if (tmpS1 > segment1.extent)
                                {
                                    s1 = segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                                else if (tmpS1 >= -segment1.extent)
                                {
                                    s1 = tmpS1;
                                    sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                                }
                                else
                                {
                                    s1 = -segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                            }
                        }
                    }
                }
                else
                { // 평행
                    if (s1 >= -extDet1)
                    {
                        if (s1 <= extDet1)  // region 5 (side)
                        {
                            s0 = -segment0.extent;
                            tmpS1 = -((a01 * s0) + b1);
                            if (tmpS1 < -segment1.extent)
                            {
                                s1 = -segment1.extent;
                                sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                          (s0 * (s0 + (2 * b0))) + c;
                            }
                            else if (tmpS1 <= segment1.extent)
                            {
                                s1 = tmpS1;
                                sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                            }
                            else
                            {
                                s1 = segment1.extent;
                                sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                          (s0 * (s0 + (2 * b0))) + c;
                            }
                        }
                        else  // region 4 (corner)
                        {
                            s1 = segment1.extent;
                            tmpS0 = -((a01 * s1) + b0);
                            if (tmpS0 > segment0.extent)
                            {
                                s0 = segment0.extent;
                                sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                          (s1 * (s1 + (2 * b1))) + c;
                            }
                            else if (tmpS0 >= -segment0.extent)
                            {
                                s0 = tmpS0;
                                sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                            }
                            else
                            {
                                s0 = -segment0.extent;
                                tmpS1 = -((a01 * s0) + b1);
                                if (tmpS1 < -segment1.extent)
                                {
                                    s1 = -segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                                else if (tmpS1 <= segment1.extent)
                                {
                                    s1 = tmpS1;
                                    sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                                }
                                else
                                {
                                    s1 = segment1.extent;
                                    sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                              (s0 * (s0 + (2 * b0))) + c;
                                }
                            }
                        }
                    }
                    else   // region 6 (corner)
                    {
                        s1 = -segment1.extent;
                        tmpS0 = -((a01 * s1) + b0);
                        if (tmpS0 > segment0.extent)
                        {
                            s0 = segment0.extent;
                            sqrDist = (s0 * (s0 - (2 * tmpS0))) +
                                      (s1 * (s1 + (2 * b1))) + c;
                        }
                        else if (tmpS0 >= -segment0.extent)
                        {
                            s0 = tmpS0;
                            sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
                        }
                        else
                        {
                            s0 = -segment0.extent;
                            tmpS1 = -((a01 * s0) + b1);
                            if (tmpS1 < -segment1.extent)
                            {
                                s1 = -segment1.extent;
                                sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                          (s0 * (s0 + (2 * b0))) + c;
                            }
                            else if (tmpS1 <= segment1.extent)
                            {
                                s1 = tmpS1;
                                sqrDist = (-s1 * s1) + (s0 * (s0 + (2 * b0))) + c;
                            }
                            else
                            {
                                s1 = segment1.extent;
                                sqrDist = (s1 * (s1 - (2 * tmpS1))) +
                                          (s0 * (s0 + (2 * b0))) + c;
                            }
                        }
                    }
                }
            }
            else
            {
                float e0pe1 = segment0.extent + segment1.extent;
                float sign = a01 > 0 ? -1 : 1;
                float b0Avr = (float)0.5 * (b0 - (sign * b1));
                float lambda = -b0Avr;
                if (lambda < -e0pe1)
                {
                    lambda = -e0pe1;
                }
                else if (lambda > e0pe1)
                {
                    lambda = e0pe1;
                }

                s1 = -sign * lambda * segment1.extent / e0pe1;
                s0 = lambda + (sign * s1);
                sqrDist = (lambda * (lambda + (2 * b0Avr))) + c;
            }

            if (sqrDist < 0)
            {
                sqrDist = 0;
            }

            result.parameter[0] = s0;
            result.closest[0] = segment0.center + (s0 * segment0.direction);
            result.parameter2[0] = s1;
            result.closest[1] = segment1.center + (s1 * segment1.direction);

            result.sqrDistance = sqrDist;
            result.distance = (float)Math.Sqrt(sqrDist);

            return result;
        }
    }
}
