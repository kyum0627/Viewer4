using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.GeometryBuilder
{
    public enum GeometryType
    {
        Mesh,
        NurbsSurface,
        Curve,
        Solid,
        Implicit,
        Point,
        Line
    }
    //public abstract class GeometryBase
    //{
    //    public Guid Id { get; set; }
    //    public AABB3 Aabb { get; set; }
    //    public GeometryType Type { get; set; }

    //    protected GeometryBase()
    //    {
    //        Id = Guid.NewGuid();
    //    }
    //}

    public class MeshGeometry
    {
        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        //public Vector2[] UVs;
        public List<uint> Indices;
        public List<uint> EdgeIndices;

        public AABB3 Aabb { get; set; }
        public OOBB3 Oobb;
        public MeshGeometry() 
        {
            Vertices = new();
            Normals = new();
            Indices = new();
            EdgeIndices = new();
            Aabb = AABB3.Empty;
            Oobb = OOBB3.Empty;
            //Type = GeometryType.Mesh;
        }

        public MeshGeometry(List<Vector3> p, List<Vector3> n, List<uint> i, List<uint> e = null)
        {
            Vertices = p;
            Normals = n;
            Indices = i;
            EdgeIndices = e;
            Aabb = AABB3.Empty;
            Oobb = OOBB3.Empty;
            //Type = GeometryType.Mesh;
        }

        public void Append(MeshGeometry other)
        {
            uint offset = (uint)Vertices.Count;

            Vertices.AddRange(other.Vertices);
            Normals.AddRange(other.Normals);

            foreach (uint idx in other.Indices)
                Indices.Add(idx + offset);
        }
        public Triangle3f GetTriangle(int index, out AABB3 box)
        {
            int offset = index * 3;
            if (offset < 0 || offset + 2 >= Indices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"»ï°¢Çü ÀÎµ¦½º {index}´Â À¯È¿ ¹üÀ§¸¦ ¹þ¾î³². ÃÑ {Indices.Count / 3}°³ÀÇ »ï°¢ÇüÀÌ ÀÖÀ½.");
            }
            Vector3[] v = new Vector3[]
            {
                Vertices[(int)Indices[offset]],
                Vertices[(int)Indices[offset + 1]],
                Vertices[(int)Indices[offset + 2]]
            };
            box = AABB3.Empty;
            box.Contain(v[0]);
            box.Contain(v[1]);
            box.Contain(v[2]);
            return new Triangle3f(v[0], v[1], v[2]);
        }
    }
}
