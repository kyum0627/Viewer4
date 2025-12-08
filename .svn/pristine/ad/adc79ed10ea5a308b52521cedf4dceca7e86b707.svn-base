using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render.Materials;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl.Render.Auxilliary
{
    /// <summary>
    /// 법선 벡터 렌더링 Pass - RenderPassBase를 상속받아 법선 벡터 표시
    /// </summary>
    internal class NormalVectorsPass : RenderPassBase
    {
        public override string Name => "NormalVectorsPass";

        private DrawInstanceGeometry<GLVertex, uint, BasicInstance>? _buffer;
        private Func<List<GLVertex>>? _instanceDataProvider; // 함수로 변경!
        private Vector4 _color = new Vector4(0, 0, 1, 0.5f);

        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;

        public override void Initialize(object? context1 = null, object? context2 = null)
        {
            base.Initialize(context1, context2);

            Debug.WriteLine($"[NormalVectorsPass] Initialize called, context1 type: {context1?.GetType().Name ?? "null"}");

            // 리스트 자체가 아닌 리스트를 제공하는 함수를 받음
            if (context1 is Func<List<GLVertex>> provider)
            {
                _instanceDataProvider = provider;
                Debug.WriteLine($"[NormalVectorsPass] Instance data provider set");
            }
            else if (context1 is List<GLVertex> list)
            {
                // 하위 호환성: 직접 리스트를 받으면 함수로 래핑
                _instanceDataProvider = () => list;
                Debug.WriteLine($"[NormalVectorsPass] Direct list provided, wrapped in provider");
            }
            else
            {
                Debug.WriteLine($"[NormalVectorsPass] Initialize failed: context1 is not List<GLVertex> or provider");
                return;
            }

            PassShader = ShaderManager.Create(
                "Vector",
                ShaderSource.VertexShaderSourceInstancedAttrib,
                ShaderSource.FragmentShaderSourceInstancedAttrib,
                false,
                true);

            var (vertices, indices) = Arrow.GenerateMesh();
            int maxInstances = 100; // 충분한 크기로 초기화
            _buffer = new DrawInstanceGeometry<GLVertex, uint, BasicInstance>(
                vertices,
                indices,
                new BasicInstance[maxInstances]
            );

            if (context2 is Vector4 c)
            {
                _color = c;
            }

            Debug.WriteLine($"[NormalVectorsPass] Initialize completed, Camera: {Camera != null}, Shader: {PassShader != null}, Buffer: {_buffer != null}");
        }

        public override void Execute()
        {
            if (!Enabled || _instanceDataProvider == null || _buffer == null)
            {
                Debug.WriteLine($"[NormalVectorsPass] Execute skipped - Enabled: {Enabled}, Provider: {_instanceDataProvider != null}, Buffer: {_buffer != null}");
                return;
            }

            // 매 프레임마다 최신 데이터를 가져옴!
            var instanceData = _instanceDataProvider();

            Debug.WriteLine($"[NormalVectorsPass] Execute called - InstanceData count: {instanceData?.Count ?? 0}");

            if (instanceData == null || instanceData.Count == 0)
            {
                Debug.WriteLine($"[NormalVectorsPass] No instance data to render");
                return;
            }

            var vectorInstances = instanceData.Select(d => new VectorInstance(d, _color));

            var matrices = MatrixInstanceHelper.CalculateInstanceMatrices(
                vectorInstances,
                _viewMatrix,
                _projectionMatrix,
                1);

            if (matrices == null || matrices.Length == 0)
            {
                Debug.WriteLine($"[NormalVectorsPass] No matrices calculated");
                return;
            }

            Debug.WriteLine($"[NormalVectorsPass] Calculated {matrices.Length} instance matrices");

            // ✅ UpdateInstanceRange 메서드 사용
            _buffer.UpdateInstanceRange(matrices, 0);

            using (PassShader!.Use())
            {
                // Uniform 설정
                PassShader.SetUniformIfExist("uView", _viewMatrix);
                PassShader.SetUniformIfExist("uProjection", _projectionMatrix);
                PassShader.SetUniformIfExist("vObjectColor", _color);
                
                Debug.WriteLine($"[NormalVectorsPass] Uniforms set - executing draw");
                
                // VAO 바인딩하고 Draw 호출
                _buffer.Bind();
                GL.DrawElementsInstanced(
                    _buffer.Renderer.PrimType,  // ✅ Renderer 속성 사용
                    _buffer.IndexCount,
                    _buffer.ElementType,
                    IntPtr.Zero,
                    _buffer.InstanceCount);
                _buffer.Unbind();
                
                Debug.WriteLine($"[NormalVectorsPass] Draw completed");
            }
        }

        public override void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            _viewMatrix = view;
            _projectionMatrix = projection;
            Debug.WriteLine($"[NormalVectorsPass] SetCameraUniforms called");
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _buffer?.Dispose();
            base.Dispose();
        }
    }
}