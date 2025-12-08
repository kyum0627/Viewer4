using System;
using OpenTK.Mathematics;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Intersect
{
    // https://www.geometrictools.com/Documentation/MethodOfSeparatingAxes.pdf
    /// <summary>
    /// 3차원 공간상의 임의의 3d box간 교차 계산
    /// </summary>
    public class IntersectBox3Box3
    {
        public bool intersect;
        //private float epsilon;
        //private int[] separating = new int[2];

        public bool Test(OOBB3 b1, OOBB3 b2, float epsilon = 0)
        {
            // Convenience variables.
            Vector3 C0 = b1.center;
            Vector3[] A0 = new Vector3[3] { b1.axisX, b1.axisY, b1.axisZ };// box0.axisX;
            Vector3 E0 = b1.extent + Vector3.One;
            Vector3 C1 = b2.center;
            Vector3[] A1 = new Vector3[3] { b2.axisX, b2.axisY, b2.axisZ };// box1.axisX;
            Vector3 E1 = b2.extent;

            float cutoff = 1 - epsilon;
            bool existsParallelPair = false;

            int[] separating = new int[2];

            Vector3 D = C1 - C0;

            // Dot(A0[i],A1[j]) = A1[j,i]
            float[,] dot01 = new float[3, 3];

            // |dot01[i,j]|
            float[,] absDot01 = new float[3, 3];

            // Dot(D, A0[i])
            float[] dotDA0 = new float[3];

            // center간 거리
            float r0, r1, r;

            // r0 + r1
            float r01;

            // test for axis C0 + t*A0[0].
            for (int i = 0; i < 3; ++i)
            {
                dot01[0, i] = Vector3.Dot(A0[0], A1[i]);
                absDot01[0, i] = Math.Abs(dot01[0, i]);
                if (absDot01[0, i] > cutoff)
                {
                    existsParallelPair = true;
                }
            }

            dotDA0[0] = Vector3.Dot(D, A0[0]);
            r = Math.Abs(dotDA0[0]);
            r1 = (E1[0] * absDot01[0, 0]) + (E1[1] * absDot01[0, 1]) + (E1[2] * absDot01[0, 2]);
            r01 = E0[0] + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 0;
                separating[1] = -1;
                return intersect;
            }

            // test for axis C0 + t*A0[1].
            for (int i = 0; i < 3; ++i)
            {
                dot01[1, i] = Vector3.Dot(A0[1], A1[i]);
                absDot01[1, i] = Math.Abs(dot01[1, i]);
                if (absDot01[1, i] > cutoff)
                {
                    existsParallelPair = true;
                }
            }
            dotDA0[1] = Vector3.Dot(D, A0[1]);
            r = Math.Abs(dotDA0[1]);
            r1 = (E1[0] * absDot01[1, 0]) + (E1[1] * absDot01[1, 1]) + (E1[2] * absDot01[1, 2]);
            r01 = E0[1] + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 1;
                separating[1] = -1;
                return intersect;
            }

            // test for axis C0 + t*A0[2].
            for (int i = 0; i < 3; ++i)
            {
                dot01[2, i] = Vector3.Dot(A0[2], A1[i]);
                absDot01[2, i] = Math.Abs(dot01[2, i]);
                if (absDot01[2, i] > cutoff)
                {
                    existsParallelPair = true;
                }
            }
            dotDA0[2] = Vector3.Dot(D, A0[2]);
            r = Math.Abs(dotDA0[2]);
            r1 = (E1[0] * absDot01[2, 0]) + (E1[1] * absDot01[2, 1]) + (E1[2] * absDot01[2, 2]);
            r01 = E0[2] + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 2;
                separating[1] = -1;
                return intersect;
            }

            // test for axis C0 + t*A1[0].
            r = Math.Abs(Vector3.Dot(D, A1[0]));
            r0 = (E0[0] * absDot01[0, 0]) + (E0[1] * absDot01[1, 0]) + (E0[2] * absDot01[2, 0]);
            r01 = r0 + E1[0];
            if (r > r01)
            {
                intersect = false;
                separating[0] = -1;
                separating[1] = 0;
                return intersect;
            }

            // test for axis C0 + t*A1[1].
            r = Math.Abs(Vector3.Dot(D, A1[1]));
            r0 = (E0[0] * absDot01[0, 1]) + (E0[1] * absDot01[1, 1]) + (E0[2] * absDot01[2, 1]);
            r01 = r0 + E1[1];
            if (r > r01)
            {
                intersect = false;
                separating[0] = -1;
                separating[1] = 1;
                return intersect;
            }

            // test for axis C0 + t*A1[2].
            r = Math.Abs(Vector3.Dot(D, A1[2]));
            r0 = (E0[0] * absDot01[0, 2]) + (E0[1] * absDot01[1, 2]) + (E0[2] * absDot01[2, 2]);
            r01 = r0 + E1[2];
            if (r > r01)
            {
                intersect = false;
                separating[0] = -1;
                separating[1] = 2;
                return intersect;
            }

            // 두 box의 축들중 최소 한 쌍 이상이 평행하므로,
            // 2D 상에서 두 박스가 떨어져 있을 수 있고 hedge 축들은 test 필요 없음
            if (existsParallelPair)
            {
                intersect = true;
                return intersect;
            }

            // Test for axis C0 + t*A0[0]xA1[0].
            r = Math.Abs((dotDA0[2] * dot01[1, 0]) - (dotDA0[1] * dot01[2, 0]));
            r0 = (E0[1] * absDot01[2, 0]) + (E0[2] * absDot01[1, 0]);
            r1 = (E1[1] * absDot01[0, 2]) + (E1[2] * absDot01[0, 1]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 0;
                separating[1] = 0;
                return intersect;
            }

            // Test for C0 + t*A0[0]xA1[1].
            r = Math.Abs((dotDA0[2] * dot01[1, 1]) - (dotDA0[1] * dot01[2, 1]));
            r0 = (E0[1] * absDot01[2, 1]) + (E0[2] * absDot01[1, 1]);
            r1 = (E1[0] * absDot01[0, 2]) + (E1[2] * absDot01[0, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 0;
                separating[1] = 1;
                return intersect;
            }

            // Test for C0 + t*A0[0]xA1[2].
            r = Math.Abs((dotDA0[2] * dot01[1, 2]) - (dotDA0[1] * dot01[2, 2]));
            r0 = (E0[1] * absDot01[2, 2]) + (E0[2] * absDot01[1, 2]);
            r1 = (E1[0] * absDot01[0, 1]) + (E1[1] * absDot01[0, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 0;
                separating[1] = 2;
                return intersect;
            }

            // Test for C0 + t*A0[1]xA1[0].
            r = Math.Abs((dotDA0[0] * dot01[2, 0]) - (dotDA0[2] * dot01[0, 0]));
            r0 = (E0[0] * absDot01[2, 0]) + (E0[2] * absDot01[0, 0]);
            r1 = (E1[1] * absDot01[1, 2]) + (E1[2] * absDot01[1, 1]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 1;
                separating[1] = 0;
                return intersect;
            }

            // Test for C0 + t*A0[1]xA1[1].
            r = Math.Abs((dotDA0[0] * dot01[2, 1]) - (dotDA0[2] * dot01[0, 1]));
            r0 = (E0[0] * absDot01[2, 1]) + (E0[2] * absDot01[0, 1]);
            r1 = (E1[0] * absDot01[1, 2]) + (E1[2] * absDot01[1, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 1;
                separating[1] = 1;
                return intersect;
            }

            // Test for C0 + t*A0[1]xA1[2].
            r = Math.Abs((dotDA0[0] * dot01[2, 2]) - (dotDA0[2] * dot01[0, 2]));
            r0 = (E0[0] * absDot01[2, 2]) + (E0[2] * absDot01[0, 2]);
            r1 = (E1[0] * absDot01[1, 1]) + (E1[1] * absDot01[1, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 1;
                separating[1] = 2;
                return intersect;
            }

            // Test for C0 + t*A0[2]xA1[0].
            r = Math.Abs((dotDA0[1] * dot01[0, 0]) - (dotDA0[0] * dot01[1, 0]));
            r0 = (E0[0] * absDot01[1, 0]) + (E0[1] * absDot01[0, 0]);
            r1 = (E1[1] * absDot01[2, 2]) + (E1[2] * absDot01[2, 1]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 2;
                separating[1] = 0;
                return intersect;
            }

            // Test for C0 + t*A0[2]xA1[1].
            r = Math.Abs((dotDA0[1] * dot01[0, 1]) - (dotDA0[0] * dot01[1, 1]));
            r0 = (E0[0] * absDot01[1, 1]) + (E0[1] * absDot01[0, 1]);
            r1 = (E1[0] * absDot01[2, 2]) + (E1[2] * absDot01[2, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 2;
                separating[1] = 1;
                return intersect;
            }

            // Test for C0 + t*A0[2]xA1[2].
            r = Math.Abs((dotDA0[1] * dot01[0, 2]) - (dotDA0[0] * dot01[1, 2]));
            r0 = (E0[0] * absDot01[1, 2]) + (E0[1] * absDot01[0, 2]);
            r1 = (E1[0] * absDot01[2, 1]) + (E1[1] * absDot01[2, 0]);
            r01 = r0 + r1;
            if (r > r01)
            {
                intersect = false;
                separating[0] = 2;
                separating[1] = 2;
                return intersect;
            }

            intersect = true;
            return intersect;
        }
    }
}
