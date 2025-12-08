using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL 상태 스냅샷 (불변 레코드)
    /// 주요 OpenGL 상태를 캡처하고 복원하는 기능 제공
    /// 
    /// 포함 상태:
    /// - 클리어 색상
    /// - 깊이 테스트/마스크
    /// - 클립 거리
    /// - 컬 페이스
    /// - 블렌딩
    /// - 프레임버퍼
    /// - 뷰포트
    /// </summary>
    public record GLStateSnapshot(
        Vector4 ClearColor,
        bool DepthTestEnabled,
        bool ClipDistance0Enabled,
        bool DepthMaskEnabled,
        DepthFunction DepthFuncMode,
        bool CullFaceEnabled,
        TriangleFace CullFaceMode,
        FrontFaceDirection FrontFaceDirection,
        bool BlendingEnabled,
        BlendingFactor BlendingSrcFactor,
        BlendingFactor BlendingDstFactor,
        int BoundFramebuffer,
        Vector4i Viewport)
    {
        /// <summary>
        /// 현재 OpenGL 상태 캡처
        /// 모든 주요 상태를 쿼리하여 스냅샷 생성
        /// </summary>
        /// <returns>현재 상태를 담은 불변 스냅샷</returns>
        public static GLStateSnapshot Capture()
        {
            // 클리어 색상
            GL.GetFloat(GetPName.ColorClearValue, out Vector4 clearColor);
            
            // 깊이 관련
            bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            bool clipDistance0Enabled = GL.IsEnabled(EnableCap.ClipDistance0);
            bool depthMaskEnabled = GL.GetBoolean(GetPName.DepthWritemask);
            GL.GetInteger(GetPName.DepthFunc, out int depthFuncInt);
            DepthFunction depthFuncMode = (DepthFunction)depthFuncInt;
            
            // 컬 페이스
            bool cullFaceEnabled = GL.IsEnabled(EnableCap.CullFace);
            GL.GetInteger(GetPName.CullFaceMode, out int cullFaceModeInt);
            TriangleFace cullFaceMode = (TriangleFace)cullFaceModeInt;
            GL.GetInteger(GetPName.FrontFace, out int frontFaceInt);
            FrontFaceDirection frontFaceDirection = (FrontFaceDirection)frontFaceInt;
            
            // 블렌딩
            bool blendingEnabled = GL.IsEnabled(EnableCap.Blend);
            GL.GetInteger(GetPName.BlendSrcRgb, out int srcFactorInt);
            GL.GetInteger(GetPName.BlendDstRgb, out int dstFactorInt);
            BlendingFactor blendingSrcFactor = (BlendingFactor)srcFactorInt;
            BlendingFactor blendingDstFactor = (BlendingFactor)dstFactorInt;
            
            // 프레임버퍼 및 뷰포트
            GL.GetInteger(GetPName.DrawFramebufferBinding, out int boundFramebuffer);
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            return new GLStateSnapshot(
                clearColor,
                depthTestEnabled,
                clipDistance0Enabled,
                depthMaskEnabled,
                depthFuncMode,
                cullFaceEnabled,
                cullFaceMode,
                frontFaceDirection,
                blendingEnabled,
                blendingSrcFactor,
                blendingDstFactor,
                boundFramebuffer,
                new Vector4i(viewport[0], viewport[1], viewport[2], viewport[3]));
        }

        /// <summary>
        /// 스냅샷 상태를 OpenGL에 적용하여 복원
        /// GLUtil 헬퍼를 통해 안전하게 적용
        /// </summary>
        public void Apply()
        {
            GLUtil.Apply(this);
        }
    }
}