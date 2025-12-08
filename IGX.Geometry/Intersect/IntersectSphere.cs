using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

/// =======================================================================================================================
/// 다양한 기하 형상간의 교차점, 선 면을 계산하기 위해 필요한 가장 기본적인 utility (직선과 삼각형 및 구)
/// =======================================================================================================================
///
/// <summary>
/// 참고: https://www.geometrictools.com/Samples/Intersection.html
/// 상기 site에서 하기와 같은 샘플 애플리케이션들(CPP, C#, PS)을 볼 수 있음
///	AllPairsTriangles           :	삼각 메쉬로 구성된 두 모델간의 교차 계산
///	        https://www.geometrictools.com/Samples/Intersection.html#AllPairsTriangles
/// IntersectBoxCone            :	직육면체와 원뿔간의 교차계산
///         https://www.geometrictools.com/Samples/Intersection.html#IntersectBoxCone
///	IntersectBoxCylinder        :	특정 축 혹은 world 좌표계에 평행한 직육면체와 원기둥의 교차계산
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectBoxCylinder
///	IntersectBoxSphere          :	특정 축 혹은 world 좌표계에 평행한 직육면체와 구의 교차계산
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectBoxSphere
///	IntersectConvexPolyhedra    :	두 볼록 다각형간 교차계산
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectConvexPolyhedra
///	IntersectInfiniteCylinders  :	두 개의 무한한 길이의 원기둥간 교차계산
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectInfiniteCylinders
///	IntersectSphereCone         :	구와 원뿔간 교차 계산
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectSphereCone
///	IntersectTriangleBox        :	삼각 메쉬와 직육면체간 교차하는지 검토
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectTriangleBox
///	IntersectTriangles2D        :	평면상의 두 삼각형간 교차여부 검토
///	        https://www.geometrictools.com/Samples/Intersection.html#IntersectTriangles2D
///	MovingCircleRectangle       :	움직이고 있는 원과 사각형간에 처음 만나는 점
///	        https://www.geometrictools.com/Samples/Intersection.html#MovingCircleRectangle
///	MovingSphereBox             :	움직이고 있는 구와 박스간 처음 만나는 점
///	        https://www.geometrictools.com/Samples/Intersection.html#MovingSphereBox
///	MovingSphereTriangle        :	움직이고 있는 구와 삼각형간 처음 만나는 점
///	        https://www.geometrictools.com/Samples/Intersection.html#MovingSphereTriangle
/// </summary>
///
namespace IGX.Geometry.Intersect
{
    public static class IntersectSphere
    {
        /// <summary>
        /// 라인과 구의 교차여부 계산
        /// https://www.geometrictools.com/GTE/Mathematics/IntrLine3Sphere3.h
        /// </summary>
        public static bool LineSphereTest(ref Vector3 lineOrigin, ref Vector3 lineDirection, ref Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 diff = lineOrigin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(lineDirection, diff);

            float discr = (a1 * a1) - a0;
            return discr >= 0;
        }

        public static bool LineSphereTest(ref Line3f line, ref Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 diff = line.origin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(line.direction, diff);

            float discr = (a1 * a1) - a0;
            return discr >= 0;
        }

        /// <summary>
        /// 구와 ray간 교차 여부 계산
        /// </summary>
        ///
        public static bool LineSphere(ref Vector3 lineOrigin, ref Vector3 lineDirection, ref Vector3 sphereCenter, float sphereRadius, ref IntersectionResult2 result)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Sphere3.h

            Vector3 diff = lineOrigin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(lineDirection, diff);

            float discr = (a1 * a1) - a0;
            if (discr > 0)
            {
                result.intersects = true;
                result.quantity = 2;
                float root = (float)Math.Sqrt(discr);
                result.parameter1.a = -a1 - root;
                result.parameter1.b = -a1 + root;
            }
            else if (discr < 0)
            {
                result.intersects = false;
                result.quantity = 0;
            }
            else
            {
                result.intersects = true;
                result.quantity = 1;
                result.parameter1.a = -a1;
                result.parameter1.b = -a1;
            }
            return result.intersects;
        }

        public static bool LineSphere(ref Line3f line, ref Vector3 sphereCenter, float sphereRadius, ref IntersectionResult2 result)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Sphere3.h

            Vector3 diff = line.origin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(line.direction, diff);

