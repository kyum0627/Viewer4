using ClipperLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGX.Geometry.DataStructure
{
    public static class PolygonClipper
    {
        private const float Epsilon = 1e-6f;
        private const long Scale = 10000;

        public static List<List<Vector3>> ClipPolygons(List<List<Vector3>> poly1, List<List<Vector3>> poly2, bool bExcludeBoundary, float offsetAmount)
        {
            var normal1 = CalculatePolygonNormal(poly1[0]);
            var normal2 = CalculatePolygonNormal(poly2[0]);

            Vector3 commonNormal;
            Vector3 commonOrigin;

            if (!GetCommonPlaneInfo(poly1, poly2, normal1, normal2, out commonNormal, out commonOrigin))
            {
                return new List<List<Vector3>>();
            }

            var basis = GetPlaneBasis(commonNormal);
            var poly1_2d = To2D(poly1, commonOrigin, basis.U, basis.V);
            var poly2_2d = To2D(poly2, commonOrigin, basis.U, basis.V);

            var subject = new List<List<IntPoint>>();
            var clip = new List<List<IntPoint>>();

            foreach (var poly in poly1_2d) subject.Add(ToClipperIntPoints(poly, Scale));
            foreach (var poly in poly2_2d) clip.Add(ToClipperIntPoints(poly, Scale));

            // 오프셋 적용
            var offsettedSubject = OffsetPolygons(subject, offsetAmount * Scale);

            var solution = new List<List<IntPoint>>();
            var clipper = new Clipper();
            clipper.AddPaths(offsettedSubject, PolyType.ptSubject, true);
            clipper.AddPaths(clip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            // 클리핑 결과 복원 (Reverse Offset)
            var restoredSolution = OffsetPolygons(solution, -offsetAmount * Scale);

            // 경계 제외 옵션 적용
            if (bExcludeBoundary)
            {
                ExcludeBoundarySegments(ref restoredSolution, clip);
            }

            List<List<Vector3>> result = new List<List<Vector3>>();
            if (restoredSolution.Count > 0)
            {
                foreach (var polygon in restoredSolution)
                {
                    var poly3d = new List<Vector3>();
                    foreach (var point in polygon)
                    {
                        var p2d = new Vector2((float)point.X / Scale, (float)point.Y / Scale);
                        poly3d.Add(To3D(p2d, commonOrigin, basis.U, basis.V));
                    }
                    result.Add(poly3d);
                }
            }
            return result;
        }

        public static List<List<Vector3>> ClipLineString(List<Vector3> line, List<List<Vector3>> polygon, bool bExcludeBoundary)
        {
            var polygonNormal = CalculatePolygonNormal(polygon[0]);
            Vector3 commonNormal;
            Vector3 commonOrigin;

            var allPoints = new List<List<Vector3>>(polygon)
            {
                line
            };

            if (!GetCommonPlaneInfo(allPoints, polygonNormal, out commonNormal, out commonOrigin))
            {
                return new List<List<Vector3>>();
            }

            var basis = GetPlaneBasis(commonNormal);
            var line_2d = To2D(new List<List<Vector3>> { line }, commonOrigin, basis.U, basis.V);
            var polygon_2d = To2D(polygon, commonOrigin, basis.U, basis.V);

            var subject = ToClipperIntPoints(line_2d[0], Scale);
            var clip = new List<List<IntPoint>>();
            foreach (var poly in polygon_2d)
            {
                clip.Add(ToClipperIntPoints(poly, Scale));
            }

            var solution = new List<List<IntPoint>>();
            var clipper = new Clipper();
            clipper.AddPath(subject, PolyType.ptSubject, true);
            clipper.AddPaths(clip, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (bExcludeBoundary)
            {
                ExcludeBoundarySegments(ref solution, clip);
            }

            List<List<Vector3>> result = new List<List<Vector3>>();
            if (solution.Count > 0)
            {
                foreach (var solutionPath in solution)
                {
                    var line3d = new List<Vector3>();
                    foreach (var point in solutionPath)
                    {
                        var p2d = new Vector2((float)point.X / Scale, (float)point.Y / Scale);
                        line3d.Add(To3D(p2d, commonOrigin, basis.U, basis.V));
                    }
                    result.Add(line3d);
                }
            }
            return result;
        }

        /// <summary>
        /// 클리핑 결과에서 클립 폴리곤의 경계와 일치하는 선분들을 제거. (ref 매개변수 사용)
        /// </summary>
        private static void ExcludeBoundarySegments(ref List<List<IntPoint>> solution, List<List<IntPoint>> clip)
        {
            var filteredSolution = new List<List<IntPoint>>();
            var clipEdges2D = new List<(IntPoint, IntPoint)>();
            foreach (var poly in clip)
            {
                for (int i = 0; i < poly.Count; i++)
                {
                    clipEdges2D.Add((poly[i], poly[(i + 1) % poly.Count]));
                }
            }

            foreach (var solutionPath in solution)
            {
                var newPath = new List<IntPoint>();
                for (int i = 0; i < solutionPath.Count; i++)
                {
                    var p1 = solutionPath[i];
                    var p2 = solutionPath[(i + 1) % solutionPath.Count];

                    if (IsOnClipBoundary(p1, p2, clipEdges2D))
                    {
                        continue;
                    }

                    newPath.Add(p1);
                    newPath.Add(p2);
                }

                if (newPath.Count > 0)
                {
                    filteredSolution.Add(newPath.Distinct().ToList());
                }
            }
            solution = filteredSolution;
        }

        private static bool IsOnClipBoundary(IntPoint p1, IntPoint p2, List<(IntPoint start, IntPoint end)> clipEdges)
        {
            foreach (var edge in clipEdges)
            {
                if (IsOnSegment(p1, edge.start, edge.end) && IsOnSegment(p2, edge.start, edge.end))
                {
                    var p1DistToStart = Minus(p1, edge.start).Length();
                    var p2DistToStart = Minus(p2, edge.start).Length();
                    var segmentLength = Minus(p2, p1).Length();

                    if (Math.Abs(p1DistToStart + p2DistToStart - segmentLength) < Epsilon * Scale)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // ClipperOffset 클래스를 사용하여 폴리곤을 오프셋하는 헬퍼 함수
        private static List<List<IntPoint>> OffsetPolygons(List<List<IntPoint>> polygons, double delta)
        {
            var offsetter = new ClipperOffset();
            offsetter.AddPaths(polygons, JoinType.jtRound, EndType.etClosedPolygon);
            var offsettedPolygons = new List<List<IntPoint>>();
            offsetter.Execute(ref offsettedPolygons, delta);
            return offsettedPolygons;
        }

        public static IntPoint Minus(IntPoint a, IntPoint b)
        {
            return new IntPoint(a.X - b.X, a.Y - b.Y);
        }

        public static double Length(this IntPoint p)
        {
            return System.Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
        }

        private static bool IsOnSegment(IntPoint p, IntPoint a, IntPoint b)
        {
            var crossProduct = (long)(p.Y - a.Y) * (b.X - a.X) - (long)(p.X - a.X) * (b.Y - a.Y);
            if (Math.Abs(crossProduct) > Epsilon * Scale * Scale)
            {
                return false;
            }

            var dotProduct = (p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y);
            if (dotProduct < 0)
            {
                return false;
            }

            var squaredLength = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
            if (dotProduct > squaredLength)
            {
                return false;
            }

            return true;
        }

        private static bool GetCommonPlaneInfo(List<List<Vector3>> poly1, List<List<Vector3>> poly2, Vector3 n1, Vector3 n2, out Vector3 commonNormal, out Vector3 commonOrigin)
        {
            commonNormal = Vector3.Zero;
            commonOrigin = Vector3.Zero;

            if (poly1.Count == 0 || poly2.Count == 0) return false;

            if (Math.Abs(Vector3.Dot(n1, n2) - 1.0f) > Epsilon) return false;
            if (Math.Abs(Vector3.Dot(n1, poly1[0][0] - poly2[0][0])) > Epsilon) return false;
            if (!IsAllVerticesCoplanar(poly1, n1, poly1[0][0]) || !IsAllVerticesCoplanar(poly2, n2, poly2[0][0])) return false;

            commonNormal = (n1 + n2).Normalized();
            commonOrigin = (poly1[0][0] + poly2[0][0]) / 2.0f;

            return true;
        }

        private static bool GetCommonPlaneInfo(List<List<Vector3>> allPoints, Vector3 normal, out Vector3 commonNormal, out Vector3 commonOrigin)
        {
            commonNormal = normal;
            commonOrigin = allPoints[0][0];

            foreach (var polygon in allPoints)
            {
                foreach (var vertex in polygon)
                {
                    if (Math.Abs(Vector3.Dot(normal, vertex - commonOrigin)) > Epsilon)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsAllVerticesCoplanar(List<List<Vector3>> polygons, Vector3 normal, Vector3 origin)
        {
            foreach (var polygon in polygons)
            {
                foreach (var vertex in polygon)
                {
                    if (Math.Abs(Vector3.Dot(normal, vertex - origin)) > Epsilon)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static Vector3 CalculatePolygonNormal(List<Vector3> polygon)
        {
            if (polygon.Count < 3) return Vector3.Zero;
            return Vector3.Cross(polygon[1] - polygon[0], polygon[2] - polygon[0]).Normalized();
        }

        private static (Vector3 U, Vector3 V) GetPlaneBasis(Vector3 normal)
        {
            Vector3 u = Vector3.Cross(normal, Vector3.UnitY);
            if (u.LengthSquared < Epsilon)
            {
                u = Vector3.Cross(normal, Vector3.UnitX);
            }
            u.Normalize();
            Vector3 v = Vector3.Cross(normal, u);
            v.Normalize();
            return (u, v);
        }

        private static List<List<Vector2>> To2D(List<List<Vector3>> poly3d, Vector3 origin, Vector3 u, Vector3 v)
        {
            var poly2dList = new List<List<Vector2>>();
            foreach (var poly in poly3d)
            {
                var poly2d = new List<Vector2>();
                foreach (var p in poly)
                {
                    poly2d.Add(new Vector2(Vector3.Dot(p - origin, u), Vector3.Dot(p - origin, v)));
                }
                poly2dList.Add(poly2d);
            }
            return poly2dList;
        }

        private static List<IntPoint> ToClipperIntPoints(List<Vector2> poly2d, long scale)
        {
            var intPoints = new List<IntPoint>();
            foreach (var p in poly2d)
            {
                intPoints.Add(new IntPoint((long)(p.X * scale), (long)(p.Y * scale)));
            }
            return intPoints;
        }

        private static Vector3 To3D(Vector2 p2d, Vector3 origin, Vector3 u, Vector3 v)
        {
            return origin + p2d.X * u + p2d.Y * v;
        }
    }
}