using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine2Triangle2
    {
        Line2f line;
        Triangle2 triangle;

        public IntersectionResult2 result;

        public IntersectLine2Triangle2(Line2f line, Triangle2 triangle)
        {
            this.line = line;
            this.triangle = triangle;
        }

        public bool Compute()
        {
            // 2차원 평면상 기준선을 중심으로 비교 대상 도형의 점이 놓여있는 방향과 수량을 검토하여 nID, pID, z를 Continuity 
            //      nID = numNegative,
            //      pID = numPositive,
            //      z = numZero.
            // 하기 table과 같이 검토하면 교차 유형을 알 수 있음
            //   nID pID z  intersection
            //   ------------------------------------
            //   0 3 0  none, 세 점 모두 직선의 좌측 
            //   0 2 1  vertex, 두 점은 좌측, 한 점은 직선상에
            //   0 1 2  hedge, 한 점은 좌측, 두 점은 직선상에
            //   0 0 3  none (degenerate triangle), 삼각형이 아님, 직선
            //   1 2 0  default_nsegs (2 halfedgeSet 교차)
            //   1 1 1  default_nsegs (1 hedge 교차)
            //   1 0 2  hedge
            //   2 1 0  default_nsegs (2 halfedgeSet 교차)
            //   2 0 1  vertex
            //   3 0 0  none

            // 이미 계산한 놈이면 return
            if (result.status != IntersectionResult.NOTCOMPUTED)
            {
                return result.status == IntersectionResult.INTERSECT;
            }

            // 아직 계산하지 않은 놈이면
            if (!line.Direction.IsNormalized())
            { // 정규화된 line 유형이 아니면, 즉, Direction vector가 단위 벡터가 아니면
                result.type = IntersectionType.EMPTY;
                result.status = IntersectionResult.INVALID;
                return false;
            }

            int positive = 0, negative = 0, zero = 0;
            float[] dist = new float[3] { 0, 0, 0 }; // Vector3.Zero;
            int[] sign = new int[3] { 0, 0, 0 }; // Vector3i.Zero;

            Classify(line.Point, line.Direction, triangle, ref dist, ref sign, ref positive, ref negative, ref zero);

            if (positive == 3 || negative == 3)
            {
                // 삼각형의 세 점이 모두 직선의 좌측 혹은 우측에 존재하면 서로 교차하지 않음
                result.quantity = 0;
                result.type = IntersectionType.EMPTY;
            }
            else
            {
                float[] param = new float[2] { 0, 0 };
                GetInterval(line.Point, line.Direction, triangle, ref dist, ref sign, ref param);

                IntersectManager intr = new(param[0], param[1], -float.MaxValue, +float.MaxValue);
                intr.Find();

                result.quantity = intr.quantity;
                if (result.quantity == 2)
                {
                    // 선분 교차
                    result.type = IntersectionType.SEGMENT;
                    result.parameter1[0] = intr.GetIntersection(0);
                    result.points = new List<Vector2>
                    {
                        line.Point + (result.parameter1[0] * line.Direction)
                    };
                    result.parameter1[1] = intr.GetIntersection(1);
                    result.points[1] = line.Point + (result.parameter1[1] * line.Direction);
                }
                else if (result.quantity == 1)
                {
                    // 점 교차
                    result.type = IntersectionType.POINT;
                    result.parameter1[0] = intr.GetIntersection(0);
                    result.points = new List<Vector2>
                    {
                        line.Point + (result.parameter1[0] * line.Direction)
                    };
                }
                else
                {
                    // 교차 없음
                    result.type = IntersectionType.EMPTY;
                }
            }

            result.status = result.type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT :
                IntersectionResult.NOTINTERSECT;

            return result.status == IntersectionResult.INTERSECT;
        }

        /// <summary>
        /// barycenter를 이용하여 교차점의 윛 판단
        /// </summary>
        /// <param name="ori">직선의 원점</param>
        /// <param name="dir">직선의 방향, 단위 벡터</param>
        /// <param name="tri">교차계산 대상 삼각형</param>
        /// <param name="s">교차점의 parameter</param>
        /// <param name="sign"></param>
        /// <param name="pos">left point 수</param>
        /// <param name="neg">right point 수</param>
        /// <param name="zero">on line point 수</param>
        public static void Classify(Vector2 ori, Vector2 dir, Triangle2 tri, ref float[] s, ref int[] sign, ref int pos, ref int neg, ref int zero)
        {
            pos = neg = zero = 0;
            for (int i = 0; i < 3; ++i)
            {
                Vector2 df = tri[i] - ori;
                s[i] = df.PerpDot(dir);
                if (s[i] > MathUtil.Epsilon)
                {
                    sign[i] = 1;
                    ++pos;
                }
                else if (s[i] < -MathUtil.Epsilon)
                {
                    sign[i] = -1;
                    ++neg;
                }
                else
                {
                    s[i] = 0.0f;
                    sign[i] = 0;
                    ++zero;
                }
            }
        }

        /// <summary>
        /// 교점의 parameter 값을 인자로 return
        /// </summary>
        /// <param name="ori">선의 원점</param>
        /// <param name="dir">선의 방향</param>
        /// <param name="tri">검사할 삼각형</param>
        /// <param name="dist">삼각형의 꼭지점의 DistanceManager parameter</param>
        /// <param name="sign"></param>
        /// <param name="param">직선 원점에서 꼭지점까지 parametric 거리</param>
        public static void GetInterval(Vector2 ori, Vector2 dir, Triangle2 tri, ref float[] dist, ref int[] sign, ref float[] param)
        {
            float[] proj = new float[3] { 0, 0, 0 };//.Zero;
            int i;

            for (i = 0; i < 3; ++i) // 삼각형의 꼭지점들을 ori, dir 선위에 투영 (평면 벡터)
            {
                Vector2 df = tri[i] - ori;
                proj[i] = Vector2.Dot(dir, df);
            }

            // 선과 삼각형의 횡방향 edge간 교차점 계산
            float numer, denom;
            int i0, i1, i2;
            int quantity = 0;

            for (i0 = 2, i1 = 0; i1 < 3; i0 = i1++)
            {
                if (sign[i0] * sign[i1] < 0) // 부호가 반대방향이면 평면의 상하에 존재하므로 교차 가능
                {
                    if (quantity >= 2)
                    {
                        throw new Exception("교점 계산 불가");
                    }

                    numer = (dist[i0] * proj[i1]) - (dist[i1] * proj[i0]);
                    denom = dist[i0] - dist[i1];
                    param[quantity++] = numer / denom;
                }
            }

            if (quantity < 2)
            {
                for (i0 = 1, i1 = 2, i2 = 0; i2 < 3; i0 = i1, i1 = i2++)
                {
                    if (sign[i2] == 0)
                    {
                        if (quantity >= 2)
                        {
                            throw new Exception("교점 계산 불가");
                        }

                        param[quantity++] = proj[i2];
                    }
                }
            }

            if (quantity < 1)
            {
                throw new Exception("교점 없음");
            }

            if (quantity == 2)
            {
                if (param[0] > param[1])
                {
                    float save = param[0];
                    param[0] = param[1];
                    param[1] = save;
                }
            }
            else
            {
                param[1] = param[0];
            }
        }

        public IntersectionType GetIntersectionType()
        {
            float[] dist = new float[3] { 0, 0, 0 };
            int[] sign = new int[3] { 0, 0, 0 };

            int pos = 0; // positive
            int neg = 0; // negative
            int zero = 0;

            Classify(line.Point, line.Direction, triangle, ref dist, ref sign, ref pos, ref neg, ref zero);

            if (pos == 3 || neg == 3)
            {
                result.type = IntersectionType.EMPTY;
            }
            else
            {
                float[] param = new float[2] { 0, 0 };
                GetInterval(line.Point, line.Direction, triangle, ref dist, ref sign, ref param);

                IntersectManager intr = new(param[0], param[1], -float.MaxValue, +float.MaxValue);
                intr.Find();

                result.quantity = intr.quantity;

                if (result.quantity == 2)
                {
                    result.type = IntersectionType.SEGMENT;
                }
                else if (result.quantity == 1)
                {
                    result.type = IntersectionType.POINT;
                }
                else
                {
                    result.type = IntersectionType.EMPTY;
                }
            }

            return result.type;
        }
    }
}
