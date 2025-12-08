using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectSeg2Triangle2
    {
        Segment2f segment;
        Triangle2 triangle;

        public Vector2 polit0;
        public Vector2 point1;
        public float param0;
        public float param1;

        public int qualtity = 0;
        public IntersectionResult Result = IntersectionResult.NOTCOMPUTED;
        public IntersectionType Type = IntersectionType.EMPTY;

        public Segment2f Segment
        {
            get
            {
                return segment;
            }
            set
            {
                segment = value;
                Result = IntersectionResult.NOTCOMPUTED;
            }
        }

        public Triangle2 Triangle
        {
            get
            {
                return triangle;
            }
            set
            {
                triangle = value;
                Result = IntersectionResult.NOTCOMPUTED;
            }
        }

        public bool IsSimpleIntersection
        {
            get { return Result == IntersectionResult.INTERSECT && Type == IntersectionType.POINT; }
        }

        public IntersectSeg2Triangle2(Segment2f s, Triangle2 t)
        {
            segment = s;
            triangle = t;
        }

        public bool Compute()
        {
            // 1. 규칙에 맞게 제대로 만들어진 segment인가?
            if (!segment.direction.IsNormalized())
            {
                Type = IntersectionType.EMPTY;
                Result = IntersectionResult.INVALID;
                return false;
            }

            // 2. 교차 가능한지 검사
            float[] dist = new float[3] { 0, 0, 0 };// Vector3.Zero;
            int[] sign = new int[3] { 0, 0, 0 }; // Vector3i.Zero;
            int pos = 0, neg = 0, zero = 0;

            IntersectLine2Triangle2.Classify(segment.center, segment.direction, triangle, ref dist, ref sign, ref pos, ref neg, ref zero);

            // 3. 교차 형태 검사
            if (pos == 3 || neg == 3)
            {   // 3-1. 삼각형의 세 점 모두 segment의 한쪽에 치우쳐 있으면 교차 없음
                qualtity = 0;
                Type = IntersectionType.EMPTY;
            }
            else
            {   // 3-2. 교차 가능성 있으므로 uniqueindex 의 세 정점의 projection parameter와 default_nsegs [-extent, extent] 구간 비교
                float[] param = new float[2] { 0, 0 };

                // 3-2-1. uniqueindex 의 세 정점의 parameter interval 계산
                IntersectLine2Triangle2.GetInterval(segment.center, segment.direction, triangle, ref dist, ref sign, ref param);

                // 3-2-1. uniqueindex parameter 구간과 default_nsegs [-extent, extent] 구간이 겹치는 부분 계산
                IntersectManager intr = new(param[0], param[1], -segment.extent, +segment.extent);
                intr.Find();

                // 구간 만남? 혹은 점 만남?
                qualtity = intr.quantity;

                if (qualtity == 2)
                {
                    // 구간 만남, Segment 교차
                    Type = IntersectionType.SEGMENT;
                    param0 = intr.GetIntersection(0);
                    polit0 = segment.center + (param0 * segment.direction);
                    param1 = intr.GetIntersection(1);
                    point1 = segment.center + (param1 * segment.direction);
                }
                else if (qualtity == 1)
                {
                    // 점 만남
                    Type = IntersectionType.POINT;
                    param0 = intr.GetIntersection(0);
                    polit0 = segment.center + (param0 * segment.direction);
                }
                else
                {
                    // 교차 없음.
                    Type = IntersectionType.EMPTY;
                }
            }

            Result = Type != IntersectionType.EMPTY ?
                IntersectionResult.INTERSECT :
                IntersectionResult.NOTINTERSECT;

            return Result == IntersectionResult.INTERSECT;
        }
    }
}
