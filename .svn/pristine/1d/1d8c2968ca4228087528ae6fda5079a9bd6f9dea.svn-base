namespace IGX.Geometry.DataStructure.IgxMesh
{
    public class PartialVid
    {
        public int pID;
        public int nID;
        public PartialVid(int positionID, int normalID)
        {
            pID = positionID;
            nID = normalID;
        }
        public override bool Equals(object? obj)
        { // Equals 메서드 재정의
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PartialVid other = (PartialVid)obj;
            return pID == other.pID && nID == other.nID;
        }
        public override int GetHashCode()
        { // GetHashCode 메서드 재정의
            int hash1 = pID.GetHashCode();
            int hash2 = nID.GetHashCode();
            return (hash1 * 397) ^ hash2;
        }
    }
}
