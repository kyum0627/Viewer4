using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using IGX.Geometry.DataStructure;
using IGX.Geometry.DataStructure.IgxMesh;
using IGX.Geometry.Tessellation;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace IGX.Geometry.GeometryBuilder
{
    public class FacetVolumeTessellator : ITessellator<FacetVolume>
    {
        public MeshGeometry Tessellate(FacetVolume f, uint n_seg, bool bBcap = true, bool bTcap = true)
        {
            return new MeshGeometry(f.Positions ?? new List<Vector3>(), f.Normals ?? new List<Vector3>(), f.Indices ?? new List<uint>());
        }
    }
    public class FacetVolume : PrimitiveBase, IEquatable<FacetVolume>
    {
        public override bool IsTransformable => false;
        public IgxMesh Volume { get; init; }
        public List<TrianglesAdjacency> Triangles => Volume.Triangles;
        public float Thickness { get => Volume.Thickness; }
        public bool IsFlat { get; init; }
        public Vector3 COG => Volume?.TotalCentroid ?? Vector3.Zero;
        public int SurfaceCount => Volume?.Surfaces?.Count ?? 0;
        public List<SurfaceMesh> Surfaces => Volume?.Surfaces ?? new List<SurfaceMesh>();
        public List<Vector3> GetOutBoundPoints(int surfid) => Volume.GetOutboundaryPoints(surfid) ?? new List<Vector3>();
        public SurfaceMesh? MoldSurface => Volume.MoldSurface;
        public SurfaceMesh? OffsetSurface => Volume.OffsetSurface;
        public Vector3 MoldSurfaceNormal => Volume?.MoldSurface!.Normal ?? Vector3.Zero;
        public Vector3 OffsetSurfaceNormal => Volume?.OffsetSurface!.Normal ?? Vector3.Zero;

        public FacetVolume(Matrix4 matrix, AABB3 bbox, List<Facet3>? facets, bool areAlltriangles)
        {
            Mesh.Aabb = bbox;
            Mesh.Oobb = OOBB3.Empty; IsFlat = !areAlltriangles; Dictionary<Vertex, uint> vtx = new();
            Mesh.Indices = new List<uint>();
            TransformComp.Trans = matrix;

            if (facets != null)
            {
                foreach (Facet3 facet in facets)
                {
                    Facet2Mesh.TessellateResult res = Facet2Mesh.Tessellate(facet); if (res.indices == null || res.indices.Count % 3 != 0 || res.indices.Count < 3)
                    {
                        continue;
                    }

                    uint icount = (uint)res.indices.Count;
                    uint vcount = (uint)res.vertices.Length;
                    foreach (uint i in res.indices)
                    {
                        if (i < 0 || i >= vcount || i >= icount)
                        {
                            continue;
                        }
                        AddIfNotExists(vtx, new Vertex(res.vertices[i], res.normals[i]));
                    }
                }
            }

            Volume = new IgxMesh();
            var vtxSpan = CollectionsMarshal.AsSpan(vtx.Keys.ToList());
            var indSpan = CollectionsMarshal.AsSpan(Indices);
            Volume.ProcessIgxMesh(GetInitialVolumeVertices(vtxSpan, indSpan));

            Mesh.Vertices = Volume.Positions;
            Mesh.Normals = Volume.Normals;
            Mesh.Indices = Volume.Indices;
            Mesh.EdgeIndices = Volume.GetBoundaryEdgeIndices();
            IsFlat = Surfaces[0].IsFlat;

            if (Positions.Count > 0)
            {
                Mesh.Oobb = new MinimumVolumeBox(Positions).Oobb;
            }
            else
            {
                Mesh.Oobb = OOBB3.Empty;
            }
            SortMoldOffset();
        }
        private void SortMoldOffset()
        {
            if (Surfaces.Count < 2)
            {
                return;
            }
            var moldCandidate = Surfaces[0];
            var offsetCandidate = Surfaces[1];
            var mtest = moldCandidate.Centroid + moldCandidate.Normal * 10;
            var otest = offsetCandidate.Centroid + offsetCandidate.Normal * 10;

            var majorAxis = Oobb.axisZ.MaxLengthCoordinate();

            if (mtest.ManhattanDistance() > otest.ManhattanDistance())
            {
                if (Surfaces.Count == 2 && Surfaces[0].Centroid == Surfaces[1].Centroid)
                {
                    (Surfaces[0], Surfaces[1]) = (Surfaces[1], Surfaces[0]);
                    Surfaces.Remove(Surfaces[1]);
                }
            }

            if (IsFlat)
            {
                if (moldCandidate.Centroid[majorAxis] * moldCandidate.Normal[majorAxis] > 0)
                {
                    (Surfaces[0], Surfaces[1]) = (Surfaces[1], Surfaces[0]);
                }
            }
            else
            {
                if (moldCandidate.Normal.Y * moldCandidate.Centroid.Y > 0 && SurfaceCount > 1)
                {
                    (Surfaces[0], Surfaces[1]) = (Surfaces[1], Surfaces[0]);
                }
            }
        }
        public List<Vector3> SurfaceOutBoundaryPoints(int surfaceid)
        {
            return Volume.GetOutboundaryPoints(surfaceid);
        }

        private void AddIfNotExists(Dictionary<Vertex, uint> vtx, Vertex v)
        {
            if (!vtx.TryGetValue(v, out uint vtxIndex))
            {
                Indices.Add((uint)vtx.Count);
                vtx[v] = (uint)vtx.Count;
            }
            else
            {
                Indices.Add(vtxIndex);
            }
        }

        private Vertex[] GetInitialVolumeVertices(ReadOnlySpan<Vertex> vtx, ReadOnlySpan<uint> ind)
        {
            if (vtx.IsEmpty || ind.IsEmpty)
                return Array.Empty<Vertex>();

            // 최대 크기 만큼 미리 배열 생성
            Vertex[] result = new Vertex[ind.Length];
            int count = 0;

            foreach (uint idx in ind)
            {
                if (idx < vtx.Length)
                {
                    result[count++] = vtx[(int)idx];
                }
            }

            // 실제 사용된 만큼 잘라내고 반환
            if (count == result.Length)
                return result;

            Array.Resize(ref result, count);
            return result;
        }

        public override ParaPrimType GetParametricType() => ParaPrimType.FacetVolume;

        public bool Equals(FacetVolume? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if ((Aabb != AABB3.Empty || Aabb != AABB3.Infinite)
                && (Oobb != OOBB3.Empty))
            {
                if (Aabb != other.Aabb) return false;
                if (Oobb != other.Oobb) return false;
                if (Volume.Positions.Count != other.Volume.Positions.Count) return false;
                if (Volume.Normals.Count != other.Volume.Normals.Count) return false;
                if (Volume.Positions != other.Volume.Positions) return false;
                if (Volume.Normals != other.Volume.Normals) return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Aabb.GetHashCode();
                hash = hash * 23 + Oobb.GetHashCode();
                if (Volume.Positions != null)
                {
                    foreach (var pos in Volume.Positions)
                    {
                        hash = hash * 23 + pos.GetHashCode();
                    }
                }
                if (Volume.Normals != null)
                {
                    foreach (var normal in Volume.Normals)
                    {
                        hash = hash * 23 + normal.GetHashCode();
                    }
                }
                return hash;
            }
        }
    }
}