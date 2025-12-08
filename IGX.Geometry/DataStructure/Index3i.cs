using System;

namespace IGX.Geometry.DataStructure
{
    //// 3D 모델의 정점들을 저장하는 배열에 사용한 사례
    //Vector3[] uniqpos = new Vector3[] 
    //{ 
    //    new Vector3(0, 0, 0),   // 정점 0 
    //    new Vector3(1, 0, 0),   // 정점 1 
    //    new Vector3(0, 1, 0),   // 정점 2 
    //    new Vector3(1, 1, 0)    // 정점 3 
    //};
    //// 삼각형 면을 나타내는 정점 인덱스 배열
    //Index3i[] contour_vertex_indices = new Index3i[] 
    //{ 
    //    new Index3i(0, 1, 2),  // 첫 번째 삼각형 
    //    new Index3i(1, 3, 2)   // 두 번째 삼각형 
    //};
    //// 정점 인덱스를 사용하여 삼각형의 정점 위치를 참조
    //foreach (var triangle in contour_vertex_indices)
    //{ 
    //    Vector3 vertex1 = uniqpos[triangle.a]; 
    //    Vector3 vertex2 = uniqpos[triangle.b]; 
    //    Vector3 vertex3 = uniqpos[triangle.c]; 
    //    // 삼각형을 그리는 로직 
    //}

    //// Index3i 구조체: 3D 공간에서 삼각형의 정점들을 인덱스로 표현하는 구조체
    public struct Index3i : IEquatable<Index3i>
    {
        public int a, b, c; // 정점의 x, y, z 각 요소값, 3차원좌표값, 노말값 등의 인덱스
        static public readonly Index3i One = new(1, 1, 1);
        static public readonly Index3i Max = new(int.MaxValue, int.MaxValue, int.MaxValue);
        static public readonly Index3i Min = new(int.MinValue, int.MinValue, int.MinValue);
        static public readonly Index3i Zero = new(0, 0, 0);

        public Index3i(int z) { a = b = c = z; }
        public Index3i(int i, int j, int k) { a = i; b = j; c = k; }
        public Index3i(int[] i2) { a = i2[0]; b = i2[1]; c = i2[2]; }
        public Index3i(Index3i copy) { a = copy.a; b = copy.b; c = copy.c; }
        public ReadOnlyMemory<int> Array => new(new int[] { a, b, c });
        public int LengthSquared => (a * a) + (b * b) + (c * c);
        public int Length => (int)Math.Sqrt((a * a) + (b * b) + (c * c));
        public void Set(int ii, int jj, int kk) { a = ii; b = jj; c = kk; }
        public int this[int key]
        {
            get
            {
                return key < 0 || key > 2
                    ? throw new ArgumentOutOfRangeException(nameof(key), "Index must be 0, 1, or 2.")
                    : key == 0 ? a : key == 1 ? b : c;
            }
            set
            {
                if (key < 0 || key > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(key), "Index must be 0, 1, or 2.");
                }

                if (key == 0)
                {
                    a = value;
                }
                else if (key == 1)
                {
                    b = value;
                }
                else
                {
                    c = value;
                }
            }
        }
        public override string ToString() => $"[{a},{b},{c}]";
        public static Index3i operator +(Index3i v0, Index3i v1) => new(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c);
        public static Index3i operator -(Index3i v0, Index3i v1) => new(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c);
        public static Index3i operator *(Index3i v, int offset) => new(v.a * offset, v.b * offset, v.c * offset);
        public static Index3i operator /(Index3i v, int offset) => new(v.a / offset, v.b / offset, v.c / offset);
        public override bool Equals(object? obj) => obj is Index3i other && Equals(other);
        public bool Equals(Index3i other) => a == other.a && b == other.b && c == other.c;
        public override int GetHashCode() => unchecked((a, b, c).GetHashCode());
        public static bool operator ==(Index3i a, Index3i b) => a.Equals(b);
        public static bool operator !=(Index3i a, Index3i b) => !(a == b);
    }
}