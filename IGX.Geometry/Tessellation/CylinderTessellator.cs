using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace IGX.Geometry.Tessellation
{
    public class CylinderTessellator : ITessellator<Cylinder>
    {
        public MeshGeometry Tessellate(Cylinder c, uint n_seg, bool bBcap, bool bTcap)
        {
            if (!TessellationUtility.cachedUnitCylinders.TryGetValue((n_seg, true, true), out UnitPrimitiveMesh m))
            {
                m = TessellationUtility.CreateUnitCylinder(n_seg, true, true);
            }

            c.Mesh.Vertices = m.Vertices.Select(v => new Vector3(v.X * c.Radius, v.Y * c.Radius, v.Z * c.Height)).ToList();
            c.Mesh.Normals = (List<Vector3>)m.Normals;
            c.Mesh.Indices = (List<uint>)m.Indices;

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(c.Transform, c.Positions, c.Normals);
            c.Mesh.Vertices = res.Positions;
            c.Mesh.Normals = res.Normals;

            return c.Mesh;
        }
    }
}