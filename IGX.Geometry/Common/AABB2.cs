using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// Axis Aligned Bounding Cube
    /// 2D 형상을 포함하는 최소 면적 사각형(Global좌표축 기준)
    /// </summary>
    public struct AABB2// : IEquatable<AABB2>
    {
        private Vector2 min; // left bottom
        private Vector2 max; // right top

        public static readonly AABB2 Empty = new(float.NaN, float.NaN, float.NaN, float.NaN);
        public static readonly AABB2 Infinite = new(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
        public static readonly AABB2 Unit = new(0, 0, 1, 1); // 가로세로 size가 1인 정사각형
        public static readonly AABB2 Zero = new(0, 0, 0, 0); // 가로세로 size가 0

        #region 생성자

        public AABB2(Vector2 p0, Vector2 p1)
        {
            min = new Vector2(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y));
            max = new Vector2(Math.Max(p0.X, p1.X), Math.Max(p0.Y, p1.Y));
        }

        public AABB2(float x0, float y0, float x1, float y1)
        {
            min = new Vector2(Math.Min(x0, x1), Math.Min(y0, y1));
            max = new Vector2(Math.Max(x0, x1), Math.Max(y0, y1));
        }

        public AABB2(Vector2 p)
        {
            min = new Vector2(p.X, p.Y);
            max = new Vector2(p.X, p.Y);
        }

        public AABB2(List<Vector2> p)
        {
            min = new Vector2(p[0].X, p[0].Y);
            max = new Vector2(p[0].X, p[0].Y);
            for (int i = 1; i < p.Count; i++)
            {
                Contain(p[i]);
            }
        }
        #endregion
        #region operator , compare, ...
        //---------------------------- for IComparable<BoundingBox2f> -----------------
        public readonly int CompareTo(AABB2 other)
        {
            return Center.CompareTo(other.Center) & Extents.CompareTo(other.Extents);
        }

        //---------------------------- for IEquatable<BoundingBox2f> ------------------
        public override readonly bool Equals(object? obj)
        {// 2025.02.12 HJJO
            return obj is AABB2 other
                && IsEqual(min.X, other.min.X) &&
                       IsEqual(min.Y, other.min.Y) &&
                       IsEqual(max.X, other.max.X) &&
                       IsEqual(max.Y, other.max.Y);
        }

        // NaN 값을 처리하기 위한 비교 메서드
        private static bool IsEqual(float a, float b)
        {// 2025.02.12 HJJO
            return float.IsNaN(a) && float.IsNaN(b) || a == b;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(min.X, min.Y, max.X, max.Y);
        }

        public static bool operator ==(AABB2 a, AABB2 b)
        {
            //return a.min == b.min && a.max == b.max; // 2025.02.12 HJJO
            return a.Equals(b);
        }

        public static bool operator !=(AABB2 a, AABB2 b)
        {
            //return a.min != b.min || a.max != b.max;// 2025.02.12 HJJO
            return !a.Equals(b);
        }

        #endregion
        #region Get Set
        public readonly Vector2 Center { get { return new Vector2(0.5f * (min.X + max.X), 0.5f * (min.Y + max.Y)); } }
        public readonly Vector2 Extents { get { return new Vector2((max.X - min.X) * 0.5f, (max.Y - min.Y) * 0.5f); } }
        public Vector2 Min { readonly get { return min; } set { min = value; } }
        public Vector2 Max { readonly get { return max; } set { max = value; } }
        public readonly Vector2 Size { get { return new Vector2(Dx, Dy); } }

        public readonly float Dx { get { return Math.Max(max.X - min.X, 0); } }
        public readonly float Dy { get { return Math.Max(max.Y - min.Y, 0); } }
        public readonly Vector2 Diagonal { get { return new Vector2(max.X - min.X, max.Y - min.Y); } }
        public readonly float DiagonalLength { get { return (float)Math.Sqrt(((max.X - min.X) * (max.X - min.X)) + ((max.Y - min.Y) * (max.Y - min.Y))); } }
        public readonly float Area { get { return Dx * Dy; } }

        public readonly Vector2 BottomLeft { get { return min; } }
        public readonly Vector2 BottomRight { get { return new Vector2(max.X, min.Y); } }
        public readonly Vector2 TopLeft { get { return new Vector2(min.X, max.Y); } }
        public readonly Vector2 TopRight { get { return max; } }
        public readonly Vector2 CenterLeft { get { return new Vector2(min.X, (min.Y + max.Y) * 0.5f); } }
        public readonly Vector2 CenterRight { get { return new Vector2(max.X, (min.Y + max.Y) * 0.5f); } }
        public readonly Vector2 CenterBottom { get { return new Vector2((min.X + max.X) * 0.5f, min.Y); } }
        public readonly Vector2 CenterTop { get { return new Vector2((min.X + max.X) * 0.5f, max.Y); } }
        #endregion

        /// <summary>
        /// this 박스가 other 박스와 허용오차 내에서 겹치는지 검사
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool Collide(AABB2 other, float eps = MathUtil.Epsilon)
        {
            for (int i = 0; i < 2; i++)
            {
                if (max[i] + eps < other.min[i] || min[i] > other.max[i] + eps)
                {
                    return false;
                }
            }
            return true;
        }

        public void Contain(Vector2 v)
        {
            if (this == Empty)
            {
                min = v;
                max = v;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (max[i] < v[i])
                    {
                        max[i] = v[i];
                    }

                    if (min[i] > v[i])
                    {
                        min[i] = v[i];
                    }
                }
            }
        }

        public AABB2 Contain(AABB2 box)
        {
            if (this == Empty)
            {
                min = box.min;
                max = box.max;
            }
            else
            {
                min.X = Math.Min(min.X, box.min.X);
                min.Y = Math.Min(min.Y, box.min.Y);
                max.X = Math.Max(max.X, box.max.X);
                max.Y = Math.Max(max.Y, box.max.Y);
            }
            return this;
        }

        public readonly bool Contains(Vector2 point)
        {
            if (this == Empty)
            {
                return false;
            }

            for (int i = 0; i < 2; ++i)
            {
                float value = point[i];
                if (value < min[i] || value > max[i])
                {
                    return false;
                }
            }
            return true;
        }

        public readonly bool Contains(AABB2 aabb2)
        {
            return Contains(aabb2.min) && Contains(aabb2.max);
        }

        /// <summary>
        /// 박스가 정점 v를 포함하면 true 아니면 false
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public readonly bool Collide(Vector2 v)//, float eps = MathUtil.Epsilon)
        {
            for (int i = 0; i < 2; i++)
            {
                if (max[i] < v[i] || min[i] > v[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 박스의 크기를 사방으로 contract만킄 축소
        /// </summary>
        /// <param name="contract"></param>
        public void Contract(float contract)
        {
            min.X += contract;
            min.Y += contract;

            max.X -= contract;
            max.Y -= contract;
        }

        /// <summary>
        /// Box와 v 간 거리
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public readonly float Distance(Vector2 v)
        {
            float dx = (float)Math.Abs(v.X - Center.X); // Cube 센터에서 v까지 x 거리
            float dy = (float)Math.Abs(v.Y - Center.Y); // Cube 센터에서 v까지 y 거리

            float halfH = Dx * 0.5f;
            float halfW = Dy * 0.5f;

            if (dx < halfH && dy < halfW) // 정점 v 가 박스내에 있으므로 거리는 0
            {
                return 0.0f;
            }
            else if (dx > halfH && dy > halfW) // 정점 v는 박스의 모퉁이와 가장 가까움
            {
                return (float)Math.Sqrt(((dx - halfH) * (dx - halfH)) + ((dy - halfW) * (dy - halfW)));
            }
            else if (dx > halfH) // 정점 v는 박스의 좌/우 변중 하나와 가장 가까움
            {
                return dx - halfH;
            }
            else if (dy > halfW) // 정점 v는 박스의 상/하 변중 하나와 가장 가까움
            {
                return dy - halfW;
            }

            return 0.0f;
        }

        /// <summary>
        /// 박스의 크기를 사방으로 expand만큼 확대
        /// </summary>
        /// <param name="expand"></param>
        public void Expand(float expand)
        {
            min.X -= expand; min.Y -= expand;
            max.X += expand; max.Y += expand;
        }

        public readonly void GetCenteredForm(ref Vector2 center, ref Vector2 extent)
        {
            center = (max + min) * 0.5f;
            extent = (max - min) * 0.5f;
        }

        /// <summary>
        /// this 박스가 other 박스와 겹치는 부분의 박스 생성
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly AABB2 Intersect(AABB2 other, float eps = MathUtil.Epsilon)
        {
            AABB2 intersection = new();

            if (!Collide(other, eps))
            {
                return Empty;
            }

            for (int i = 0; i < 2; i++)
            {
                intersection.max[i] = max[i] < other.max[i] ? max[i] : other.max[i];
                intersection.min[i] = min[i] > other.min[i] ? min[i] : other.min[i];
            }
            return intersection;
        }

        public readonly override string ToString()
        {
            return string.Format("[{0:F8},{1:F8}] [{2:F8},{3:F8}]", min.X, max.X, min.Y, max.Y);
        }
    }
}