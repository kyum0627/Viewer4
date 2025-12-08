using IGX.ViewControl.Buffer;
using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render
{
    /// <summary>
    /// 통합 드로우 렌더러
    /// 전략 패턴을 사용하여 다양한 렌더링 방식 지원
    /// </summary>
    public class DrawRenderer : IDrawBuffer
    {
        private readonly IDrawStrategy _strategy;
        
        public Shader? Shader { get; set; }
        public PrimitiveType PrimType { get; set; } = PrimitiveType.Triangles;

        /// <summary>
        /// 드로우 렌더러 생성
        /// </summary>
        /// <param name="strategy">렌더링 전략</param>
        /// <param name="shader">기본 쉐이더</param>
        public DrawRenderer(IDrawStrategy strategy, Shader? shader = null)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy), "전략이 null입니다.");
            Shader = shader;
        }

        #region IDrawBuffer 구현
        
        /// <summary>
        /// 전체 렌더링 실행
        /// </summary>
        public void Execute()
        {
            if (!_strategy.CanDraw) return;

            ExecuteDrawCommon(
                drawAction: () => _strategy.ExecuteDraw(PrimType),
                overrideShader: null);
        }

        /// <summary>
        /// 범위 지정 렌더링 실행
        /// </summary>
        public void ExecuteRange(int offset, int count)
        {
            if (count <= 0 || !_strategy.CanDraw) return;

            ExecuteDrawCommon(
                drawAction: () => _strategy.ExecuteDrawRange(PrimType, offset, count),
                overrideShader: null);
        }

        /// <summary>
        /// 특정 쉐이더로 렌더링 실행
        /// </summary>
        public void ExecuteWithShader(Shader? shader)
        {
            ArgumentNullException.ThrowIfNull(shader, "쉐이더가 null입니다.");
            
            if (!_strategy.CanDraw) return;

            ExecuteDrawCommon(
                drawAction: () => _strategy.ExecuteDraw(PrimType),
                overrideShader: shader);
        }
        
        #endregion

        #region 공통 실행 패턴
        
        /// <summary>
        /// 드로우 실행 공통 패턴 (바인딩 + 실행 + 언바인딩)
        /// </summary>
        private void ExecuteDrawCommon(Action drawAction, Shader? overrideShader)
        {
            GLUtil.EnsureContextActive();

            try
            {
                BindResources(overrideShader);
                drawAction();
            }
            finally
            {
                _strategy.Unbind();
            }
        }

        /// <summary>
        /// 렌더링에 필요한 리소스 바인딩 (지오메트리 + 쉐이더)
        /// </summary>
        private void BindResources(Shader? overrideShader)
        {
            _strategy.Bind();
            
            Shader? activeShader = overrideShader ?? Shader;
            activeShader?.Use();
        }
        
        #endregion
    }
}
