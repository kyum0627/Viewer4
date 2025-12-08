//using IGX.ViewControl.Scene;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;

//namespace IGX.ViewControl.Render
//{
//    public class SceneRenderer : IRenderer
//    {
//        private readonly IDrawCommandManager _geometryManager;
//        private readonly IMatrixCalculator _matrixCalculator;
//        private readonly SceneGraph _sceneGraph;

//        private readonly RendererPass _defaultPass;
//        private readonly RendererPass _highlightPass;
//        private readonly RendererPass _selectionPass;

//        public bool Enabled { get; set; } = true;
//        public SceneRenderer(IDrawCommandManager geometryManager)
//        {
//            _geometryManager = geometryManager;

//            _defaultPass = new RendererPass();
//            _highlightPass = new RendererPass();
//            _selectionPass = new RendererPass();
//        }

//        public SceneRenderer(
//            IDrawCommandManager geometryManager,
//            IMatrixCalculator matrixCalculator,
//            SceneGraph sceneGraph)
//        {
//            _geometryManager = geometryManager ?? throw new ArgumentNullException(nameof(geometryManager));
//            _matrixCalculator = matrixCalculator ?? throw new ArgumentNullException(nameof(matrixCalculator));
//            _sceneGraph = sceneGraph ?? throw new ArgumentNullException(nameof(sceneGraph));

//            _defaultPass = new RendererPass();
//            _highlightPass = new RendererPass();
//            _selectionPass = new RendererPass();

//            // 예시: 패스별 쉐이더 설정 (실제로는 외부에서 주입되어야 함)
//            (_defaultPass as RendererPass).PassShader = new Shader("Default", "default.vert", "default.frag", false);
//            (_highlightPass as RendererPass).PassShader = new Shader("Default", "default.vert", "default.frag", false);
//            (_selectionPass as RendererPass).PassShader = new Shader("Default", "default.vert", "default.frag", false);
//        }
//        public void Initialize(object? context = null, object? color = null)
//        {
//            // 초기 GL 상태 설정 (여기서는 간단한 예시)
//            GL.Enable(EnableCap.DepthTest);
//            GL.DepthFunc(DepthFunction.Less);
//            // GL.Enable(EnableCap.CullFace);
//        }
//        public void Resize(int w, int h)
//        {
//            GL.Viewport(0, 0, w, h);
//            _matrixCalculator?.InvalidateCache();
//        }
//        public void Draw(IMyCamera camera, bool drawAux)
//        {
//            if (!Enabled) return;

//            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
//            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
//            GL.Enable(EnableCap.DepthTest);
//            GL.DepthFunc(DepthFunction.Less);

//            Matrix4 viewMatrix = camera.ViewMatrix;
//            Matrix4 projectionMatrix = camera.ProjectionMatrix;

//            UpdateInstanceData(viewMatrix, projectionMatrix);
//            (_defaultPass as RendererPass)?.PassShader?.Use();
//            _geometryManager.ExecuteIndirectDraws();

//            _highlightPass.PrimType = PrimitiveType.Lines;
//            _highlightPass.Execute();
//            _selectionPass.PrimType = PrimitiveType.TriangleIndices;
//            _selectionPass.Execute();
//            // GL.Disable(EnableCap.Blend); // 상태 복구
//        }
//        private void UpdateInstanceData(Matrix4 view, Matrix4 projection)
//        {
//            // 씬 그래프 순회
//            foreach (var rootNode in _sceneGraph.RootNodes)
//            {
//                ProcessNodeForDrawing(rootNode);
//            }

//            // 모든 CPU측 인스턴스 데이터를 GPU 버퍼로 전송
//            _geometryManager.FlushInstanceTransforms();

//            // 각 Pass의 쉐이더에 카메라 유니폼을 설정
//            (_defaultPass as RendererPass)?.SetCameraUniforms(view, projection);
//            (_highlightPass as RendererPass)?.SetCameraUniforms(view, projection);
//            (_selectionPass as RendererPass)?.SetCameraUniforms(view, projection);
//        }

//        private void ProcessNodeForDrawing(SceneNode node)
//        {
//            if (node.SceneGeometries.CommandCount > 0)
//            {
//                Matrix4 globalTransform = node.GlobalTransformCache;

//                foreach (var geoRef in node.SceneGeometries)
//                {
//                    // IDrawCommandManager에 변환 행렬 업데이트
//                    _geometryManager.UpdateInstanceTransform(
//                        baseInstanceIndex: geoRef.BaseInstance,
//                        transform: globalTransform
//                    );

//                    // IDrawCommandManager에 상태 업데이트
//                    _geometryManager.UpdateInstanceStatus(
//                        baseInstanceIndex: geoRef.BaseInstance,
//                        isVisible: node.Visible,
//                        isSelected: node.Selected,
//                        isHighlighted: node.Highlighted
//                    );
//                }
//            }

//            // 자식 노드 순회
//            foreach (var child in node.Children)
//            {
//                ProcessNodeForDrawing(child);
//            }
//        }
//            // 자식 노드 순회
//        public void Dispose()
//        {
//            // 자원 해제 로직
//        }
//    }
//}