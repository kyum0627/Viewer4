using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using IGX.Geometry.Common;
using System.Diagnostics;

namespace IGX.Geometry.ConvexHull
{
    // https://en.wikipedia.org/wiki/Convex_hull_algorithms
    //ConVexhull을 찾는 기법 중 하나
    //볼록 껍질은 주어진 점들을 모두 포함하는 가장 작은 볼록 다각형
    //------ 알고리즘 개요
    //1. 분할 정복(Divide and Conquer) 기법을 사용하여 점들을 볼록 껍질의 일부로 나누고,
    //   각 부분에서 가장 멀리 떨어진 점을 찾아 볼록 껍질의 경계를 형성
    //2. 주어진 점들 중에서 가장 왼쪽 점과 오른쪽 점을 찾아 점들을 좌우로 분할
    //3. 분할된 각 부분별로 재귀적으로 QuickHull3D 적용
    //   - 왼쪽 부분에 대해 QuickHull을 재귀적으로 호출하여 왼쪽 볼록 껍질 구성
    //   - 오른쪽 부분에 대해 QuickHull을 재귀적으로 호출하여 오른쪽 볼록 껍질 구성
    //4. 왼쪽 볼록 껍질과 오른쪽 볼록 껍질을 결합하여 전체 볼록 껍질 형성
    //   - 왼쪽 볼록 껍질 오른쪽 점과 오른쪽 볼록 껍질 왼쪽 점을 연결하여 볼록 껍질의 경계를 완성
    public class QuickHull3D
    {
        const int UNASSIGNED = -2; // 사용되지 않은 포인트를 표시하는 상수
        const int INSIDE = -1; // 볼록 껍질 내부에 있는 포인트를 표시하는 상수
        const float EPSILON = MathUtil.Epsilon; // 동일한 점으로 간주하기 위한 오차

        /// <summary>
        /// 삼각 메쉬 face
        /// CCW로 구성되는 점들의 index (Vertex0, Vertex1 and Vertex2)
        /// 실 좌표값은 ControlPoints array에 저장
        /// Opposite0, Opposite1 and Opposite2 는 각 edge를 공유하는 edge의 index
        /// 즉, Vertex0와 Vertex2를 연결한 edge의 opposite는 vertex2 와 Vertex1을 연결한 hedge
        /// NormalPlane : 삼각형의 norID
        /// </summary>
        struct TriangleMesh
        {
            public int Vertex0; // 첫 번째 꼭지점 인덱스
            public int Vertex1;
            public int Vertex2;
            public int Opposite0; // 첫 번째 edge의 반대편 꼭지점 인덱스
            public int Opposite1;
            public int Opposite2;
            public Vector3 Normal; // 삼각형의 norID 벡터
            public TriangleMesh(int v0, int v1, int v2, int o0, int o1, int o2, Vector3 normal)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;
                Opposite0 = o0;
                Opposite1 = o1;
                Opposite2 = o2;
                Normal = normal;
            }
            public bool Equals(TriangleMesh other)
            {
                return Vertex0 == other.Vertex0
                    && Vertex1 == other.Vertex1
                    && Vertex2 == other.Vertex2
                    && Opposite0 == other.Opposite0
                    && Opposite1 == other.Opposite1
                    && Opposite2 == other.Opposite2
                    && Normal == other.Normal;
            }
        }

        /// <summary>
        /// 점과 삼각 메쉬를 매핑하는 구조체
        /// Dimension0 = ControlPoints 배열에서의 점 인덱스
        /// ConvexFace = key dictionary에서의 face key
        /// DistanceManager = face로부터 점까지의 거리
        /// </summary>
        struct PointFace
        {
            public int Point; // 점의 인덱스
            public int Face; // face의 인덱스
            public float Distance;// face로부터의 거리
            public PointFace(int p, int f, float d)
            {
                Point = p;
                Face = f;
                Distance = d;
            }
        }

        /// <summary>
        /// 수평 hedge
        /// Edge0, Edge1 는 CCW 방향
        /// ConvexFace = horizon의 반대 face.
        /// </summary>
        struct HorizonEdge
        {
            public int Face;
            public int Edge0;
            public int Edge1;
        }

