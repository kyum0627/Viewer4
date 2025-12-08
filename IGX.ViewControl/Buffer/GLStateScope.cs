namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL 상태 자동 복원 스코프 (RAII 패턴)
    /// using 블록을 벗어날 때 자동으로 이전 상태 복원
    /// 
    /// 사용 예:
    /// <code>
    /// using (new GLStateScope())
    /// {
    ///     // 임시로 상태 변경
    ///     GLUtil.SetDepthTest(false);
    ///     GLUtil.SetBlending(true);
    ///     
    ///     // 렌더링 작업
    ///     RenderTransparentObjects();
    /// } 
    /// // 자동으로 이전 상태 복원됨
    /// </code>
    /// 
    /// 주의: 상태 캡처/복원 비용이 있으므로 과도한 중첩은 피할 것
    /// </summary>
    public class GLStateScope : IDisposable
    {
        private readonly GLStateSnapshot _previousState;

        /// <summary>
        /// 현재 OpenGL 상태 캡처하여 스코프 생성
        /// </summary>
        public GLStateScope()
        {
            _previousState = GLStateSnapshot.Capture();
        }

        /// <summary>
        /// using 블록 종료 시 호출되어 이전 상태 복원
        /// </summary>
        public void Dispose()
        {
            _previousState.Apply();
        }
    }
}