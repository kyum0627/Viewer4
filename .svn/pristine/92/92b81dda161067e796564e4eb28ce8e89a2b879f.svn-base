using System;
using MessagePack;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// world coordinate 기준 3d Geometry를 내포하는 좌표계와 평행한 bounding box
    /// 부재간 간섭 체크, 부재 display 등에 사용    ///
    /// </summary>
    [MessagePackObject]
    public struct AABB3
    {
        [Key(0)]
        public Vector3 min;
        [Key(1)]
        public Vector3 max;
        [Key(2)]
        public static AABB3 Empty = new(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
        [Key(3)]
        public static AABB3 Infinite = new(float.MinValue, float.MinValue, float.MinValue, float.MaxValue, float.MaxValue, float.MaxValue);
        [Key(4)]
        public static AABB3 UnitBox = new(1);
        [Key(5)]
        public static AABB3 Zero = new(0);

        #region Get, Set
        [Key(6)]
        public float Dx { get { return Math.Max(max.X - min.X, 0); } }
        [Key(7)]
        public float Dy { get { return Math.Max(max.Y - min.Y, 0); } }
        [Key(8)]
        public float Dz { get { return Math.Max(max.Z - min.Z, 0); } }
        [Key(9)]
        public float MaxDim { get { return Math.Max(Dx, Math.Max(Dy, Dz)); } }
        [Key(10)]
        public float Volume { get { return Dx * Dy * Dz; } }
        [Key(11)]
        public float SurfaceArea { get { return (Dx * Dy) + (Dy * Dz) + (Dz * Dx); } }
        [Key(12)]
        public Vector3 Center { get { return new Vector3(0.5f * (min.X + max.X), 0.5f * (min.Y + max.Y), 0.5f * (min.Z + max.Z)); } }
        [Key(13)]
        public Vector3 Diagonal { get { return new Vector3(max.X - min.X, max.Y - min.Y, max.Z - min.Z); } }
        [Key(14)]
        public Vector3 Extents { get { return new Vector3((max.X - min.X) * 0.5f, (max.Y - min.Y) * 0.5f, (max.Z - min.Z) * 0.5f); } }
        #endregion

        #region 생성자
        public AABB3(AABB3 copy)
        {
            min = new Vector3(copy.min.X, copy.min.Y, copy.min.Z);
            max = new Vector3(copy.max.X, copy.max.Y, copy.max.Z);
        }

        public AABB3(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            min = new Vector3(Math.Min(x0, x1), Math.Min(y0, y1), Math.Min(z0, z1));
            max = new Vector3(Math.Max(x0, x1), Math.Max(y0, y1), Math.Max(z0, z1));
        }

        public AABB3(float Length)
        {
            float hl = 0.5f * Length;
            min = -new Vector3(hl);
            max = new Vector3(hl);
        }

        public AABB3(Vector3 p0, Vector3 p1)
        {
            min = new Vector3(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y), Math.Min(p0.Z, p1.Z));
            max = new Vector3(Math.Max(p0.X, p1.X), Math.Max(p0.Y, p1.Y), Math.Max(p0.Z, p1.Z));
        }
        public Vector3[] GenerateBoxVertexes()
        {
            Vector3[] cube = new[]
            {
                new Vector3(min.X, min.Y, min.Z), // 0
                new Vector3(max.X, min.Y, min.Z), // 1
                new Vector3(max.X, max.Y, min.Z), // 2
                new Vector3(min.X, max.Y, min.Z), // 3
                new Vector3(min.X, min.Y, max.Z), // 4
                new Vector3(max.X, min.Y, max.Z), // 5
                new Vector3(max.X, max.Y, max.Z), // 6
                new Vector3(min.X, max.Y, max.Z)  // 7
            };
            return cube;
        }
        public Vector3[] Vertices()
        {
            Vector3[] v = new Vector3[8];
            Vertices(v);
            return v;
        }

        private void Vertices(Vector3[] vertex)
        {
            Vector3 extaxis0 = Extents.X * Vector3.UnitX;
            Vector3 extaxis1 = Extents.Y * Vector3.UnitY;
            Vector3 extaxis2 = Extents.Z * Vector3.UnitZ;

            vertex[0] = Center - extaxis0 - extaxis1 - extaxis2;
            vertex[1] = Center + extaxis0 - extaxis1 - extaxis2;
            vertex[2] = Center + extaxis0 + extaxis1 - extaxis2;
            vertex[3] = Center - extaxis0 + extaxis1 - extaxis2;
            vertex[4] = Center - extaxis0 - extaxis1 + extaxis2;
            vertex[5] = Center + extaxis0 - extaxis1 + extaxis2;
            vertex[6] = Center + extaxis0 + extaxis1 + extaxis2;
            vertex[7] = Center - extaxis0 + extaxis1 + extaxis2;
        }

        #endregion

        #region Operator, Compare, ...

        public void Flip()
        {
            Vector3 c = Center;
            Vector3 e = Extents;

            c.Y *= -1;
            min = c - e;
            max = c + e;
        }

        public AABB3 Transform(Matrix4 matrix)
        {
            Vector3[] corners = new Vector3[8];
            Vertices(corners);
            AABB3 transformedBox = AABB3.Empty;
            for (int i = 0; i < 8; i++)
            {
                Vector4 transformedCorner = new Vector4(corners[i], 1.0f) * matrix;
                transformedBox.Contain(new Vector3(transformedCorner.X, transformedCorner.Y, transformedCorner.Z));
            }
            return transformedBox;
        }

        //---------------------------- for IComparable<BoundingBox2f> -----------------

        public int CompareTo(AABB3 other)
        {
            return Center.CompareTo(other.Center) & Extents.CompareTo(other.Extents);
        }

        //---------------------------- for IEquatable<BoundingBox2f> ------------------
        // Equals 메서드 구현

        public override bool Equals(object? obj)
        {// 2025.02.12 HJJO
            return obj is AABB3 other
                && IsEqual(min.X, other.min.X) &&
                        IsEqual(min.Y, other.min.Y) &&
                        IsEqual(min.Z, other.min.Z) &&
                        IsEqual(max.X, other.max.X) &&
                        IsEqual(max.Y, other.max.Y) &&
                        IsEqual(max.Z, other.max.Z);
        }

        // NaN 값을 처리하기 위한 비교 메서드
        private static bool IsEqual(float a, float b)
        {// 2025.02.12 HJJO
            return (float.IsNaN(a) && float.IsNaN(b)) || (a == b);
        }

        /// <summary>
        /// 임의의 List형 데이터에 대해 고정된 길이의 데이터로 매핑
        /// 같은 입력값에 대해서는 같은 출력값 보장
        /// 빠른 검색, 빠른 삽입이 가능
        /// </summary>
        /// <returns></returns>
        // GetHashCode 오버라이드 (Equals 메서드를 오버라이드 하면 GetHashCode도 오버라이드해야 함)
        public override int GetHashCode()
        {
            return HashCode.Combine(min.X, min.Y, min.Z, max.X, max.Y, max.Z);
        }

        public static bool operator ==(AABB3 a, AABB3 b)
        {
            //return a.min == b.min && a.max == b.max; // 2025.02.12 HJJO
            return a.Equals(b);
        }

        public static bool operator !=(AABB3 a, AABB3 b)
        {
            //return a.min != b.min || a.max != b.max;// 2025.02.12 HJJO
            return !a.Equals(b);
        }

        #endregion

        /// <summary>
        /// this 박스가 other 박스와 겹치는지 검사
        /// </summary>
        /// <param name="other"></param>
        /// <param name="eps">허용 오차</param>
        /// <returns></returns>
        public bool Collide(AABB3 other, float eps = MathUtil.Epsilon)
        {
            if (this == Empty)
            {
                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                if (max[i] + eps < other.min[i] || min[i] > other.max[i] + eps)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contains(Vector3 v)
        {
            if (this == Empty)
            {
                return false;
            }

            return v.X >= min.X && v.X <= max.X
                && v.Y >= min.Y && v.Y <= max.Y
                && v.Z >= min.Z && v.Z <= max.Z;
        }

        public bool Contains(AABB3 other)
        {
            return this != Empty
                && other.min.CompareTo(min) == 1 && other.max.CompareTo(max) == -1 &&
                other.max.CompareTo(min) == 1 && other.max.CompareTo(max) == -1;
        }

        /// <summary>
        /// 정점 v를 포함하도록 박스 확대
        /// </summary>
        /// <param name="v"></param>
        public void Contain(Vector3 v)
        {
            if (this == Empty)
            {
                min = new Vector3(v.X, v.Y, v.Z);
                max = new Vector3(v.X, v.Y, v.Z);
            }
            else
            {
                min.X = Math.Min(min.X, v.X);
                min.Y = Math.Min(min.Y, v.Y);
                min.Z = Math.Min(min.Z, v.Z);
                max.X = Math.Max(max.X, v.X);
                max.Y = Math.Max(max.Y, v.Y);
                max.Z = Math.Max(max.Z, v.Z);
            }

        }

        /// <summary>
        /// 박스 other를 포함하도록 박스 확장
        /// </summary>
        /// <param name="box"></param>
        public AABB3 Contain(AABB3 box)
        {
            // box가 Empty면 현재 박스를 그대로 반환 (변경 없음)
            if (box == Empty)
            {
                return this;
            }
            
            // 현재 박스가 Empty면 box로 초기화
            if (this == Empty)
            {
                this = box;
            }
            else
            {
                // 두 박스를 모두 포함하도록 확장
                min.X = Math.Min(min.X, box.min.X);
                min.Y = Math.Min(min.Y, box.min.Y);
                min.Z = Math.Min(min.Z, box.min.Z);
                max.X = Math.Max(max.X, box.max.X);
                max.Y = Math.Max(max.Y, box.max.Y);
                max.Z = Math.Max(max.Z, box.max.Z);
            }
            return this;
        }

        /// <summary>
        /// bounding box를 contract만큼 축소/확대
        /// </summary>
        /// <param name="expand"></param>
        public void Expand(float expand)
        {
            if (this == Empty)
            {
                return;
            }

            min.X -= expand; min.Y -= expand; min.Z -= expand;
            max.X += expand; max.Y += expand; max.Z += expand;
        }

        /// <summary>
        /// bounding box의 corner 좌표
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector3 Corner(int i)
        {
            float x = (i & 1) != 0 ^ (i & 2) != 0 ? max.X : min.X;
            float y = i / 2 % 2 == 0 ? min.Y : max.Y;
            float z = i < 4 ? min.Z : max.Z;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// 점 v와 bounding box의 min 혹은 max 까지 거리
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public float Distance(Vector3 v)
        {
            return (float)Math.Sqrt(DistanceQSquared(v));
        }

        /// <summary>
        /// 점 v와 bounding box의 min 혹은 max 까지 거리의 제곱
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private float DistanceQSquared(Vector3 v)
        {
            float dx = v.X < min.X ? min.X - v.X : v.X > max.X ? v.X - max.X : 0;
            float dy = v.Y < min.Y ? min.Y - v.Y : v.Y > max.Y ? v.Y - max.Y : 0;
            float dz = v.Z < min.Z ? min.Z - v.Z : v.Z > max.Z ? v.Z - max.Z : 0;

            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        public AABB2 Get2dBox(int ignore)
        {
            switch (ignore)
            {
                case 0: return new AABB2(min.Y, min.Z, max.Y, max.Z);
                case 1: return new AABB2(min.Z, min.X, max.Z, max.X);
                case 2: return new AABB2(min.X, min.Y, max.X, max.Y);
            }
            return AABB2.Empty;
        }

        public void GetCenteredForm(ref Vector3 cen, ref Vector3 ext)
        {
            cen = (max + min) * 0.5f;
            ext = (max - min) * 0.5f;
        }

        /// <summary>
        /// this 박스가 other 박스와 겹치는 부분의 박스 생성
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public AABB3 Intersect(AABB3 other, float eps = MathUtil.Epsilon)
        {
            AABB3 intersection = new();

            if (!Collide(other, eps))
            {
                return Empty;
            }

            for (int i = 0; i < 3; i++)
            {
                intersection.max[i] = max[i] < other.max[i] ? max[i] : other.max[i];
                intersection.min[i] = min[i] > other.min[i] ? min[i] : other.min[i];
            }
            return intersection;
        }
        public bool Intersects(Matrix4 projectionViewMatrix)
        {
            // 간단한 AssemblyAABB-Frustum 교차 판정 (더 정밀한 알고리즘 사용 가능)
            Vector3[] corners = new Vector3[8];
            corners[0] = min;
            corners[1] = new Vector3(max.X, min.Y, min.Z);
            corners[2] = new Vector3(min.X, max.Y, min.Z);
            corners[3] = new Vector3(max.X, max.Y, min.Z);
            corners[4] = new Vector3(min.X, min.Y, max.Z);
            corners[5] = new Vector3(max.X, min.Y, max.Z);
            corners[6] = new Vector3(min.X, max.Y, max.Z);
            corners[7] = max;

            for (int i = 0; i < 8; i++)
            {
                Vector4 clipPos = new Vector4(corners[i], 1.0f) * projectionViewMatrix;
                clipPos.X /= clipPos.W;
                clipPos.Y /= clipPos.W;
                clipPos.Z /= clipPos.W;
                if (clipPos.X >= -1.0f && clipPos.X <= 1.0f &&
                    clipPos.Y >= -1.0f && clipPos.Y <= 1.0f &&
                    clipPos.Z >= -1.0f && clipPos.Z <= 1.0f)
                {
                    return true; // 하나라도 꼭짓점이 Frustum 안에 있으면 가시적
                }
            }
            return false;
        }
        public bool Intersects(Ray3f ray, ref float? distance)
        {
            distance = null; // 교차하지 않을 경우의 기본 거리 (의미 없는 값)

            Vector3 origin = ray.origin;
            Vector3 direction = ray.direction;

            float tMin = float.NegativeInfinity;
            float tMax = float.PositiveInfinity;

            float[] t = new float[6];
            float tNear, tFar, temp;
            bool inside = true;

            // 각 축에 대해 검사
            for (int i = 0; i < 3; ++i)
            {
                if (Math.Abs(direction[i]) < 1e-6f) // Ray 방향이 축과 거의 평행한 경우
                {
                    // Ray 시작점이 AssemblyAABB 범위 밖에 있으면 교차 없음
                    if (origin[i] < min[i] || origin[i] > max[i])
                    {
                        return false;
                    }
                }
                else
                {
                    tNear = (min[i] - origin[i]) / direction[i];
                    tFar = (max[i] - origin[i]) / direction[i];

                    if (tNear > tFar)
                    {
                        temp = tNear;
                        tNear = tFar;
                        tFar = temp;
                    }

                    tMin = Math.Max(tMin, tNear);
                    tMax = Math.Min(tMax, tFar);

                    // AssemblyAABB 전체가 Ray 시작점 뒤에 있는 경우
                    if (tMax < 0)
                    {
                        return false;
                    }
                }

                // Ray 시작점이 AssemblyAABB 내부에 있지 않다면 inside는 false
                if (origin[i] < min[i] || origin[i] > max[i])
                {
                    inside = false;
                }
            }

            // Ray 시작점이 AssemblyAABB 내부에 있으면 거리 0
            if (inside)
            {
                distance = 0f;
                return true;
            }

            // tMin이 tMax보다 크면 교차 없음
            if (tMin > tMax)
            {
                return false;
            }

            distance = tMin;
            return true;
        }
        public override string ToString()
        {
            return string.Format("x[{0:F8},{1:F8}] y[{2:F8},{3:F8}] z[{4:F8},{5:F8}]", min.X, max.X, min.Y, max.Y, min.Z, max.Z);
        }

        public Matrix4 ToMatrix()
        {
            return Matrix4.CreateScale(2 * Extents) * Matrix4.CreateTranslation(Center);
        }
    }
}