using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    class ClipSeg2ByPoly2
    {
        public Vector2[] ClipSeg2ByPolygon2(
            out bool starts_outside_polygon,
            Segment2f seg,
            Polygon2 poly)
        {
            Vector2 p0 = seg.P0;
            Vector2 p1 = seg.P1;
            List<Vector2> points = poly.points;

            // 교차점 및 교차점의 parameter 값을 저장할 변수
            List<Vector2> intersections = new();
            List<float> t_values = new();

            // 세그먼트 시작점 추가
            intersections.Add(p0);
            t_values.Add(0f);
            starts_outside_polygon = !PointIsInPolygon(p0, points);

            // 폴리곤 에지별 test
            for (int i1 = 0; i1 < points.Count; i1++)
            {
                int i2 = (i1 + 1) % points.Count; // seg = { ControlPoints[I1], ControlPoints[I2] }

                // 입력된 세그먼트와  polygon을 구성하는 segment간 교차 여부 조사

                FindIntersection(p0, p1,
                    points[i1], points[i2],
                    out bool lines_intersect, out bool segments_intersect,
                    out Vector2 intersection, out Vector2 close_p1, out Vector2 close_p2,
                    out float t1, out float t2);

                if (segments_intersect)
                {   // segment가 edge와 교차하면, 
                    intersections.Add(intersection);
                    t_values.Add(t1);
                }
            }

            // segment의 끝 점 추가
            intersections.Add(p1);
            t_values.Add(1f);

            // 교차점의 리스트를 parameter 값에 따라 정렬
            Vector2[] intersections_array = intersections.ToArray();
            float[] t_array = t_values.ToArray();
            Array.Sort(t_array, intersections_array);

            // 정렬된 리스트를 리턴
            return intersections_array;
        }

        /// <summary>
        /// 정점 p0ID, p1로 구성된 세그먼트와 p3, p4로 구성된 segment간 교차 계산
        /// line_intersect = true, segments_intersect = false, 연장선간 교차
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="lines_intersect">true = 무한직선간 교차</param>
        /// <param name="segments_intersect">true = 세그먼트간 교차</param>
        /// <param name="intersection">교차점</param>
        /// <param name="close_p1">주어진 점 p0ID, p2로 구성된 default_nsegs 위의 가장가까운 점</param>
        /// <param name="close_p2">주어진 점 p3, p4로 구성된 default_nsegs 위의 가장가까운 점/param>
        /// <param name="t1">close_p1의 parameter</param>
        /// <param name="t2">close_p1의 parameter</param>
        private void FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            out bool lines_intersect, out bool segments_intersect,
            out Vector2 intersection, out Vector2 close_p1, out Vector2 close_p2,
            out float t1, out float t2)
        {
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            float denominator = (dy12 * dx34) - (dx12 * dy34);
            t1 = (((p1.X - p3.X) * dy34) + ((p3.Y - p1.Y) * dx34)) / denominator;
            if (float.IsInfinity(t1))
            {
                // 두 세그먼트는 평행(수치계산 오차내에서 평행).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Vector2(float.NaN, float.NaN);
                close_p1 = new Vector2(float.NaN, float.NaN);
                close_p2 = new Vector2(float.NaN, float.NaN);
                t2 = float.PositiveInfinity;
                return;
            }
            lines_intersect = true;
            t2 = (((p3.X - p1.X) * dy12) + ((p1.Y - p3.Y) * dx12)) / -denominator;

            // 교차점 계산
            intersection = new Vector2(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
            // t1, t2 의 값이 0 에서 1 사이이면 교차, 벗어나면 [0, 1] 로 잘라냄
            segments_intersect = t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new Vector2(p1.X + (dx12 * t1), p1.Y + (dy12 * t1));
            close_p2 = new Vector2(p3.X + (dx34 * t2), p3.Y + (dy34 * t2));
        }

        /// <summary>
        /// p가 polygon 내부에 있으면 true
        /// </summary>
        /// <param name="p">test할 점</param>
        /// <param name="polygon_points">polygon을 구성하는 점</param>
        /// <returns></returns>
        private bool PointIsInPolygon(Vector2 p, List<Vector2> polygon_points)
        {
            // 주어진 점 p와 polygon의 시작 및 끝 점간 RotationAngles 계산
            int max_point = polygon_points.Count - 1;
            float total_angle = GetAngle(
                polygon_points[max_point].X, polygon_points[max_point].Y,
                p.X, p.Y,
                polygon_points[0].X, polygon_points[0].Y);

            // polygon을 구성하는 나머지 edge들과 각도 계산 누적
            for (int i = 0; i < max_point; i++)
            {
                total_angle += GetAngle(
                    polygon_points[i].X, polygon_points[i].Y,
                    p.X, p.Y,
                    polygon_points[i + 1].X, polygon_points[i + 1].Y);
            }
            // 각도의 누적이 2 * PI or -2 * PI 이면 내부 0에 가까우면 외부
            return Math.Abs(total_angle) > 0.000001;
        }

        /// <summary>
        /// 점 A, B, C가 이루는 각도 계산 [ -PI, PI ]
        /// </summary>
        /// <param name="Ax"></param>
        /// <param name="Ay"></param>
        /// <param name="Bx"></param>
        /// <param name="By"></param>
        /// <param name="Cx"></param>
        /// <param name="Cy"></param>
        /// <returns></returns>
        private static float GetAngle(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);
            float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the RotationAngles.
            return (float)Math.Atan2(cross_product, dot_product);
        }

        private static float GetAngle(Vector2 A, Vector2 B, Vector2 C)
        {
            float dot_product = DotProduct(A.X, A.Y, B.X, B.Y, C.X, C.Y);
            float cross_product = CrossProductLength(A.X, A.Y, B.X, B.Y, C.X, C.Y);

            // Calculate the RotationAngles.
            return (float)Math.Atan2(cross_product, dot_product);
        }

        private static float DotProduct(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            return (BAx * BCx) + (BAy * BCy);
        }

        private static float CrossProductLength(float Ax, float Ay,
            float Bx, float By, float Cx, float Cy)
        {
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            return (BAx * BCy) - (BAy * BCx);
        }
    }
}