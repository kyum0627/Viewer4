using IGX.Loader;
using OpenTK.GLControl;

namespace IGX.ViewControl.Render
{
    internal interface IVAOSetup
    {
        int SetupVAO(GLControl context, IModel3D sharedResources);
        Action<GLControl, int> GetRenderAction();
        Action<GLControl, int, ISet<int>> GetHighlightRenderAction();
    }
}
