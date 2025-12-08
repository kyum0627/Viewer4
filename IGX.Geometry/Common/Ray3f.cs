using IGX.Geometry.Distance;
using OpenTK.Mathematics;
using System;

namespace IGX.Geometry.Common
{
    public struct Ray3f
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray3f(Vector3 origin, Vector3 dir)
        {
            this.origin = origin;
            direction = Vector3.Normalize(dir);
        }
        public Ray3f Transform(Matrix4 matrix)
        {
            // Origin 변환 (쓰기 좋게 Vector4 사용)
            Vector4 newOrigin4 = new Vector4(origin, 1.0f);
            newOrigin4 = newOrigin4.Transform(matrix);

            // Direction 변환
            // 방향에는 Translation이 적용되면 안되므로 w=0
            Vector4 newDir4 = new Vector4(direction, 0.0f);
            newDir4 = newDir4.Transform(matrix);

            // 방향 벡터는 다시 정규화
            Vector3 newDirection = new Vector3(newDir4.X, newDir4.Y, newDir4.Z).Normalized();

            return new Ray3f(
                new Vector3(newOrigin4.X, newOrigin4.Y, newOrigin4.Z),
                newDirection
            );
        }

        public bool IntersectRayPlane(Plane3f plane, out Vector3 hit)
        {
            float denominator = Vector3.Dot(direction, plane.normal);

            // 광선과 평면이 평행한 경우
            if (Math.Abs(denominator) < 1e-05f)
            {
                hit = Vector3.Zero;
                return false;
            }

            Vector3 originToPoint = (plane.normal * plane.distance) - origin;
            float numerator = Vector3.Dot(originToPoint, plane.normal);
            float t = numerator / denominator;

            // 교점이 광선 뒤에 있는 경우 (반직선 고려)
            if (t < 0)
            {
                hit = Vector3.Zero;
                return false;
            }

            hit = origin + (t * direction);
            return true;
        }

        public Vector3 GetPoint(float distance)
        {
            return origin + (direction * distance);
        }
        public Vector3 PointAt(float d)
        {
            return origin + (d * direction);
        }

        public float Project(Vector3 p)
        {
            return Vector3.Dot(p - origin, direction);
        }

        public float DistanceSquared(Vector3 p)
        {
            float t = Vector3.Dot(p - origin, direction);
            Vector3 proj = origin + (t * direction);
            return (proj - p).LengthSquared;
        }
        public float DistanceToLineSegment(Segment3f seg)
        {
            Result3f result = new();
            Vector3 segCenter, segDirection;
            float segExtent;
            segCenter = seg.center;
            segDirection = seg.direction;
            segExtent = seg.extent;

            Vector3 diff = origin - segCenter;
            float a01 = -Vector3.Dot(direction, segDirection);
            float b0 = Vector3.Dot(diff, direction);
            float s0, s1;

            if (Math.Abs(a01) < 1f)
            {
                float det = 1 - (a01 * a01);
                float extDet = segExtent * det;
                float b1 = -Vector3.Dot(diff, segDirection);
                s0 = (a01 * b1) - b0;
                s1 = (a01 * b0) - b1;

                if (s0 >= 0)
                {
                    if (s1 >= -extDet)
                    {
                        if (s1 <= extDet)  // region 0
                        {
                            s0 /= det;
                            s1 /= det;
                        }
                        else  // region 1
                        {
                            s1 = segExtent;
                            s0 = Math.Max(-((a01 * s1) + b0), 0);
                        }
                    }
                    else  // region 5
                    {
                        s1 = -segExtent;
                        s0 = Math.Max(-((a01 * s1) + b0), 0);
                    }
                }
                else
                {
                    if (s1 <= -extDet)  // region 4
                    {
                        s0 = -((-a01 * segExtent) + b0);
                        if (s0 > 0)
                        {
                            s1 = -segExtent;
                        }
                        else
                        {
                            s0 = 0;
                            s1 = -b1;
                            if (s1 < -segExtent)
                            {
                                s1 = -segExtent;
                            }
                            else if (s1 > segExtent)
                            {
                                s1 = segExtent;
                            }
                        }
                    }
                    else if (s1 <= extDet)  // region 3
                    {
                        s0 = 0;
                        s1 = -b1;
                        if (s1 < -segExtent)
                        {
                            s1 = -segExtent;
                        }
                        else if (s1 > segExtent)
                        {
                            s1 = segExtent;
                        }
                    }
                    else  // region 2
                    {
                        s0 = -((a01 * segExtent) + b0);
                        if (s0 > 0)
                        {
                            s1 = segExtent;
                        }
                        else
                        {
                            s0 = 0;
                            s1 = -b1;
                            if (s1 < -segExtent)
                            {
                                s1 = -segExtent;
                            }
                            else if (s1 > segExtent)
                            {
                                s1 = segExtent;
                            }
                        }
                    }
                }
            }
            else
            {
                if (a01 > 0)
                {
                    s1 = -segExtent;
                }
                else
                {
                    s1 = segExtent;
                }

                s0 = Math.Max(-((a01 * s1) + b0), 0);
            }

            result.parameter[0] = s0;
            result.parameter2[0] = s1;
            result.closest[0] = origin + (s0 * direction);
            result.closest[1] = segCenter + (s1 * segDirection);
            diff = result.closest[0] - result.closest[1];
            result.sqrDistance = Vector3.Dot(diff, diff);
            return (float)Math.Sqrt(result.sqrDistance);
        }

