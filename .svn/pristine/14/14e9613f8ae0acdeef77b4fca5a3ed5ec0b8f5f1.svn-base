using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using IGX.ViewControl.Render;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    /// <summary>
    /// 전체 렌더링 프레임을 관리하는 최상위 객체
    /// Execute(IMyCamera camera, bool drawAux) 메서드를 통해 한 프레임의 렌더링을 시작하고
    /// IRenderPass들을 순서대로 호출하여 전체 장면을 구성
    /// IRenderer -> IRenderPass: IRenderer는 하나 이상의 IRenderPass를 소유하고 순차적으로 실행
    /// </summary>
    public interface IRenderer : IDisposable
    {
        bool Enabled { get; set; }
        void Draw(IMyCamera camera, bool drawAux);
        void Initialize(object? context = null, object? color = null);
        void Resize(int width, int height);
    }

    /// <summary>
    /// 특정 목적을 가진 렌더링 단계(Pass)를 정의하고 관리
    /// 경량화된 인터페이스: 핵심 책임만 정의하고 나머지는 파이프라인이 조율
    /// </summary>
    public interface IRenderPass
    {
        string Name { get; }
        int Order { get; set; }
        bool Enabled { get; set; }

        // Pipeline → Pass 관계
        void OnAddedToPipeline(IRenderPipeline pipeline);
        void OnRemovedFromPipeline(IRenderPipeline pipeline);

        // Camera 전달
        void SetCamera(IMyCamera camera);

        // Shader 관리
        Shader? PassShader { get; set; }

        // 카메라 유니폼 설정 (Pipeline이 호출)
        void SetCameraUniforms(Matrix4 view, Matrix4 projection);

        // 실제 렌더링 실행
        void Execute();

        // 리사이즈 처리
        void Resize(int width, int height);

        // 리소스 해제
        void Dispose();

        // 초기화
        void Initialize(object? context1 = null, object? context2 = null);

        // Pass 옵션
        bool ClearColor { get; set; }
        bool ClearDepth { get; set; }
        Vector4 ClearColorValue { get; set; }
    }


    /// <summary>
    /// 단일 지오메트리 묶음(예: 단일 모델)과 쉐이더에 결합되어 실제로 GPU 드로우 콜을 실행하는 가장 작은 단위
    /// 단순화된 인터페이스: 렌더링 실행 명령만 담당 (리소스 관리는 별도)
    /// IRenderPass -> IDrawBuffer: IRenderPass는 하나 이상의 IDrawBuffer를 수집하고 Execute 시 모두에게 드로우 명령을 내림.
    /// </summary>
    public interface IDrawBuffer : IDrawCommand
    {
        // IDrawCommand만 상속 - Execute, ExecuteRange, ExecuteWithShader, PrimType
        // 리소스 관리(VAO, VBO, IBO)는 Geometry 클래스가 담당
        // Material 관리는 별도의 IMaterial 인터페이스로 분리
    }

    public interface IDrawResource : IDisposable
    {
        int VAO { get; }
        int VBO { get; }
        int IBO { get; }
        int VertexCount { get; }
        int IndexCount { get; }

        void Create();
        void Upload();
        void Dispose();
        void Bind();
        void Unbind();
    }

    public interface IDrawCommand
    {
        PrimitiveType PrimType { get; set; }
        void Execute();
        void ExecuteRange(int startIndex, int drawCount);
        void ExecuteWithShader(Shader? Shader);
    }

    public interface IDrawMaterial
    {
        IMaterial? Material { get; set; }
        void Draw();
        void ApplyMaterial(IMyCamera? camera = null, ILight? light = null);
    }


    /// <summary>
    /// IDrawBuffer들이 사용할 데이터(인스턴스 변환 행렬 등)를 관리하고
    /// 최적화된 간접 드로우(ExecuteIndirectDraws)를 실행하는 역할 수행
    /// 즉, 데이터 관리 및 최적화된 실행 경로를 담당하는 별도의 핵심 구성 요소
    /// 이 구조를 통해서,
    /// - IRenderer는 전체 흐름
    /// - IRenderPass는 단계별 실행 목록
    /// - IDrawBuffer는 단일 그리기를 책임짐
    /// </summary>
    public interface IDrawCommandManager
    {
        CommandAllocationInfo FindOrRegister(PrimitiveBase primitive, IAssembly assembly);
        void UpdateInstanceTransform(int baseInstanceIndex, Matrix4 transform);
        void UpdateInstanceStatus(
            int baseInstanceIndex,
            bool isVisible,
            bool isSelected,
            bool isHighlighted,
            float opacity = 1.0f);
        void UnregisterGeometry(int drawCommandIndex);
        void FlushInstanceTransforms();
        IDrawBuffer? GetRendererForBaseInstance(int baseInstanceIndex);
        void PrepareVisibleDrawCommands(IReadOnlyList<int> visibleDrawCommandIndices);
    }

    public interface IMatrixCalculator
    {
        Matrix4 CalculateMVP(GLControl control, IMyCamera camera, IRenderConfig renderConfig);
        void InvalidateCache();
    }
    public interface IViewportManager
    {
        void SetupViewport(GLControl control);
        Size GetViewportSize(GLControl control);
    }
    public interface IRenderConfig
    {
        Color4 BackgroundColor { get; }
        Matrix4 GetModelMatrix(float deltaTime);
        ISet<int> SelectedIndices { get; }
        Color4 HighlightColor { get; }
    }
    public interface IContextManager
    {
        GLControl CreateControl(string name);
        void MakeCurrent(GLControl control);
        void SetSharedContext(GLControl control, GLControl sharedContext);
    }
    public interface IGeometryBuffer : IDisposable
    {
        int VAO { get; }
        // GPU Buffer Handles
        int VBO { get; }
        int IBO { get; }
        int VertexCount { get; }
        int IndexCount { get; }
        int CommandCount { get; }
        DrawElementsType ElementType { get; }
        IDrawBuffer Renderer { get; set; }
        void Bind();
        void Unbind();
        void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles);
        void Upload(float[] vertices, int[] indices);// Resize or recreate GPU resources
        int GpuMemoryBytes { get; }// Optional: GPU 공간 사용량
    }

    public interface IModelGeometryBuffer<NST> : IGeometryBuffer
    {
        IDrawBuffer Renderer { get; }
        void UpdateInstanceTransform(int baseInstanceIndex, Matrix4 transform);
        void UpdateSingleInstance(int baseInstanceIndex, NST instanceData);
        object GetLocalInstanceData(int baseInstanceIndex);
        void FlushInstanceData();
    }

    public interface IInstancedGeometryBuffer<NST>
    {
        void FlushInstanceData();
        void UpdateInstanceTransform(int index, Matrix4 transform);
        object GetLocalInstanceData(int baseInstanceIndex);
        void UpdateSingleInstance(int baseInstanceIndex, NST instanceData);
    }
    public interface IGeometry : IDisposable
    {
        IGeometryBuffer Buffer { get; }// GPU Buffer 
        Matrix4 LocalTransform { get; set; }// Transform + Node 관계 없이도 독립적으로 렌더 가능하도록 
        AABB3 LocalBounds { get; }// Bounds
        AABB3 WorldBounds { get; }// Bounds
        void UpdateWorldBounds(Matrix4 parentTransform);
        PrimitiveType Primitive { get; set; }// Rendering info
        bool Visible { get; set; }
        IMaterial Material { get; set; } // Material
        void Upload(float[] vertices, int[] indices); // Upload new data
        string Name { get; set; } // Optional
    }
    public interface IMaterial : IDisposable
    {
        string Name { get; set; }

        Shader Shader { get; }

        // Per-object uniforms
        Matrix4 ModelMatrix { get; set; }
        Vector4 BaseColor { get; set; }
        float Metallic { get; set; }
        float Roughness { get; set; }
        float Opacity { get; set; }

        bool Transparent { get; }
        bool DoubleSided { get; set; }

        // Render state
        void SetRenderState();
        void ResetRenderState();

        // Bind shader + upload uniforms
        void Apply(IMyCamera camera, ILight lighting);
    }
    public class DefaultMaterial : IMaterial
    {
        public string Name { get; set; } = "DefaultMaterial";

        public Shader Shader { get; private set; }

        // Uniforms
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public Vector4 BaseColor { get; set; } = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
        public float Metallic { get; set; } = 0.0f;
        public float Roughness { get; set; } = 0.8f;
        public float Opacity { get; set; } = 1.0f;

        public bool DoubleSided { get; set; } = false;
        public bool Transparent => Opacity < 1.0f;

        public DefaultMaterial()
        {
            Shader = ShaderManager.Create(
                "DefaultMaterial",
                ShaderSource.forwardVtx,
                ShaderSource.forwardFrg,
                false,
                useCache: true
            );
        }

        public void SetRenderState()
        {
            if (Transparent)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            if (DoubleSided)
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
            }
        }

        public void ResetRenderState()
        {
            // Reset alpha blending
            GL.Disable(EnableCap.Blend);

            // Restore cullface
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        public void Apply(IMyCamera camera, ILight lighting)
        {
            Shader.Use();

            // Camera uniforms
            Shader.SetUniformIfExist("uView", camera.ViewMatrix);
            Shader.SetUniformIfExist("uProjection", camera.ProjectionMatrix);
            Shader.SetUniformIfExist("uCameraPosition", camera.Position);

            // Object uniforms
            Shader.SetUniformIfExist("uModel", ModelMatrix);
            Shader.SetUniformIfExist("uBaseColor", BaseColor);
            Shader.SetUniformIfExist("uMetallic", Metallic);
            Shader.SetUniformIfExist("uRoughness", Roughness);
            Shader.SetUniformIfExist("uOpacity", Opacity);

            // Lighting
            lighting?.SetLightingUniforms(Shader);

            SetRenderState();
        }

        public void Dispose()
        {
            Shader?.Dispose();
        }
    }
}