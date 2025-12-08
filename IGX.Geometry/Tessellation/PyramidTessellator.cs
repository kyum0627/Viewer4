using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class PyramidTessellator : ITessellator<Pyramid>
    {
        public MeshGeometry Tessellate(Pyramid p, uint n_seg, bool bBcap = true, bool bTcap = true)
        {
            p.Mesh = TessellationUtility.SixFacetsVolume(p.Xbottom, p.Ybottom, p.Xtop, p.Ytop, p.Xoffset, p.Yoffset, p.Height);

            if (p.Positions != null && p.Normals != null)
            {
                (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(
                    p.Transform, p.Positions, p.Normals);
                p.Mesh.Vertices = res.Positions;
                p.Mesh.Normals = res.Normals;
            }

            return new MeshGeometry(p.Positions!, p.Normals!, p.Indices);
        }
    }
}