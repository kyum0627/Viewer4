using IGX.Geometry.GeometryBuilder;
using IGX.Geometry.Tessellation;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render.Materials
{
    public static class Arrow
    {
        public static int margin = 20;
        public static int viewportWidth = 100 + margin;
        public static int viewportHeight = 100 + margin;

        const float length = 1f;
        const float cylinderRatio = 0.7f;
        const float arrowRadiusRatio = 1f / 24f;
        const float coneRadiusRatio = 1f / 8f;
        const int segments = 16;
        const float arrowCylinderLength = length * cylinderRatio;
        const float arrowConeLength = length * (1f - cylinderRatio);
        const float arrowRadius = length * arrowRadiusRatio;
        const float coneRadius = length * coneRadiusRatio;
        public static (GLVertex[] Vertices, uint[] Indices) GenerateMesh()
        {
            Vector3 from = Vector3.Zero;
            Vector3 to = new(0, 0, arrowCylinderLength);
            Vector3 end = to + new Vector3(0, 0, arrowConeLength);

            var vertices = new List<GLVertex>();
            var indices = new List<uint>();

            Cylinder cylinderModel = new(from, to, arrowRadius);
            var temp = new CylinderTessellator();
            cylinderModel.Mesh = temp.Tessellate(cylinderModel, segments, true, false);

            Snout coneModel = new(to, end, coneRadius, 0);
            var temp2 = new SnoutTessellator();
            coneModel.Mesh = temp2.Tessellate(coneModel, segments, true, false);

            Append(cylinderModel, vertices, indices);
            Append(coneModel, vertices, indices);

            return (vertices.ToArray(), indices.ToArray());
        }

        private static void Append(PrimitiveBase mesh, List<GLVertex> vertices, List<uint> indices)
        {
            int baseVertex = vertices.Count;
            int firstIndex = indices.Count;

            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                vertices.Add(new GLVertex(mesh.Positions[i], mesh.Normals[i]));
            }

            foreach (var index in mesh.Indices)
            {
                indices.Add((uint)(index + baseVertex));
            }
        }
    }
}