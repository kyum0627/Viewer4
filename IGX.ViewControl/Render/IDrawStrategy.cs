using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render
{
    /// <summary>
    /// OpenGL 드로우 호출 전략 인터페이스
    /// 각 렌더링 방식(Indirect, Instanced, SSBO, Elements)에 대한 구체적 구현 제공
    /// </summary>
    public interface IDrawStrategy
    {
        /// <summary>
        /// 전체 렌더링 실행
        /// </summary>
        /// <param name="primType">프리미티브 타입</param>
        void ExecuteDraw(PrimitiveType primType);
        
        /// <summary>
        /// 범위 지정 렌더링 실행
        /// </summary>
        /// <param name="primType">프리미티브 타입</param>
        /// <param name="offset">시작 오프셋</param>
        /// <param name="count">렌더링할 개수</param>
        void ExecuteDrawRange(PrimitiveType primType, int offset, int count);
        
        /// <summary>
        /// 지오메트리 바인딩
        /// </summary>
        void Bind();
        
        /// <summary>
        /// 지오메트리 언바인딩
        /// </summary>
        void Unbind();
        
        /// <summary>
        /// 렌더링 가능 여부 (데이터 검증)
        /// </summary>
        bool CanDraw { get; }
    }
}