        Dictionary<int, TriangleMesh> faces = new();// face들을 저장하는 dictionary
        List<PointFace> openSet = new();        // 계산할 점들의 list
        HashSet<int> litFaces = new();          // point set이 놓인 면 (계산 과정에 사용된 기준 면)
        List<HorizonEdge> horizon = new();      // 현재(계산 중) 사용된 수평 hedge <- FindHorizon() DepthFirstSearch search
        Dictionary<int, int> hullVerts = new(); // convex hull을 구성하는데 사용될 정점
        int openSetTail = -1;           // openSet의 끝을 의미
        int faceCount = 0;// face의 개수

        /// <summary>
        /// 주어진 점들을 이용하여 convex hull을 생성하고 MeshGeometry 형태로 저장
        /// </summary>
        /// <param name="inputpoints">3차원 convex hull을 구성할 점들의 좌표 리스트</param>
        /// <param name="splitVertices">true면 정점, 삼각형을 구성하는 각 정점의 MeshID, 정점의 normal을 중복해서 생성 (정점 좌표 및 norID 값이 중복될 수 있음), false면 중복 없음</param>
        /// <param name="vertices">convex hull을 형성하는 정점들의 리스트</param>
        /// <param name="triangles">convex hull을 구성하는 삼각형들의 인덱스 리스트</param>
        /// <param name="normals">convex hull을 구성하는 정점들의 norID 벡터 리스트</param>
        public bool Compute(
            List<Vector3> inputpoints,
            bool splitVertices,
            ref List<Vector3> vertices,
            ref List<int> triangles,
            ref List<Vector3> normals)
        {
            if (inputpoints.Count < 4) // JJJJJJJJJJJJJJJJJJJJJJJ
            {
                return false;
                //throw new ArgumentException("3D convex hull을 생성하려면 최소 4개의 linear independent point가 필요함");
            }

            if (inputpoints.Count == 4)
            {
                vertices = ComputeConvexHullFromFourPoints(inputpoints);
                return true;
            }

            FindInitialHullIndices(inputpoints, out int b0, out int b1, out int b2, out int b3);
            if (b1 == -1 || b1 == -1 || b2 == -1 || b3 == -1)
            {
                return false; // 모든 점들이 동일 평면에 존재
            }

            Initialize(inputpoints, splitVertices);
            bool made = GenerateInitialHull(inputpoints, b0, b1, b2, b3);

            if (made)
            {
                while (openSetTail >= 0)
                {
                    GrowHull(inputpoints);
                }
                ExportMesh(inputpoints, splitVertices, ref vertices, ref triangles, ref normals);
                return true;
            }
            return false;
        }

        private List<Vector3> ComputeConvexHullFromFourPoints(List<Vector3> points)
        {
            int[] indices = { 0, 1, 2, 3 };// 네 점의 인덱스
            List<int[]> faces = new()
            {
                new int[] { indices[0], indices[1], indices[2] },
                new int[] { indices[0], indices[1], indices[3] },
                new int[] { indices[0], indices[2], indices[3] },
                new int[] { indices[1], indices[2], indices[3] }
            };

            List<Vector3> convexHull = new(); // 사면체의 면을 정의하는 삼각형을 반환.
            foreach (int[] face in faces)
            {
                convexHull.AddRange(new[]
                {
                points[face[0]],
                points[face[1]],
                points[face[2]]
            });
            }
            return convexHull;
        }

