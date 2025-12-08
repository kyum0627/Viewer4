using IGX.Geometry.Common;

namespace IGX.ViewControl.Render
{
    public class CameraHelper
    {
        public static void InitializeCamera(IMyCamera camera, AABB3 boundingBox)
        {
            AABB3 box = boundingBox != AABB3.Empty ? boundingBox : AABB3.UnitBox;
            OpenTK.Mathematics.Vector3 target = (box.min + box.max) / 2;
            float maxSize = box.MaxDim;

            camera.FitToModel(box);
        }
    }
}
