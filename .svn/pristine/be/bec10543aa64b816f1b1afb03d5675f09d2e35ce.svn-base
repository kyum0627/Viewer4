using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render.Strategies;

namespace IGX.ViewControl.Render
{
    /// <summary>
    /// 드로우 렌더러 팩토리
    /// 지오메트리 타입에 따라 적절한 렌더링 전략을 가진 렌더러 생성
    /// </summary>
    public static class DrawRendererFactory
    {
        /// <summary>
        /// 간접 드로우 지오메트리용 렌더러 생성
        /// </summary>
        public static IDrawBuffer Create<VTX, IDX, NST>(
            DrawIndirectGeometry<VTX, IDX, NST> geometry, 
            Shader? shader = null)
            where VTX : unmanaged
            where IDX : unmanaged
            where NST : unmanaged
        {
            var strategy = new IndirectDrawStrategy<VTX, IDX, NST>(geometry);
            return new DrawRenderer(strategy, shader);
        }

        /// <summary>
        /// 인스턴스드 지오메트리용 렌더러 생성
        /// </summary>
        public static IDrawBuffer Create<VTX, IDX, NST>(
            DrawInstanceGeometry<VTX, IDX, NST> geometry, 
            Shader? shader = null)
            where VTX : unmanaged
            where IDX : unmanaged
            where NST : unmanaged
        {
            var strategy = new InstanceDrawStrategy<VTX, IDX, NST>(geometry);
            return new DrawRenderer(strategy, shader);
        }

        /// <summary>
        /// SSBO 기반 지오메트리용 렌더러 생성
        /// </summary>
        public static IDrawBuffer Create<VTX, NST>(
            DrawSSBObaseGeometry<VTX, NST> geometry, 
            Shader? shader = null)
            where VTX : unmanaged
            where NST : unmanaged
        {
            var strategy = new SSBODrawStrategy<VTX, NST>(geometry);
            return new DrawRenderer(strategy, shader);
        }

        /// <summary>
        /// 인덱스 기반 지오메트리용 렌더러 생성
        /// </summary>
        public static IDrawBuffer Create<VTX>(
            DrawElementGeometry<VTX> geometry, 
            Shader? shader = null)
            where VTX : unmanaged
        {
            var strategy = new ElementDrawStrategy<VTX>(geometry);
            return new DrawRenderer(strategy, shader);
        }

        /// <summary>
        /// 배열 기반 지오메트리용 렌더러 생성
        /// </summary>
        public static IDrawBuffer Create<VTX>(
            DrawArraysGeometry<VTX> geometry, 
            Shader? shader = null)
            where VTX : unmanaged
        {
            var strategy = new ArraysDrawStrategy<VTX>(geometry);
            return new DrawRenderer(strategy, shader);
        }
    }
}
