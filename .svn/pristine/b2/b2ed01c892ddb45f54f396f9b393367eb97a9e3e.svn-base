using IGX.Geometry.Common;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGX.Geometry.Tessellation
{
    public class CircularTorusTessllator :ITessellator<CircularTorus>
    {
        public MeshGeometry Tessellate(CircularTorus ct, uint n_seg, bool bcap = true, bool tcap = true)
        {
            uint nn = Sagitta.SegmentCount(ct.Angle, ct.Offset + ct.Radius, 1, 5);
            uint n_longi = nn + 1;
            float del_beta = ct.Angle / nn;

            List<Vector2> nor2 = TessellationUtility.CreateUnitCircle(n_seg).ToList();

            List<Vector3> section = nor2.ConvertAll(n => new Vector3(ct.Radius * n.X, ct.Radius * n.X, ct.Radius * n.Y));
            for (int i = 0; i < n_longi; i++)
            {
                float beta = del_beta * i; float cosr = (float)Math.Cos(beta); float sinr = (float)Math.Sin(beta);
                Vector3 translate = new(ct.Offset * cosr, ct.Offset * sinr, 0f);

                for (int j = 0; j < n_seg; j++)
                {
                    Vector3 sectionPoint = section[j];
                    float x = sectionPoint.X * cosr + translate.X;
                    float y = sectionPoint.Y * sinr + translate.Y; float z = sectionPoint.Z + translate.Z;
                    ct.Positions.Add(new Vector3(x, y, z));

                    Vector3 normal = new Vector3(nor2[j].X * cosr, nor2[j].X * sinr, nor2[j].Y).Normalized();
                    ct.Normals.Add(normal);
                }
            }

            CreateTorusIndices(n_longi, n_seg, ct.Indices);

            if (bcap)
            {
                TessellationUtility.AddCircularCap(0, n_seg, ct.Positions, ct.Normals, ct.Indices, true);
            }
            if (tcap)
            {
                TessellationUtility.AddCircularCap(n_longi * n_seg, n_seg, ct.Positions, ct.Normals, ct.Indices, false);
            }

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(ct.Transform, ct.Positions, ct.Normals);
            ct.Mesh.Vertices = res.Positions;
            ct.Mesh.Normals = res.Normals;
            return new MeshGeometry(ct.Mesh.Vertices, ct.Mesh.Normals, ct.Mesh.Indices);
        }

        static void CreateTorusIndices(uint n_longi, uint n_segs, List<uint> indices)
        {
            for (uint i = 0; i < n_longi - 1; i++)
            {
                uint next_loop = i + 1;
                for (uint j = 0; j < n_segs; j++)
                {
                    uint next_j = (j + 1) % n_segs;
                    TessellationUtility.ZigZag(indices, i * n_segs + j, next_loop * n_segs + j, i * n_segs + next_j, next_loop * n_segs + next_j);
                }
            }
        }
    }
}