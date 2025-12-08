using System.Linq;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public class IntrinsicsVector2
    {
        public int Dimension { get; private set; }

        public IntrinsicsVector2(int numPoints, Vector2[] points, float epsilon)
        {
            if (numPoints == 0)
            {
                Dimension = 0; // 점이 없는 경우, 차원은 0
                return;
            }

            ComputeDimension(points, epsilon);
        }

        private void ComputeDimension(Vector2[] points, float epsilon)
        {
            bool allSame = points.Skip(1).All(p => (p - points[0]).LengthSquared <= epsilon * epsilon);
            if (allSame)
            {
                Dimension = 0; // 모든 점이 같은 위치에 있음 (차원 0)
                return;
            }

            bool isLine = ArePointsOnLine(points, epsilon);
            if (isLine)
            {
                Dimension = 1; // 모든 점이 한 직선 상에 있음 (차원 1)
                return;
            }

            Dimension = 2; // 볼록 다각형으로 간주하여 2차원으로 설정 (기본값)
        }

        private bool ArePointsOnLine(Vector2[] points, float epsilon)
        {
            // 첫 번째 점과 두 번째 점 사이의 방향을 기준으로 모든 점이 한 직선 상에 있는지 확인
            Vector2 direction = points[1] - points[0];
            direction.Normalize();

            return points.Skip(2).All(p =>
            {
                Vector2 currentDirection = p - points[0];
                currentDirection.Normalize();
                float dot = Vector2.Dot(direction, currentDirection);
                return dot >= 1.0f - (epsilon * epsilon);
            });
        }
    }
}
