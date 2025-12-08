using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public class Ray2f
    {
        public Vector2 origin;
        public Vector2 direction;

        public Ray2f(Vector2 origin, Vector2 dir)
        {
            this.origin = origin;
            direction = dir;
        }
    }
}