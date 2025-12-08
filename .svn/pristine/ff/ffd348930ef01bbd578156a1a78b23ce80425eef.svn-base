using IGX.Geometry.Common;
using OpenTK.Mathematics;
using System;

namespace IGX.Geometry.DataStructure.IgxMesh
{
    public struct TrianglesAdjacency : IEquatable<TrianglesAdjacency>
    {
        public int V0, V1, V2;
        public int AdjV0, AdjV1, AdjV2;
        public Vector3 Centroid;
        public Vector3 Normal;
        public float Area;
        public TrianglesAdjacency(int v0, int v1, int v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            AdjV0 = V0;
            AdjV1 = V0;
            AdjV2 = V0;
            Centroid = Vector3.Zero;
            Normal = Vector3.Zero;
            Area = 0;
        }

        public bool Equals(TrianglesAdjacency other)
        {
            // 두 삼각형이 동일한 세 정점을 공유하는지 확인
            if (this.GetHashCode() != other.GetHashCode()) return false;

            // 회전 순서까지 고려하여 비교
            return (V0 == other.V0 && V1 == other.V1 && V2 == other.V2) ||
                   (V0 == other.V1 && V1 == other.V2 && V2 == other.V0) ||
                   (V0 == other.V2 && V1 == other.V0 && V2 == other.V1);
        }

        public override int GetHashCode()
        {
            // 회전 불변 해시 코드 생성. 세 정점의 해시 코드를 XOR 연산
            return V0.GetHashCode() ^ V1.GetHashCode() ^ V2.GetHashCode();
        }
        public Vertex[] GetVertices(IgxMesh mesh)
        {
            return new Vertex[3] { mesh.UniqueVertices[(int)V0], mesh.UniqueVertices[(int)V1], mesh.UniqueVertices[(int)V2] };
        }
    }
}