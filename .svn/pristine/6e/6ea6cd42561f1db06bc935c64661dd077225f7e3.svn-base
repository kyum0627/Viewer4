using System;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public struct Line3f : IEquatable<Line3f>
    {
        public Vector3 origin;
        public Vector3 direction;
        public Line3f(Vector3 p0, Vector3 p1)
        {
            origin = p0;
            direction = Vector3.Normalize(p1 - p0);
        }
        public Line3f(Line3f line)
        {
            origin = line.origin;
            direction = Vector3.Normalize(line.direction);
        }
        public float Distance(Line3f other)
        {
            Vector3 u = direction;
            Vector3 v = direction;
            Vector3 w = origin - other.origin;
            float a = Vector3.Dot(u, u); //  >= 0
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v); //  >= 0
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float D = (a * c) - (b * b);//  >= 0
            float sc, tc;

            // 최단 거리 계산
            if (D < MathUtil.Epsilon)
            {   // 두 선은 거의 평행
                sc = 0.0f;
                tc = b > c ? d / b : e / c; // 큰 값으로 나누기 위해....
            }
            else
            {
                sc = ((b * e) - (c * d)) / D;
                tc = ((a * e) - (b * d)) / D;
            }

            // 두 closest point의 거리 계산
            Vector3 dP = w + (sc * u) - (tc * v);  // =  L1(sc) - L2(tc)
            return dP.Length;
        }

        public bool IsClosed
        {
            get { return false; }
        }

        public Vector3 PointAtDistance(float d)
        {
            return origin + (d * direction);
        }

        public void Reverse()
        {
            direction *= -1;
        }

        //--------------------- for IEquatable --------------------
        public static bool operator ==(Line3f a, Line3f b)
        {
            return a.origin == b.origin
                 && a.direction == b.direction;
        }
        public static bool operator !=(Line3f a, Line3f b)
        {
            return a.origin != b.origin
                 || a.direction != b.direction;
        }
        public override bool Equals(object? obj)
        {
            if (obj is null || !(obj is Line3f))
            {
                return false;
            }

            Line3f otherLine = (Line3f)obj;
            return this == otherLine;
        }
        public bool Equals(Line3f other)
        {
            return origin == other.origin && origin == other.origin;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ origin.GetHashCode();
                hash = (hash * 16777619) ^ direction.GetHashCode();
                return hash;
            }
        }
    }
}