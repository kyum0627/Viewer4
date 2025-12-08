using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render.Auxilliary;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    /// <summary>
    /// RenderPass 생성을 중앙화하는 Factory 클래스
    /// </summary>
    public static class StandardRenderPassFactory
    {
        public static IRenderPass CreateBackgroundPass(IMyCamera camera)
        {
            var pass = new BackgroundPass();
            pass.Order = 0; // 가장 먼저 렌더링
            pass.Initialize(camera);
            return pass;
        }

        public static IRenderPass CreateCoordinatePass(IMyCamera camera)
        {
            var pass = new CoordinatePass();
            pass.Order = 200; // Depth test를 고려해 늦게 렌더링
            pass.Initialize(camera);
            return pass;
        }

        public static IRenderPass CreateObjectBoxPass(IMyCamera camera, Func<List<BasicInstance>> boxesProvider, Vector4 color)
        {
            var pass = new ObjectBoxPass();
            pass.Order = 150; // 메인 오브젝트 이후
            pass.SetCamera(camera); // Camera 먼저 설정!
            pass.Initialize(boxesProvider, color); // Provider 전달
            return pass;
        }

        public static IRenderPass CreateNormalVectorsPass(IMyCamera camera, Func<List<GLVertex>> vectorsProvider, Vector4 color)
        {
            var pass = new NormalVectorsPass();
            pass.Order = 160; // ObjectBox 이후
            pass.SetCamera(camera); // Camera 먼저 설정!
            pass.Initialize(vectorsProvider, color); // Provider 전달
            return pass;
        }

        public static IRenderPass CreateClipPlanePass(SceneParameters sceneParams, Vector4 color)
        {
            var pass = new ClipPlanePass();
            pass.Order = 170; // NormalVectors 이후
            pass.Initialize(sceneParams, color);
            return pass;
        }

        public static IRenderPass CreateForwardPass(IMyCamera camera, IRenderPassContext context)
        {
            var pass = new ForwardPass(context);  // ? IRenderPassContext 주입
            pass.Order = 100; // Background 이후, 보조 요소들 이전
            pass.Initialize(camera);
            return pass;
        }

        /// <summary>
        /// 모든 표준 보조 Pass를 생성하여 반환
        /// </summary>
        public static IEnumerable<IRenderPass> CreateStandardAuxiliaryPasses(
            IMyCamera camera,
            Func<List<BasicInstance>> boxesProvider,
            Func<List<GLVertex>> vectorsProvider,
            SceneParameters sceneParams,
            Vector4 boxColor,
            Vector4 vectorColor)
        {
            yield return CreateBackgroundPass(camera);      // Order: 0
            yield return CreateObjectBoxPass(camera, boxesProvider, boxColor);  // Order: 150
            yield return CreateNormalVectorsPass(camera, vectorsProvider, vectorColor); // Order: 160
            yield return CreateClipPlanePass(sceneParams, boxColor); // Order: 170
            yield return CreateCoordinatePass(camera);      // Order: 200 (가장 마지막, depth test 무시)
        }
    }
}
