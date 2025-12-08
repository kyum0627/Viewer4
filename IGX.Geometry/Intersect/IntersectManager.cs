using IGX.Geometry.Common;
using System;

namespace IGX.Geometry.Intersect
{
    public class IntersectManager
    {
        // 교차 계산 interval
        public Interval u; // U parameter 구간
        public Interval v; // V parameter 구간

        public int quantity = 0; // 0 : 교차없음, 1 : 점으로 교차, 2: 선으로 교차
        private Interval Intersections = Interval.Zero;

        public float Length
        {
            get
            {
                return Intersections.Length;
            }
        }

        public IntersectManager(float u0, float u1, float v0, float v1)
        {
            u = new Interval(Math.Min(u0, u1), Math.Max(u0, u1));
            v = new Interval(Math.Min(v0, v1), Math.Max(v0, v1));
        }

        public IntersectManager(Interval s, Interval t)
        {
            u = s;
            v = t;
        }

        public float GetIntersection(int i)
        {
            return Intersections[i];
        }

        public bool Find()
        {
            if (u.b < v.a || u.a > v.b)
            { // 겹치는 구간 없음
                quantity = 0;
            }
            else if (u.b > v.a)
            {
                if (u.a < v.b)
                {
                    quantity = 2;
                    Intersections.a = u.a < v.a ? v.a : u.a;
                    Intersections.b = u.b > v.b ? v.b : u.b;
                    if (Intersections.a == Intersections.b)
                    {
                        quantity = 1;
                    }
                }
                else // U.a == V.b
                {
                    quantity = 1;
                    Intersections.a = u.a;
                }
            }
            else // U.b == V.a
            {
                quantity = 1;
                Intersections.a = u.b;
            }
            return quantity > 0;
        }
    }
}
