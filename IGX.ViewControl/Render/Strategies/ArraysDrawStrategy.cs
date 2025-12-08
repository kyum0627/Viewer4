using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Render.Strategies
{
    /// <summary>
    /// 배열 기반 드로우 전략 (DrawArrays)
    /// 인덱스 없이 정점 순서대로 렌더링
    /// </summary>
    internal class ArraysDrawStrategy<VTX> : IDrawStrategy
        where VTX : unmanaged
    {
        private readonly DrawArraysGeometry<VTX> _geometry;

        /// <summary>
        /// 배열 기반 드로우 전략 생성
        /// </summary>
        public ArraysDrawStrategy(DrawArraysGeometry<VTX> geometry)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry), "지오메트리가 null입니다.");
        }

        /// <summary>
        /// 렌더링 가능 여부
        /// </summary>
        public bool CanDraw => _geometry.VertexCount > 0;

        /// <summary>
        /// 전체 정점 렌더링
        /// </summary>
        public void ExecuteDraw(PrimitiveType primType)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[ArraysDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            GL.DrawArrays(primType, 0, _geometry.VertexCount);
        }

        /// <summary>
        /// 범위 지정 정점 렌더링
        /// </summary>
        public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[ArraysDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            if (offset < 0 || offset + count > _geometry.VertexCount)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + count})가 정점 개수 {_geometry.VertexCount}를 초과합니다.");

            GL.DrawArrays(primType, offset, count);
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
