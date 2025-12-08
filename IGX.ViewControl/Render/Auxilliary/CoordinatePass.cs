using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render.Materials;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl.Render
{
    public enum CoordinateDrawPosition
    {
        Center,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp
    }

    /// <summary>
    /// 좌표축 렌더링 Pass - RenderPassBase를 상속받아 미니 뷰포트에 좌표축 표시
    /// </summary>
    internal class CoordinatePass : RenderPassBase
    {
        public override string Name => "CoordinatePass";

        private DrawInstanceGeometry<GLVertex, uint, BasicInstance>? _buffer;
        public CoordinateDrawPosition coordPosition = CoordinateDrawPosition.LeftDown;
        public float arrowsize = 1.0f;

        private readonly List<VectorInstance> coordVectors = new List<VectorInstance>
        {
            new VectorInstance(new GLVertex(Vector3.Zero, Vector3.UnitX), new Vector4(Vector3.UnitX, 1)),
            new VectorInstance(new GLVertex(Vector3.Zero, Vector3.UnitY), new Vector4(Vector3.UnitY, 1)),
            new VectorInstance(new GLVertex(Vector3.Zero, Vector3.UnitZ), new Vector4(Vector3.UnitZ, 1))
        };

        public override void Initialize(object? context1 = null, object? context2 = null)
        {
            base.Initialize(context1, context2);

            Debug.WriteLine($"[CoordinatePass] Initialize called, context1 type: {context1?.GetType().Name ?? "null"}");

            if (context1 is IMyCamera camera)
            {
                SetCamera(camera);
                Debug.WriteLine($"[CoordinatePass] Camera set from context1");
            }

            PassShader = ShaderManager.Create(
                "Coordinate",
                ShaderSource.VertexShaderSourceInstancedAttrib,
                ShaderSource.FragmentShaderSourceInstancedAttrib,
                false,
                true);

            var (vertices, indices) = Arrow.GenerateMesh();
            _buffer = new DrawInstanceGeometry<GLVertex, uint, BasicInstance>(
                vertices,
                indices,
                new BasicInstance[3]);

            Debug.WriteLine($"[CoordinatePass] Initialize completed, Camera: {Camera != null}, Shader: {PassShader != null}, Buffer: {_buffer != null}");
        }

        public override void Execute()
        {
            Debug.WriteLine($"[CoordinatePass] Execute called - Enabled: {Enabled}, Camera: {Camera != null}, Buffer: {_buffer != null}");

            if (!Enabled || Camera == null || _buffer == null)
            {
                Debug.WriteLine($"[CoordinatePass] Execute skipped - Enabled: {Enabled}, Camera: {Camera != null}, Buffer: {_buffer != null}");
                return;
            }

            // 기존 뷰포트 저장
            int[] originalViewport = new int[4];
            GL.GetInteger(GetPName.Viewport, originalViewport);
            GL.Enable(EnableCap.DepthTest);

            var rotate = Camera.RotationMatrix;

            // Viewport 및 Projection 계산
            CalculateGizmoProjectionMatrix(Camera, coordPosition,
                out int viewportWidth, out int viewportHeight,
                out int viewportX, out int viewportY, out Matrix4 gizmoProjection);

            Matrix4 gizmoView = rotate;
            GL.Viewport(viewportX, viewportY, viewportWidth, viewportHeight);

            Debug.WriteLine($"[CoordinatePass] Viewport: ({viewportX}, {viewportY}, {viewportWidth}, {viewportHeight})");

            var instanceData = MatrixInstanceHelper.CalculateInstanceMatrices(
                coordVectors, gizmoView, gizmoProjection, arrowsize);
            
            if (instanceData != null && instanceData.Length > 0)
            {
                Debug.WriteLine($"[CoordinatePass] Calculated {instanceData.Length} instance matrices");
                // ✅ UpdateInstanceRange 메서드 사용
                _buffer.UpdateInstanceRange(instanceData, 0);

                // Shader 활성화를 ExecuteWithShader에 맡기지 않고 직접 제어
                using (PassShader!.Use())
                {
                    // Uniform 설정
                    PassShader.SetUniformIfExist("uView", gizmoView);
                    PassShader.SetUniformIfExist("uProjection", gizmoProjection);
                    
                    Debug.WriteLine($"[CoordinatePass] Executing draw");
                    
                    // VAO 바인딩하고 Draw 호출
                    _buffer.Bind();
                    GL.DrawElementsInstanced(
                        _buffer.Renderer.PrimType,  // ✅ Renderer 속성 사용
                        _buffer.IndexCount,
                        _buffer.ElementType,
                        IntPtr.Zero,
                        _buffer.InstanceCount);
                    _buffer.Unbind();
                }
            }
            else
            {
                Debug.WriteLine($"[CoordinatePass] No instance data calculated");
            }

            // 원래 뷰포트 복원
            GL.Viewport(originalViewport[0], originalViewport[1], originalViewport[2], originalViewport[3]);
            Debug.WriteLine($"[CoordinatePass] Execute completed, viewport restored");
        }

        public override void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            if (PassShader == null) return;

            PassShader.SetUniformIfExist("uView", view);
            PassShader.SetUniformIfExist("uProjection", projection);
        }

        public override void Dispose()
        {
            if (_disposed) return;
            
            _buffer?.Dispose();
            base.Dispose();
        }

        private static void CalculateGizmoProjectionMatrix(
            IMyCamera camera,
            CoordinateDrawPosition coordPosition,
            out int viewportWidth,
            out int viewportHeight,
            out int viewportX,
            out int viewportY,
            out Matrix4 gizmoProjection)
        {
            var screenWidth = camera.ViewportWidth;
            var screenHeight = camera.ViewportHeight;

            int margin = 20;
            viewportWidth = 100 + margin;
            viewportHeight = 100 + margin;

            switch (coordPosition)
            {
                case CoordinateDrawPosition.Center:
                    viewportX = (screenWidth / 2) - (viewportWidth / 2);
                    viewportY = (screenHeight / 2) - (viewportHeight / 2);
                    break;
                case CoordinateDrawPosition.LeftDown:
                    viewportX = margin;
                    viewportY = margin;
                    break;
                case CoordinateDrawPosition.LeftUp:
                    viewportX = margin;
                    viewportY = screenHeight - viewportHeight - margin;
                    break;
                case CoordinateDrawPosition.RightDown:
                    viewportX = screenWidth - viewportWidth - margin;
                    viewportY = margin;
                    break;
                case CoordinateDrawPosition.RightUp:
                    viewportX = screenWidth - viewportWidth - margin;
                    viewportY = screenHeight - viewportHeight - margin;
                    break;
                default:
                    viewportX = margin;
                    viewportY = margin;
                    break;
            }

            var projectionHalf = (float)Math.Max(viewportWidth, viewportHeight) / 2.0f;
            gizmoProjection = Matrix4.CreateOrthographicOffCenter(
                -projectionHalf, projectionHalf,
                -projectionHalf, projectionHalf,
                -projectionHalf, projectionHalf * 100);
        }
    }
}