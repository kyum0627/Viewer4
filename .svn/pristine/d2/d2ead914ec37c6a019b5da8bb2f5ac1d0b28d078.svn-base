using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Render.Strategies
{
    /// <summary>
    /// 간접 드로우 전략 (MultiDrawElementsIndirect)
    /// 여러 드로우 커맨드를 한 번에 실행하는 효율적인 배치 렌더링
    /// </summary>
    internal class IndirectDrawStrategy<VTX, IDX, NST> : IDrawStrategy
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        private readonly DrawIndirectGeometry<VTX, IDX, NST> _geometry;
        private static readonly int _commandSize = Marshal.SizeOf<IndirectCommandData>();

        /// <summary>
        /// 간접 드로우 전략 생성
        /// </summary>
        public IndirectDrawStrategy(DrawIndirectGeometry<VTX, IDX, NST> geometry)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry), "지오메트리가 null입니다.");
        }

        /// <summary>
        /// 렌더링 가능 여부
        /// </summary>
        public bool CanDraw => _geometry.CommandCount > 0;

        /// <summary>
        /// 전체 드로우 커맨드 실행
        /// </summary>
        public void ExecuteDraw(PrimitiveType primType)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[IndirectDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            GL.MultiDrawElementsIndirect(
                primType,
                _geometry.ElementType,
                IntPtr.Zero,
                _geometry.CommandCount,
                _commandSize);
        }

        /// <summary>
        /// 범위 지정 드로우 커맨드 실행
        /// </summary>
        public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[IndirectDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            if (offset < 0 || offset + count > _geometry.CommandCount)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + count})가 커맨드 개수 {_geometry.CommandCount}를 초과합니다.");

            IntPtr byteOffset = (nint)(offset * _commandSize);
            
            GL.MultiDrawElementsIndirect(
                primType,
                _geometry.ElementType,
                byteOffset,
                count,
                _commandSize);
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
