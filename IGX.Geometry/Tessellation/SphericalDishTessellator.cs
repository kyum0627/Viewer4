using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class SphericalDishTessellator : ITessellator<SphericalDish>
    {
        public MeshGeometry Tessellate(SphericalDish s, uint n_segs, bool bcap = true, bool tcap = true)
        {
            float baseRadius = s.Radius;
            float height = s.Height;
            float sphereRadius = (baseRadius * baseRadius + height * height) / (2.0f * height);

            double sinval = Math.Min(1.0f, Math.Max(-1.0, baseRadius / sphereRadius));
            s.arc = (float)Math.Asin(sinval);

            if (baseRadius < height)
            {
                s.arc = (float)Math.PI - s.arc;
            }

            s.Mesh = TessellationUtility.GenerateSphereShape(n_segs, sphereRadius, s.arc, 0f, height - sphereRadius, 1f, 1, 5);

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(s.Transform, s.Positions, s.Normals);
            s.Mesh.Vertices = res.Positions;
            s.Mesh.Normals = res.Normals;
            return new MeshGeometry(s.Positions, s.Normals, s.Indices);
        }
    }
}