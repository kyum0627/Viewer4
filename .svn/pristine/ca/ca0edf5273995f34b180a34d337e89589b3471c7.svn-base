using IGX.Geometry.Common;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class SnoutTessellator : ITessellator<Snout>
    {
        public MeshGeometry Tessellate(Snout s, uint n_segs, bool bcap = true, bool tcap = true)
        {
            (Vector2[] vertexArray, Vector2[] norarr) = GeometryHelper.MakeArc2(1, n_segs, MathHelper.TwoPi, 0);
            List<Vector2> unitcircle = new(vertexArray);
            float h2 = s.Height * 0.5f;
            float x2 = s.Xoffset * 0.5f;
            float y2 = s.Yoffset * 0.5f;

            List<Vector3> btm = unitcircle.ConvertAll(p => s.Rbottom * new Vector3(p.X, p.Y, 0));
            List<Vector3> top = unitcircle.ConvertAll(p => s.Rtop * new Vector3(p.X, p.Y, 0));

            Vector2 slopeBtm = new((float)Math.Tan(s.XbottomShear), (float)Math.Tan(s.YbottomShear));
            Vector2 slopeTop = new((float)Math.Tan(s.XtopShear), (float)Math.Tan(s.YtopShear));

            List<Vector3> nor = new((int)n_segs);
            for (int i = 0; i < n_segs; i++)
            {
                float xb = btm[i].X - x2; float yb = btm[i].Y - y2; float zb = -h2 + slopeBtm.X * btm[i].X + slopeBtm.Y * btm[i].Y;
                btm[i] = new Vector3(xb, yb, zb);
                float xt = top[i].X + x2; float yt = top[i].Y + y2; float zt = h2 + slopeTop.X * top[i].X + slopeTop.Y * top[i].Y;
                top[i] = new Vector3(xt, yt, zt);
                Vector3 slant = top[i] - btm[i];
                Vector3 nnn = new Vector3(-btm[i].Y, btm[i].X, 0).Cross(slant).Normalized();
                nor.Add(nnn);
            }

            s.Positions.AddRange(btm);
            s.Positions.AddRange(top);
            s.Normals.AddRange(nor);
            s.Normals.AddRange(nor);
            for (uint i = 0; i < n_segs; i++)
            {
                TessellationUtility.ZigZag(s.Indices, n_segs + i, i, n_segs + (i + 1) % n_segs, (i + 1) % n_segs);
            }

            if (bcap && s.Rbottom > 0)
            {
                TessellationUtility.AddCircularCap(0, n_segs, s.Positions, s.Normals, s.Indices, true);
            }
            if (tcap && s.Rtop > 0)
            {
                TessellationUtility.AddCircularCap(n_segs, n_segs, s.Positions, s.Normals, s.Indices, false);
            }

            (List<Vector3> Positions, List<Vector3> Normals) res = TessellationUtility.MatrixApply(s.Transform, s.Positions, s.Normals);
            s.Mesh.Vertices = res.Positions;
            s.Mesh.Normals = res.Normals;

            return s.Mesh;
        }
    }
}