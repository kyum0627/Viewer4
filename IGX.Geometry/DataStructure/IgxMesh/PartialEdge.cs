namespace IGX.Geometry.DataStructure.MeshDecomposer
{
    public class PartialEdge
    { // Partial edge. till now, this class supports half edge data structure only!!!! Need to be enhanced later! 20250305 HJJO
        public int p0ID;
        public int p1ID;
        public PartialEdge Twin => new(p1ID, p0ID);
        public PartialEdge(int p0id, int p1id)
        {
            p0ID = p0id;
            p1ID = p1id;
        }
        public override bool Equals(object? obj)
        { // Equals 메서드 재정의
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PartialEdge other = (PartialEdge)obj;
            return p0ID == other.p0ID && p1ID == other.p1ID;
        }
        public override int GetHashCode()
        { // GetHashCode 메서드 재정의
            int hash1 = p0ID.GetHashCode();
            int hash2 = p1ID.GetHashCode();
            return (hash1 * 397) ^ hash2;
        }
    }
}
