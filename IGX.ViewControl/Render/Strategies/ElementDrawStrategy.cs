using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Render.Strategies
{
    /// <summary>
    /// 인덱스 기반 드로우 전략 (DrawElements)
    /// 인스턴싱 없이 단일 메시 렌더링
    /// </summary>
    internal class ElementDrawStrategy<VTX> : IDrawStrategy
        where VTX : unmanaged
    {
        private readonly DrawElementGeometry<VTX> _geometry;
        private static readonly int _indexSize = Marshal.SizeOf<uint>();

        /// <summary>
        /// 인덱스 기반 드로우 전략 생성
        /// </summary>
        public ElementDrawStrategy(DrawElementGeometry<VTX> geometry)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry), "지오메트리가 null입니다.");
        }

        /// <summary>
        /// 렌더링 가능 여부
        /// </summary>
        public bool CanDraw => _geometry.IndexCount > 0;

        /// <summary>
        /// 전체 지오메트리 렌더링
        /// </summary>
        public void ExecuteDraw(PrimitiveType primType)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[ElementDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            GL.DrawElements(
                primType,
                _geometry.IndexCount,
                _geometry.ElementType,
                IntPtr.Zero);
        }

        /// <summary>
        /// 범위 지정 지오메트리 렌더링
        /// </summary>
        public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[ElementDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            if (offset < 0 || offset + count > _geometry.IndexCount)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + count})가 인덱스 개수 {_geometry.IndexCount}를 초과합니다.");

            IntPtr byteOffset = (nint)(offset * _indexSize);

            GL.DrawElements(
                primType,
                count,
                _geometry.ElementType,
                byteOffset);
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
