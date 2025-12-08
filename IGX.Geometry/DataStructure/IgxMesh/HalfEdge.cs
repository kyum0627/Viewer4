namespace IGX.Geometry.DataStructure.IgxMesh
{
    public struct HalfEdge
    {
        public int vertex;
        public int twin;
        public int next;
        public int face;
    }
}