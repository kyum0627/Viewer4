using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 3D 벡터 쌍
    /// </summary>
    public struct Vector3Tuple2
    {
        public Vector3 V0; // 첫 번째 벡터
        public Vector3 V1; // 두 번째 벡터

        /// <summary>
        /// 3D 벡터 쌍 초기화
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        public Vector3Tuple2(Vector3 v0, Vector3 v1)
        {
            V0 = v0;
            V1 = v1;
        }

        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">0일 경우 V0id, 1일 경우 V1을 반환</param>
        public Vector3 this[int key]
        {
            get
            {
                if (key < 0 || key > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key == 0 ? V0 : V1; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key == 0)
                {
                    V0 = value; // 첫 번째 벡터 설정
                }
                else
                {
                    V1 = value; // 두 번째 벡터 설정
                }
            }
        }
        public override string ToString() => $"({V0}, {V1})"; // 문자열 형식으로 반환
    }

    /// <summary>
    /// 3D 벡터 Triple Tuple
    /// </summary>
    public struct Vector3Tuple3
    {
        public Vector3 V0; // 첫 번째 벡터
        public Vector3 V1; // 두 번째 벡터
        public Vector3 V2; // 세 번째 벡터

        /// <summary>
        ///
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        /// <param name="v2">세 번째 벡터</param>
        public Vector3Tuple3(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">각각 0 -> V0id, 1 -> Vertex0, 2 -> V2를 반환</param>
        public Vector3 this[int key]
        {
            get
            {
                if (key < 0 || key > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key == 0 ? V0 : key == 1 ? V1 : V2; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key == 0)
                {
                    V0 = value; // 첫 번째 벡터 설정
                }
                else if (key == 1)
                {
                    V1 = value; // 두 번째 벡터 설정
                }
                else
                {
                    V2 = value; // 세 번째 벡터 설정
                }
            }
        }
        public override string ToString() => $"({V0}, {V1}, {V2})"; // 문자열 형식으로 반환
    }

    /// <summary>
    /// 3D 벡터 Quadraple
    /// </summary>
    public struct Vector3Tuple4
    {
        public Vector3 V0; // 첫 번째 벡터
        public Vector3 V1; // 두 번째 벡터
        public Vector3 V2; // 세 번째 벡터
        public Vector3 V3; // 네 번째 벡터

        /// <summary>
        /// 3D 벡터 사중을 초기화.
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        /// <param name="v2">세 번째 벡터</param>
        /// <param name="v3">네 번째 벡터</param>
        public Vector3Tuple4(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">0, 1, 2 또는 3. 각각 V0id, Vertex0, Vertex1, V3를 반환</param>
        public Vector3 this[int key]
        {
            get
            {
                if (key < 0 || key > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key > 1 ? key == 2 ? V2 : V3 : key == 1 ? V1 : V0; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key > 1)
                {
                    if (key == 2)
                    {
                        V2 = value; // 세 번째 벡터 설정
                    }
                    else
                    {
                        V3 = value; // 네 번째 벡터 설정
                    }
                }
                else
                {
                    if (key == 1)
                    {
                        V1 = value; // 두 번째 벡터 설정
                    }
                    else
                    {
                        V0 = value; // 첫 번째 벡터 설정
                    }
                }
            }
        }

        public override string ToString() => $"({V0}, {V1}, {V2}, {V3})"; // 문자열 형식으로 반환
    }

    /// <summary>
    /// 2D 벡터 쌍
    /// </summary>
    public struct Vector2Tuple2
    {
        public Vector2 V0; // 첫 번째 벡터
        public Vector2 V1; // 두 번째 벡터

        /// <summary>
        ///
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        public Vector2Tuple2(Vector2 v0, Vector2 v1)
        {
            V0 = v0;
            V1 = v1;
        }

        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">0 또는 1. 0일 경우 V0id, 1일 경우 V1을 반환</param>
        public Vector2 this[int key]
        {
            get
            {
                if (key < 0 || key > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key == 0 ? V0 : V1; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key == 0)
                {
                    V0 = value; // 첫 번째 벡터 설정
                }
                else
                {
                    V1 = value; // 두 번째 벡터 설정
                }
            }
        }

        public override string ToString() => $"({V0}, {V1})"; // 문자열 형식으로 반환
    }

    /// <summary>
    /// 2D 벡터 삼중을 나타내는 구조체
    /// </summary>
    public struct Vector2Tuple3
    {
        public Vector2 V0; // 첫 번째 벡터
        public Vector2 V1; // 두 번째 벡터
        public Vector2 V2; // 세 번째 벡터

        /// <summary>
        /// 2D 벡터 삼중을 초기화
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        /// <param name="v2">세 번째 벡터</param>
        public Vector2Tuple3(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">0, 1 또는 2. 각각 V0id, Vertex0, V2를 반환</param>
        public Vector2 this[int key]
        {
            get
            {
                if (key < 0 || key > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key == 0 ? V0 : key == 1 ? V1 : V2; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key == 0)
                {
                    V0 = value; // 첫 번째 벡터 설정
                }
                else if (key == 1)
                {
                    V1 = value; // 두 번째 벡터 설정
                }
                else
                {
                    V2 = value; // 세 번째 벡터 설정
                }
            }
        }

        public override string ToString() => $"({V0}, {V1}, {V2})"; // 문자열 형식으로 반환
    }

    /// <summary>
    /// 2D 벡터 사중을 나타내는 구조체
    /// </summary>
    public struct Vector2fTuple4
    {
        public Vector2 V0; // 첫 번째 벡터
        public Vector2 V1; // 두 번째 벡터
        public Vector2 V2; // 세 번째 벡터
        public Vector2 V3; // 네 번째 벡터

        /// <summary>
        /// 2D 벡터 사중을 초기화
        /// </summary>
        /// <param name="v0">첫 번째 벡터</param>
        /// <param name="v1">두 번째 벡터</param>
        /// <param name="v2">세 번째 벡터</param>
        /// <param name="v3">네 번째 벡터</param>
        public Vector2fTuple4(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
        public List<Vector2> ToBuffer()
        {
            return new List<Vector2> { V0, V1, V2, V3 };
        }
        /// <summary>
        /// 인덱서를 통해 벡터에 접근
        /// </summary>
        /// <param name="key">0, 1, 2 또는 3. 각각 V0id, Vertex0, Vertex1, V3를 반환</param>
        public Vector2 this[int key]
        {
            get
            {
                if (key < 0 || key > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                return key > 1 ? key == 2 ? V2 : V3 : key == 1 ? V1 : V0; // 해당 인덱스의 벡터 반환
            }
            set
            {
                if (key < 0 || key > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(key)); // 유효하지 않은 인덱스 예외 처리
                }

                if (key > 1)
                {
                    if (key == 2)
                    {
                        V2 = value; // 세 번째 벡터 설정
                    }
                    else
                    {
                        V3 = value; // 네 번째 벡터 설정
                    }
                }
                else
                {
                    if (key == 1)
                    {
                        V1 = value; // 두 번째 벡터 설정
                    }
                    else
                    {
                        V0 = value; // 첫 번째 벡터 설정
                    }
                }
            }
        }
        public override string ToString() => $"({V0}, {V1}, {V2}, {V3})"; // 문자열 형식으로 반환
    }
}
