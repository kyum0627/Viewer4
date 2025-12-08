using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace IGX.Geometry.DataStructure
{
    public class GeometryInstance : IEquatable<GeometryInstance>
    {
        public Matrix4 Model { get; set; } = Matrix4.Identity;// scale, rotation, translation
        public Vector4 Color { get; set; }
        public SelectTo SelectionMode { get; set; } = SelectTo.None;
        public int EassemblyID { get; set; }
        public int GeometryID { get; set; }
        public int MeshID { get; set; }
        public ParaPrimType GeomType { get; set; }  // 12가지 유형 중 하나, 실린더, 박스, 구, ....
        public int Layer { get; set; }
        public static int SizeInBytes => Marshal.SizeOf(typeof(GeometryInstance));

        public bool Equals(GeometryInstance? other)
        {
            if (GeometryID != other!.GeometryID
                && EassemblyID != other.EassemblyID
                && GeomType != other.GeomType
                && Model != other.Model) return false;
            return true;
        }
    }
}
