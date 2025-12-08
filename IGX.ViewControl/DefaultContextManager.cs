using OpenTK.GLControl;

namespace IGX.ViewControl
{
    public class DefaultContextManager : IContextManager
    {
        public GLControl CreateControl(string name)
        {
            var control = new GLControl
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(4, 6, 0, 0),
                Dock = DockStyle.Fill,
                Flags = OpenTK.Windowing.Common.ContextFlags.Default,
                IsEventDriven = true,
                Profile = OpenTK.Windowing.Common.ContextProfile.Compatability,
                Name = name,
                Text = name,
                Visible = true
            };
            Console.WriteLine($"Created GLControl: {name}");
            return control;
        }

        public void MakeCurrent(GLControl control)
        {
            if (!control.IsHandleCreated)
            {
                control.CreateControl();
                Console.WriteLine($"Created handle for {control.Name}");
            }
            control.MakeCurrent();
            Console.WriteLine($"Made current: {control.Name}");
        }

        public void SetSharedContext(GLControl control, GLControl sharedContext)
        {
            if (sharedContext != null && sharedContext.IsHandleCreated)
            {
                control.SharedContext = sharedContext;
                Console.WriteLine($"Set shared context for {control.Name}");
            }
        }
    }
}