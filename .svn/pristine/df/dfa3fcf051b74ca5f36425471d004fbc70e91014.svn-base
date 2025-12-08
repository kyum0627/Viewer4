using IGX.Geometry.Common;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render.Materials
{
    public readonly struct VectorInstance
    {
        public GLVertex Vtx { get; }
        public Vector4 Color { get; }
        public VectorInstance(GLVertex v, Vector4 color)
        {
            Vtx = v;
            Color = color;
        }
    }
    public static class MatrixInstanceHelper
    {
        public static BasicInstance[] CalculateInstanceMatrices(IEnumerable<VectorInstance> arrowDataList, Matrix4 view, Matrix4 projection, float size)
        {
            if (arrowDataList == null || !arrowDataList.Any())
                return Array.Empty<BasicInstance>();

            var instanceData = new BasicInstance[arrowDataList.Count()];
            int i = 0;
            foreach (var arrowData in arrowDataList)
            {
                Vector4 worldPos = new(arrowData.Vtx.Position, 1.0f);
                Vector4 clipPos = worldPos * view * projection;

                float scaleFactor = size * clipPos.W / projection.M11;
                Quaternion rotation = Vector3.UnitZ.FromUnitVectors(arrowData.Vtx.Normal.Normalized());
                Matrix4 matrix = Matrix4.CreateScale(scaleFactor) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(arrowData.Vtx.Position);

                instanceData[i++] = new BasicInstance
                {
                    Model = matrix,
                    Color = arrowData.Color
                };
            }
            return instanceData;
        }
    }
}