using IGX.Geometry.DataStructure;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.GLDataStructure
{

    [StructLayout(LayoutKind.Sequential)]
    public struct GLVertex : IEquatable<GLVertex>
    {
        [Vertex(Location = 0, Size = 3)] public Vector3 Position;
        [Vertex(Location = 1, Size = 3)] public Vector3 Normal;
        public GLVertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
        public static readonly int SizeInBytes = Marshal.SizeOf<GLVertex>();
        public override readonly string ToString()
        {
            return Position.ToString() + "  " + Normal.ToString() + "\n";
        }
        public readonly float[] ToArray => new float[] { Position[0], Position[1], Position[2], Normal[0], Normal[1], Normal[2] };
        public bool Equals(GLVertex other)
        {
            // Position과 Normal을 오차 범위 내에서 비교
            return Position.Equals(other.Position) &&
                   Normal.Equals(other.Normal);
        }
        public readonly bool Equals(GLVertex? other, bool compareNormal = true)
        {// 비교할 때, Position만 비교할지, Position과 Normal을 비교할지 선택할 수 있도록 수정
            if (other == null)
            {
                return false;
            }

            return compareNormal ? Position == other.Value.Position && Normal == other.Value.Normal : Position == other.Value.Position;
        }

        public override readonly int GetHashCode()
        {// 기본적으로 Position과 Normal을 기반으로 해시값을 생성
            return GetHashCode(true);
        }
        public readonly int GetHashCode(bool compareNormal = true)
        {// 비교 기준에 따라 GetHashCode를 다르게 계산
            return compareNormal ? HashCode.Combine(Position, Normal) : Position.GetHashCode();
        }
    }
}
