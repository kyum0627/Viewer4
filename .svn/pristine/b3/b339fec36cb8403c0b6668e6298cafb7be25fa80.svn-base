using System;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 평면상에서 두 점을 연결한 선을 정의, v0Id 에서 to 로 방향을 가지며, 다음의 기능을 수행
    /// 1. 클래스 생성시 Direction vector를 계산해서 저장
    /// 2. 선과 선간의 교차점 계산
    /// 3. 제3의 점이 선의 진행방향의 오른쪽에 있는지 왼쪽에 있는지 검사
    /// 4. 제3의 점을 직선에 직각으로 투영 (점과 선의 최단 거리 ...)
    /// 5. 선의 진행 방향 변경 ( v0Id ~ to )
    /// </summary>
    public struct Line2f : IEquatable<Line2f>
    {
        public Vector2 Point;    // 시작점
        public Vector2 Direction; // 직선의 방향

        public bool IsClosed
        {
            get { return false; }
        }

        public Line2f(bool bZero = true)
        {
            Point = Vector2.Zero;
            Direction = Vector2.Zero;
        }

        /// <summary>
        /// 직선 p0ID --> p0ID
        /// </summary>
        /// <param name="p0">시작점</param>
        /// <param name="p1">끝점</param>
        public Line2f(Vector2 p0, Vector2 p1)
        {
            Point = p0;
            Direction = Vector2.Normalize(p1 - Point);
        }

        public Line2f(float x1, float y1, float x2, float y2)
        {
            Vector2 p1 = new(x2, y2);
            Point = new Vector2(x1, y1);
            Direction = Vector2.Normalize(p1 - Point);
        }

        public Line2f(Line2f copy)
        {
            Point = copy.Point;
            Direction = copy.Direction;
        }

        /// <summary>
        /// 점과 선의 최단 거리
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public float Distance(Vector2 pt)
        {
            return pt.DistanceTo(GeometricTools.ClosestPoint(pt, this));
        }

        public float DistanceSquared(Vector2 pt)
        {
            return (pt - GeometricTools.ClosestPoint(pt, this)).LengthSquared;
        }

        /// <summary>
        /// 시작점에서 Direction 방향으로 d 만큼 진행한 점
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public Vector2 PointAtDistance(float d)
        {
            return Point + (Direction * d);
        }

        /// <summary>
        /// 직선 방향 바꾸기
        /// </summary>
        public void Reverse()
        {
            Direction *= -1;
        }

        //--------------------- for IEquatable --------------------

        public static bool operator ==(Line2f a, Line2f b)
        {
            // 직선이 겹치는 경우도 같다고 볼 것인가???
            return a.Point == b.Point
                && a.Direction == b.Direction;
        }
        public static bool operator !=(Line2f a, Line2f b)
        {
            return a.Point != b.Point
                || a.Direction != b.Direction;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || !(obj is Line2f))
            {
                return false;
            }

            Line2f otherLine = (Line2f)obj;
            return this == otherLine;
        }

        public bool Equals(Line2f other)
        {
            return Point == other.Point && Point == other.Point;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Point.GetHashCode();
                hash = (hash * 16777619) ^ Direction.GetHashCode();
                return hash;
            }
        }
    }
}