            float discr = (a1 * a1) - a0;
            if (discr > 0)
            {
                result.intersects = true;
                result.quantity = 2;
                float root = (float)Math.Sqrt(discr);
                result.parameter1.a = -a1 - root;
                result.parameter1.b = -a1 + root;
            }
            else if (discr < 0)
            {
                result.intersects = false;
                result.quantity = 0;
            }
            else
            {
                result.intersects = true;
                result.quantity = 1;
                result.parameter1.a = -a1;
                result.parameter1.b = -a1;
            }
            return result.intersects;
        }

        public static bool LineSphere(ref Ray3f ray, ref Vector3 sphereCenter, float sphereRadius, ref IntersectionResult2 result)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Sphere3.h

            Vector3 diff = ray.origin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(ray.direction, diff);

            float discr = (a1 * a1) - a0;
            if (discr > 0)
            {
                result.intersects = true;
                result.quantity = 2;
                float root = (float)Math.Sqrt(discr);
                result.parameter1.a = -a1 - root;
                result.parameter1.b = -a1 + root;
            }
            else if (discr < 0)
            {
                result.intersects = false;
                result.quantity = 0;
            }
            else
            {
                result.intersects = true;
                result.quantity = 1;
                result.parameter1.a = -a1;
                result.parameter1.b = -a1;
            }
            return result.intersects;
        }

        /// <summary>
        /// 직선과 구 사이 교차 계산
        /// </summary>
        /// <param name="lineOrigin"></param>
        /// <param name="lineDirection"></param>
        /// <param name="sphereCenter"></param>
        /// <param name="sphereRadius"></param>
        /// <returns></returns>
        public static IntersectionResult2 LineSphere(ref Vector3 lineOrigin, ref Vector3 lineDirection, ref Vector3 sphereCenter, float sphereRadius)
        {
            IntersectionResult2 result = new();
            LineSphere(ref lineOrigin, ref lineDirection, ref sphereCenter, sphereRadius, ref result);
            return result;
        }

        public static IntersectionResult2 LineSphere(ref Line3f line, ref Vector3 sphereCenter, float sphereRadius)
        {
            IntersectionResult2 result = new();
            LineSphere(ref line, ref sphereCenter, sphereRadius, ref result);
            return result;
        }

        /// <summary>
        /// 무한하게 연장되는 직선(광선)과 구 사이의 교차 여부 검토
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <param name="sphereCenter"></param>
        /// <param name="sphereRadius"></param>
        /// <returns></returns>
        public static bool RaySphereTest(ref Vector3 rayOrigin, ref Vector3 rayDirection, ref Vector3 sphereCenter, float sphereRadius)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

            Vector3 diff = rayOrigin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            if (a0 <= 0)
            {
                return true;  // P is inside the sphere.
            }
            // else: P is outside the sphere
            float a1 = Vector3.Dot(rayDirection, diff);
            if (a1 >= 0)
            {
                return false;
            }

            // Intersection occurs when Q(t) has float roots.
            float discr = (a1 * a1) - a0;
            return discr >= 0;
        }

        public static bool RaySphereTest(ref Ray3f ray, ref Vector3 sphereCenter, float sphereRadius)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

            Vector3 diff = ray.origin - sphereCenter;
            float a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
            if (a0 <= 0)
            {
                return true;  // P is inside the sphere.
            }
            // else: P is outside the sphere
            float a1 = Vector3.Dot(ray.direction, diff);
            if (a1 >= 0)
            {
                return false;
            }

            // Intersection occurs when Q(t) has float roots.
            float discr = (a1 * a1) - a0;
            return discr >= 0;
        }

        /// <summary>
        /// 직선(광선)과 구 사이의 교차 여부를 검토하고 계산 결과를 리턴
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <param name="sphereCenter"></param>
        /// <param name="sphereRadius"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool RaySphere(ref Vector3 rayOrigin, ref Vector3 rayDirection, ref Vector3 sphereCenter, float sphereRadius, ref IntersectionResult2 result)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

            LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
            if (result.intersects)
            {
                if (result.parameter1.b < 0)
                {
                    result.intersects = false;
                    result.quantity = 0;
                }
                else if (result.parameter1.a < 0)
                {
                    result.quantity--;
                    result.parameter1.a = result.parameter1.b;
                }
            }
            return result.intersects;
        }

        public static bool RaySphere(ref Ray3f ray, ref Vector3 sphereCenter, float sphereRadius, ref IntersectionResult2 result)
        {
            // https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

            LineSphere(ref ray, ref sphereCenter, sphereRadius, ref result);
            if (result.intersects)
            {
                if (result.parameter1.b < 0)
                {
                    result.intersects = false;
                    result.quantity = 0;
                }
                else if (result.parameter1.a < 0)
                {
                    result.quantity--;
                    result.parameter1.a = result.parameter1.b;
                }
            }
            return result.intersects;
        }

        /// <summary>
        /// 직선(광선)과 구 사이의 교차 여부를 검토
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <param name="sphereCenter"></param>
        /// <param name="sphereRadius"></param>
        /// <returns></returns>
        public static IntersectionResult2 RaySphere(ref Vector3 rayOrigin, ref Vector3 rayDirection, ref Vector3 sphereCenter, float sphereRadius)
        {
            IntersectionResult2 result = new();
            LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
            return result;
        }

        public static IntersectionResult2 RaySphere(ref Ray3f ray, ref Vector3 sphereCenter, float sphereRadius)
        {
            IntersectionResult2 result = new();
            LineSphere(ref ray, ref sphereCenter, sphereRadius, ref result);
            return result;
        }
    }
}
