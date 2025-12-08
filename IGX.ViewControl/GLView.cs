using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Auxilliary;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace IGX.ViewControl
{
    public partial class GLView : UserControl
    {
        public readonly GLControl _glControl;
        private readonly IgxViewAPI _apis;
        private readonly IMyCamera _camera;

        private bool _isDisposed;
        private bool _isDragging;
        private bool _mouseDragged;
        private bool _drawClipplane;
        private readonly object _strategyLock = new();

        private MouseButtons _mouseButton = MouseButtons.None;
        private Point _prevMousePos = Point.Empty;
        public float ZoomInRatio { get; set; } = 1.25f;
        public float ZoomOutRatio { get; set; } = 0.8f;

        public RenderPipeline Renderer = new RenderPipeline();

        // Main rendering passes
        private ForwardPass? _forwardPass;
        private FBOPass? _gBufferPass;
        private PostFBOPass? _postFBOPass;

        // Auxiliary rendering passes
        private BackgroundPass? _backgroundPass;
        private CoordinatePass? _coordinatePass;
        private ObjectBoxPass? _objectboxPass;
        private NormalVectorsPass? _normalvectorPass;
        private ClipPlanePass? _clipPlanePass;

        private bool _isClipBoxMode = false;

        public GLView(IgxViewAPI api, MyCamera? camera = null, GLControl? shared = null)
        {
            InitializeComponent();

            _apis = api ?? throw new ArgumentNullException(nameof(api));

            GLControlSettings glc = new GLControlSettings
            {
                APIVersion = new Version(4, 6, 0, 0),
                AutoLoadBindings = true,
                Flags = ContextFlags.Default,
                Profile = ContextProfile.Core,
                API = ContextAPI.OpenGL,
                IsEventDriven = true,
                SharedContext = null
            };

            if (shared != null)
            {
                glc.SharedContext = shared.Context;
            }
            _glControl = new GLControl(glc);

            _camera = camera ?? (MyCamera)api.SceneParameter.Camera;
            _drawClipplane = _apis.Drawing.ClipPlanes;

            _apis.Shading.ModeChanged += OnShadeModeChanged;
            _apis.Drawing.SettingChanged += OnDrawingSettingChanged;
            SetStrategyBasedOnShadeMode();

            SetupGLControl();
            
            // GLView 자체도 키 이벤트를 처리할 수 있도록 설정
            this.KeyDown += GLView_KeyDown;
        }

        private void SetupGLControl()
        {
            _glControl.Dock = DockStyle.Fill;
            
            // GLControl이 키보드 입력을 받을 수 있도록 설정
            _glControl.TabStop = true;
            
            this.Controls.Add(_glControl);

            _glControl.MouseDown += OnMouseDown;
            _glControl.MouseMove += OnMouseMove;
            _glControl.MouseUp += OnMouseUpForPicking;
            _glControl.MouseWheel += OnMouseWheel;
            _glControl.MouseLeave += OnMouseLeave;
            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += GlControl_Resize;
            
            // 키보드 이벤트 연결
            _glControl.KeyDown += GlControl_KeyDown;
            _glControl.PreviewKeyDown += GlControl_PreviewKeyDown;
            
            // GLControl이 생성되면 즉시 포커스 설정
            _glControl.Enter += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine("[GLView] GLControl received focus");
            };
        }

        private List<BasicInstance> _boxesToDraw => _apis.SelectionManager.InstancedBoxes;
        private List<GLVertex> _vectorsToDraw => _apis.SelectionManager.InstancedVectors;
        private Vector4 _boxColor => _apis.SceneParameter.BoxColor;
        private Vector4 _vectorColor => _apis.SceneParameter.VectorColor;

        private void GlControl_Load(object? sender, EventArgs e)
        {
            if (_isDisposed) return;
            _glControl.MakeCurrent();
            _camera.SetViewportSize(_glControl.Width, _glControl.Height);

            InitializeRenderPasses();
            ConfigureRenderPipeline();
            UpdatePassEnabledStates();
            
            // GLControl에 포커스 설정 (키보드 입력 활성화)
            _glControl.Focus();
            System.Diagnostics.Debug.WriteLine("[GLView] GLControl focused after load");
        }

        private void InitializeRenderPasses()
        {
            // Main rendering passes with explicit dependencies
            _forwardPass = new ForwardPass(_apis);  // ? IRenderPassContext 주입 (_apis가 구현함)
            _forwardPass.Initialize(_camera);

            _gBufferPass = new FBOPass();
            _gBufferPass.Initialize(
                dataProvider: _apis,
                settings: _apis);

            _postFBOPass = new PostFBOPass();
            _postFBOPass.Initialize(
                sourceGBuffer: _gBufferPass.Gbuffer!,
                lighting: _apis,
                settings: _apis);

            // Auxiliary passes
            _backgroundPass = new BackgroundPass();
            _backgroundPass.Initialize(_camera);

            _coordinatePass = new CoordinatePass();
            _coordinatePass.Initialize(_camera);

            _objectboxPass = new ObjectBoxPass();
            _objectboxPass.Initialize(_boxesToDraw, _boxColor);

            _normalvectorPass = new NormalVectorsPass();
            _normalvectorPass.Initialize(_vectorsToDraw, _vectorColor);

            _clipPlanePass = new ClipPlanePass();
            _clipPlanePass.Initialize(_apis.SceneParameter, _boxColor);
        }

        private void ConfigureRenderPipeline()
        {
            // Clear existing passes
            Renderer.ClearPasses();

            // Add passes based on rendering mode
            bool useDeferredRendering = _apis.Shading.Mode == ShadeMode.Xray;

            if (useDeferredRendering)
            {
                // Deferred rendering: GBuffer -> PostFBO
                _gBufferPass!.Order = 10;
                _postFBOPass!.Order = 20;

                Renderer.AddPass(_gBufferPass);
                Renderer.AddPass(_postFBOPass);
            }
            else
            {
                // Forward rendering
                _forwardPass!.Order = 10;
                Renderer.AddPass(_forwardPass);
            }

            // Add auxiliary passes (always rendered)
            _backgroundPass!.Order = 100;
            _coordinatePass!.Order = 200;
            _objectboxPass!.Order = 300;
            _normalvectorPass!.Order = 400;
            _clipPlanePass!.Order = 500;

            Renderer.AddPass(_backgroundPass);
            Renderer.AddPass(_coordinatePass);
            Renderer.AddPass(_objectboxPass);
            Renderer.AddPass(_normalvectorPass);
            Renderer.AddPass(_clipPlanePass);
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            if (_isDisposed || !_glControl.IsHandleCreated) return;
            _glControl.MakeCurrent();

            Renderer.Execute(_camera);

            _glControl.SwapBuffers();
        }

        private void GlControl_Resize(object? sender, EventArgs e)
        {
            if (_isDisposed || !_glControl.IsHandleCreated || (_glControl.FindForm()?.WindowState == FormWindowState.Minimized)) return;
            _glControl.MakeCurrent();

            int w = Math.Max(1, _glControl.Width);
            int h = Math.Max(1, _glControl.Height);

            GL.Viewport(0, 0, w, h);
            _camera.SetViewportSize(w, h);

            Renderer.Resize(w, h);
            _glControl.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_glControl != null)
                {
                    _glControl.MouseDown -= OnMouseDown;
                    _glControl.MouseMove -= OnMouseMove;
                    _glControl.MouseUp -= OnMouseUpForPicking;
                    _glControl.MouseWheel -= OnMouseWheel;
                    _glControl.MouseLeave -= OnMouseLeave;
                    _glControl.Load -= GlControl_Load;
                    _glControl.Paint -= GlControl_Paint;
                    _glControl.Resize -= GlControl_Resize;
                    _glControl.KeyDown -= GlControl_KeyDown;
                    _glControl.PreviewKeyDown -= GlControl_PreviewKeyDown;
                }
                
                this.KeyDown -= GLView_KeyDown;

                if (_apis != null)
                {
                    _apis.Shading.ModeChanged -= OnShadeModeChanged;
                    _apis.Drawing.SettingChanged -= OnDrawingSettingChanged;
                }

                // Dispose all passes
                _forwardPass?.Dispose();
                _gBufferPass?.Dispose();
                _postFBOPass?.Dispose();
                _backgroundPass?.Dispose();
                _coordinatePass?.Dispose();
                _objectboxPass?.Dispose();
                _normalvectorPass?.Dispose();
                _clipPlanePass?.Dispose();

                Renderer?.Dispose();

                if (_glControl != null && _glControl.IsHandleCreated && !_glControl.IsDisposed)
                {
                    _glControl.MakeCurrent();
                    Shader.ProcessPendingDeletions();
                    FrameBufferObject.ProcessPendingDeletions();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        public void FitModel_Click(object? sender, EventArgs e)
        {
            _camera.Fit(_apis.SceneParameter.TotalBoundingBox);
            _glControl.Invalidate();
        }

        private void SetViewDirection(object? sender, EventArgs e)
        {
            if (sender is Control ctrl && ctrl.Tag is ViewDirection d1)
                _camera.SetViewDirection(d1);
            else if (sender is ToolStripItem item && Enum.TryParse<ViewDirection>(item.Tag?.ToString(), out var dir))
                _camera.SetViewDirection(dir);

            _glControl.Invalidate();
        }

        public void ToggleRenderMode(object? sender, EventArgs e)
        {
            _apis.Shading.Mode = _apis.Shading.Mode switch
            {
                ShadeMode.Flat => ShadeMode.Xray,
                ShadeMode.Xray => ShadeMode.OUTLINE,
                ShadeMode.OUTLINE => ShadeMode.Phong,
                ShadeMode.Phong => ShadeMode.Flat,
                _ => _apis.Shading.Mode
            };
            _glControl.Invalidate();
        }

        public void ToggleSelect(object? sender, EventArgs e)
        {
            _apis.SelectionManager.Selection.Enable = !_apis.SelectionManager.Selection.Enable;
            _glControl.Invalidate();
        }

        public void ToggleClipBoxMode(object? sender, EventArgs e)
        {
            _isClipBoxMode = !_isClipBoxMode;
            System.Diagnostics.Debug.WriteLine($"GLView: ToggleClipBoxMode - Mode={_isClipBoxMode}, Pass={_clipPlanePass != null}");

            _clipPlanePass?.SetClipBoxMode(_isClipBoxMode);

            var clipSystem = _clipPlanePass?.GetClipPlaneSystem();
            System.Diagnostics.Debug.WriteLine($"GLView: ClipSystem={clipSystem != null}, Enabled={clipSystem?.Enabled}");
            
            // 사용자에게 조작 방법 안내
            if (_isClipBoxMode)
            {
                System.Diagnostics.Debug.WriteLine("[GLView] ClipBox Mode Active:");
                System.Diagnostics.Debug.WriteLine("  - Left Mouse: Rotate ClipBox");
                System.Diagnostics.Debug.WriteLine("  - Right Mouse: Resize ClipBox");
                System.Diagnostics.Debug.WriteLine("  - Mouse Wheel: Zoom (Camera)");
                System.Diagnostics.Debug.WriteLine("  - Ctrl + Left Mouse: Rotate Camera");
                System.Diagnostics.Debug.WriteLine("  - Ctrl + Right Mouse: Pan Camera");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Normal Camera Mode Active");
            }

            _glControl.Invalidate();
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            // 마우스 클릭 시 GLControl에 포커스 설정 (키보드 입력 활성화)
            if (!_glControl.Focused)
            {
                _glControl.Focus();
                System.Diagnostics.Debug.WriteLine("[GLView] GLControl focused on mouse down");
            }
            
            _mouseButton = e.Button;
            _prevMousePos = e.Location;
            _isDragging = true;
            _mouseDragged = false;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            // ClipBox 모드에서도 Ctrl 키를 누르면 카메라 조작 가능
            bool allowCameraControl = !_isClipBoxMode || (ModifierKeys.HasFlag(Keys.Control));

            if (_isClipBoxMode && !allowCameraControl)
            {
                // ClipBox 전용 모드: ClipBox만 조작
                var clipSystem = _clipPlanePass?.GetClipPlaneSystem();
                if (clipSystem != null)
                {
                    clipSystem.Update(e.Location, _mouseButton == MouseButtons.Left);
                    _glControl.Invalidate();
                    return;
                }
            }

            // 카메라 조작 모드 (일반 모드 또는 ClipBox 모드 + Ctrl)
            int dx = Math.Abs(e.X - _prevMousePos.X);
            int dy = Math.Abs(e.Y - _prevMousePos.Y);
            if (dx > 5 || dy > 5) _mouseDragged = true;

            int cx = e.X, cy = e.Y;
            if (_mouseButton == MouseButtons.Left)
                _camera.Rotate(_prevMousePos.X, _prevMousePos.Y, cx, cy);
            else if (_mouseButton == MouseButtons.Right)
                _camera.Pan(_prevMousePos.X, _prevMousePos.Y, cx, cy);

            _prevMousePos = new Point(cx, cy);
            _glControl.Invalidate();
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            _mouseButton = MouseButtons.None;
            _isDragging = false;
            _glControl.Invalidate();
        }

        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta == 0) return;
            
            // ClipBox 모드에서도 마우스 휠은 항상 줌 기능 수행
            float ratio = e.Delta > 0 ? ZoomInRatio : ZoomOutRatio;
            _camera.Zoom(e.X, e.Y, ratio);
            _glControl.Invalidate();
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            _mouseButton = MouseButtons.None;
            _isDragging = false;
            _prevMousePos = Point.Empty;
        }

        private void OnMouseUpForPicking(object? sender, MouseEventArgs e)
        {
            OnMouseUp(sender, e);
            if (!_mouseDragged && !_isClipBoxMode)
            {
                _glControl.MakeCurrent();
                PickHelper.PickUnPick(Keys.None, e, _glControl, _apis, false);
            }
            _mouseDragged = false;
            _glControl.Invalidate();
        }

        private void GlControl_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            // 특정 키 조합이 기본 동작으로 처리되지 않도록 설정
            if (e.Shift && (e.KeyCode == Keys.C || e.KeyCode == Keys.N || e.KeyCode == Keys.B))
            {
                e.IsInputKey = true;
            }
        }

        private void GlControl_KeyDown(object? sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[GLView] GlControl_KeyDown: {e.KeyData}");
            
            // Shift+C: ClipBox 모드 토글
            if (e.KeyData == (Keys.Shift | Keys.C))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+C pressed in KeyDown");
                ToggleClipBoxMode(null, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            
            // Shift+N: Normals 표시 토글
            if (e.KeyData == (Keys.Shift | Keys.N))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+N pressed in KeyDown");
                _apis.Drawing.Normals = !_apis.Drawing.Normals;
                _glControl.Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            
            // Shift+B: ObjectBox 표시 토글
            if (e.KeyData == (Keys.Shift | Keys.B))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+B pressed in KeyDown");
                _apis.Drawing.ObjectBox = !_apis.Drawing.ObjectBox;
                _glControl.Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
        }

        private void GLView_KeyDown(object? sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[GLView] GLView_KeyDown: {e.KeyData}, Focused: {this.Focused}, GLControl Focused: {_glControl?.Focused}");
            
            // Shift+C: ClipBox 모드 토글
            if (e.KeyData == (Keys.Shift | Keys.C))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+C pressed in GLView_KeyDown");
                ToggleClipBoxMode(null, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            
            // Shift+N: Normals 표시 토글
            if (e.KeyData == (Keys.Shift | Keys.N))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+N pressed in GLView_KeyDown");
                _apis.Drawing.Normals = !_apis.Drawing.Normals;
                _glControl.Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            
            // Shift+B: ObjectBox 표시 토글
            if (e.KeyData == (Keys.Shift | Keys.B))
            {
                System.Diagnostics.Debug.WriteLine("[GLView] Shift+B pressed in GLView_KeyDown");
                _apis.Drawing.ObjectBox = !_apis.Drawing.ObjectBox;
                _glControl.Invalidate();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
        }

        private void OnShadeModeChanged(object? sender, EventArgs e) => SetStrategyBasedOnShadeMode();

        private void OnDrawingSettingChanged(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[GLView] Drawing settings changed");
            UpdatePassEnabledStates();
            _glControl?.Invalidate();
        }

        private void UpdatePassEnabledStates()
        {
            if (_normalvectorPass != null)
            {
                _normalvectorPass.Enabled = _apis.Drawing.Normals;
                System.Diagnostics.Debug.WriteLine($"[GLView] NormalVectorsPass.Enabled = {_normalvectorPass.Enabled}");
            }

            if (_objectboxPass != null)
            {
                _objectboxPass.Enabled = _apis.Drawing.ObjectBox;
                System.Diagnostics.Debug.WriteLine($"[GLView] ObjectBoxPass.Enabled = {_objectboxPass.Enabled}");
            }

            if (_coordinatePass != null)
            {
                _coordinatePass.Enabled = _apis.Drawing.Coordinates;
            }

            if (_clipPlanePass != null)
            {
                _clipPlanePass.Enabled = _apis.Drawing.ClipPlanes;
            }
        }

        private void SetStrategyBasedOnShadeMode()
        {
            lock (_strategyLock)
            {
                if (_glControl?.Width > 0 && _glControl?.Height > 0)
                {
                    ConfigureRenderPipeline();
                    UpdatePassEnabledStates();
                    Renderer.Resize(_glControl.Width, _glControl.Height);
                }
            }
            _glControl?.Invalidate();
        }

        public void ReplicateInstances(Matrix4[] sharedTransforms)
        {
            Console.WriteLine($"Replicated {sharedTransforms.Length} instances to local SSBO");
            _glControl.Invalidate();
        }
    }
}
