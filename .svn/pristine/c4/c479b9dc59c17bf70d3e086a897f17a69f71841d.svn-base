using System;

namespace IGX.Geometry.DataStructure
{
    // Index2i 구조체 선언
    public struct Index2i : IEquatable<Index2i>
    {
        // 두 개의 정수 필드 a와 b를 선언
        public int a;
        public int b;

        // 생성자들
        // 모든 필드를 같은 값으로 초기화하는 생성자
        public Index2i(int z) { a = b = z; }
        // 두 개의 값을 받아 각각 a와 b에 할당하는 생성자
        public Index2i(int ii, int jj) { a = ii; b = jj; }
        // 정수 배열을 받아 첫 번째와 두 번째 값을 a와 b에 할당하는 생성자
        public Index2i(int[] i2) { a = i2[0]; b = i2[1]; }
        // 다른 Index2i 객체를 복사하여 값을 할당하는 생성자
        public Index2i(Index2i copy) { a = copy.a; b = copy.b; }

        // 정적 필드들
        // 모든 값이 1인 Index2i 객체
        public static readonly Index2i One = new(1, 1);
        // int형의 최댓값을 가지는 Index2i 객체
        public static readonly Index2i Max = new(int.MaxValue, int.MaxValue);
        // int형의 최솟값을 가지는 Index2i 객체
        public static readonly Index2i Min = new(int.MinValue, int.MinValue);
        // 모든 값이 0인 Index2i 객체
        public static readonly Index2i Zero = new(0, 0);

        // 속성들
        // 현재 Index2i 객체를 정수 배열로 반환하는 속성
        public int[] Array => new int[] { a, b };
        // 벡터의 길이의 제곱을 반환하는 속성
        public int LengthSquared => (a * a) + (b * b);
        // 벡터의 길이를 반환하는 속성
        public int Length => (int)Math.Sqrt(LengthSquared);

        // 인덱서
        // 인덱스로 0을 주면 a를 반환하고, 1을 주면 b를 반환하는 인덱서
        public int this[int key]
        {
            get => key == 0 ? a : b;
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

        // ToString 메서드 재정의
        // Index2i 객체를 "[a,b]" 형식의 문자열로 반환
        public override string ToString() => $"[{a},{b}]";

        // 산술 연산자들
        // 두 Index2i 객체를 받아 각 요소별로 더한 Index2i 객체를 반환하는 연산자
        public static Index2i operator +(Index2i v0, Index2i v1) => new(v0.a + v1.a, v0.b + v1.b);
        // Index2i 객체의 부호를 바꾼 Index2i 객체를 반환하는 단항 마이너스 연산자
        public static Index2i operator -(Index2i v) => new(-v.a, -v.b);
        // Index2i 객체와 정수를 받아 각 요소별로 빼기 연산을 수행한 Index2i 객체를 반환하는 연산자
        public static Index2i operator -(Index2i v0, Index2i v1) => new(v0.a - v1.a, v0.b - v1.b);
        // 정수와 Index2i 객체를 받아 정수를 Index2i 객체의 각 요소에 곱한 Index2i 객체를 반환하는 연산자
        public static Index2i operator *(int f, Index2i v) => new(f * v.a, f * v.b);
        // Index2i 객체와 정수를 받아 정수를 Index2i 객체의 각 요소에 곱한 Index2i 객체를 반환하는 연산자
        public static Index2i operator *(Index2i v, int f) => new(f * v.a, f * v.b);
        // 두 Index2i 객체를 받아 각 요소별로 곱한 Index2i 객체를 반환하는 연산자
        public static Index2i operator *(Index2i a, Index2i b) => new(a.a * b.a, a.b * b.b);
        // Index2i 객체와 정수를 받아 Index2i 객체의 각 요소를 정수로 나눈 Index2i 객체를 반환하는 연산자
        public static Index2i operator /(Index2i v, int f) => new(v.a / f, v.b / f);
        // 두 Index2i 객체를 받아 각 요소별로 나눈 Index2i 객체를 반환하는 연산자
        public static Index2i operator /(Index2i a, Index2i b) => new(a.a / b.a, a.b / b.b);

        //--------------------- IEquatable 인터페이스 구현 --------------------

        // 동치 비교 연산자들
        // 두 Index2i 객체가 같은지 비교하는 연산자
        public static bool operator ==(Index2i a, Index2i b) => a.a == b.a && a.b == b.b;
        // 두 Index2i 객체가 다른지 비교하는 연산자
        public static bool operator !=(Index2i a, Index2i b) => a.a != b.a || a.b != b.b;

        // Equals 메서드 재정의
        // object 형식을 받아 Index2i 객체와 비교하여 같은지 여부를 반환
        public override bool Equals(object? obj) => obj is Index2i other && this == other;
        // Index2i 객체를 받아 현재 객체와 같은지 여부를 반환
        public bool Equals(Index2i other) => a == other.a && b == other.b;

        // GetHashCode 메서드 재정의
        // 객체의 해시 코드를 반환
        public override int GetHashCode()
        {
            unchecked
            {
                // 초기 해시값 17
                int hash = 17;
                // a의 해시코드를 더하여 해시값 갱신
                hash = (hash * 23) + a.GetHashCode();
                // b의 해시코드를 더하여 최종 해시값 반환
                hash = (hash * 23) + b.GetHashCode();
                return hash;
            }
        }
    }
}
