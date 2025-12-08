using IGX.Geometry.Common;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Tessellation
{
    public class RectangularTorusTessellator : ITessellator<RectangularTorus>
    {
        public MeshGeometry Tessellate(RectangularTorus r, uint n_segs, bool bcap = true, bool tcap = true)
        {
            uint n_rad = n_segs;
            if (n_rad < 3)
            {
                n_rad = Sagitta.SegmentCount(r.Angle, r.Routside, 1, 5);
            }

            uint samples = n_rad + 1;
            const bool shell = true;
            bool[] cap = { true, true };

            float[,] square = MakeSquare();
            float[] rotate = MakeRotateParameters(n_rad, samples);

            if (shell)
            {
                for (uint i = 0; i < samples; i++)
                {
                    float[,] n = MakeSectionNormals(rotate, i);
                    CalculateSectionVertices(square, rotate, i, n);
                }
            }

            if (cap[0])
            {
                for (uint k = 0; k < 4; k++)
                {
                    r.Positions.Add(new Vector3(square[k, 0] * rotate[0], square[k, 0] * rotate[1], square[k, 1]));
                    r.Normals.Add(new Vector3(0.0f, -1f, 0f));
                }
            }

            if (cap[1])
            {
                for (uint k = 0; k < 4; k++)
                {
                    r.Positions.Add(new Vector3(square[k, 0] * rotate[2 * (samples - 1) + 0], square[k, 0] * rotate[2 * (samples - 1) + 1], square[k, 1]));
                    r.Normals.Add(new Vector3(-rotate[2 * (samples - 1) + 1], rotate[2 * (samples - 1) + 0], 0f));
                }
            }

            uint offID = 0;
            if (shell)
            {
                List<uint> newind = new(8 * (int)samples);
                for (uint i = 0; i + 1 < samples; i++)
                {
                    for (uint k = 0; k < 4; k++)
                    {
                        uint a = 8 * i + 2 * k;
                        uint b = a + 1;
                        uint c = a + 8;
                        uint d = c + 1;
                        newind.AddRange(new uint[6] { a, b, c, c, b, d });
                    }
                }
                r.Indices.AddRange(newind); offID += 8 * samples;
            }

            if (cap[0])
            {
                r.Indices.AddRange(new uint[6] { offID, offID + 2, offID + 1, offID + 2, offID, offID + 3 });
                offID += 4;
            }

            if (cap[1])
            {
                r.Indices.AddRange(new uint[6] { offID, offID + 1, offID + 2, offID + 2, offID + 3, offID });
                offID += 4;
            }

            var res = TessellationUtility.MatrixApply(
                r.Transform, r.Positions, r.Normals);
            r.Mesh.Vertices = res.Positions;
            r.Mesh.Normals = res.Normals;

            return new MeshGeometry(r.Positions, r.Normals, r.Indices);

            float[,] MakeSquare()
            {
                float h2 = 0.5f * r.Height; float[,] square =
                {
                { r.Routside, -h2 },
                { r.Rinside, -h2 },
                { r.Rinside, h2 },
                { r.Routside, h2 },
            };
                return square;
            }

            float[] MakeRotateParameters(uint n_segs, uint samples)
            {
                float[] rotate = new float[2 * samples + 1]; for (uint i = 0; i < samples; i++)
                {
                    rotate[2 * i] = (float)Math.Cos(r.Angle / n_segs * i); rotate[2 * i + 1] = (float)Math.Sin(r.Angle / n_segs * i);
                }
                return rotate;
            }

            static float[,] MakeSectionNormals(float[] rotate, uint i)
            {
                return new float[,]
                {
                { 0.0f, 0.0f, -1.0f },
                { -rotate[2 * i], -rotate[2 * i + 1], 0.0f },
                { 0.0f, 0.0f, 1.0f },
                { rotate[2 * i], rotate[2 * i + 1], 0.0f },
                };
            }

            void CalculateSectionVertices(float[,] square, float[] rotate, uint i, float[,] n)
            {
                for (uint k = 0; k < 4; k++)
                {
                    uint kk = k + 1 & 3;
                    uint id = i * 2;
                    r.Positions.Add(new Vector3(square[k, 0] * rotate[2 * i], square[k, 0] * rotate[id + 1], square[k, 1]));
                    r.Normals.Add(new Vector3(n[k, 0], n[k, 1], n[k, 2]));
                    r.Positions.Add(new Vector3(square[kk, 0] * rotate[2 * i], square[kk, 0] * rotate[id + 1], square[kk, 1]));
                    r.Normals.Add(new Vector3(n[k, 0], n[k, 1], n[k, 2]));
                }
            }
        }
    }
}