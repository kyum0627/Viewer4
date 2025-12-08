using IGX.Loader;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Materials;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl
{
    #region Provider Interfaces
    
    /// <summary>
    /// 렌더링 데이터 제공자
    /// DrawBuffer 목록을 제공하여 렌더 패스가 필요한 데이터만 접근
    /// </summary>
    public interface IRenderDataProvider
    {
        /// <summary>
        /// 렌더링할 DrawBuffer 목록
        /// </summary>
        IReadOnlyList<IDrawBuffer> DrawBuffers { get; }
    }

    /// <summary>
    /// 렌더링 설정 제공자
    /// 쉐이딩 모드, 외곽선 등 렌더링 관련 설정 제공
    /// </summary>
    public interface IRenderSettings
    {
        /// <summary>
        /// 쉐이딩 모드 (Flat, Phong, Xray 등)
        /// </summary>
        ShadeMode ShadeMode { get; }

        /// <summary>
        /// 배경색 (RGBA)
        /// </summary>
        Vector4 BackgroundColor { get; }

        /// <summary>
        /// 외곽선 표시 여부
        /// </summary>
        bool DisplayEdges { get; }

        /// <summary>
        /// 외곽선 두께 (픽셀)
        /// </summary>
        float EdgeThickness { get; }

        /// <summary>
        /// 외곽선 색상 (RGB)
        /// </summary>
        Vector3 EdgeColor { get; }
    }

    /// <summary>
    /// 조명 정보 제공자
    /// </summary>
    public interface ILightingProvider
    {
        /// <summary>
        /// 조명 방향 (정규화된 벡터)
        /// </summary>
        Vector3 Direction { get; }

        /// <summary>
        /// 조명 색상 (RGBA)
        /// </summary>
        Vector4 Color { get; }

        /// <summary>
        /// 조명 객체 (Shader.Use 호환용)
        /// </summary>
        ILight Light { get; }
    }

    /// <summary>
    /// 렌더 패스 컨텍스트
    /// 렌더 패스가 필요한 최소 정보만 제공하여 의존성 최소화
    /// </summary>
    public interface IRenderPassContext
    {
        /// <summary>
        /// 렌더링 데이터 제공자
        /// </summary>
        IRenderDataProvider DataProvider { get; }

        /// <summary>
        /// 렌더링 설정 제공자
        /// </summary>
        IRenderSettings Settings { get; }

        /// <summary>
        /// 조명 정보 제공자
        /// </summary>
        ILightingProvider LightingProvider { get; }
    }
    
    #endregion

    /// <summary>
    /// IGX Viewer 핵심 API
    /// 3D 모델 관리, 렌더링, 선택 등 통합 기능 제공
    /// </summary>
    public class IgxViewAPI : IRenderDataProvider, IRenderSettings, ILightingProvider, IRenderPassContext
    {
        #region 데이터 관리
        
        /// <summary>
        /// 3D 모델 데이터 관리자 (정점, 인덱스 등)
        /// </summary>
        public Model3dDataManager ModelManager { get; }
        
        /// <summary>
        /// 렌더링 버퍼 관리자 (VBO, IBO 등)
        /// </summary>
        public Model3dBufferManager RenderManager { get; }
        
        /// <summary>
        /// 선택 관리자 (선택 객체 관리 및 하이라이트)
        /// </summary>
        public SelectionManager SelectionManager { get; }
        
        #endregion

        #region 렌더링 설정
        
        /// <summary>
        /// 쉐이더 파라미터 (쉐이딩 모드, 외곽선 등)
        /// </summary>
        public ShaderParameters Shading { get; }
        
        /// <summary>
        /// 보조 렌더링 설정 (좌표축, 법선, 박스 등)
        /// </summary>
        public AuxillaryDrawSetting Drawing { get; }
        
        /// <summary>
        /// 조명 객체 (방향, 색상 등)
        /// </summary>
        public ILight Lighting { get; }
        
        /// <summary>
        /// 씬 파라미터 (카메라, 배경색, 바운딩 박스 등)
        /// </summary>
        public SceneParameters SceneParameter { get; }
        
        #endregion

        #region 렌더링 파이프라인
        
        /// <summary>
        /// 메인 렌더링 파이프라인 (Forward/Deferred 렌더링)
        /// </summary>
        public RenderPipeline MainPipeline { get; private set; }
        
        /// <summary>
        /// 보조 렌더링 파이프라인 (좌표축, 법선, 박스 등)
        /// </summary>
        public RenderPipeline AuxiliaryPipeline { get; private set; }
        
        #endregion

        #region IRenderDataProvider 구현
        
        /// <summary>
        /// 렌더링할 DrawBuffer 목록
        /// </summary>
        public IReadOnlyList<IDrawBuffer> DrawBuffers => RenderManager.AllDrawBuffers;
        
        #endregion

        #region IRenderSettings 구현
        
        /// <summary>
        /// 현재 쉐이딩 모드
        /// </summary>
        public ShadeMode ShadeMode => Shading.Mode;

        /// <summary>
        /// 배경색
        /// </summary>
        public Vector4 BackgroundColor => SceneParameter.BackGroundColor;

        /// <summary>
        /// 외곽선 표시 여부
        /// </summary>
        public bool DisplayEdges => Shading.DisplayEdge;

        /// <summary>
        /// 외곽선 두께
        /// </summary>
        public float EdgeThickness => Shading.EdgeThickness;

        /// <summary>
        /// 외곽선 색상
        /// </summary>
        public Vector3 EdgeColor => Shading.EdgeColor;
        
        #endregion

        #region ILightingProvider 구현
        
        /// <summary>
        /// 조명 방향
        /// </summary>
        public Vector3 Direction => Lighting.Direction;

        /// <summary>
        /// 조명 색상
        /// </summary>
        public Vector4 Color => new Vector4(Lighting.Color, 1.0f);

        /// <summary>
        /// 조명 객체
        /// </summary>
        public ILight Light => Lighting;

        /// <summary>
        /// Shader에 조명 정보 설정
        /// </summary>
        public void SetLightingUniforms(Shader shader)
        {
            Lighting.SetLightingUniforms(shader);
        }
        
        #endregion

        #region IRenderPassContext 구현
        
        /// <summary>
        /// 렌더링 데이터 제공자 (자기 자신)
        /// </summary>
        public IRenderDataProvider DataProvider => this;

        /// <summary>
        /// 렌더링 설정 제공자 (자기 자신)
        /// </summary>
        public IRenderSettings Settings => this;

        /// <summary>
        /// 조명 정보 제공자 (자기 자신)
        /// </summary>
        public ILightingProvider LightingProvider => this;
        
        #endregion

        #region 하위 호환성
        
        /// <summary>
        /// 배경색 (하위 호환용, BackgroundColor 사용 권장)
        /// </summary>
        public Vector4 BackGroundColor => SceneParameter.BackGroundColor;
        
        #endregion

        #region 생성자
        
        /// <summary>
        /// IgxViewAPI 생성자 (모든 의존성 주입)
        /// </summary>
        public IgxViewAPI(
            Model3dDataManager modelManager,
            Model3dBufferManager renderManager,
            SelectionManager selectionManager,
            ShaderParameters shading,
            AuxillaryDrawSetting drawing,
            ILight lighting,
            SceneParameters sceneParameter)
        {
            ModelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager), "모델 관리자가 null입니다.");
            RenderManager = renderManager ?? throw new ArgumentNullException(nameof(renderManager), "렌더 관리자가 null입니다.");
            SelectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager), "선택 관리자가 null입니다.");
            Shading = shading ?? throw new ArgumentNullException(nameof(shading), "쉐이더 파라미터가 null입니다.");
            Drawing = drawing ?? throw new ArgumentNullException(nameof(drawing), "보조 렌더링 설정이 null입니다.");
            Lighting = lighting ?? throw new ArgumentNullException(nameof(lighting), "조명 객체가 null입니다.");
            SceneParameter = sceneParameter ?? throw new ArgumentNullException(nameof(sceneParameter), "씬 파라미터가 null입니다.");

            MainPipeline = new RenderPipeline();
            AuxiliaryPipeline = new RenderPipeline();
            
            Debug.WriteLine("[IgxViewAPI] 생성 완료");
        }
        
        #endregion

        #region 렌더링 파이프라인 관리
        
        /// <summary>
        /// 렌더링 파이프라인 초기화
        /// </summary>
        public void InitializeRenderPipeline(IMyCamera camera)
        {
            Debug.WriteLine("[IgxViewAPI] 파이프라인 초기화 시작");

            // Forward 패스 추가
            var forwardPass = new ForwardPass(this);
            MainPipeline.AddPass(forwardPass);
            Debug.WriteLine("[IgxViewAPI] ForwardPass 추가");

            // 보조 패스들 추가 (좌표축, 법선, 박스 등)
            var auxiliaryPasses = StandardRenderPassFactory.CreateStandardAuxiliaryPasses(
                camera,
                () => SelectionManager.InstancedBoxes,
                () => SelectionManager.InstancedVectors,
                SceneParameter,
                SceneParameter.BoxColor,
                SceneParameter.VectorColor);

            foreach (var pass in auxiliaryPasses)
            {
                AuxiliaryPipeline.AddPass(pass);
                Debug.WriteLine($"[IgxViewAPI] 보조 패스 추가: {pass.Name}");
            }

            // 파이프라인 초기화
            MainPipeline.Initialize(camera);
            AuxiliaryPipeline.Initialize(camera);
            
            Debug.WriteLine("[IgxViewAPI] 파이프라인 초기화 완료");
        }

        /// <summary>
        /// 렌더링 실행
        /// </summary>
        /// <param name="camera">카메라</param>
        /// <param name="drawAuxiliary">보조 요소 렌더링 여부</param>
        public void Render(IMyCamera camera, bool drawAuxiliary = true)
        {
            MainPipeline.Execute(camera);
            
            if (drawAuxiliary)
            {
                AuxiliaryPipeline.Execute(camera);
            }
        }

        /// <summary>
        /// 화면 크기 변경 처리
        /// </summary>
        public void Resize(int width, int height)
        {
            MainPipeline.Resize(width, height);
            AuxiliaryPipeline.Resize(width, height);
        }
        
        #endregion

        #region 모델 관리
        
        /// <summary>
        /// 모델 로딩 완료 이벤트
        /// </summary>
        public event EventHandler? ModelsLoaded;

        /// <summary>
        /// 새 모델 추가 (기존 모델 유지)
        /// </summary>
        public void NewModels(Dictionary<string, Model3D> source)
        {
            Debug.WriteLine($"[IgxViewAPI] 새 모델 추가: {source.Count}개");
            
            ModelManager.NewModel(source);
            RenderManager.MakeModel3dBuffer(ModelManager.Buffers);
            SceneParameter.TotalBoundingBox = ModelManager.TotalAabb;
            CameraHelper.InitializeCamera(SceneParameter.Camera, SceneParameter.TotalBoundingBox);
            
            Debug.WriteLine($"[IgxViewAPI] 모델 추가 완료: 총 {ModelManager.Models.Count}개");
            ModelsLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 모델 데이터 언로드 (선택 정보 포함)
        /// </summary>
        public void UnloadModelData()
        {
            ModelManager.Clear();
            SelectionManager.PickedItems.Clear();
            SelectionManager.InstancedVectors.Clear();
            SelectionManager.InstancedBoxes.Clear();
        }

        /// <summary>
        /// 씬 전체 클리어
        /// </summary>
        public void ClearScene()
        {
            SelectionManager.Clear();
            RenderManager.ClearRenderResources();
            ModelManager.Clear();
        }

        /// <summary>
        /// 모델 로드 (기존 씬 교체)
        /// </summary>
        public void LoadModels(Dictionary<string, Model3D> source)
        {
            Debug.WriteLine($"[IgxViewAPI] 모델 로드: {source.Count}개");
            
            ClearScene();
            ModelManager.NewModel(source);
            RenderManager.MakeModel3dBuffer(ModelManager.Buffers);
            SceneParameter.TotalBoundingBox = ModelManager.TotalAabb;
            CameraHelper.InitializeCamera(SceneParameter.Camera, SceneParameter.TotalBoundingBox);
            
            var cbox = new ClippingBox();
            cbox.ResetBox(SceneParameter.TotalBoundingBox);
            
            Debug.WriteLine($"[IgxViewAPI] 모델 로드 완료: 총 {ModelManager.Models.Count}개");
            ModelsLoaded?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion

        #region 리소스 해제
        
        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            MainPipeline?.Dispose();
            AuxiliaryPipeline?.Dispose();
        }
        
        #endregion

        #region 팩토리
        
        /// <summary>
        /// 기본 IgxViewAPI 인스턴스 생성
        /// </summary>
        public static IgxViewAPI CreateDefault()
        {
            var modelManager = new Model3dDataManager();
            var renderManager = new Model3dBufferManager(modelManager);

            return new IgxViewAPI(
                modelManager,
                renderManager,
                new SelectionManager(modelManager, renderManager),
                new ShaderParameters(),
                new AuxillaryDrawSetting(),
                new LightingProvider(),
                new SceneParameters()
            );
        }
        
        #endregion
    }
}