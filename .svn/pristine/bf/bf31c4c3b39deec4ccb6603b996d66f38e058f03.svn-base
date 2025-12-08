using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.GLDataStructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshInstanceGL
    {
        [Vertex(Location =  2, Size =16)] public Matrix4 Model;
        [Vertex(Location =  6, Size = 4)] public Vector4 Color;
        [Vertex(Location =  7, Size = 1)] public int SelectionMode;
        [Vertex(Location =  8, Size = 1, Type = All.Int)] public int EassemblyID;
        [Vertex(Location =  9, Size = 1, Type = All.Int)] public int MeshId;
        [Vertex(Location = 10, Size = 1, Type = All.Int)] public ParaPrimType GeomType;
        [Vertex(Location = 11, Size = 1, Type = All.Int)] public int Layer;

        public static int SizeInBytes => Marshal.SizeOf(typeof(MeshInstanceGL));

        public MeshInstanceGL(GeometryInstance instanceData, string grandType)
        {
            EassemblyID = instanceData.EassemblyID;
            MeshId = instanceData.GeometryID;
            GeomType = instanceData.GeomType;
            if (grandType == "INSU")
            {
                Layer = 2;
            }
            else if (grandType == "OBST")
            {
                Layer = 3;
            }
            else
            {
                Layer = 1;
            }
            if (GeomType != ParaPrimType.FacetVolume)
            {
                Model.Row0 = Vector4.UnitX;
                Model.Row1 = Vector4.UnitY;
                Model.Row2 = Vector4.UnitZ;
                Model.Row3 = Vector4.UnitW;
            }
            else
            {
                Model = instanceData.Model;
            }
            Color = instanceData.Color; // 이미 Build 메서드에서 설정됨
            instanceData.SelectionMode = 0;
            SelectionMode = (int)instanceData.SelectionMode; // 그려라
        }
    }
}