        /// <summary>
        /// 알고리즘이 사용하게 될 변수와 buffer 초기화
        /// </summary>
        /// <summary>
        /// 알고리즘이 사용하게 될 변수와 buffer 초기화
        /// </summary>
        void Initialize(List<Vector3> points, bool splitVerts)
        {
            faceCount = 0;
            openSetTail = -1;

            // loopsIDs, litFaces, horizon, openSet 초기화
            faces ??= new Dictionary<int, TriangleMesh>();
            litFaces ??= new HashSet<int>();
            horizon ??= new List<HorizonEdge>();
            openSet ??= new List<PointFace>(points.Count);

            // openSet 용량 설정
            if (openSet.Capacity < points.Count)
            {
                openSet.Capacity = points.Count;
            }

            // splitVerts에 따라 hullVerts 초기화
            if (!splitVerts)
            {
                hullVerts ??= new Dictionary<int, int>();
            }
            else
            {
                hullVerts?.Clear();
            }
        }
        /// <summary>
        /// 최초 seed가 될 hull을 생성
        /// </summary>
        bool GenerateInitialHull(List<Vector3> points, int b0, int b1, int b2, int b3)
        {
            //FindInitialHullIndices(ControlPoints, out int b0, out int b1, out int b2, out int b3);// 4면체 구성에 적합한 4개의 점을 선택
            Vector3 v0 = points[b0];
            Vector3 v1 = points[b1];
            Vector3 v2 = points[b2];
            Vector3 v3 = points[b3];
            bool above = Vector3.Dot(v3 - v1, Cross(v1 - v0, v2 - v0)) > 0.0f;
            faceCount = 0;
            if (faces == null)
            {
                Debug.WriteLine("Convexhull 생성 불가. 오류 !!!!");
                return false; // 프로그램을 종료
            }
            if (above)
            {//선택된 4개의 점을 이용하여 사면체를 구성할 4개의 face를 생성
                faces[faceCount++] = new TriangleMesh(b0, b2, b1, 3, 1, 2, NormalFace(points[b0], points[b2], points[b1]));
                faces[faceCount++] = new TriangleMesh(b0, b1, b3, 3, 2, 0, NormalFace(points[b0], points[b1], points[b3]));
                faces[faceCount++] = new TriangleMesh(b0, b3, b2, 3, 0, 1, NormalFace(points[b0], points[b3], points[b2]));
                faces[faceCount++] = new TriangleMesh(b1, b2, b3, 2, 1, 0, NormalFace(points[b1], points[b2], points[b3]));
            }
            else
            {
                faces[faceCount++] = new TriangleMesh(b0, b1, b2, 3, 2, 1, NormalFace(points[b0], points[b1], points[b2]));
                faces[faceCount++] = new TriangleMesh(b0, b3, b1, 3, 0, 2, NormalFace(points[b0], points[b3], points[b1]));
                faces[faceCount++] = new TriangleMesh(b0, b2, b3, 3, 1, 0, NormalFace(points[b0], points[b2], points[b3]));
                faces[faceCount++] = new TriangleMesh(b1, b3, b2, 2, 0, 1, NormalFace(points[b1], points[b3], points[b2]));
            }

            openSet ??= new List<PointFace>();

            for (int i = 0; i < points.Count; i++)
            { // 선택(사용)되지 않은 점들을 list up, unassigned로 tagging
                if (i == b0 || i == b1 || i == b2 || i == b3)
                {
                    continue;
                }

                openSet.Add(new PointFace(i, UNASSIGNED, 0.0f));
            }
            // seed hull에 사용된 점들을 list에 추가, inside로 tagging
            openSet.Add(new PointFace(b0, INSIDE, float.NaN));
            openSet.Add(new PointFace(b1, INSIDE, float.NaN));
            openSet.Add(new PointFace(b2, INSIDE, float.NaN));
            openSet.Add(new PointFace(b3, INSIDE, float.NaN));

            openSetTail = openSet.Count - 5;

            for (int i = 0; i <= openSetTail; i++)
            {
                bool assigned = false;
                PointFace fp = openSet[i];
                for (int j = 0; j < 4; j++)
                {
                    TriangleMesh face = faces[j];
                    float dist = PointFaceDistance(points[fp.Point], points[face.Vertex0], face);
                    if (dist > 0)
                    {
                        fp.Face = j;
                        fp.Distance = dist;
                        openSet[i] = fp;
                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {// 내부 점
                    fp.Face = INSIDE;
                    fp.Distance = float.NaN;
                    openSet[i] = openSet[openSetTail];
                    openSet[openSetTail] = fp;
                    openSetTail -= 1;
                    i -= 1;
                }
            }
            return true;
        }

        void FindInitialHullIndices(List<Vector3> points, out int b0, out int b1, out int b2, out int b3)
        {
            int count = points.Count;

            // 초기 인덱스 변수 설정
            b0 = b1 = b2 = b3 = -1;

            // 4개의 유효한 점을 찾기 위해 반복
            for (int i0 = 0; i0 < count - 3; i0++)
            {
                for (int i1 = i0 + 1; i1 < count - 2; i1++)
                {
                    Vector3 p0 = points[i0];
                    Vector3 p1 = points[i1];

                    // 두 점이 동일하지 않은지 확인
                    if (AreCoincident(p0, p1, MathUtil.Epsilon))
                    {
                        continue;
                    }

                    for (int i2 = i1 + 1; i2 < count - 1; i2++)
                    {
                        Vector3 p2 = points[i2];

                        // 세 점이 일직선상에 있지 않은지 확인
                        if (AreCollinear(p0, p1, p2, MathUtil.Epsilon))
                        {
                            continue;
                        }

                        for (int i3 = i2 + 1; i3 < count; i3++)
                        {
                            Vector3 p3 = points[i3];

                            // 네 점이 같은 평면에 있지 않은지 확인
                            if (AreCoplanar(p0, p1, p2, p3, MathUtil.Epsilon))
                            {
                                continue;
                            }

                            // 유효한 4개의 점을 찾았으므로 ...
                            b0 = i0;
                            b1 = i1;
                            b2 = i2;
                            b3 = i3;
                            return;
                        }
                    }
                }
            }
            // 유효한 점들을 찾지 못한 경우 예외 발생
            //throw new ArgumentException("모든 점들이 동일 평면에 존재하여 volume을 형성할 수 없음");
        }

        /// <summary>
        /// convex hull을 최대로 확장.
        /// </summary>
        void GrowHull(List<Vector3> points)
        {
            // 가장 먼 점을 선택하여 hull을 키워나감
            int furthestPoint = 0;
            float dist = openSet[0].Distance;

            for (int i = 1; i <= openSetTail; i++)
            {
                if (openSet[i].Distance > dist)
                {
                    furthestPoint = i;
                    dist = openSet[i].Distance;
                }
            }

            FindHorizon(
                points,
                points[openSet[furthestPoint].Point],
                openSet[furthestPoint].Face,
                faces[openSet[furthestPoint].Face]);

            ConstructCone(points, openSet[furthestPoint].Point);
            ReassignPoints(points);
        }

        void FindHorizon(List<Vector3> points, Vector3 point, int fi, TriangleMesh face)
        {
            litFaces.Clear();
            horizon.Clear();

            litFaces.Add(fi);
            {
                TriangleMesh oppositeFace = faces[face.Opposite0];

                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite0,
                        Edge0 = face.Vertex1,
                        Edge1 = face.Vertex2,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite0, oppositeFace);
                }
            }

            if (!litFaces.Contains(face.Opposite1))
            {
                TriangleMesh oppositeFace = faces[face.Opposite1];
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite1,
                        Edge0 = face.Vertex2,
                        Edge1 = face.Vertex0,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite1, oppositeFace);
                }
            }

            if (!litFaces.Contains(face.Opposite2))
            {
                TriangleMesh oppositeFace = faces[face.Opposite2];

                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite2,
                        Edge0 = face.Vertex0,
                        Edge1 = face.Vertex1,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite2, oppositeFace);
                }
            }
        }

        void SearchHorizon(List<Vector3> points, Vector3 point, int prevFaceIndex, int faceCount, TriangleMesh face)
        {
            litFaces.Add(faceCount);

            int nextFaceIndex0;
            int nextFaceIndex1;
            int edge0;
            int edge1;
            int edge2;

            if (prevFaceIndex == face.Opposite0)
            {
                nextFaceIndex0 = face.Opposite1;
                nextFaceIndex1 = face.Opposite2;

                edge0 = face.Vertex2;
                edge1 = face.Vertex0;
                edge2 = face.Vertex1;
            }
            else if (prevFaceIndex == face.Opposite1)
            {
                nextFaceIndex0 = face.Opposite2;
                nextFaceIndex1 = face.Opposite0;

                edge0 = face.Vertex0;
                edge1 = face.Vertex1;
                edge2 = face.Vertex2;
            }
            else
            {
                nextFaceIndex0 = face.Opposite0;
                nextFaceIndex1 = face.Opposite1;

                edge0 = face.Vertex1;
                edge1 = face.Vertex2;
                edge2 = face.Vertex0;
            }

            if (!litFaces.Contains(nextFaceIndex0))
            {
                TriangleMesh oppositeFace = faces[nextFaceIndex0];

                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex0,
                        Edge0 = edge0,
                        Edge1 = edge1,
                    });
                }
                else
                {
                    SearchHorizon(points, point, faceCount, nextFaceIndex0, oppositeFace);
                }
            }

            if (!litFaces.Contains(nextFaceIndex1))
            {
                TriangleMesh oppositeFace = faces[nextFaceIndex1];

                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex1,
                        Edge0 = edge1,
                        Edge1 = edge2,
                    });
                }
                else
                {
                    SearchHorizon(points, point, faceCount, nextFaceIndex1, oppositeFace);
                }
            }
        }

        void ConstructCone(List<Vector3> points, int farthestPoint)
        {
            foreach (int fi in litFaces)
            {
                faces.Remove(fi);
            }

            int firstNewFace = faceCount;

            for (int i = 0; i < horizon.Count; i++)
            {
                int v0 = farthestPoint;
                int v1 = horizon[i].Edge0;
                int v2 = horizon[i].Edge1;

                int o0 = horizon[i].Face;
                int o1 = i == horizon.Count - 1 ? firstNewFace : firstNewFace + i + 1;
                int o2 = i == 0 ? firstNewFace + horizon.Count - 1 : firstNewFace + i - 1;

                int fi = faceCount++;

                faces[fi] = new TriangleMesh(
                    v0, v1, v2,
                    o0, o1, o2,
                    NormalFace(points[v0], points[v1], points[v2]));

                TriangleMesh horizonFace = faces[horizon[i].Face];

                if (horizonFace.Vertex0 == v1)
                {
                    horizonFace.Opposite1 = fi;
                }
                else if (horizonFace.Vertex1 == v1)
                {
                    horizonFace.Opposite2 = fi;
                }
                else
                {
                    horizonFace.Opposite0 = fi;
                }

                faces[horizon[i].Face] = horizonFace;
            }
        }

        void ReassignPoints(List<Vector3> points)
        {
            for (int i = 0; i <= openSetTail; i++)
            {
                PointFace fp = openSet[i];

                if (litFaces.Contains(fp.Face))
                {
                    bool assigned = false;
                    Vector3 point = points[fp.Point];

                    foreach (KeyValuePair<int, TriangleMesh> kvp in faces)
                    {
                        int fi = kvp.Key;
                        TriangleMesh face = kvp.Value;

                        float dist = PointFaceDistance(
                            point,
                            points[face.Vertex0],
                            face);

                        if (dist > EPSILON)
                        {
                            assigned = true;

                            fp.Face = fi;
                            fp.Distance = dist;

                            openSet[i] = fp;
                            break;
                        }
                    }

                    if (!assigned)
                    {
                        fp.Face = INSIDE;
                        fp.Distance = float.NaN;

                        openSet[i] = openSet[openSetTail];
                        openSet[openSetTail] = fp;

                        i--;
                        openSetTail--;
                    }
                }
            }
        }

        void ExportMesh(
            List<Vector3> points,
            bool splitVerts,
            ref List<Vector3> hullPoints,
            ref List<int> meshIndices,
            ref List<Vector3> pointsNormals)
        {
            if (hullPoints == null)
            {
                hullPoints = new List<Vector3>();
            }
            else
            {
                hullPoints.Clear();
            }

            if (meshIndices == null)
            {
                meshIndices = new List<int>();
            }
            else
            {
                meshIndices.Clear();
            }

            if (pointsNormals == null)
            {
                pointsNormals = new List<Vector3>();
            }
            else
            {
                pointsNormals.Clear();
            }

            foreach (TriangleMesh face in faces.Values)
            {
                int vi0, vi1, vi2;
                if (splitVerts)
                {
                    vi0 = hullPoints.Count; hullPoints.Add(points[face.Vertex0]);
                    vi1 = hullPoints.Count; hullPoints.Add(points[face.Vertex1]);
                    vi2 = hullPoints.Count; hullPoints.Add(points[face.Vertex2]);

                    pointsNormals.Add(face.Normal);
                    pointsNormals.Add(face.Normal);
                    pointsNormals.Add(face.Normal);
                }
                else
                {
                    if (!hullVerts.TryGetValue(face.Vertex0, out vi0))
                    {
                        vi0 = hullPoints.Count;
                        hullVerts[face.Vertex0] = vi0;
                        hullPoints.Add(points[face.Vertex0]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex1, out vi1))
                    {
                        vi1 = hullPoints.Count;
                        hullVerts[face.Vertex1] = vi1;
                        hullPoints.Add(points[face.Vertex1]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex2, out vi2))
                    {
                        vi2 = hullPoints.Count;
                        hullVerts[face.Vertex2] = vi2;
                        hullPoints.Add(points[face.Vertex2]);
                    }
                    //pointsNormals.Add(face.NormalPlane);
                }

                meshIndices.Add(vi0);
                meshIndices.Add(vi1);
                meshIndices.Add(vi2);
            }
        }

        /// <summary>
        /// 점과 face 사이의 Signed DistanceManager 계산, positive = 바깥
        /// </summary>
        float PointFaceDistance(Vector3 point, Vector3 pointOnFace, TriangleMesh face)
        {
            return Vector3.Dot(face.Normal, point - pointOnFace);
        }

        /// <summary>
        /// 삼각형의 norID 계산
        /// </summary>
        Vector3 NormalFace(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        }

        static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                (a.Y * b.Z) - (a.Z * b.Y),
                (a.Z * b.X) - (a.X * b.Z),
                (a.X * b.Y) - (a.Y * b.X));
        }

        // 점이 같은 위치에 있는지 확인
        bool AreCoincident(Vector3 p0, Vector3 p1, float epsilon)
        {
            return (p0 - p1).LengthSquared < epsilon * epsilon;
        }

        // 점들이 일직선상에 있는지 확인
        bool AreCollinear(Vector3 p0, Vector3 p1, Vector3 p2, float epsilon)
        {
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p2 - p0;
            Vector3 crossProduct = Vector3.Cross(v1, v2);
            return crossProduct.LengthSquared < epsilon * epsilon;
        }

        // 점들이 같은 평면에 있는지 확인
        bool AreCoplanar(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float epsilon)
        {
            Vector3 v1 = p1 - p0;
            Vector3 v2 = p2 - p0;
            Vector3 v3 = p3 - p0;
            Vector3 normal = Vector3.Cross(v1, v2);
            float distance = Math.Abs(Vector3.Dot(normal, v3)) / normal.Length;
            return distance < epsilon;
        }

        /// <summary>
        /// f가 e0, e1으로 구성된 face인지 검사
        /// </summary>
        bool HasEdge(TriangleMesh f, int e0, int e1)
        {
            return (f.Vertex0 == e0 && f.Vertex1 == e1)
                || (f.Vertex1 == e0 && f.Vertex2 == e1)
                || (f.Vertex2 == e0 && f.Vertex0 == e1);
        }

        /// <summary>
        /// free face 찾기
        /// </summary>
        /// <param name="points"></param>
        void VerifyOpenSet(List<Vector3> points)
        {
            for (int i = 0; i < openSet.Count; i++)
            {
                if (i > openSetTail)
                {
                    Assert(openSet[i].Face == INSIDE);
                }
                else
                {
                    Assert(openSet[i].Face != INSIDE);
                    Assert(openSet[i].Face != UNASSIGNED);

                    Assert(PointFaceDistance(
                            points[openSet[i].Point],
                            points[faces[openSet[i].Face].Vertex0],
                            faces[openSet[i].Face]) > 0.0f);
                }
            }
        }

        void VerifyHorizon()
        {
            for (int i = 0; i < horizon.Count; i++)
            {
                int prev = i == 0 ? horizon.Count - 1 : i - 1;

                Assert(horizon[prev].Edge1 == horizon[i].Edge0);
                Assert(HasEdge(faces[horizon[i].Face], horizon[i].Edge1, horizon[i].Edge0));
            }
        }

        void VerifyFaces(List<Vector3> points)
        {
            foreach (KeyValuePair<int, TriangleMesh> kvp in faces)
            {
                int fi = kvp.Key;
                TriangleMesh face = kvp.Value;

                Assert(faces.ContainsKey(face.Opposite0));
                Assert(faces.ContainsKey(face.Opposite1));
                Assert(faces.ContainsKey(face.Opposite2));

                Assert(face.Opposite0 != fi);
                Assert(face.Opposite1 != fi);
                Assert(face.Opposite2 != fi);

                Assert(face.Vertex0 != face.Vertex1);
                Assert(face.Vertex0 != face.Vertex2);
                Assert(face.Vertex1 != face.Vertex2);

                Assert(HasEdge(faces[face.Opposite0], face.Vertex2, face.Vertex1));
                Assert(HasEdge(faces[face.Opposite1], face.Vertex0, face.Vertex2));
                Assert(HasEdge(faces[face.Opposite2], face.Vertex1, face.Vertex0));

                Assert((face.Normal - NormalFace(
                            points[face.Vertex0],
                            points[face.Vertex1],
                            points[face.Vertex2])).Length < EPSILON);
            }
        }

        void VerifyMesh(List<Vector3> points, ref List<Vector3> verts, ref List<int> tris)
        {
            Assert(tris.Count % 3 == 0);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < tris.Count; j += 3)
                {
                    Vector3 t0 = verts[tris[j]];
                    Vector3 t1 = verts[tris[j + 1]];
                    Vector3 t2 = verts[tris[j + 2]];

                    Assert(Vector3.Dot(points[i] - t0, Vector3.Cross(t1 - t0, t2 - t0)) <= EPSILON);
                }
            }
        }

        static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assertion Exception");
            }
        }
    }
}
