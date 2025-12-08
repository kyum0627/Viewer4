using IGX.Geometry.Common;
using IGX.ViewControl.Render.Materials;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace IGX.ViewControl.Render.ClipPlane
{
    public enum ClipMode
    {
        None = 0,
        SinglePlane = 1,
        Box = 2,
        Section = 3
    }

    public class ClipPlaneSystem : IDisposable
    {
        public ClipMode Mode { get; set; } = ClipMode.Box;
        public bool Enabled { get; set; } = true;
        public bool ShowBox { get; set; } = true;

        public readonly ClippingBox ClipBoxData;
        private readonly ClipBoxGizmoRenderer _gizmoRenderer;
        private readonly ClipBoxController _controller;
        private readonly IMyCamera _camera;

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

        public ClipPlaneSystem(IMyCamera camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            ClipBoxData = new ClippingBox();
            _gizmoRenderer = new ClipBoxGizmoRenderer(ClipBoxData);
            _controller = new ClipBoxController(_camera, ClipBoxData, _gizmoRenderer);
        }

        /// <summary>
        /// WinForms MouseEventArgs로부터 마우스 입력 처리
        /// </summary>
        public void Update(Point mousePosition, bool isLeftButtonDown)
        {
            if (Enabled && Mode == ClipMode.Box)
            {
                _controller.HandleInput(mousePosition, isLeftButtonDown);
            }
        }

        public void Draw(Vector4 color)
        {
            System.Diagnostics.Debug.WriteLine($"ClipPlaneSystem.Draw: Enabled={Enabled}, ShowBox={ShowBox}, Mode={Mode}");
            
            if (Enabled && ShowBox && Mode == ClipMode.Box)
            {
                System.Diagnostics.Debug.WriteLine("ClipPlaneSystem.Draw: Calling _gizmoRenderer.Draw");
                _gizmoRenderer.Draw(_camera, color);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ClipPlaneSystem.Draw: Skipped (Enabled={Enabled}, ShowBox={ShowBox}, Mode={Mode})");
            }
        }

        public void ApplyToShader(int shaderHandle)
        {
            if (!Enabled)
            {
                return;
            }

            int modeLoc = GL.GetUniformLocation(shaderHandle, "u_ClipMode");
            int planeLoc = GL.GetUniformLocation(shaderHandle, "u_ClipPlanes");

            GL.Uniform1(modeLoc, (int)Mode);
            if (Mode != ClipMode.None)
            {
                GL.Uniform4(planeLoc, ClipBoxData.ClipPlanes.Length, ref ClipBoxData.ClipPlanes[0].X);
            }
        }

        public void ApplyToShader(Shader shader)
        {
            if (!Enabled || shader == null)
            {
                return;
            }

            shader.SetUniformIfExist("u_ClipMode", (int)Mode);
            if (Mode != ClipMode.None)
            {
                shader.SetUniformIfExist("u_ClipPlanes", ClipBoxData.ClipPlanes);
            }
        }

        public void ResetBox(AABB3 box)
        {
            ClipBoxData.ResetBox(box);
            _gizmoRenderer.NotifyUpdateNeeded();
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            _controller.IsEnabled = enabled;
        }

        public void Dispose()
        {
            _gizmoRenderer.Dispose();
        }
    }
}