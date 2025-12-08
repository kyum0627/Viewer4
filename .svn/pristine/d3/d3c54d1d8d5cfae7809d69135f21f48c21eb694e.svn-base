using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public static class Vector3Extensions
    {
        public static Matrix4 RotationMatrixUsingBasis(this Vector3 axis, float angleRad = 0)
        {
            Vector3 n = axis.Normalized(); // 입력 axis가 단위 벡터가 아닐 경우를 대비하여 정규화
            Vector3 t;
            if (Math.Abs(n.X) < Math.Abs(n.Y))
            {
                if (Math.Abs(n.X) < Math.Abs(n.Z))
                {
                    t = new Vector3(1, 0, 0); // X 성분이 가장 작음
                }
                else
                {
                    t = new Vector3(0, 0, 1); // Z 성분이 가장 작음
                }
            }
            else
            {
                if (Math.Abs(n.Y) < Math.Abs(n.Z))
                {
                    t = new Vector3(0, 1, 0); // Y 성분이 가장 작음
                }
                else
                {
                    t = new Vector3(0, 0, 1); // Z 성분이 가장 작음
                }
            }
            Vector3 u = Vector3.Cross(n, t).Normalized(); // n과 t의 외적, 그리고 정규화
            Vector3 v = Vector3.Cross(n, u); // u, n이 직교 단위이므로 v도 직교 단위
            float cosTheta = (float)Math.Cos(angleRad); // cnrkwjrdls 
            float sinTheta = (float)Math.Sin(angleRad);

            Vector3 uPrime = (u * cosTheta) + (v * sinTheta);
            Vector3 vPrime = (u * -sinTheta) + (v * cosTheta);

            Matrix4 rotationMatrix = new();

            rotationMatrix.Column0 = new Vector4(uPrime);
            rotationMatrix.Column1 = new Vector4(vPrime);
            rotationMatrix.Column2 = new Vector4(n);
            rotationMatrix.Column3 = Vector4.UnitW;

            return rotationMatrix;
        }
        public static Vector3 ToVector3(this Vector<double> vec)
        {
            return vec.Count != 3
                ? throw new ArgumentException("Vector must have exactly 3 elements to convert to Vector3.")
                : new Vector3((float)vec[0], (float)vec[1], (float)vec[2]);
        }
        public static string ToString(this Vector3 v)
        {
            return string.Format("{0,14:F2} {1,14:F2} {2,14:F2}", v.X, v.Y, v.Z);
        }
        public static Vector3 Cross(this Vector3 v1, Vector3 v2)
        {
            float z = (v1.X * v2.Y) - (v1.Y * v2.X);
            float x = (v1.Y * v2.Z) - (v1.Z * v2.Y);
            float y = (v1.Z * v2.X) - (v1.X * v2.Z);
            return new Vector3(x, y, z);
        }
        public static int CompareTo(this Vector3 a, Vector3 b)
        {
            // 먼저 x 성분을 기준으로 비교
            int compareX = a.X.CompareTo(b.X);

            // x 성분이 같으면 y 성분을 기준으로 비교
            if (compareX == 0)
            {
                int compareY = a.Y.CompareTo(b.Y);
                return compareY == 0 ? a.Z.CompareTo(b.Z) : compareY;
            }
            return compareX;
        }
        public static Vector3 UnitCross(this Vector3 a, Vector3 b)
        {
            Vector3 crossProduct = a.Cross(b);

            // 벡터가 0벡터인지 확인하고, 0벡터라면 정규화하지 않음
            if (crossProduct.LengthSquared < 1e-6f)  // 매우 작은 값으로 비교
            {
                return Vector3.Zero; // 0벡터인 경우 그대로 반환
            }
            return crossProduct.Normalized();  // 0벡터가 아니라면 정규화
        }

        /// <summary>
        /// 두 벡터의 각도
        /// </summary>
        /// <param name="B"></param>
        /// <returns>this 벡터와 A가 이루는 각도, radian</returns>
        public static float AngleRad(this Vector3 var, Vector3 B)
        {
            if (var.LengthSquared == 0 || B.LengthSquared == 0)
            {// 벡터 길이가 0인 경우 예외 처리
                throw new ArgumentException("벡터의 길이가 0일 수 없음");
            }

            // 내적 계산 및 클램핑
            float fDot = Math.Clamp(Vector3.Dot(var, B), -1f, 1f);

            // 아크코사인 계산 후 각도 반환
            return (float)Math.Acos(fDot);
        }

        public static float AngleDeg(this Vector3 a, Vector3 b)
        {
            return a.AngleRad(b) * MathUtil.Rad2Deg;
        }

        public static Vector3 Transform(this Vector3 vector, Matrix4 matrix)
        {
            if (matrix == Matrix4.Zero)
            {
                throw new ArgumentNullException(nameof(matrix),
                    "Vector3Extnsion Class Transformg함수에 사용될 Model 값이 비어있음.");
            }
            Vector4 v4 = new(vector, 1f);
            v4 = v4 * matrix;
            return v4.Xyz;
        }

        public static float DistanceTo(this Vector3 a, Vector3 b)
        {
            return (float)Math.Sqrt(a.DistanceSquared(b));
        }

        public static Vector2 Xy(this Vector3 a)
        {
            return new Vector2(a.X, a.Y);
        }

        public static Vector2 Yz(this Vector3 a)
        {
            return new Vector2(a.Y, a.Z);
        }

        public static Vector2 Zx(this Vector3 a)
        {
            return new Vector2(a.Z, a.X);
        }

        public static float DotAfterCross(this Vector3 a, Vector3 b)
        {
            Vector3 cross = a.Cross(b);
            return Vector3.Dot(a, cross);
        }

        /// <summary>
        /// 3차원 평면상의 도형의 norID 벡터 요소중 가장 큰 값의 좌표를 제외하고 Xy, Yz, Zx 면에 투영
        /// 즉, i 요소(normal의 x 값)가 가장 크면, X값을 무시하여 Yz에 투영된 도형을 얻어내기 위함
        /// </summary>
        /// <returns>제외시킬 좌표축, 0: x, 1: y, 2:z 무시</returns>
        public static int MaxLengthCoordinate(this Vector3 var)
        {
            float dx = Math.Abs(var.X);
            float dy = Math.Abs(var.Y);
            float dz = Math.Abs(var.Z);

            return dx > dy ? dx > dz ? 0 : 2 : dy > dz ? 1 : 2;
        }

        public static Vector2 Get2DVector(this Vector3 var, int ignore)
        {
            switch (ignore)
            {
                case 0:
                    return var.Yz();
                case 1:
                    return var.Zx();
                case 2:
                    return var.Xy();
            }
            return new();
        }

        public static float SignedArea(this Vector3 normal, List<Vector3> points)
        {
            int n = points.Count;
            if (n < 3)
            {
                return 0; // 면적이 0
            }

            Vector3 aa = Vector3.Zero;
            Vector3 centroidContribution = Vector3.Zero;

            Parallel.For(0, n, i =>
            {
                Vector3 crossProduct = points[i].Cross(points[(i + 1) % n]);
                aa += crossProduct;
            });

            // 최종 면적
            return Vector3.Dot(aa, normal) * 0.5f;
        }

        //public static float SignedArea(this Vector3 Normal, List<Vector3> ControlPoints)
        //{
        //    int nID = ControlPoints.Continuity;
        //    if (nID < 3)
        //        return 0;

        //    Vector3 aa = Vector3.Zero;

        //    for (int i = 0; i < nID; i++)
        //    {
        //        aa += ControlPoints[i].Cross(ControlPoints[(i + 1) % nID]);
        //    }

        //    return 0.5f * Vector3.Dot(aa, Normal);
        //}

        /// <summary>
        /// 각 벡터 요소의 절대 값인 벡터를 반환
        /// </summary>
        /// <returns></returns>
        public static Vector3 Abs(ref this Vector3 value)
        {
            if (value.X < 0)
            {
                value.X *= -1;
            }

            if (value.Y < 0)
            {
                value.Y *= -1;
            }

            if (value.Z < 0)
            {
                value.Z *= -1;
            }

            return value;
        }

        public static float DistanceBetween(this Vector3 A, Vector3 B)
        {
            Vector3 df = A - B;
            return (float)Math.Sqrt(df.Dot(df));
        }

        public static float DistanceSquared(this Vector3 A, Vector3 B)
        {
            Vector3 df = A - B;
            return df.Dot(df);
        }

        public static float Dot(this Vector3 A, Vector3 B)
        {
            return (A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z);
        }

        /// <summary>
        /// triple scalar ptoduct, v1과 v2의 cross product 계산 후 v1와 dot 연산
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float DotAfterCross(this Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return v0.Dot(v1.Cross(v2));
        }

        public static bool EpsilonEqual(this Vector3 A, Vector3 B, float eps = MathUtil.Epsilon)
        {
            return Math.Abs(A.X - B.X) < eps && Math.Abs(A.Y - B.Y) < eps && Math.Abs(A.Z - B.Z) < eps;
        }

        public static void Flip(ref this Vector3 var)
        {
            var.Y = -var.Y;
        }

        public static Quaternion QuaternionFromAxisAngle(this Vector3 axis, float angle)
        {
            Vector3 v = axis;
            v = Vector3.Normalize(v); // convert to unit vector
            float sine = (float)Math.Sin(angle); // RotationAngles is radian
            return new Quaternion((float)Math.Cos(angle), v.X * sine, v.Y * sine, v.Z * sine);
        }

        /// <summary>
        /// https://ko.wikipedia.org/wiki/%EB%A7%A8%ED%95%B4%ED%8A%BC_%EA%B1%B0%EB%A6%AC
        /// </summary>
        public static float ManhattanDistance(this Vector3 v)
        {
            return Math.Abs(v.X) + Math.Abs(v.Y) + Math.Abs(v.Z);
        }
        /// <summary>
        /// 하나의 벡터가 주어졌을 때 정규 직교 벡터 계산
        /// Duff et all method
        /// https://graphics.pixar.com/library/OrthonormalB/paper.pdf
        /// </summary>
        public static Vector3 MakeUVnormalsFromW(this Vector3 W, out Vector3 U, out Vector3 V)
        {
            if (W.Z < 0.0)
            {
                float a = 1.0f / (1.0f - W.Z);
                float b = W.X * W.Y * a;

                U.X = 1.0f - (W.X * W.X * a);
                U.Y = -b;
                U.Z = W.X;
                V.X = b;
                V.Y = (W.Y * W.Y * a) - 1.0f;
                V.Z = -W.Y;
            }
            else
            {
                float a = 1.0f / (1.0f + W.Z);
                float b = -W.X * W.Y * a;

                U.X = 1.0f - (W.X * W.X * a);
                U.Y = b;
                U.Z = -W.X;
                V.X = b;
                V.Y = 1.0f - (W.Y * W.Y * a);
                V.Z = -W.Y;
            }

            U.Normalize();
            V.Normalize();
            return W;
        }

        public static float[] ToBuffer(this Vector3 var)
        {
            return new float[3] { var.X, var.Y, var.Z };
        }

        public static string ToString(this Vector3 var, string fmt)
        {
            return string.Format("{0} {1} {2}", var.X.ToString(fmt), var.Y.ToString(fmt), var.Z.ToString(fmt));
        }

        public static bool GreaterThan(this Vector3 left, Vector3 right)
        {
            if (left.X > right.X)
            {
                return true;
            }
            else if (left.Y > right.Y)
            {
                return true;
            }
            else
            {
                return left.Z > right.Y;
            }
        }

        public static bool LessThan(this Vector3 left, Vector3 right)
        {
            if (left.X < right.X)
            {
                return true;
            }
            else if (left.Y < right.Y)
            {
                return true;
            }
            else
            {
                return left.Z < right.Y;
            }
        }
        public static Plane3f GetPlaneFromShearAndPoint(this Vector3 point, float shearX, float shearY)
        { // 평면 암시적 형식(법선 벡터와 dc)을 사용하여 ImplicitFormPlane 객체를 생성하여 반환
            Quaternion rX = Quaternion.FromAxisAngle(Vector3.UnitY, -shearX);
            Quaternion rY = Quaternion.FromAxisAngle(Vector3.UnitX, shearY);
            Quaternion ro = rX * rY;
            Vector3 n = Vector3.Transform(Vector3.UnitZ, ro);
            float h = -Vector3.Dot(n, point);
            return new Plane3f(n, h);
        }
        // 쿼터니언을 생성하는 메서드
        public static Quaternion CreateFromAxisAngle(this Vector3 axis, float angle)
        {
            // 축 벡터를 정규화
            Vector3 normalizedAxis = Vector3.Normalize(axis);

            // 각도를 라디안으로 변환
            float halfAngle = angle / 2.0f;
            float s = (float)Math.Sin(halfAngle);
            float c = (float)Math.Cos(halfAngle);

            // 쿼터니언 계산
            return new Quaternion(normalizedAxis.X * s,
                    normalizedAxis.Y * s,
                    normalizedAxis.Z * s,
                    c);
        }
        public static Vector2 ProjectToPlane(this Vector3 point, Plane3f plane)
        {
            // 평면에 투영된 점을 2D 평면으로 변환하는 방법을 구현
            Vector3 projectedPoint = plane.Project(point);
            return new Vector2(projectedPoint.X, projectedPoint.Y); // X, Y만 사용하여 2D로 변환
        }
        public static Vector3 ProjectToPlane3(this Vector3 point, Plane3f plane)
        {
            // 평면에 투영된 점을 2D 평면으로 변환하는 방법을 구현
            Vector3 projectedPoint = plane.Project(point);
            return new Vector3(projectedPoint.X, projectedPoint.Y, projectedPoint.Z) * plane.normal; // X, Y만 사용하여 2D로 변환
        }
        /// <summary>
        /// 3차원 평면상의 도형을, 해당 도형의 Normal 벡터 요소중 가장 큰 값의 좌표를 제외시킴으로써  Xy, Yz, Zx 면에 투영
        /// 즉, i 요소(normal의 x 값)가 가장 크면, X값을 무시하여 Yz에 투영된 도형을 얻어내기 위함
        /// </summary>
        /// <returns>제외시킬 좌표축, 0: x, 1: y, 2:z 무시</returns>
        public static int IgnoredCoordinate(this Vector3 var)
        {
            float dx = Math.Abs(var.X);
            float dy = Math.Abs(var.Y);
            float dz = Math.Abs(var.Z);

            return (dx > dy) ? (dx > dz ? 0 : 2) : (dy > dz ? 1 : 2);
        }
        public static bool Equals(this Vector3 a, Vector3 b, float tolerance)
        {
            return Math.Abs(a.X - b.X) <= tolerance &&
                   Math.Abs(a.Y - b.Y) <= tolerance &&
                   Math.Abs(a.Z - b.Z) <= tolerance;
        }
        public static float[] ToArray(this Vector3 v)
        {
            return new float[3] { v.X, v.Y, v.Z };
        }
    }
}
