using IGX.Geometry.Common;
using OpenTK.Mathematics;

namespace IGX.Geometry.GeometryBuilder
{
    public class Pyramid : PrimitiveBase
    {
        public override bool IsTransformable => false;

        public float Xbottom;
        public float Ybottom;
        public float Xtop;
        public float Ytop;
        public float Xoffset;
        public float Yoffset;
        public float Height;

        public Pyramid(Matrix4 matrix, AABB3 bBoxLocal, float bx = 1f, float by = 1f, float tx = 1f, float ty = 1f, float ox = 0f, float oy = 0f, float h = 1f)
        {
            Xbottom = bx;
            Ybottom = by;
            Xtop = tx;
            Ytop = ty;
            Xoffset = ox;
            Yoffset = oy;
            Height = h;
            TransformComp.Trans = matrix;
        }
        public override ParaPrimType GetParametricType() => ParaPrimType.Pyramid;
        //public override MeshGeometry Tessellate(uint n_seg, bool bBcap = true, bool bTcap = true)
        //{
        //    MeshGeometry = TessellationUtility.SixFacetsVolume(Xbottom, Ybottom, Xtop, Ytop, Xoffset, Yoffset, Height);

        //    if (Vertices != null && Normals != null)
        //    {
        //        (List<Vector3> Vertices, List<Vector3> Normals) res = TessellationUtility.MatrixApply(
        //            Trans, Vertices, Normals);
        //        MeshGeometry.Vertices = res.Vertices;
        //        MeshGeometry.Normals = res.Normals;
        //    }

        //    return new MeshGeometry(Vertices!, Normals!, Indices);
        //}
    }
}