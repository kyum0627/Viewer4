using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl
{
    public class DefaultViewportManager : IViewportManager
    {
        private Size _viewportSize;

        public void SetupViewport(GLControl control)
        {
            if (!control.IsHandleCreated)
            {
                control.CreateControl();
                Console.WriteLine($"Created handle for {control.Name} in SetupViewport");
            }
            control.MakeCurrent();
            _viewportSize = control.Size;
            GL.Viewport(0, 0, _viewportSize.Width, _viewportSize.Height);
            Console.WriteLine($"Set viewport for {control.Name}: {_viewportSize.Width}x{_viewportSize.Height}, AspectRatio: {(float)_viewportSize.Width / _viewportSize.Height}");
        }

        public Size GetViewportSize(GLControl control)
        {
            if (_viewportSize.Width == 0 || _viewportSize.Height == 0)
            {
                _viewportSize = control.Size;
                Console.WriteLine($"Retrieved viewport size for {control.Name}: {_viewportSize.Width}x{_viewportSize.Height}");
            }
            return _viewportSize;
        }
    }
}