        /// <summary>
        /// 광선(Ray)이 구(Sphere)와 교차하는지 확인.
        /// </summary>
        /// <param name="ray">Ray3f 인스턴스.</param>
        /// <param name="sphereCenter">구의 중심점.</param>
        /// <param name="sphereRadius">구의 반지름.</param>
        /// <param name="intersectionPoint">교차점이 발생할 경우, 가장 가까운 교차점의 위치.</param>
        /// <returns>광선이 구와 교차하면 true, 아니면 false를 반환.</returns>
        public bool IntersectsSphere(Vector3 sphereCenter, float sphereRadius, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.Zero;

            // 광선의 원점에서 구의 중심까지의 벡터
            Vector3 m = origin - sphereCenter;

            // 광선 방정식(P(t) = O + t*D)과 구 방정식(|P(t) - C|^2 = R^2)을 결합하여 t에 대한 이차 방정식(at^2 + bt + c = 0)을 유도.
            // a = D.D = 1 (광선 방향 벡터가 정규화되어 있으므로)
            // b = 2*(D.m)
            // c = (m.m) - R^2

            float b = Vector3.Dot(m, direction);
            float c = Vector3.Dot(m, m) - (sphereRadius * sphereRadius);

            // c > 0 이고 b > 0 이면, 광선의 원점이 구 밖에 있고 구와 멀어지는 방향. 교차 불가.
            if (c > 0.0f && b > 0.0f)
            {
                return false;
            }
            float discriminant = (b * b) - c;
            if (discriminant < 0.0f)
            {// 판별식이 음수이면 실근이 없으므로 광선이 구를 지나침
                return false;
            }

            float t = -b - MathF.Sqrt(discriminant);

            // t가 음수이면 교차점이 광선의 원점 뒤에 존재. 즉, 광선의 원점이 구 내부에 있다는 의미.
            // 이 경우 t를 0으로 설정하여 광선 시작점을 교차점으로 간주.
            if (t < 0.0f)
            {
                t = 0.0f;
            }
            intersectionPoint = origin + (t * direction); // 월드 좌표계에서 실제 교차점의 위치를 계산.
            return true;
        }
        public static bool operator ==(Ray3f left, Ray3f right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray3f left, Ray3f right)
        {
            return !left.Equals(right);
        }

        public static bool Equals(Ray3f left, Ray3f right)
        {
            return left.direction == right.direction && left.origin == right.origin;
        }

        public readonly bool Equals(Ray3f c) => direction.Equals(c.direction) && origin.Equals(c.origin);

#pragma warning disable CS8765 // 매개 변수 형식의 null 허용 여부가 재정의된 멤버와 일치하지 않음(null 허용 여부 특성 때문일 수 있음)
        public override readonly bool Equals(object obj) => obj is Ray3f c && Equals(c);
#pragma warning restore CS8765 // 매개 변수 형식의 null 허용 여부가 재정의된 멤버와 일치하지 않음(null 허용 여부 특성 때문일 수 있음)

        public override readonly int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ origin.GetHashCode();
                hash = (hash * 16777619) ^ direction.GetHashCode();
                return hash;
            }
        }
    }
}
