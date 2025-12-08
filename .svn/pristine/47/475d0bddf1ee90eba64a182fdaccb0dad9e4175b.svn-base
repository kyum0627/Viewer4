using System;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// Test할 정점이 mVertices(세 점)으로 구성된 선분,
    /// 삼각형, 원(내접/ 외접)의 기준으로 선상/좌/우,혹은 내/외
    /// 어느 곳에 위치해 있는지에 대한 정보를 제공
    /// </summary>
    public class QueryVector2Position
    {
        Vector2Tuple3 mVertices;// 2D 벡터의 튜플(3개의 벡터)

        /// <summary>
        /// 3개의 Vector2 인스턴스를 받아 mVertices를 초기화
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public QueryVector2Position(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            mVertices = new Vector2Tuple3(v0, v1, v2);
        }

        /// <summary>
        /// Vector2Tuple3 인스턴스를 받아 mVertices를 초기화
        /// </summary>
        /// <param name="tuple"></param>
        public QueryVector2Position(Vector2Tuple3 tuple)
        {
            mVertices = tuple;
        }

        /// <summary>
        /// 주어진 점(checkPoint)와 두 정점(v0, v1)을 사용하여
        /// 테스트 점이 선의 오른쪽(+), 왼쪽(-), 또는 선 위(0)에 있는지를 반환
        /// </summary>
        /// <param name="checkPoint"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public int ToLine(Vector2 checkPoint, int v0, int v1)
        {
            // 정점의 순서를 정렬하고, 정렬된 여부를 positive 변수에 저장
            bool positive = Sort(ref v0, ref v1);

            Vector2 vec0 = mVertices[v0]; // 첫 번째 정점
            Vector2 vec1 = mVertices[v1]; // 두 번째 정점

            // checkPoint 점과 vec0 사이의 차이 계산
            float x0 = checkPoint[0] - vec0[0];
            float y0 = checkPoint[1] - vec0[1];
            // vec0와 vec1 사이의 차이 계산
            float x1 = vec1[0] - vec0[0];
            float y1 = vec1[1] - vec0[1];

            // 2D 행렬식 계산
            float det = Det2(x0, y0, x1, y1);
            if (!positive)
            {
                det = -det; // 정점 순서가 반대라면 부호를 반전
            }

            // 행렬식의 값에 따라 반환: 양수(+1), 음수(-1), 0
            return det > 0 ? +1 : det < 0 ? -1 : 0;
        }

        /// <summary>
        /// 주어진 인덱스 i와 두 정점 v0, v1을 사용하여
        /// 테스트 점이 선의 오른쪽(+)인지, 왼쪽(-)인지, 선 위(0)에 있는지를 반환
        /// </summary>
        /// <param name="i"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public int ToLine(int i, int v0, int v1)
        {
            return ToLine(mVertices[i], v0, v1);
        }

        // 주어진 인덱스 i와 세 정점 v0, v1, v2를 사용하여 
        // 테스트 점이 삼각형의 외부(+1), 내부(-1), 또는 삼각형 위(0)에 있는지를 반환
        public int ToTriangle(int i, int v0, int v1, int v2)
        {
            return ToTriangle(mVertices[i], v0, v1, v2);
        }

        // 주어진 점(checkPoint)와 세 정점(v0, v1, v2)을 사용하여
        // 테스트 점이 삼각형의 외부(+1), 내부(-1), 또는 삼각형 위(0)에 있는지를 반환
        public int ToTriangle(Vector2 test, int v0, int v1, int v2)
        {
            // 첫 번째 선 (v1, v2)과의 관계 확인
            int sign0 = ToLine(test, v1, v2);
            if (sign0 > 0)
            {
                return +1; // 외부
            }

            // 두 번째 선 (v0, v2)과의 관계 확인
            int sign1 = ToLine(test, v0, v2);
            if (sign1 < 0)
            {
                return +1; // 외부
            }

            // 세 번째 선 (v0, v1)과의 관계 확인
            int sign2 = ToLine(test, v0, v1);
            if (sign2 > 0)
            {
                return +1; // 외부
            }

            // 모두 내부이거나 선 위에 있는 경우
            return sign0 != 0 && sign1 != 0 && sign2 != 0 ? -1 : 0; // 내부 또는 삼각형 위
        }

        // 주어진 인덱스 i와 세 정점 v0, v1, v2를 사용하여
        // 테스트 점이 외접원(+1), 내접원(-1), 또는 정점으로 만든 원(0)에 있는지를 반환
        public int ToCircumcircle(int i, int v0, int v1, int v2)
        {
            return ToCircumcircle(mVertices[i], v0, v1, v2);
        }

        // 주어진 점(checkPoint)와 세 정점(v0, v1, v2)을 사용하여
        // 테스트 점이 외접원(+1), 내접원(-1), 또는 정점으로 만든 원(0)에 있는지를 반환
        public int ToCircumcircle(Vector2 test, int v0, int v1, int v2)
        {
            // 정점의 순서를 정렬하고, 정렬된 여부를 positive 변수에 저장
            bool positive = Sort(ref v0, ref v1, ref v2);

            // 정점의 위치를 가져옴
            Vector2 vec0 = mVertices[v0];
            Vector2 vec1 = mVertices[v1];
            Vector2 vec2 = mVertices[v2];

            // 각 정점과 테스트 점 사이의 합 및 차 계산
            float s0x = vec0.X + test.X;
            float d0x = vec0.X - test.X;
            float s0y = vec0.Y + test.Y;
            float d0y = vec0.Y - test.Y;
            float s1x = vec1.X + test.X;
            float d1x = vec1.X - test.X;
            float s1y = vec1.Y + test.Y;
            float d1y = vec1.Y - test.Y;
            float s2x = vec2.X + test.X;
            float d2x = vec2.X - test.X;
            float s2y = vec2.Y + test.Y;
            float d2y = vec2.Y - test.Y;

            // 3D 행렬식 계산
            float z0 = (s0x * d0x) + (s0y * d0y); // vec0에 대한 계산
            float z1 = (s1x * d1x) + (s1y * d1y); // vec1에 대한 계산
            float z2 = (s2x * d2x) + (s2y * d2y); // vec2에 대한 계산
            float det = Det3(d0x, d0y, z0, d1x, d1y, z1, d2x, d2y, z2);
            if (!positive)
            {
                det = -det; // 정점 순서가 반대라면 부호를 반전
            }

            // 행렬식의 값에 따라 반환: 외접원(+1), 내접원(-1), 또는 원(0)
            return det < 0 ? 1 : det > 0 ? -1 : 0;
        }

        // 벡터의 내적
        public static float Dot(float x0, float y0, float x1, float y1)
        {
            return (x0 * x1) + (y0 * y1);
        }

        // 2D 행렬식 계산
        public static float Det2(float x0, float y0, float x1, float y1)
        {
            return (x0 * y1) - (x1 * y0);
        }

        // 3D 행렬식 계산
        public static float Det3(float x0, float y0, float z0, float x1, float y1,
            float z1, float x2, float y2, float z2)
        {
            // 3D 행렬식의 각 성분 계산
            float c00 = (y1 * z2) - (y2 * z1);
            float c01 = (y2 * z0) - (y0 * z2);
            float c02 = (y0 * z1) - (y1 * z0);
            return (x0 * c00) + (x1 * c01) + (x2 * c02); // 최종 행렬식 반환
        }

        // 두 정점(v0, v1)의 순서를 정렬하고, 정렬 여부를 반환
        public static bool Sort(ref int v0, ref int v1)
        {
            //int j0, j1;
            bool positive;

            if (v0 < v1) // v0가 v1보다 작으면
            {
                //0 = 0; j1 = 1; // 정렬된 순서
                positive = true; // 정렬된 상태
            }
            else // v1이 더 작으면
            {
                //j0 = 1; j1 = 0; // 정렬된 순서
                positive = false; // 반전된 상태
            }

            // Index2i 객체를 생성하여 정렬된 인덱스를 저장
            //Index2i value = new Index2i(v0, v1);
            Tuple<int, int> value = new(v0, v1);
            v0 = value.Item1;
            v1 = value.Item2;
            //v0 = value[j0]; // 정렬된 인덱스 저장
            //v1 = value[j1];
            return positive; // 정렬된 여부 반환
        }

        // 세 정점(v0, v1, v2)의 순서를 정렬하고, 정렬 여부를 반환
        public static bool Sort(ref int v0, ref int v1, ref int v2)
        {
            int j0, j1, j2;
            bool positive;

            if (v0 < v1) // v0가 v1보다 작으면
            {
                if (v2 < v0) // v2 < v0 < v1
                {
                    j0 = 2;
                    j1 = 0;
                    j2 = 1;
                    positive = true; // 정렬된 상태
                }
                else if (v2 < v1) // v0 < v2 < v1
                {
                    j0 = 0;
                    j1 = 2;
                    j2 = 1;
                    positive = false; // 반전된 상태
                }
                else // v0 < v1 < v2
                {
                    j0 = 0;
                    j1 = 1;
                    j2 = 2;
                    positive = true; // 정렬된 상태
                }
            }
            else // v1이 더 작으면
            {
                if (v2 < v1) // v2 < v1 < v0
                {
                    j0 = 2;
                    j1 = 1;
                    j2 = 0;
                    positive = false; // 반전된 상태
                }
                else if (v2 < v0) // v1 < v2 < v0
                {
                    j0 = 1;
                    j1 = 2;
                    j2 = 0;
                    positive = true; // 정렬된 상태
                }
                else // v2 < v1 < v0
                {
                    j0 = 1;
                    j1 = 0;
                    j2 = 2;
                    positive = false; // 반전된 상태
                }
            }

            // Index3i 객체를 생성하여 정렬된 인덱스를 저장
            int[] value = new int[3] { v0, v1, v2 };
            //Index3i value = new Index3i(v0, v1, v2);

            v0 = value[j0];
            v1 = value[j1];
            v2 = value[j2];
            return positive; // 정렬된 여부 반환
        }

        // 네 정점(v0, v1, v2, v3)의 순서를 정렬하고, 정렬 여부를 반환
        public static bool Sort(ref int v0, ref int v1, ref int v2, ref int v3)
        {
            int j0, j1, j2, j3;
            bool positive;

            // v0와 v1의 크기 비교
            if (v0 < v1)
            {
                // v2와 v3의 크기 비교
                if (v2 < v3)
                {
                    // v1과 v2의 크기 비교
                    if (v1 < v2)
                    {
                        j0 = 0;
                        j1 = 1;
                        j2 = 2;
                        j3 = 3;
                        positive = true; // 정렬된 상태
                    }
                    else if (v3 < v0)
                    {
                        j0 = 2;
                        j1 = 3;
                        j2 = 0;
                        j3 = 1;
                        positive = true; // 정렬된 상태
                    }
                    else if (v2 < v0)
                    {
                        if (v3 < v1)
                        {
                            j0 = 2;
                            j1 = 0;
                            j2 = 3;
                            j3 = 1;
                            positive = false; // 반전된 상태
                        }
                        else
                        {
                            j0 = 2;
                            j1 = 0;
                            j2 = 1;
                            j3 = 3;
                            positive = true; // 정렬된 상태
                        }
                    }
                    else
                    {
                        if (v3 < v1)
                        {
                            j0 = 0;
                            j1 = 2;
                            j2 = 3;
                            j3 = 1;
                            positive = true; // 정렬된 상태
                        }
                        else
                        {
                            j0 = 0;
                            j1 = 2;
                            j2 = 1;
                            j3 = 3;
                            positive = false; // 반전된 상태
                        }
                    }
                }
                else // v2가 v3보다 크면
                {
                    if (v1 < v3)
                    {
                        j0 = 0;
                        j1 = 1;
                        j2 = 3;
                        j3 = 2;
                        positive = false; // 반전된 상태
                    }
                    else if (v2 < v0)
                    {
                        j0 = 3;
                        j1 = 2;
                        j2 = 0;
                        j3 = 1;
                        positive = false; // 반전된 상태
                    }
                    else if (v3 < v0)
                    {
                        if (v2 < v1)
                        {
                            j0 = 3;
                            j1 = 0;
                            j2 = 2;
                            j3 = 1;
                            positive = true; // 정렬된 상태
                        }
                        else
                        {
                            j0 = 3;
                            j1 = 0;
                            j2 = 1;
                            j3 = 2;
                            positive = false; // 반전된 상태
                        }
                    }
                    else
                    {
                        if (v2 < v1)
                        {
                            j0 = 0;
                            j1 = 3;
                            j2 = 2;
                            j3 = 1;
                            positive = false; // 반전된 상태
                        }
                        else
                        {
                            j0 = 0;
                            j1 = 3;
                            j2 = 1;
                            j3 = 2;
                            positive = true; // 정렬된 상태
                        }
                    }
                }
            }
            else // v1이 v0보다 크면
            {
                if (v2 < v3) // v2가 v3보다 작으면
                {
                    if (v0 < v2) // v0 < v2
                    {
                        j0 = 1;
                        j1 = 0;
                        j2 = 2;
                        j3 = 3;
                        positive = false; // 반전된 상태
                    }
                    else if (v3 < v1) // v3가 v1보다 작으면
                    {
                        j0 = 2;
                        j1 = 3;
                        j2 = 1;
                        j3 = 0;
                        positive = false; // 반전된 상태
                    }
                    else if (v2 < v1) // v2가 v1보다 작으면
                    {
                        if (v3 < v0)
                        {
                            j0 = 2;
                            j1 = 1;
                            j2 = 3;
                            j3 = 0;
                            positive = true; // 정렬된 상태
                        }
                        else
                        {
                            j0 = 2;
                            j1 = 1;
                            j2 = 0;
                            j3 = 3;
                            positive = false; // 반전된 상태
                        }
                    }
                    else // v3가 v0보다 크면
                    {
                        if (v3 < v0)
                        {
                            j0 = 1;
                            j1 = 2;
                            j2 = 3;
                            j3 = 0;
                            positive = false; // 반전된 상태
                        }
                        else
                        {
                            j0 = 1;
                            j1 = 2;
                            j2 = 0;
                            j3 = 3;
                            positive = true; // 정렬된 상태
                        }
                    }
                }
                else // v2가 v3보다 크면
                {
                    if (v0 < v3)
                    {
                        j0 = 1;
                        j1 = 0;
                        j2 = 3;
                        j3 = 2;
                        positive = true; // 정렬된 상태
                    }
                    else if (v2 < v1)
                    {
                        j0 = 3;
                        j1 = 2;
                        j2 = 1;
                        j3 = 0;
                        positive = true; // 정렬된 상태
                    }
                    else if (v3 < v1) // v3가 v1보다 작으면
                    {
                        if (v2 < v0)
                        {
                            j0 = 3;
                            j1 = 1;
                            j2 = 2;
                            j3 = 0;
                            positive = false; // 반전된 상태
                        }
                        else
                        {
                            j0 = 3;
                            j1 = 1;
                            j2 = 0;
                            j3 = 2;
                            positive = true; // 정렬된 상태
                        }
                    }
                    else // v2가 v0보다 크면
                    {
                        if (v2 < v0)
                        {
                            j0 = 1;
                            j1 = 3;
                            j2 = 2;
                            j3 = 0;
                            positive = true; // 정렬된 상태
                        }
                        else
                        {
                            j0 = 1;
                            j1 = 3;
                            j2 = 0;
                            j3 = 2;
                            positive = false; // 반전된 상태
                        }
                    }
                }
            }

            // Index4i 객체를 생성하여 정렬된 인덱스를 저장
            int[] value = new int[4] { v0, v1, v2, v3 };
            // Index4i value = new Index4i(v0, v1, v2, v3);

            v0 = value[j0];
            v1 = value[j1];
            v2 = value[j2];
            v3 = value[j3];
            return positive; // 정렬된 여부 반환
        }
    }
}
