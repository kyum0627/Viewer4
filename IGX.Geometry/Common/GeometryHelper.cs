using System;
using System.Linq;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public static class GeometryHelper
    {
        public static (Vector2[], Vector2[]) MakeArc2(float radius = 1, uint samples = 16, float angle = MathUtil.TwoPi, float startAngle = 0)
        {
            Vector2[] nor = CalculateArcSection(samples, angle, startAngle);
            Vector2[] pos = nor.Select(n => n * radius).ToArray();
            return (pos, nor);
            // ---
            /// <summary>
            /// 주어진 샘플 수에 대한 원형 단면을 계산
            /// </summary>
            /// <param name="samples">샘플 수</param>
            /// <param name="angle">각도</param>
            /// <param name="startAngle">시작 각도</param>
            /// <returns>계산된 원형 단면 좌표 배열</returns>
            static Vector2[] CalculateArcSection(uint samples = 16, float angle = MathUtil.TwoPi, float startAngle = 0)
            {
                if (samples < 3)
                {
                    samples = 3; // 최소 샘플 수를 3으로 설정
                }

                Vector2[] points = new Vector2[samples]; // 정점 좌표 배열
                for (int i = 0; i < samples; i++)
                {
                    // 코사인 및 사인 값을 계산
                    float theta = (angle * i / samples) + startAngle;
                    float cosTheta = (float)Math.Cos(theta);
                    float sinTheta = (float)Math.Sin(theta);

                    // 정점 좌표 및 법선 벡터 계산
                    points[i].X = cosTheta; // X 좌표
                    points[i].Y = sinTheta; // Y 좌표
                }
                return points;
            }
        }
        public static (Vector2[], Vector2[]) MakeSquare2(float lx = 1, float ly = 1)
        {
            float dx = lx / 2;
            float dy = ly / 2;

            // 4개의 정점 (사각형의 4개 모서리)
            Vector2[] p4 = new Vector2[4] { new(-dx, -dy), new(dx, -dy), new(dx, dy), new(-dx, dy) };

            // 8개의 정점: 각 면에 대해 2개의 정점씩
            Vector2[] pos = new Vector2[8] { p4[0], p4[1], p4[1], p4[2], p4[2], p4[3], p4[3], p4[0] };

            // 각 정점에 대한 법선 벡터
            Vector2[] nor = new Vector2[8]
            {
                -Vector2.UnitY, // Bottom-left
                -Vector2.UnitY, // Bottom-right
                 Vector2.UnitX, // Bottom-right
                 Vector2.UnitX, // Top-right
                 Vector2.UnitY, // Top-right
                 Vector2.UnitY, // Top-left
                -Vector2.UnitX, // Top-left
                -Vector2.UnitX  // Bottom-left
            };

            return (pos, nor);
        }
        public static (Vector2[], Vector2[]) MakeLinearSection(Vector2 p0, Vector2 p1)
        {
            Vector2 n = new Vector2(p0.Y - p1.Y, p1.X - p0.X).Normalized();
            return (new Vector2[] { p0, p1 }, new Vector2[2] { n, n });
        }
    }
}
