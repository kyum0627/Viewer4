using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render.Strategies
{
    /// <summary>
    /// SSBO 기반 인스턴스드 드로우 전략
    /// Shader Storage Buffer Object를 사용한 대용량 인스턴스 렌더링
    /// </summary>
    internal class SSBODrawStrategy<VTX, NST> : IDrawStrategy
        where VTX : unmanaged
        where NST : unmanaged
    {
        private readonly DrawSSBObaseGeometry<VTX, NST> _geometry;

        /// <summary>
        /// SSBO 드로우 전략 생성
        /// </summary>
        public SSBODrawStrategy(DrawSSBObaseGeometry<VTX, NST> geometry)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry), "지오메트리가 null입니다.");
        }

        /// <summary>
        /// 렌더링 가능 여부
        /// </summary>
        public bool CanDraw => _geometry.IndexCount > 0 && _geometry.InstanceCount > 0;

        /// <summary>
        /// 전체 인스턴스 렌더링 (SSBO 사용)
        /// </summary>
        public void ExecuteDraw(PrimitiveType primType)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[SSBODrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            // SSBO 기반 인스턴스드 렌더링
            GL.DrawElementsInstancedBaseInstance(
                primType,
                _geometry.IndexCount,
                _geometry.ElementType,
                IntPtr.Zero,
                _geometry.InstanceCount,
                0);  // baseInstance는 SSBO에서 관리
        }

        /// <summary>
        /// 범위 지정 렌더링 (SSBO는 전체 렌더링으로 폴백)
        /// </summary>
        public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
        {
            // SSBO는 범위 렌더링을 지원하지 않음 - 전체 렌더링으로 폴백
            System.Diagnostics.Debug.WriteLine($"[SSBODrawStrategy] 범위 렌더링은 지원하지 않음 - 전체 렌더링 실행");
            ExecuteDraw(primType);
        }

        /// <summary>
        /// 지오메트리 바인딩
        /// </summary>
        public void Bind() => _geometry.Bind();

        /// <summary>
        /// 지오메트리 언바인딩
        /// </summary>
        public void Unbind() => _geometry.Unbind();
    }
}
