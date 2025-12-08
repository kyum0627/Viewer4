using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public class Plane2f
    {
        public Vector2 Normal;
        public float Distance;

        public Vector2 V0;
        public Plane2f()
        {
            Normal = Vector2.Normalize(Normal);
            Distance = 0f;
        }

        public Plane2f(Vector2 pos, Vector2 normal)
        {
            V0 = pos;
            this.Normal = normal;
        }

        // Specify N and c directly.
        public Plane2f(Vector2 inNormal, float inConstant)
        {
            Normal = inNormal;
            Distance = inConstant;
        }

        // Comparisons to support sorted containers.
        public static bool operator ==(Plane2f A, Plane2f B)
        {
            return B.Normal == A.Normal && B.Distance == A.Distance;
        }

        public static bool operator !=(Plane2f A, Plane2f B)
        {
            return B.Normal != A.Normal || B.Distance != A.Distance;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ Distance.GetHashCode();
                hash = (hash * 16777619) ^ Normal.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj is Plane2f)
            {
                Plane2f otherPlane = (Plane2f)obj;
                return Distance == otherPlane.Distance && Normal == otherPlane.Normal;
            }
            else
            {
                return false;
            }
        }
    }
}
