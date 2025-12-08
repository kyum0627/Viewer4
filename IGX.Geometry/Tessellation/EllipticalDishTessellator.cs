using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class EllipticalDishTessellator : ITessellator<EllipticalDish>
    {

        public MeshGeometry Tessellate(EllipticalDish c, uint n_seg, bool bBcap = true, bool bTcap = true)
        {
            c.Mesh = TessellationUtility.GenerateSphereShape(n_seg, c.BaseRadius, c.arc, c.startAngle, c.shift_z, c.scale_z, 1, 5);

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(
                c.Transform, c.Positions, c.Normals);
            c.Mesh.Vertices = res.Positions;
            c.Mesh.Normals = res.Normals;

            return new MeshGeometry(c.Positions, c.Normals, c.Indices);
        }
    }
}