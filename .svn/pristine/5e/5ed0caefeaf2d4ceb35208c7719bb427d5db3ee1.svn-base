using System;

namespace IGX.Geometry.Common
{
    // 구간값 [a, b]과 구간값의 parametric value [0, 1] 관리, [a, b] = [0, 1]
    // intersection 계산 interval 등 설정하고 관리
    public struct Interval
    {
        public float a;
        public float b;

        static public readonly Interval Empty = new(float.MaxValue, -float.MaxValue);
        static public readonly Interval Infinite = new(-float.MaxValue, float.MaxValue);
        static public readonly Interval Zero = new(0.0f, 0.0f);

        public Interval(float f) { a = b = f; }
        public Interval(float x, float y) { a = x; b = y; }
        public Interval(float[] v2) { a = v2[0]; b = v2[1]; }
        public Interval(Interval copy) { a = copy.a; b = copy.b; }

        /// <summary>
        /// 구간 [a, b] 의 중간 값
        /// </summary>
        public float Median // GeometryCenter
        {
            get { return (b + a) * 0.5f; }
        }

        /// <summary>
        /// fvalue 를 a ~ b 사이의 값으로 clamp
        /// </summary>
        /// <param name="fvalue"></param>
        /// <returns></returns>
        public float Clamp(float fvalue)
        {
            return fvalue < a ? a : fvalue > b ? b : fvalue;
        }

        /// <summary>
        /// fvalue 를 포함하도록 구간 확대
        /// </summary>
        /// <param name="fvalue"></param>
        public void Contain(float fvalue)
        {
            if (fvalue < a)
            {
                a = fvalue;
            }

            if (fvalue > b)
            {
                b = fvalue;
            }
        }

        /// <summary>
        /// fvalue 가 구간에 포함되는지 검토
        /// </summary>
        /// <param name="fvalue"></param>
        /// <returns></returns>
        public bool Contains(float fvalue)
        {
            return fvalue >= a && fvalue <= b;
        }

        /// <summary>
        /// fvalue를 구간 [0, 1] 사이의 parameter 값으로 interpolation & clamping
        /// </summary>
        /// <param name="fvalue"></param>
        /// <returns></returns>
        public float GetT(float fvalue)
        {
            if (fvalue <= a)
            {
                return 0f;
            }
            else if (fvalue >= b)
            {
                return 1f;
            }
            else
            {
                return a == b ? 0.5f : (fvalue - a) / (b - a);
            }
        }

        /// <summary>
        /// t [0, 1]에 해당하는 구간 [a, b] 값 linear interpolation
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public float Interpolate(float t)
        {
            return ((1 - t) * a) + (t * b);
        }

        /// <summary>
        /// 겹치는 구간 값 찾기
        /// 예: a[2, 5], b[3, 10] 이면 결과로 [3, 5] 리턴
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Interval Intersect(Interval o)
        {
            return o.a > b || o.b < a ? Empty : new Interval(Math.Max(a, o.a), Math.Min(b, o.b));
        }

        /// <summary>
        /// 구간의 시작과 끝이 같으면 true
        /// </summary>
        public bool IsConstant
        {
            get { return b == a; }
        }

        /// <summary>
        /// 구간의 길이
        /// </summary>
        public float Length
        {
            get { return b - a; }
        }

        public float Length2
        {
            get { return (a - b) * (a - b); }
        }

        /// <summary>
        /// this 구간이 o 구간과 겹치는 부분이 있으면 true
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Intersects(Interval o)
        {
            return !(o.a > b || o.b < a);
        }

        // by YJS on 2025-03-25 at 17:39
        public bool Intersects(Interval other, float tolerance)
        {
            return !(this.b + tolerance < other.a || other.b + tolerance < this.a);
        }

        public float this[int key]
        {
            get { return key == 0 ? a : b; }
            set { if (key == 0)
                {
                    a = value;
                }
                else
                {
                    b = value;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[{0:F8},{1:F8}]", a, b);
        }

        public static Interval Unsorted(float x, float y)
        {
            return x < y ? new Interval(x, y) : new Interval(y, x);
        }

        public static Interval operator +(Interval a, float f) { return new Interval(a.a + f, a.b + f); }
        public static Interval operator -(Interval v) { return new Interval(-v.a, -v.b); }
        public static Interval operator -(Interval a, float f) { return new Interval(a.a - f, a.b - f); }
        public static Interval operator *(Interval a, float f) { return new Interval(a.a * f, a.b * f); }

        public static Interval operator +(Interval a, Interval b) { return new Interval(a.a + b.a, a.b + b.b); }
        public static Interval operator -(Interval a, Interval b) { return new Interval(a.a - b.a, a.b - b.b); }
        public static Interval operator *(Interval a, Interval b) { return new Interval(a.a * b.a, a.b * b.b); }
        public static bool operator ==(Interval a, Interval b) { return a.a == b.a && a.b == b.b; }
        public static bool operator !=(Interval a, Interval b) { return a.a != b.a || a.b != b.b; }

        public override bool Equals(object? obj) => obj is Interval p && Equals(p);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ a.GetHashCode();
                hash = (hash * 16777619) ^ b.GetHashCode();
                return hash;
            }
        }
    }
}