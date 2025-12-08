using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.DataStructure
{
    public class Facet3 : IEquatable<Facet3>
    {
        public uint Id;
        public bool isCurved = false;
        public Loop3 Outter;
        public List<Loop3> Holes;
        public Vector3 Normal; // facet을 구성하는 정점들의 norID 평균
        public AABB3 box;
        public OOBB3 Oobb;

        public Vector3 COG = Vector3.Zero;
        public float area = 0;
        public float perimeter = 0;
        public Plane3f plane;

        public Facet3()
        {
            Outter = new Loop3();
            Holes = new List<Loop3>();
        }
        public bool Equals(Facet3? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Outter != other.Outter) return false;
            if (Holes != other.Holes) return false;
            return true;
        }

        public bool IsCurved()
        {
            List<Vector3> norms = new();
            norms = Outter.GetUniqueNormals().ToList();
            if (norms.Count == 1)
            {
                Normal = norms[0];
                plane = new Plane3f(Normal, Normal.Dot(Outter.Vertices[0].Position));
                isCurved = false;
            }
            else
            {
                List<Vector3> positions = Outter.GetUniquePoints().ToList();
                plane = new Plane3f(positions[0], positions[1], positions[2]);
                isCurved = true;
            }
            return isCurved;
        }
    }
}
