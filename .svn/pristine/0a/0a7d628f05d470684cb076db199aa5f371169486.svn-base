using IGX.Geometry.Common;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IGX.Geometry.DataStructure
{
    public static partial class ContactAnalysisHelper
    {
        public static class GJKEPAAlgorithm
        {
            private const float Epsilon = 1e-6f;
            private const int MaxIterations = 100;

            public enum IntersectionType
            {
                NotIntersecting,
                Intersecting,
                Coincident,
            }

            public struct IntersectionResult
            {
                public bool intersects;
                public IntersectionType type;
                public float distance;
                public List<Vector3> contactPoints; // 교차점, 선분, 또는 다각형의 정점들을 담을 리스트
            }

            // GJK 알고리즘의 Support 함수는 두 물체의 Minkowski 차이에서 특정 방향으로 가장 먼 점.
            private struct SupportPoint
            {
                public Vector3 V; // 민코프스키 차이(Minkowski Difference) 공간의 점
                public Vector3 V1; // 첫 번째 삼각형(t1) 위의 점
                public Vector3 V2; // 두 번째 삼각형(t2) 위의 점
            }

            // GJK 알고리즘을 위한 Support 함수. 주어진 방향에서 두 삼각형의 민코프스키 차이 공간의 가장 먼 점.
            private static SupportPoint Support(Triangle3f t1, Triangle3f t2, Vector3 direction)
            {
                Vector3 v1 = Support(t1, direction);
                Vector3 v2 = Support(t2, -direction);
                return new SupportPoint
                {
                    V = v1 - v2,
                    V1 = v1,
                    V2 = v2
                };
            }

            // 단일 삼각형에서 주어진 방향으로 가장 먼 정점.
            private static Vector3 Support(Triangle3f triangle, Vector3 direction)
            {
                float maxDot = float.MinValue;
                Vector3 farthestVertex = Vector3.Zero;
                Vector3[] vertices = { triangle.V0, triangle.V1, triangle.V2 };
                foreach (var v in vertices)
                {
                    float dot = Vector3.Dot(v, direction);
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        farthestVertex = v;
                    }
                }
                return farthestVertex;
            }

            // 심플렉스(단순체)를 갱신하고, 새로운 탐색 방향을 설정.
            // 원점(Origin)이 심플렉스 내부에 포함되면 true를 반환하여 충돌을 감지.
            private static bool UpdateSimplex(ref List<SupportPoint> simplex, ref Vector3 direction)
            {
                // 심플렉스의 마지막에 추가된 점을 A로 설정
                Vector3 a = simplex.Last().V;
                Vector3 ao = -a;

                if (simplex.Count == 2)
                {
                    Vector3 b = simplex[0].V;
                    Vector3 ab = b - a;

                    // 원점이 선분 AB의 밖에 있고, B 쪽에 가까울 경우 (수정된 로직)
                    if (Vector3.Dot(ab, ao) < 0)
                    {
                        simplex.RemoveAt(0);
                        direction = ao;
                    }
                    else
                    {
                        // 원점이 선분 AB에 있을 경우
                        direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
                    }
                }
                else if (simplex.Count == 3)
                {
                    Vector3 b = simplex[1].V;
                    Vector3 c = simplex[0].V;
                    Vector3 ab = b - a;
                    Vector3 ac = c - a;
                    Vector3 abcNormal = Vector3.Cross(ab, ac);

                    // 원점이 삼각형 평면의 법선 방향에 있을 때
                    if (Vector3.Dot(abcNormal, ao) > 0)
                    {
                        direction = abcNormal;
                    }
                    else
                    {
                        // A-B 모서리 보로노이 영역에 원점이 있을 때
                        if (Vector3.Dot(Vector3.Cross(ab, ao), ab) > 0)
                        {
                            simplex.RemoveAt(0); // C 제거
                            direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
                        }
                        // A-C 모서리 보로노이 영역에 원점이 있을 때
                        else if (Vector3.Dot(Vector3.Cross(ac, ao), ac) > 0)
                        {
                            simplex.RemoveAt(1); // B 제거
                            direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
                        }
                        // 원점이 삼각형 내부에 있을 때 (충돌 감지)
                        else
                        {
                            return true;
                        }
                    }
                }
                else if (simplex.Count == 4)
                {
                    // 사면체 내부에 원점 포함 시 충돌
                    Vector3 b = simplex[2].V;
                    Vector3 c = simplex[1].V;
                    Vector3 d = simplex[0].V;
                    Vector3 aoo = -a;

                    Vector3 abcNormal = Vector3.Cross(b - a, c - a);
                    Vector3 acdNormal = Vector3.Cross(c - a, d - a);
                    Vector3 adbNormal = Vector3.Cross(d - a, b - a);

                    if (Vector3.Dot(abcNormal, aoo) > 0)
                    {
                        simplex.RemoveAt(0);
                        direction = abcNormal;
                    }
                    else if (Vector3.Dot(acdNormal, aoo) > 0)
                    {
                        simplex.RemoveAt(2);
                        direction = acdNormal;
                    }
                    else if (Vector3.Dot(adbNormal, aoo) > 0)
                    {
                        simplex.RemoveAt(1);
                        direction = adbNormal;
                    }
                    else
                    {
                        return true; // 원점 포함
                    }
                }

                return false;
            }

            // GJK-EPA 알고리즘의 메인 함수.
            public static IntersectionResult GetClosestPoints(Triangle3f t1, Triangle3f t2)
            {
                // 1. 동일 평면 위에 있는지 확인 (특수 케이스)
                Vector3 normal1 = Vector3.Cross(t1.V1 - t1.V0, t1.V2 - t1.V0).Normalized();
                Vector3 normal2 = Vector3.Cross(t2.V1 - t2.V0, t2.V2 - t2.V0).Normalized();
                if (Math.Abs(Math.Abs(Vector3.Dot(normal1, normal2)) - 1.0f) < Epsilon)
                {
                    var poly1 = new List<List<Vector3>> { new List<Vector3> { t1.V0, t1.V1, t1.V2 } };
                    var poly2 = new List<List<Vector3>> { new List<Vector3> { t2.V0, t2.V1, t2.V2 } };
                    List<List<Vector3>> intersectionVertices = PolygonClipper.ClipPolygons(poly1, poly2, false, 0.0f);

                    if (intersectionVertices.Count > 0)
                    {
                        List<Vector3> flatIntersectionPoints = intersectionVertices.SelectMany(poly => poly).ToList();

                        return new IntersectionResult
                        {
                            intersects = true,
                            type = IntersectionType.Coincident,
                            distance = 0.0f,
                            contactPoints = flatIntersectionPoints
                        };
                    }
                    else
                    {
                        // 겹치지 않는 경우: GJK/EPA로 최단 거리 계산
                        // 아래의 GJK 알고리즘으로 이동
                    }
                }

                // 2. GJK 알고리즘으로 충돌 여부 확인 및 최단 거리 계산
                var COG1 = (t1.V0 + t1.V1 + t1.V2) / 3;
                var COG2 = (t2.V0 + t2.V1 + t2.V2) / 3;
                Vector3 direction = COG2 - COG1;
                if (direction.LengthSquared < Epsilon)
                {
                    return new IntersectionResult
                    {
                        intersects = true,
                        type = IntersectionType.Coincident,
                        distance = 0.0f,
                        contactPoints = new List<Vector3> { COG1, COG2 }
                    };
                }

                List<SupportPoint> simplex = new List<SupportPoint>
                {
                    Support(t1, t2, direction)
                };
                direction = -simplex[0].V;

                for (int i = 0; i < MaxIterations; i++)
                {
                    SupportPoint newPoint = Support(t1, t2, direction);
                    if (Vector3.Dot(newPoint.V, direction) < 0)
                    {
                        Vector3 p1, p2;
                        GetClosestPointsBetweenNonIntersectingTriangles(simplex, out p1, out p2);
                        return new IntersectionResult
                        {
                            intersects = false,
                            type = IntersectionType.NotIntersecting,
                            distance = (p1 - p2).Length,
                            contactPoints = new List<Vector3> { p1, p2 }
                        };
                    }
                    simplex.Add(newPoint);

                    if (UpdateSimplex(ref simplex, ref direction))
                    {
                        Vector3 p1, p2;
                        if (EPA(t1, t2, simplex, out p1, out p2))
                        {
                            return new IntersectionResult
                            {
                                intersects = true,
                                type = IntersectionType.Intersecting,
                                distance = 0.0f,
                                contactPoints = new List<Vector3> { p1, p2 }
                            };
                        }
                        else
                        {
                            // EPA 실패 시 비충돌로 간주
                            return new IntersectionResult
                            {
                                intersects = false,
                                type = IntersectionType.NotIntersecting,
                                distance = -1.0f,
                                contactPoints = new List<Vector3>()
                            };
                        }
                    }
                }

                // 반복 횟수 초과 시 비충돌로 간주
                return new IntersectionResult
                {
                    intersects = false,
                    type = IntersectionType.NotIntersecting,
                    distance = -1.0f,
                    contactPoints = new List<Vector3>()
                };
            }

            // EPA 알고리즘: 충돌 시 관통 깊이와 접촉점을 계산.
            private static bool EPA(Triangle3f t1, Triangle3f t2, List<SupportPoint> initialSimplex, out Vector3 p1, out Vector3 p2)
            {
                p1 = Vector3.Zero;
                p2 = Vector3.Zero;

                if (initialSimplex.Count < 4) return false;

                // 초기 사면체의 면 목록을 생성
                var faces = new List<Tuple<int, int, int>>
                {
                    Tuple.Create(0, 1, 2),
                    Tuple.Create(0, 2, 3),
                    Tuple.Create(0, 3, 1),
                    Tuple.Create(1, 3, 2)
                };

                for (int i = 0; i < MaxIterations; i++)
                {
                    int closestFaceIndex = -1;
                    float minDistance = float.MaxValue;
                    Vector3 closestNormal = Vector3.Zero;

                    for (int j = 0; j < faces.Count; j++)
                    {
                        Vector3 v0 = initialSimplex[faces[j].Item1].V;
                        Vector3 v1 = initialSimplex[faces[j].Item2].V;
                        Vector3 v2 = initialSimplex[faces[j].Item3].V;

                        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).Normalized();
                        float distance = Vector3.Dot(normal, v0);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestFaceIndex = j;
                            closestNormal = normal;
                        }
                    }

                    if (closestFaceIndex == -1) return false;

                    SupportPoint newPoint = Support(t1, t2, closestNormal);
                    float newDistance = Vector3.Dot(newPoint.V, closestNormal);

                    if (newDistance - minDistance < Epsilon)
                    {
                        // 관통 깊이와 법선 발견.
                        Vector3 A = initialSimplex[faces[closestFaceIndex].Item1].V;
                        Vector3 B = initialSimplex[faces[closestFaceIndex].Item2].V;
                        Vector3 C = initialSimplex[faces[closestFaceIndex].Item3].V;

                        // 원점에서 가장 가까운 면의 무게중심 좌표를 계산
                        Vector3 barycentric = GetBarycentricCoordinates(A, B, C, Vector3.Zero);

                        // 무게중심 좌표를 사용하여 두 물체 위의 접촉점 계산
                        p1 = barycentric.X * initialSimplex[faces[closestFaceIndex].Item1].V1
                           + barycentric.Y * initialSimplex[faces[closestFaceIndex].Item2].V1
                           + barycentric.Z * initialSimplex[faces[closestFaceIndex].Item3].V1;

                        p2 = barycentric.X * initialSimplex[faces[closestFaceIndex].Item1].V2
                           + barycentric.Y * initialSimplex[faces[closestFaceIndex].Item2].V2
                           + barycentric.Z * initialSimplex[faces[closestFaceIndex].Item3].V2;

                        return true;
                    }

                    // 새로운 점을 추가하고 폴리토프 확장
                    var removedFaces = new List<int>();
                    var newEdges = new List<Tuple<int, int>>();

                    // 보이는 면 찾기
                    for (int j = 0; j < faces.Count; j++)
                    {
                        Vector3 v0 = initialSimplex[faces[j].Item1].V;
                        Vector3 normal = Vector3.Cross(initialSimplex[faces[j].Item2].V - v0, initialSimplex[faces[j].Item3].V - v0).Normalized();

                        if (Vector3.Dot(normal, newPoint.V - v0) > 0)
                        {
                            removedFaces.Add(j);
                            newEdges.Add(Tuple.Create(faces[j].Item1, faces[j].Item2));
                            newEdges.Add(Tuple.Create(faces[j].Item2, faces[j].Item3));
                            newEdges.Add(Tuple.Create(faces[j].Item3, faces[j].Item1));
                        }
                    }

                    // 중복되는 엣지 제거 (오직 하나만 남도록)
                    var uniqueEdges = newEdges.GroupBy(e => (e.Item1 < e.Item2) ? Tuple.Create(e.Item1, e.Item2) : Tuple.Create(e.Item2, e.Item1))
                                            .Where(g => g.Count() == 1)
                                            .Select(g => g.First())
                                            .ToList();

                    // 제거된 면 다시 빌드
                    var oldFaces = new List<Tuple<int, int, int>>(faces);
                    faces.Clear();
                    for (int j = 0; j < oldFaces.Count; j++)
                    {
                        if (!removedFaces.Contains(j))
                        {
                            faces.Add(oldFaces[j]);
                        }
                    }

                    // 새로운 면 추가
                    int newPointIndex = initialSimplex.Count;
                    initialSimplex.Add(newPoint);
                    foreach (var edge in uniqueEdges)
                    {
                        faces.Add(Tuple.Create(edge.Item1, edge.Item2, newPointIndex));
                    }
                }

                return false; // 수렴 실패
            }

            // 충돌하지 않는 경우, GJK 심플렉스를 사용하여 두 삼각형 사이의 최단 거리를 계산.
            private static void GetClosestPointsBetweenNonIntersectingTriangles(List<SupportPoint> simplex, out Vector3 p1, out Vector3 p2)
            {
                p1 = Vector3.Zero;
                p2 = Vector3.Zero;

                if (simplex.Count == 1)
                {
                    p1 = simplex[0].V1;
                    p2 = simplex[0].V2;
                }
                else if (simplex.Count == 2)
                {
                    // 선분 A, B를 가정
                    Vector3 A = simplex[0].V;
                    Vector3 B = simplex[1].V;
                    Vector3 AB = B - A;
                    float t = Vector3.Dot(-A, AB) / AB.LengthSquared;
                    t = Math.Max(0, Math.Min(1, t));

                    p1 = Vector3.Lerp(simplex[0].V1, simplex[1].V1, t);
                    p2 = Vector3.Lerp(simplex[0].V2, simplex[1].V2, t);
                }
                else if (simplex.Count == 3)
                {
                    Vector3 A = simplex[0].V;
                    Vector3 B = simplex[1].V;
                    Vector3 C = simplex[2].V;
                    Vector3 closest = ClosestPointOnTriangleToOrigin(A, B, C);

                    // ClosestPointOnTriangleToOrigin은 원점에서 가장 가까운 점을 반환
                    Vector3 coords = GetBarycentricCoordinates(A, B, C, closest);
                    p1 = coords.X * simplex[0].V1 + coords.Y * simplex[1].V1 + coords.Z * simplex[2].V1;
                    p2 = coords.X * simplex[0].V2 + coords.Y * simplex[1].V2 + coords.Z * simplex[2].V2;
                }
            }

            // 삼각형 평면 위의 원점에서 가장 가까운 점.
            private static Vector3 ClosestPointOnTriangleToOrigin(Vector3 A, Vector3 B, Vector3 C)
            {
                Vector3 AB = B - A;
                Vector3 AC = C - A;
                Vector3 BC = C - B;
                Vector3 AP = -A;

                // A 꼭짓점 영역 확인
                float d1 = Vector3.Dot(AB, AP);
                float d2 = Vector3.Dot(AC, AP);
                if (d1 <= 0.0f && d2 <= 0.0f) return A;

                // B 꼭짓점 영역 확인
                Vector3 BP = -B;
                float d3 = Vector3.Dot(AB, BP);
                float d4 = Vector3.Dot(AC, BP);
                if (d3 >= 0.0f && d4 <= d3) return B;

                // C 꼭짓점 영역 확인
                Vector3 CP = -C;
                float d5 = Vector3.Dot(AB, CP);
                float d6 = Vector3.Dot(AC, CP);
                if (d6 >= 0.0f && d5 <= d6) return C;

                // AB 모서리 영역 확인
                float vc = d1 * d4 - d3 * d2;
                if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
                {
                    float t = d1 / (d1 - d3);
                    return A + t * AB;
                }

                // AC 모서리 영역 확인
                float vb = d5 * d2 - d1 * d6;
                if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
                {
                    float t = d2 / (d2 - d6);
                    return A + t * AC;
                }

                // BC 모서리 영역 확인
                float va = d3 * d6 - d5 * d4;
                if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
                {
                    float t = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                    return B + t * BC;
                }

                // 삼각형 내부 영역 확인 (무게중심 좌표 사용)
                float denom = 1.0f / (va + vb + vc);
                float v = vb * denom;
                float w = vc * denom;
                float u = 1.0f - v - w;

                return u * A + v * B + w * C;
            }
            // 선분 위의 원점에서 가장 가까운 점.
            private static Vector3 ClosestPointOnSegmentToOrigin(Vector3 A, Vector3 B)
            {
                Vector3 AB = B - A;
                float t = Vector3.Dot(-A, AB) / AB.LengthSquared;
                t = Math.Max(0, Math.Min(1, t));
                return A + AB * t;
            }

            // 삼각형의 세 꼭짓점을 사용하여 임의의 점의 무게중심 좌표(barycentric coordinates)를 계산.
            private static Vector3 GetBarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
            {
                Vector3 v0 = B - A, v1 = C - A, v2 = P - A;
                float d00 = Vector3.Dot(v0, v0);
                float d01 = Vector3.Dot(v0, v1);
                float d11 = Vector3.Dot(v1, v1);
                float d20 = Vector3.Dot(v2, v0);
                float d21 = Vector3.Dot(v2, v1);
                float denom = d00 * d11 - d01 * d01;

                float v = (d11 * d20 - d01 * d21) / denom;
                float w = (d00 * d21 - d01 * d20) / denom;
                float u = 1.0f - v - w;

                return new Vector3(u, v, w);
            }
        }
    }
}