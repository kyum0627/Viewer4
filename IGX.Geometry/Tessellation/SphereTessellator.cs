using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class SphereTessellator : ITessellator<Sphere>
    {
        public MeshGeometry Tessellate(Sphere s, uint n_seg, bool bcap = true, bool tcap = true)
        {
            s.arc = (float)Math.PI;
            s.startAngle = 0f;
            s.shift_z = 0f;
            s.scale_z = 1.0f;

            s.Mesh = TessellationUtility.GenerateSphereShape(n_seg, s.Radius, s.arc, s.startAngle, s.shift_z, s.scale_z, 1, 5);

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(s.Transform, s.Positions, s.Normals);
            s.Mesh.Vertices = res.Positions;
            s.Mesh.Normals = res.Normals;
            return new MeshGeometry(s.Positions!, s.Normals!, s.Indices);
        }
    }
}