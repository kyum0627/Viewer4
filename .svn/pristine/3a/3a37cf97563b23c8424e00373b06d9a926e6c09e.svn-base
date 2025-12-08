using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Render.Strategies
{
    /// <summary>
    /// 인스턴스드 드로우 전략 (DrawElementsInstanced)
    /// 동일한 지오메트리를 여러 인스턴스로 효율적으로 렌더링
    /// </summary>
    internal class InstanceDrawStrategy<VTX, IDX, NST> : IDrawStrategy
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        private readonly DrawInstanceGeometry<VTX, IDX, NST> _geometry;
        private static readonly int _indexSize = Marshal.SizeOf<IDX>();

        /// <summary>
        /// 인스턴스드 드로우 전략 생성
        /// </summary>
        public InstanceDrawStrategy(DrawInstanceGeometry<VTX, IDX, NST> geometry)
        {
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry), "지오메트리가 null입니다.");
        }

        /// <summary>
        /// 렌더링 가능 여부
        /// </summary>
        public bool CanDraw => _geometry.IndexCount > 0 && _geometry.InstanceCount > 0;

        /// <summary>
        /// 전체 인스턴스 렌더링
        /// </summary>
        public void ExecuteDraw(PrimitiveType primType)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[InstanceDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            GL.DrawElementsInstanced(
                primType,
                _geometry.IndexCount,
                _geometry.ElementType,
                IntPtr.Zero,
                _geometry.InstanceCount);
        }

        /// <summary>
        /// 범위 지정 인스턴스 렌더링
        /// </summary>
        public void ExecuteDrawRange(PrimitiveType primType, int offset, int count)
        {
            if (!GLUtil.IsContextActive())
            {
                System.Diagnostics.Debug.WriteLine("[InstanceDrawStrategy] GL 컨텍스트가 활성화되지 않음");
                return;
            }

            if (offset < 0 || offset + count > _geometry.IndexCount)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + count})가 인덱스 개수 {_geometry.IndexCount}를 초과합니다.");

            IntPtr byteOffset = (IntPtr)(offset * _indexSize);

            GL.DrawElementsInstanced(
                primType,
                count,
                _geometry.ElementType,
                byteOffset,
                _geometry.InstanceCount);
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
