using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render.Materials;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render.ClipPlane
{
    public class ClipBoxGizmoRenderer : IDisposable
    {
        private readonly ClippingBox _clipBoxData;
        private int _clipVao;
        private VertexBuffer<Vector3>? _clipVbo;
        private ElementBuffer? _clipEbo;
        private readonly Shader _gizmoShader;
        private bool _needsBufferUpdate = true;
        private bool disposedValue = false;
        private const int LineIndicesCount = 24;
        private const int CornerVerticesCount = 8;

        uint[] indices =
        [
            0, 1, 1, 3, 3, 2, 2, 0,
            4, 5, 5, 7, 7, 6, 6, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        ];
        public ClipBoxGizmoRenderer(ClippingBox clipBoxData)
        {
            _clipBoxData = clipBoxData;
            _gizmoShader = ShaderManager.Create("Gizmo", GizmoVertex, GizmoFragment, false, true);
            SetupClipBoxBuffers();
        }
        //private DrawElementGeometry<Vector3>? _instancedGeometry;
        private void SetupClipBoxBuffers()
        {
            Vector3[] initialCorners = _clipBoxData.GetClipBoxCorners();

            _clipVao = GL.GenVertexArray();
            GL.BindVertexArray(_clipVao);

            _clipVbo = new VertexBuffer<Vector3>(initialCorners, BufferUsageHint.DynamicDraw);//, BufferTarget.ArrayBuffer);
            _clipVbo.Bind();
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _clipEbo = new ElementBuffer(indices, BufferUsageHint.DynamicDraw, true);
            _clipEbo.Bind();

            GL.BindVertexArray(0);
            _clipVbo.Unbind();
            _clipEbo.Unbind();
        }
        public void Draw(IMyCamera camera, Vector4 color)
        {
            Matrix4 view = camera.ViewMatrix;
            Matrix4 projection = camera.ProjectionMatrix;

            if (_needsBufferUpdate)
            {
                Vector3[] updatedCorners = _clipBoxData.GetClipBoxCorners();
                _clipVbo?.SyncToGpuSub(updatedCorners, 0);
                _needsBufferUpdate = false;
            }

            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.ProgramPointSize);

            using (_gizmoShader.Use())
            {
                _gizmoShader.SetUniformIfExist("view", view);
                _gizmoShader.SetUniformIfExist("projection", projection);

                GL.BindVertexArray(_clipVao);

                GL.Enable(EnableCap.DepthTest);
                _gizmoShader.SetUniformIfExist("u_Color", color);
                GL.DrawElements(PrimitiveType.Lines, LineIndicesCount, DrawElementsType.UnsignedInt, 0);

                GL.PointSize(10.0f);
                _gizmoShader.SetUniformIfExist("u_Color", new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                GL.DrawArrays(PrimitiveType.Points, 0, CornerVerticesCount);

                GL.BindVertexArray(0);
            }
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.ProgramPointSize);
        }
        public void NotifyUpdateNeeded()
        {
            _needsBufferUpdate = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _gizmoShader?.Dispose();
                }
                _clipEbo?.Dispose();
                _clipVbo?.Dispose();
                GL.DeleteVertexArray(_clipVao);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// A string containing the GLSL code for clipping, intended to be included in other shaders.
        /// </summary>
        public static string ClippingShaderHeader = @"
            uniform int u_ClipMode;
            uniform vec4 u_ClipPlanes[6];
            bool IsClipped(vec3 worldPos)
            {
                if (u_ClipMode == 0) return false;
                if (u_ClipMode == 1)
                {
                    return dot(vec4(worldPos, 1.0), u_ClipPlanes[0]) < 0.0;
                }
                if (u_ClipMode == 2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (dot(vec4(worldPos, 1.0), u_ClipPlanes[i]) < 0.0)
                            return true;
                    }
                }
                return false;
            }
        ";

        /// <summary>
        /// The vertex PassShader for rendering the gizmo.
        /// </summary>
        public static string GizmoVertex = @"
            #version 460 core
            layout (location = 0) in vec3 aPos;
            uniform mat4 view;
            uniform mat4 projection;
            out vec3 WorldPos;
            void main()
            {
                gl_Position = projection * view * vec4(aPos, 1.0);
                WorldPos = aPos;
            }
        ";

        /// <summary>
        /// The fragment PassShader for rendering the gizmo.
        /// </summary>
        public static string GizmoFragment = @"
            #version 460 core
            out vec4 FragColor;
            uniform vec4 u_Color;
            void main()
            {
                FragColor = u_Color;
            }
        ";
    }
}