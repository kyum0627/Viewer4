using IGX.Geometry.Common;
using System;

namespace IGX.Geometry.DataStructure.IgxMesh
{
    public readonly struct Eedge : IEquatable<Eedge>
    {
        public int V0 { get; }
        public int V1 { get; }

        public Eedge(int v0, int v1)
        {
            // 항상 (min, max) 순서로 저장
            if (v0 < v1)
            {
                V0 = v0;
                V1 = v1;
            }
            else
            {
                V0 = v1;
                V1 = v0;
            }
        }

        public bool Equals(Eedge other) => V0 == other.V0 && V1 == other.V1;

        // object.Equals는 Equatable 버전 호출
        //public override bool Equals(object obj)
        //{
        //    return obj is Eedge other && Equals(other);
        //}

        // 동일한 순서쌍이므로 조합해도 안전
        public override int GetHashCode() => HashCode.Combine(V0, V1);

        // 필요하면 Span(Vertex)로 반환하는 버전도 가능
        public Vertex[] GetVertices(IgxMesh mesh)
        {
            return new[]
            {
                mesh.UniqueVertices[V0],
                mesh.UniqueVertices[V1]
            };
        }

        public override string ToString() => $"({V0}, {V1})";
    }
}
