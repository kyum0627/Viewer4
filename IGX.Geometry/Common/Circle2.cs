using System;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 2 dimensional circle
    /// </summary>
    public struct Circle2 : IEquatable<Circle2>
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public float Circumference => Radius * MathUtil.TwoPi;
        public float Diameter => 2 * Radius;
        public float Area => Radius * Radius * (float)Math.PI;
        /// <summary>
        /// Creates a Circle (Radius, center point)
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public Circle2(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
        public (Vector2 center, float radius) Outside(Vector2 P1, Vector2 P2, Vector2 P3)
        {
            GetCircleBasis(P1, P2, P3, out float a, out float b, out float c, out float area);

            // 외접원의 반지름 계산 (외접원 공식)
            float radius = (a * b * c) / (4.0f * area);

            // 외심 계산 (외접원의 중심)
            float d = 2.0f * ((P1.X * (P2.Y - P3.Y)) + (P2.X * (P3.Y - P1.Y)) + (P3.X * (P1.Y - P2.Y)));

            float ux = (((a * a) * (P2.Y - P3.Y)) + ((b * b) * (P3.Y - P1.Y)) + ((c * c) * (P1.Y - P2.Y))) / d;
            float uy = (((a * a) * (P3.X - P2.X)) + ((b * b) * (P1.X - P3.X)) + ((c * c) * (P2.X - P1.X))) / d;

            // 외심을 (ux, uy)로 설정
            Vector2 center = new(ux, uy);

            return (center, radius);
        }
        public (Vector2 center, float radius) Inside(Vector2 P1, Vector2 P2, Vector2 P3)
        {
            GetCircleBasis(P1, P2, P3, out float a, out float b, out float c, out float area);

            // 내접원의 반지름 계산
            float semiPerimeter = (a + b + c) / 2.0f;  // 반둘레 계산
            float radius = area / semiPerimeter;  // 내접원 반지름 공식

            // 내심 계산 (내접원의 중심)
            float x = ((a * P1.X) + (b * P2.X) + (c * P3.X)) / (a + b + c);
            float y = ((a * P1.Y) + (b * P2.Y) + (c * P3.Y)) / (a + b + c);

            // 내심을 (x, y)로 설정
            Vector2 center = new(x, y);

            return (center, radius);
        }
        private static void GetCircleBasis(Vector2 P1, Vector2 P2, Vector2 P3, out float a, out float b, out float c, out float area)
        {
            // 삼각형의 변 길이 계산
            a = (P2 - P3).Length;
            b = (P3 - P1).Length;
            c = (P1 - P2).Length;

            // 삼각형의 면적 계산 (행렬식 이용)
            area = 0.5f * Math.Abs((P1.X * (P2.Y - P3.Y)) + (P2.X * (P3.Y - P1.Y)) + (P3.X * (P1.Y - P2.Y)));
        }
        public static bool operator ==(Circle2 left, Circle2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Circle2 left, Circle2 right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Circle2 c, float tolerance = float.Epsilon)
        {
            return tolerance < 0
                ? throw new ArgumentException("epsilon < 0")
                : Math.Abs(c.Radius - Radius) < tolerance && Math.Abs(Center.X - c.Center.X) < tolerance && Math.Abs(Center.Y - c.Center.Y) < tolerance;
        }

        public bool Equals(Circle2 c) => Radius.Equals(c.Radius) && Center.Equals(c.Center);

        public override bool Equals(object? obj)
        {
            if (obj is null || !(obj is Circle2))
            {
                return false;
            }

            Circle2 c = (Circle2)obj;
            return Math.Abs(c.Radius - Radius) < float.Epsilon && Center.Equals(c.Center);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ Center.GetHashCode();
                hash = (hash * 16777619) ^ Radius.GetHashCode();
                return hash;
            }
        }
    }
}
