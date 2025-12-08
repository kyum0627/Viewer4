using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Distance
{
    public class DistanceSeg3Seg3
    {
        Segment3f segment1;
        Segment3f segment2;

        public DistanceSeg3Seg3(Segment3f s1, Segment3f s2)
        {
            segment1 = s1;
            segment2 = s2;
        }

        public Result3f Compute()
        {
            // conventional한 방법은, s, t를 각각 < 0, = 0, > 0으로 9개의 구역으로 나누어 계산하나
            // division error가 발생하는 경우가 많으므로 .... 추천하지 않음
            // 따라서, 보다 강건한 알고리즘이라고 알려져 있는 Dan Sunday의 알고리즘을 적용
            //
            // P(s) = (1-s) * P0 + s * P1
            // Q(t) = (1-t) * Q0 + t * Q1
            // Segment P, Q 위의 두 점간의 거리의 제곱을 나타내는 식은 다음과 같음
            // Radius[s, t] = | P(s) - Q(t) |^2 
            //         = a*s^2 - 2*b*s*t + c*t^2 + 2*d*s - 2*e*t + f
            // 여기서, a, b, c, c, d, e, f는 각각 다음과 같음
            //   a = Dot(P1-P0,P1-P0), b = Dot(P1-P0,Q1-Q0), c = Dot(Q1-Q0,Q1-Q0),
            //   d = Dot(P1-P0,P0-Q0), e = Dot(Q1-Q0,P0-Q0), f = Dot(P0-Q0,P0-Q0)
            // P0, P1 및 Q0, Q1 은 같은 점일 수 있음.
            //   a*c - b^2 = | Cross (P1-P0, Q1-Q0) |^2 는 항상 0보다 크거나 같음 (거리의 제곱)
            // 1. 두 선분이 평행하지 않으면(linear independent)
            //   즉, a*c - b^2 > 0 이면
            //   Radius(s, t) 는 paraboloid 이며, Level은 타원 함수로 정의됨
            // 2. 두 선분이 평행하면
            //   즉, a*c - b^2 = 0 이면
            //   Radius(s,t)는 parabollic cylinder 이며, Level 은 line으로 정의됨
            // 따라서, [0, 1]^2 구간에서 Radius(s,t)의 최소를 찾으면 ... OK

            Result3f result = new();

            Vector3 P0 = segment1.P0;
            Vector3 P1 = segment1.P1;
            Vector3 Q0 = segment2.P0;
            Vector3 Q1 = segment2.P1;

            Vector3 P1mP0 = P1 - P0;
            Vector3 Q1mQ0 = Q1 - Q0;
            Vector3 P0mQ0 = P0 - Q0;

            float mA = P1mP0.Dot(P1mP0);
            float mB = P1mP0.Dot(Q1mQ0);
            float mC = Q1mQ0.Dot(Q1mQ0);
            float mD = P1mP0.Dot(P0mQ0);
            float mE = Q1mQ0.Dot(P0mQ0);

            float mF00 = mD;
            float mF10 = mF00 + mA;
            float mF01 = mF00 - mB;
            float mF11 = mF10 - mB;

            float mG00 = -mE;
            float mG10 = mG00 - mB;
            float mG01 = mG00 + mC;
            float mG11 = mG10 + mC;

            if (mA > 0f && mC > 0f)
            {
                // dR/ds(s0,0) = 0 및 dR/ds(s1,1) = 0 계산
                // s축 상의 sl의 위치는 
                // sI <= 0 : classifyI = -1
                // sI >= 1 : classifyI = 1
                // 0 < sI < 1 : classifyI = 0
                // 이 값을 이용하여 minimum point (s,t)를 찾음
                // 구간 {0,1}에서 fij = dR/ds(i,j)

                float[] sValue = new float[2];

                sValue[0] = GetClampedRoot(mA, mF00, mF10);
                sValue[1] = GetClampedRoot(mA, mF01, mF11);

                int[] classify = new int[2];
                for (int i = 0; i < 2; ++i)
                {
                    if (sValue[i] <= 0f)
                    {
                        classify[i] = -1;
                    }
                    else if (sValue[i] >= 1f)
                    {
                        classify[i] = +1;
                    }
                    else
                    {
                        classify[i] = 0;
                    }
                }

                if (classify[0] == -1 && classify[1] == -1)
                {
                    // s = 0, 0 <= t <= 1 일때 최소
                    result.parameter[0] = 0f;
                    result.parameter[1] = GetClampedRoot(mC, mG00, mG01);
                }
                else if (classify[0] == +1 && classify[1] == +1)
                {
                    // on s = 1 for 0 <= t <= 1 일 때 최소
                    result.parameter[0] = 1f;
                    result.parameter[1] = GetClampedRoot(mC, mG10, mG11);
                }
                else
                {
                    int[] edge = new int[2];
                    float[,] end = new float[2, 2];

                    ComputeIntersection(sValue, classify, ref edge, ref end, mF00, mB, mF10);
                    ComputeMinimumParameters(edge, end, ref result.parameter, mB, mC, mE, mG00, mG01, mG10, mG11);
                }
            }
            else
            {
                if (mA > 0f)
                {
                    result.parameter[0] = GetClampedRoot(mA, mF00, mF10);
                    result.parameter[1] = 0f;
                }
                else if (mC > 0f)
                {
                    result.parameter[0] = 0f;
                    result.parameter[1] = GetClampedRoot(mC, mG00, mG01);
                }
                else
                {
                    result.parameter[0] = 0f;
                    result.parameter[1] = 0f;
                }
            }

            result.closest[0] = ((1f - result.parameter[0]) * P0) + (result.parameter[0] * P1);
            result.closest[1] = ((1f - result.parameter[1]) * Q0) + (result.parameter[1] * Q1);
            Vector3 diff = result.closest[0] - result.closest[1];
            result.sqrDistance = diff.Dot(diff);
            result.distance = (float)Math.Sqrt(result.sqrDistance);
            return result;
        }

        private float GetClampedRoot(float slope, float h0, float h1)
        {
            float r;
            if (h0 < 0f)
            {
                if (h1 > 0f)
                {
                    r = -h0 / slope;
                    if (r > 1f)
                    {
                        r = 0.5f;
                    }
                }
                else
                {
                    r = 1f;
                }
            }
            else
            {
                r = 0f;
            }
            return r;
        }

        /// <summary>
        /// conjugate gradient algorithm for a quadratic private void .
        /// [0, 1]^2 구간에서 dR/ds = 0, 즉,
        /// line의 Direction dR/ds 은 (1,0)에 대한 conjugate,
        /// </summary>
        /// <param name="sValue"></param>
        /// <param name="classify"></param>
        /// <param name="edge"></param>
        /// <param name="end"></param>
        private void ComputeIntersection(float[] sValue, int[] classify, ref int[] edge, ref float[,] end,
            float mF00, float mB, float mF10)
        {
            // 나누기는 이론적으로 파라메터 구간[0, 1] 내에서만 수행되어야 하나,
            // 구간을 벗어나는 경우 수치계산 오류가 발생.
            // 이는 분자와 분모가 거의 0에 가까운 경우, 즉,
            // 분모는 두 세그먼트가 거의 수직에 가까운 경우 0에 근접하며
            // 분자는 P-segment가 매우 작은 경우 0에 근접
            // 이를 피하기 위해서 bisection 방법을 사용하여 해를 찾지만 계산 속도 저하를 가져옴
            if (classify[0] < 0f)
            {
                edge[0] = 0;
                end[0, 0] = 0f;
                end[0, 1] = mF00 / mB;
                if (end[0, 1] < 0f || end[0, 1] > 1)
                {
                    end[0, 1] = 0.5f;
                }

                if (classify[1] == 0)
                {
                    edge[1] = 3;
                    end[1, 0] = sValue[1];
                    end[1, 1] = 1f;
                }
                else  // classify[1] > 0
                {
                    edge[1] = 1;
                    end[1, 0] = 1f;
                    end[1, 1] = mF10 / mB;
                    if (end[1, 1] < 0f || end[1, 1] > 1f)
                    {
                        end[1, 1] = 0.5f;
                    }
                }
            }
            else if (classify[0] == 0)
            {
                edge[0] = 2;
                end[0, 0] = sValue[0];
                end[0, 1] = 0f;

                if (classify[1] < 0)
                {
                    edge[1] = 0;
                    end[1, 0] = 0f;
                    end[1, 1] = mF00 / mB;
                    if (end[1, 1] < 0f || end[1, 1] > 1f)
                    {
                        end[1, 1] = 0.5f;
                    }
                }
                else if (classify[1] == 0)
                {
                    edge[1] = 3;
                    end[1, 0] = sValue[1];
                    end[1, 1] = 1f;
                }
                else
                {
                    edge[1] = 1;
                    end[1, 0] = 1f;
                    end[1, 1] = mF10 / mB;
                    if (end[1, 1] < 0f || end[1, 1] > 1f)
                    {
                        end[1, 1] = 0.5f;
                    }
                }
            }
            else  // classify[0] > 0
            {
                edge[0] = 1;
                end[0, 0] = 1f;
                end[0, 1] = mF10 / mB;
                if (end[0, 1] < 0f || end[0, 1] > 1f)
                {
                    end[0, 1] = 0.5f;
                }

                if (classify[1] == 0)
                {
                    edge[1] = 3;
                    end[1, 0] = sValue[1];
                    end[1, 1] = 1f;
                }
                else
                {
                    edge[1] = 0;
                    end[1, 0] = 0f;
                    end[1, 1] = mF00 / mB;
                    if (end[1, 1] < 0f || end[1, 1] > 1f)
                    {
                        end[1, 1] = 0.5f;
                    }
                }
            }
        }

        private void ComputeMinimumParameters(int[] edge, float[,] end, ref float[] parameter, float mB, float mC, float mE, float mG00, float mG01, float mG10, float mG11)
        {
            float delta = end[1, 1] - end[0, 1];
            float h0 = delta * ((-mB * end[0, 0]) + (mC * end[0, 1]) - mE);
            if (h0 >= 0f)
            {
                if (edge[0] == 0)
                {
                    parameter[0] = 0f;
                    parameter[1] = GetClampedRoot(mC, mG00, mG01);
                }
                else if (edge[0] == 1)
                {
                    parameter[0] = 1f;
                    parameter[1] = GetClampedRoot(mC, mG10, mG11);
                }
                else
                {
                    parameter[0] = end[0, 0];
                    parameter[1] = end[0, 1];
                }
            }
            else
            {
                float h1 = delta * ((-mB * end[1, 0]) + (mC * end[1, 1]) - mE);
                if (h1 <= 0f)
                {
                    if (edge[1] == 0)
                    {
                        parameter[0] = 0f;
                        parameter[1] = GetClampedRoot(mC, mG00, mG01);
                    }
                    else if (edge[1] == 1)
                    {
                        parameter[0] = 1f;
                        parameter[1] = GetClampedRoot(mC, mG10, mG11);
                    }
                    else
                    {
                        parameter[0] = end[1, 0];
                        parameter[1] = end[1, 1];
                    }
                }
                else  // h0 < 0 and h1 > 0
                {
                    float z = (float)Math.Min(Math.Max(h0 / (h0 - h1), 0f), 1f);
                    float omz = 1f - z;
                    parameter[0] = (omz * end[0, 0]) + (z * end[1, 0]);
                    parameter[1] = (omz * end[0, 1]) + (z * end[1, 1]);
                }
            }
        }
    }
}
