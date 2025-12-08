using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.GLDataStructure
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct SSBOInstanceElement
    {
        public int assemblyID;
        public int MeshId;
        public ParaPrimType PrimitiveType;
        public int geoLayer;
        public Matrix4 model;
        public Vector4 Color;
        public Vector3 AabbMin;
        public Vector3 AabbMax;
        public Vector2 _padding;
        public static int SizeInBytes => Marshal.SizeOf(typeof(SSBOInstanceElement));

        public void MakeInstanceBufferData(GeometryInstance instanceData)
        {
            assemblyID = instanceData.EassemblyID;
            MeshId = instanceData.GeometryID;
            PrimitiveType = instanceData.GeomType;
            geoLayer = 1;// if 0, do not draw
            if (PrimitiveType != ParaPrimType.FacetVolume)
            { // 각 vertex에 회전 및 이동 행렬 값이 이미 적용되어 있음. SSBO를 사용해서 인스턴싱하게 되면 고쳐야 함
                model = Matrix4.Identity;
            }
            else
            {
                model = instanceData.Model;
            }
            Color = instanceData.Color; // 이미 Build 메서드에서 설정됨
            //AabbMin = Aabb.min;
            //AabbMax = Aabb.max;
        }
        public readonly bool Equals(SSBOInstanceElement other)
        {
            return assemblyID == other.assemblyID && MeshId == other.MeshId && PrimitiveType == other.PrimitiveType
                && Color == other.Color
                && AabbMin == other.AabbMin && AabbMax == other.AabbMax;
        }
    }
}
