using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Collections.Generic;
namespace IGX.Geometry.Tessellation
{
    public class CubeTessellator : ITessellator<Cube>
    {
        public MeshGeometry Tessellate(Cube primitive, uint nSeg, bool bCap, bool tCap)
        {
            float halfX = primitive.Size.X * 0.5f;
            float halfY = primitive.Size.Y * 0.5f;
            float halfZ = primitive.Size.Z * 0.5f;

            // 실제 테셀레이션
            MeshGeometry mesh = TessellationUtility.SixFacetsVolume(-halfX, halfX, -halfY, halfY, -halfZ, halfZ, 0);

            // Trans 적용
            (List<Vector3> pos, List<Vector3> norm) = TessellationUtility.MatrixApply(primitive.Transform, mesh.Vertices, mesh.Normals);
            mesh.Vertices = pos;
            mesh.Normals = norm;

            return mesh;
        }
    }
}
