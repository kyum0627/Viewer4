using IGX.Geometry.GeometryBuilder;

namespace IGX.Geometry.Tessellation
{
    public interface ITessellator<T>
    {
        MeshGeometry Tessellate(T primitive, uint nSeg, bool bCap, bool tCap);
    }
}
