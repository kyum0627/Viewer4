using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    public class DrawCurves : IDisposable
    {
        private readonly VertexArrayObject vao;
        private readonly VertexBuffer<GLVertex> vbo;
        private readonly Shader shader;

        private readonly List<(int vertexStart, int vertexCount)> curveRenderInfo = [];
        private readonly List<(int vertexStart, int vertexCount)> pointRenderInfo = [];

        public DrawCurves(GLVertex[] initialVertices, Shader shader)
        {
            this.shader = shader ?? throw new ArgumentNullException(nameof(shader));
            vao = new VertexArrayObject();
            vbo = new VertexBuffer<GLVertex>(initialVertices, BufferUsageHint.DynamicDraw);
            vao.Bind();
            vbo.SetAttributes();
            vao.Unbind();
        }

        public void AddCurveData(GLVertex[] newCurveVertices, uint[] newCurveIndices, uint[] newControlPointIndices)
        {
            int currentVertexCount = vbo.Count;
            curveRenderInfo.Add((currentVertexCount, newCurveVertices.Length));
            pointRenderInfo.Add((currentVertexCount + newCurveIndices.Length, newControlPointIndices.Length));
        }

        public void Render(Matrix4 model, IMyCamera camera, Vector4 color, bool drawPoints = false)
        {
            shader.Use();
            shader.SetUniformIfExist("uModel", model);
            shader.SetUniformIfExist("uView", camera.ViewMatrix);
            shader.SetUniformIfExist("uProjection", camera.ProjectionMatrix);
            shader.SetUniformIfExist("uObjectColor", color);

            vao.Bind();

            foreach ((int vertexStart, int vertexCount) info in curveRenderInfo)
            {
                GL.DrawArrays(PrimitiveType.LineStrip, info.vertexStart, info.vertexCount);
            }

            if (drawPoints)
            {
                GL.PointSize(5.0f);
                foreach ((int vertexStart, int vertexCount) info in pointRenderInfo)
                {
                    GL.DrawArrays(PrimitiveType.Points, info.vertexStart, info.vertexCount);
                }
            }

            vao.Unbind();
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    shader?.Dispose();
                }
                vbo?.Dispose();
                vao?.Dispose();
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}