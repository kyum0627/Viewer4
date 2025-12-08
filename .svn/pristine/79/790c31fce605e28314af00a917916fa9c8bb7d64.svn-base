using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL 상태 관리 유틸리티
    /// 
    /// 주요 기능:
    /// - 상태 변경 최소화 (캐시 사용)
    /// - 스레드 안전한 상태 관리
    /// - 디버그 모드에서 오류 자동 체크
    /// 
    /// 성능 최적화:
    /// - 상태 캐시로 불필요한 GL 호출 방지
    /// - lock을 사용한 스레드 안전성
    /// </summary>
    public static class GLUtil
    {
        private static readonly object _lock = new object();
        private static GLStateSnapshot? _cachedState = null;

        /// <summary>
        /// 구조체 크기 반환 (바이트)
        /// </summary>
        public static int SizeOf<T>() where T : struct => Marshal.SizeOf<T>();

        /// <summary>
        /// 캐시된 상태 가져오기 (없으면 새로 캡처)
        /// </summary>
        private static GLStateSnapshot GetOrCreateCachedState()
        {
            lock (_lock)
            {
                return _cachedState ??= GLStateSnapshot.Capture();
            }
        }

        /// <summary>
        /// 상태 캐시 무효화 (외부에서 GL 상태 변경 시 호출)
        /// </summary>
        public static void InvalidateCache()
        {
            lock (_lock)
            {
                _cachedState = null;
            }
        }

        /// <summary>
        /// 캐시된 상태 업데이트
        /// </summary>
        public static void UpdateCachedState(GLStateSnapshot newSnapshot)
        {
            lock (_lock)
            {
                _cachedState = newSnapshot;
            }
        }

        #region 색상 및 클리어

        /// <summary>
        /// 클리어 색상 설정
        /// </summary>
        public static void SetClearColor(Vector4 color)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.ClearColor != color)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    _cachedState = currentState with { ClearColor = color };
                }
                ErrorCheck(nameof(SetClearColor));
            }
        }

        /// <summary>
        /// 프레임버퍼 클리어 (색상, 깊이, 스텐실)
        /// </summary>
        public static void ClearDrawingBuffer()
        {
            lock (_lock)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                ErrorCheck(nameof(ClearDrawingBuffer));
            }
        }

        /// <summary>
        /// 스텐실 및 깊이 버퍼 초기화 값 설정
        /// </summary>
        public static void GLStencilAndDepthBuffersInitialize(int stencilClearValue, float depthClearValue)
        {
            lock (_lock)
            {
                GL.ClearStencil(stencilClearValue);
                GL.ClearDepth(depthClearValue);
                ErrorCheck(nameof(GLStencilAndDepthBuffersInitialize));
            }
        }

        #endregion

        #region 깊이 테스트

        /// <summary>
        /// 깊이 테스트 활성화/비활성화
        /// </summary>
        public static void SetDepthTest(bool enable)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.DepthTestEnabled != enable)
                {
                    SetEnableState(EnableCap.DepthTest, enable);
                    _cachedState = currentState with { DepthTestEnabled = enable };
                }
                ErrorCheck(nameof(SetDepthTest));
            }
        }

        /// <summary>
        /// 깊이 버퍼 쓰기 활성화/비활성화
        /// </summary>
        public static void SetDepthMask(bool enable)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.DepthMaskEnabled != enable)
                {
                    GL.DepthMask(enable);
                    _cachedState = currentState with { DepthMaskEnabled = enable };
                }
                ErrorCheck(nameof(SetDepthMask));
            }
        }

        /// <summary>
        /// 깊이 함수 설정 (Less, LessOrEqual 등)
        /// </summary>
        public static void DepthFunc(DepthFunction func)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.DepthFuncMode != func)
                {
                    GL.DepthFunc(func);
                    _cachedState = currentState with { DepthFuncMode = func };
                }
                ErrorCheck(nameof(DepthFunc));
            }
        }

        #endregion

        #region 컬 페이스

        /// <summary>
        /// 컬 페이스 설정
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        /// <param name="mode">컬링 모드 (기본: Back)</param>
        public static void SetCullFace(bool enable, TriangleFace mode = TriangleFace.Back)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.CullFaceEnabled != enable)
                {
                    SetEnableState(EnableCap.CullFace, enable);
                    _cachedState = currentState with { CullFaceEnabled = enable };
                }

                if (enable && currentState.CullFaceMode != mode)
                {
                    GL.CullFace(mode);
                    _cachedState = _cachedState! with { CullFaceMode = mode };
                }
                ErrorCheck(nameof(SetCullFace));
            }
        }

        /// <summary>
        /// 전면 방향 설정 (기본: CCW)
        /// </summary>
        public static void SetFrontFaceDirection(FrontFaceDirection direction = FrontFaceDirection.Ccw)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.FrontFaceDirection != direction)
                {
                    GL.FrontFace(direction);
                    _cachedState = currentState with { FrontFaceDirection = direction };
                }
                ErrorCheck(nameof(SetFrontFaceDirection));
            }
        }

        #endregion

        #region 블렌딩

        /// <summary>
        /// 블렌딩 설정
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        /// <param name="src">소스 블렌딩 팩터 (기본: SrcAlpha)</param>
        /// <param name="dst">대상 블렌딩 팩터 (기본: OneMinusSrcAlpha)</param>
        public static void SetBlending(bool enable, BlendingFactor src = BlendingFactor.SrcAlpha, BlendingFactor dst = BlendingFactor.OneMinusSrcAlpha)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.BlendingEnabled != enable)
                {
                    SetEnableState(EnableCap.Blend, enable);
                    _cachedState = currentState with { BlendingEnabled = enable };
                }

                if (enable && (currentState.BlendingSrcFactor != src || currentState.BlendingDstFactor != dst))
                {
                    GL.BlendFunc(src, dst);
                    _cachedState = currentState with { BlendingSrcFactor = src, BlendingDstFactor = dst };
                }
                ErrorCheck(nameof(SetBlending));
            }
        }

        #endregion

        #region 뷰포트 및 프레임버퍼

        /// <summary>
        /// 뷰포트 설정
        /// </summary>
        public static void SetViewport(int x, int y, int width, int height)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.Viewport.X != x || currentState.Viewport.Y != y || 
                    currentState.Viewport.Z != width || currentState.Viewport.W != height)
                {
                    GL.Viewport(x, y, width, height);
                    _cachedState = currentState with { Viewport = new Vector4i(x, y, width, height) };
                }
                ErrorCheck(nameof(SetViewport));
            }
        }

        /// <summary>
        /// 프레임버퍼 바인딩
        /// </summary>
        public static void BindFramebuffer(FramebufferTarget target, int handle)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.BoundFramebuffer != handle)
                {
                    GL.BindFramebuffer(target, handle);
                    _cachedState = currentState with { BoundFramebuffer = handle };
                }
                ErrorCheck(nameof(BindFramebuffer));
            }
        }

        /// <summary>
        /// 현재 뷰포트 반환
        /// </summary>
        public static Vector4i GetViewport()
        {
            lock (_lock)
            {
                return GetOrCreateCachedState().Viewport;
            }
        }

        #endregion

        #region 기타

        /// <summary>
        /// 클립 거리 0 활성화/비활성화
        /// </summary>
        public static void SetClipDistance0(bool enable)
        {
            lock (_lock)
            {
                var currentState = GetOrCreateCachedState();
                if (currentState.ClipDistance0Enabled != enable)
                {
                    SetEnableState(EnableCap.ClipDistance0, enable);
                    _cachedState = currentState with { ClipDistance0Enabled = enable };
                }
                ErrorCheck(nameof(SetClipDistance0));
            }
        }

        /// <summary>
        /// 스냅샷 상태를 OpenGL에 적용
        /// </summary>
        public static void Apply(GLStateSnapshot state)
        {
            lock (_lock)
            {
                SetClearColor(state.ClearColor);
                SetDepthTest(state.DepthTestEnabled);
                SetClipDistance0(state.ClipDistance0Enabled);
                SetDepthMask(state.DepthMaskEnabled);
                DepthFunc(state.DepthFuncMode);
                SetCullFace(state.CullFaceEnabled, state.CullFaceMode);
                SetFrontFaceDirection(state.FrontFaceDirection);
                SetBlending(state.BlendingEnabled, state.BlendingSrcFactor, state.BlendingDstFactor);
                BindFramebuffer(FramebufferTarget.Framebuffer, state.BoundFramebuffer);
                SetViewport(state.Viewport.X, state.Viewport.Y, state.Viewport.Z, state.Viewport.W);
                _cachedState = state;
            }
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// Enable/Disable 상태 설정
        /// </summary>
        private static void SetEnableState(EnableCap cap, bool enable)
        {
            if (enable)
            {
                GL.Enable(cap);
            }
            else
            {
                GL.Disable(cap);
            }
        }

        /// <summary>
        /// OpenGL 오류 체크 (DEBUG 모드만)
        /// </summary>
        [Conditional("DEBUG")]
        public static void ErrorCheck(
            string message = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                string errorMessage = $"[OpenGL 오류] {error} - {sourceFilePath}:{sourceLineNumber} ({memberName}) {message}";
                Debug.WriteLine(errorMessage);
            }
        }

        /// <summary>
        /// OpenGL 컨텍스트 활성 여부 확인
        /// </summary>
        public static bool IsContextActive()
        {
            try
            {
                // GetError 호출이 성공하면 컨텍스트 활성
                _ = GL.GetError();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 활성 컨텍스트 필수 확인 (없으면 예외)
        /// </summary>
        public static void EnsureContextActive()
        {
            if (!IsContextActive())
                throw new InvalidOperationException("OpenGL 컨텍스트가 활성화되지 않았습니다.");
        }

        #endregion
    }
}