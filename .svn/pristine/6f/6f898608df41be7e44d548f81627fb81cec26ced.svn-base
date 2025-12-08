using IGX.Geometry.Common;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    public abstract class GeometryBase : IGeometry
    {
        public string Name { get; set; } = "Geometry";
        public bool Visible { get; set; } = true;
        public PrimitiveType Primitive { get; set; } = PrimitiveType.Triangles;
        public IGeometryBuffer Buffer { get; protected set; }
        public IMaterial Material { get; set; }
        public Matrix4 LocalTransform { get; set; } = Matrix4.Identity;// Transform
        // Bounding box
        public AABB3 LocalBounds { get; protected set; } = AABB3.Empty;
        public AABB3 WorldBounds { get; protected set; } = AABB3.Empty;
        protected GeometryBase(IGeometryBuffer buffer, IMaterial? material = null)
        {
            Buffer = buffer;
            Material = material ?? new DefaultMaterial();
        }

        public virtual void UpdateWorldBounds(Matrix4 parentTransform)
        {
            var world = LocalTransform * parentTransform;
            WorldBounds = LocalBounds.Transform(world);
        }

        public virtual void Upload(float[] vertices, int[] indices)
        {
            Buffer.Upload(vertices, indices);
        }

        public virtual void UpdateLocalBounds(Vector3[] positions)
        {
            if (positions.Length == 0)
            {
                LocalBounds = AABB3.Empty;
                return;
            }

            Vector3 min = positions[0];
            Vector3 max = positions[0];

            foreach (var p in positions)
            {
                min = Vector3.ComponentMin(min, p);
                max = Vector3.ComponentMax(max, p);
            }

            LocalBounds = new AABB3(min, max);
        }

        public virtual void Draw(IMyCamera camera, ILight lighting)
        {
            if (!Visible || Buffer == null || Material == null)
                return;

            Material.ModelMatrix = LocalTransform;
            Material.Apply(camera, lighting);

            Buffer.Bind();
            Buffer.Draw(Primitive);
            Buffer.Unbind();

            Material.ResetRenderState();
        }

        public virtual void Dispose()
        {
            Buffer?.Dispose();
        }
    }
}
