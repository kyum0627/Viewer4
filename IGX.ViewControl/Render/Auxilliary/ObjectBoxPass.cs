using IGX.Geometry.Tessellation;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl.Render.Auxilliary
{
    /// <summary>
    /// 오브젝트 박스 렌더링 Pass - RenderPassBase를 상속받아 선택된 오브젝트의 바운딩 박스 표시
    /// </summary>
    internal class ObjectBoxPass : RenderPassBase
    {
        public override string Name => "ObjectBoxPass";

        public DrawInstanceGeometry<GLVertex, uint, BasicInstance>? _buffer;
        private Func<List<BasicInstance>>? _instanceDataProvider; // 함수로 변경!
        private Vector4 _color = new Vector4(0, 0, 1, 1);
        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;

        private static readonly uint[] Indices =
        {
             0, 1, 1, 2, 2, 3, 3, 0,
             4, 5, 5, 6, 6, 7, 7, 4,
             8, 9, 9,10,10,11,11, 8,
            12,13,13,14,14,15,15,12,
            16,17,17,18,18,19,19,16,
            20,21,21,22,22,23,23,20
        };

        public override void Initialize(object? context1 = null, object? context2 = null)
        {
            base.Initialize(context1, context2);

            Debug.WriteLine($"[ObjectBoxPass] Initialize called, context1 type: {context1?.GetType().Name ?? "null"}");

            // 리스트 자체가 아닌 리스트를 제공하는 함수를 받음
            if (context1 is Func<List<BasicInstance>> provider)
            {
                _instanceDataProvider = provider;
                Debug.WriteLine($"[ObjectBoxPass] Instance data provider set");
            }
            else if (context1 is List<BasicInstance> list)
            {
                // 하위 호환성: 직접 리스트를 받으면 함수로 래핑
                _instanceDataProvider = () => list;
                Debug.WriteLine($"[ObjectBoxPass] Direct list provided, wrapped in provider");
            }
            else
            {
                Debug.WriteLine($"[ObjectBoxPass] Initialize failed: context1 is not List<BasicInstance> or provider");
                return;
            }

            PassShader = ShaderManager.Create(
                "Vertex",
                ShaderSource.simpleVertex_WithNormal1,
                ShaderSource.simpleFrag_WithNormal1,
                false,
                true);

            var basemesh = TessellationUtility.SixFacetsVolume();
            var vertices = basemesh.Vertices
                .Select((p, i) => new GLVertex(p, basemesh.Normals[i]))
                .ToArray();

            int maxInstances = 100; // 충분한 크기로 초기화
            _buffer = new DrawInstanceGeometry<GLVertex, uint, BasicInstance>(
                vertices,
                Indices,
                new BasicInstance[maxInstances]
            );

            if (context2 is Vector4 c)
            {
                _color = c;
            }

            Debug.WriteLine($"[ObjectBoxPass] Initialize completed, Camera: {Camera != null}, Shader: {PassShader != null}, Buffer: {_buffer != null}");
        }

        public override void Execute()
        {
            if (!Enabled || _instanceDataProvider == null || _buffer == null)
            {
                Debug.WriteLine($"[ObjectBoxPass] Execute skipped - Enabled: {Enabled}, Provider: {_instanceDataProvider != null}, Buffer: {_buffer != null}");
                return;
            }

            // 매 프레임마다 최신 데이터를 가져옴!
            var instanceData = _instanceDataProvider();
            
            Debug.WriteLine($"[ObjectBoxPass] Execute called - InstanceData count: {instanceData?.Count ?? 0}");

            if (instanceData == null || instanceData.Count == 0)
            {
                Debug.WriteLine($"[ObjectBoxPass] No instance data to render");
                return;
            }

            Debug.WriteLine($"[ObjectBoxPass] Syncing {instanceData.Count} instances to GPU");
            // ✅ UpdateInstanceRange 메서드 사용
            _buffer.UpdateInstanceRange(instanceData.ToArray().AsSpan(), 0);

            using (PassShader!.Use())
            {
                // Uniform 설정
                PassShader.SetUniformIfExist("uView", _viewMatrix);
                PassShader.SetUniformIfExist("uProjection", _projectionMatrix);
                PassShader.SetUniformIfExist("uObjectColor", _color);

                Debug.WriteLine($"[ObjectBoxPass] Uniforms set - executing draw with PrimType.Lines");
                
                // VAO 바인딩하고 Draw 호출
                _buffer.Bind();
                GL.DrawElementsInstanced(
                    PrimitiveType.Lines,
                    _buffer.IndexCount,
                    _buffer.ElementType,
                    IntPtr.Zero,
                    _buffer.InstanceCount);
                _buffer.Unbind();
                
                Debug.WriteLine($"[ObjectBoxPass] Draw completed");
            }
        }

        public override void SetCameraUniforms(Matrix4 view, Matrix4 projection)
        {
            _viewMatrix = view;
            _projectionMatrix = projection;
            Debug.WriteLine($"[ObjectBoxPass] SetCameraUniforms called");
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _buffer?.Dispose();
            base.Dispose();
        }
    }
}