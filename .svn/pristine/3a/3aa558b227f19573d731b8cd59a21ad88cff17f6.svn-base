using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Common
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;

        public Vertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }

    public sealed class VertexEqualityComparer : IEqualityComparer<Vertex>
    {
        private readonly float _positionTolerance;
        private readonly float _normalTolerance;

        public VertexEqualityComparer(float positionTolerance, float normalTolerance)
        {
            _positionTolerance = positionTolerance;
            _normalTolerance = normalTolerance;
        }

        public bool Equals(Vertex v1, Vertex v2)
        {
            // Vector3.DistanceSquared 을 사용하여 sqrt 비용 제거
            bool posEq = (v1.Position - v2.Position).LengthSquared < _positionTolerance * _positionTolerance;
            bool norEq = (v1.Normal - v2.Normal).LengthSquared < _normalTolerance * _normalTolerance;

            return posEq && norEq;
        }

        public int GetHashCode(Vertex v)
        {
            // tolerance 범위를 고려한 quantized hash
            int qpx = (int)(v.Position.X / _positionTolerance);
            int qpy = (int)(v.Position.Y / _positionTolerance);
            int qpz = (int)(v.Position.Z / _positionTolerance);

            int qnx = (int)(v.Normal.X / _normalTolerance);
            int qny = (int)(v.Normal.Y / _normalTolerance);
            int qnz = (int)(v.Normal.Z / _normalTolerance);

            return HashCode.Combine(qpx, qpy, qpz, qnx, qny, qnz);
        }
    }
}
