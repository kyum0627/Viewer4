using System;

namespace IGX.Geometry.DataStructure
{
    public struct Index4i : IEquatable<Index4i>
    {
        public int a;
        public int b;
        public int c;
        public int d;

        static public readonly Index4i One = new(1, 1, 1, 1);
        static public readonly Index4i Max = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        static public readonly Index4i Zero = new(0, 0, 0, 0);

        public Index4i(int z) { a = b = c = d = z; }
        public Index4i(int[] i2) { a = i2[0]; b = i2[1]; c = i2[2]; d = i2[3]; }
        public Index4i(int aa, int bb, int cc, int dd) { a = aa; b = bb; c = cc; d = dd; }
        public Index4i(Index4i copy) { a = copy.a; b = copy.b; c = copy.b; d = copy.d; }

        public int[] Array { get { return new int[] { a, b, c, d }; } }

        public bool Contains(int val) { return a == val || b == val || c == val || d == val; }

        public int LengthSquared { get { return (a * a) + (b * b) + (c * c) + (d * d); } }
        public int Length { get { return (int)Math.Sqrt(LengthSquared); } }
        public void Set(Index4i o) { a = o[0]; b = o[1]; c = o[2]; d = o[3]; }
        public void Set(int aa, int bb, int cc, int dd) { a = aa; b = bb; c = cc; d = dd; }

        public void Sort()
        {
            int tmp;

            if (d < c)
            {
                tmp = d;
                d = c;
                c = tmp;
            }
            if (c < b)
            {
                tmp = c;
                c = b;
                b = tmp;
            }
            if (b < a)
            {
                tmp = b;
                b = a;
                a = tmp;
            }
            if (b > c)
            {
                tmp = c;
                c = b;
                b = tmp;
            }
            if (c > d)
            {
                tmp = d;
                d = c;
                c = tmp;
            }
            if (b > c)
            {
                tmp = c;
                c = b;
                b = tmp;
            }
        }

        public int this[int key]
        {
            get
            {
                return key == 0 ? a : key == 1 ? b : key == 2 ? c : d;
            }
            set
            {
                if (key == 0)
                {
                    a = value;
                }
                else if (key == 1)
                {
                    b = value;
                }
                else if (key == 2)
                {
                    c = value;
                }
                else
                {
                    d = value;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", a, b, c, d);
        }

        public static Index4i operator +(Index4i v, int f) { return new Index4i(v.a + f, v.b + f, v.c + f, v.d + f); }
        public static Index4i operator +(Index4i v0, Index4i v1) { return new Index4i(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c, v0.d + v1.d); }
        public static Index4i operator -(Index4i v, int f) { return new Index4i(v.a - f, v.b - f, v.c - f, v.d - f); }
        public static Index4i operator -(Index4i v0, Index4i v1) { return new Index4i(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c, v0.d - v1.d); }
        public static Index4i operator -(Index4i v) { return new Index4i(-v.a, -v.b, -v.c, -v.d); }
        public static Index4i operator *(int f, Index4i v) { return new Index4i(f * v.a, f * v.b, f * v.c, f * v.d); }
        public static Index4i operator *(Index4i v, int f) { return new Index4i(f * v.a, f * v.b, f * v.c, f * v.d); }
        public static Index4i operator *(Index4i a, Index4i b) { return new Index4i(a.a * b.a, a.b * b.b, a.c * b.c, a.d * b.d); }
        public static Index4i operator /(Index4i v, int f) { return new Index4i(v.a / f, v.b / f, v.c / f, v.d / f); }
        public static Index4i operator /(Index4i a, Index4i b) { return new Index4i(a.a / b.a, a.b / b.b, a.c / b.c, a.d / b.d); }

        //--------------------- for IEquatable --------------------

        public static bool operator ==(Index4i a, Index4i b) { return a.a == b.a && a.b == b.b && a.c == b.c && a.d == b.d; }
        public static bool operator !=(Index4i a, Index4i b) { return a.a != b.a || a.b != b.b || a.c != b.c || a.d != b.d; }

        public override bool Equals(object? obj)
        {
            return obj is Index4i other && this == other;
        }

        public bool Equals(Index4i other) { return a == other.a && b == other.b && c == other.c && d == other.d; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ a.GetHashCode();
                hash = (hash * 16777619) ^ b.GetHashCode();
                hash = (hash * 16777619) ^ c.GetHashCode();
                hash = (hash * 16777619) ^ d.GetHashCode();
                return hash;
            }
        }
    }
}
