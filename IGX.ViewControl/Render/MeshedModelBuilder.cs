using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using IGX.Loader;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl.Render
{

    public static class MeshedModelBuilder
    {
        private static readonly object _lockObject = new();
        public static MeshedModel Build(int modelId, Model3D model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            lock (_lockObject)
            {
                int geometryCount = model.Geometries?.Count ?? 0;
                MeshedModel bufferData = new(modelId);
                AABB3 modelBoundingBox = AABB3.Empty;

                var geometries = model.Geometries;
                var colors = model.IndexedColors;

                if (geometries == null || geometries.Count == 0)
                {
                    Debug.WriteLine($"경고: ID {modelId}인 모델에 지오메트리가 없음.");
                    return bufferData;
                }
                try
                {
                    var sortedGeometries = geometries
                        .OrderByDescending(g => g.Value.IsTransformable)  // Transformable(true)가 앞쪽
                        .ThenBy(g => g.Value.GeometryType)             // 선택적으로 메쉬 타입 기준 정렬
                        .ToList();

                    foreach (var entry in geometries)
                    {
                        var primitive = entry.Value;
                        var mesh = primitive.Mesh;
                        List<GLVertex> vertices = mesh.Vertices.Select((p, i) => new GLVertex(p, mesh.Normals[i])).ToList();

                        uint baseVertex = (uint)bufferData.Vertices.Length;
                        uint firstIndex = (uint)bufferData.TriIndices.Length;
                        const uint instanceCount = 1u;
                        uint baseInstance = (uint)primitive.InstanceData.GeometryID;

                        bufferData.AddVertices(vertices);
                        bufferData.AddTriIndices(mesh.Indices);
                        bufferData.AddFaceDrawCommand(new IndirectCommandData
                        {
                            count = (uint)mesh.Indices.Count,
                            instanceCount = instanceCount,
                            firstIndex = firstIndex,
                            baseVertex = baseVertex,
                            baseInstance = baseInstance
                        });

                        if (mesh.EdgeIndices != null && mesh.EdgeIndices.Count > 0)
                        {
                            uint efrstindex = (uint)bufferData.EdgeIndices.Length;
                            bufferData.AddEdgeDrawCommand(new IndirectCommandData
                            {
                                count = (uint)mesh.EdgeIndices.Count,
                                instanceCount = instanceCount,
                                firstIndex = efrstindex,
                                baseVertex = baseVertex,
                                baseInstance = baseInstance
                            });
                            bufferData.AddEdgeIndices(mesh.EdgeIndices);
                        }
                        
                        Color4 color4 = GetColor4(colors, primitive);
                        
                        primitive.InstanceData.Model = primitive.Transform;
                        primitive.InstanceData.Color = new Vector4(color4.R, color4.G, color4.B, color4.A);
                        
                        bufferData.AddInstance(primitive.InstanceData, primitive.GrandPrimType);
                        modelBoundingBox = modelBoundingBox.Contain(primitive.Aabb);
                    }

                    bufferData.ModelAabb = modelBoundingBox;
                    return bufferData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error building render data for model ID {modelId}: {ex.Message}");
                    return bufferData;
                }
            }
        }
        private static Color4 GetColor4(Dictionary<uint, VolColor> colors, PrimitiveBase geometry)
        {
            Color4 color = colors.TryGetValue(geometry.RenderComp.ColorID, out VolColor? cv)
                ? cv.ToBuffer()
                : Colors.GetColorValueFromID(geometry.RenderComp.ColorID).ToBuffer();

            color.A = geometry.RenderComp.GrandPrimType switch
            {
                "OBST" => 0.6f, // 투명
                "INSU" => 0.8f,
                _ => 1.0f,
            };

            return color;
        }
    }
}
