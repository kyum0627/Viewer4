using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.ConvexHull
{
    public class ConvexHull3
    {
        const int UNASSIGNED = -2;
        const int INSIDE = -1;
        const float EPSILON = 0.0001f;

        struct ConvexFace
        {
            public int Vertex0; // 첫 번째 정점 인덱스
            public int Vertex1; // 두 번째 정점 인덱스
            public int Vertex2; // 세 번째 정점 인덱스
            public int Opposite0; // 이 면과 연결된 반대 면 인덱스
            public int Opposite1; // 이 면과 연결된 반대 면 인덱스
            public int Opposite2; // 이 면과 연결된 반대 면 인덱스
            public Vector3 Normal; // 면의 법선 벡터

            public ConvexFace(int v0, int v1, int v2, int o0, int o1, int o2, Vector3 normal)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;
                Opposite0 = o0;
                Opposite1 = o1;
                Opposite2 = o2;
                Normal = normal;
            }

            public bool Equals(ConvexFace other)
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

        // 구조체 정의: 점과 면의 연결 정보를 담는 구조체
        struct PointFace
        {
            public int Point;
            public int Face;
            public float Distance;

            public PointFace(int p, int f, float d)
            {
                Point = p;
                Face = f;
                Distance = d;
            }
        }

        // 구조체 정의: 호라이즌 엣지 정보를 담는 구조체, 기준면
        struct HorizonEdge
        {
            public int Face; // 엣지가 속한 면의 인덱스
            public int Edge0; // 엣지의 첫 번째 정점 인덱스
            public int Edge1; // 엣지의 두 번째 정점 인덱스
        }

        Dictionary<int, ConvexFace> faces = new(); // 볼록 다면체의 면을 저장하는 사전
        List<PointFace> openSet = new(); // 점과 면 연결 정보를 담는 리스트
        HashSet<int> litFaces = new(); // 처리된 면의 인덱스를 저장하는 해시 집합
        List<HorizonEdge> horizon = new(); // 호라이즌 엣지 정보를 담는 리스트
        Dictionary<int, int> hullVerts = new(); // 볼록 껍질을 구성하는 정점 정보를 담는 사전
        int openSetTail = -1; // openSet 리스트의 마지막 인덱스
        int faceCount = 0; // 볼록 다면체의 현재 면 개수

        /// <summary>
        /// 볼록 다면체 계산
        /// </summary>
        /// <param name="points">입력 점 리스트</param>
        /// <param name="foreachtriangle">삼각형의 정점 분리(동일 좌표 중복)</param>
        /// <param name="verts">convex hull 구성 정점. splitVerts 가 true이면 각 삼각형마다 ...</param>
        /// <param name="tris">convex hull 구성 triangle index</param>
        /// <param name="normals">convex hull 구성 정점별 norID vector</param>
        public void Compute(
            List<Vector3> points, // 입력 점 리스트
            bool foreachtriangle, // 삼각형의 정점 분리 여부
            ref List<Vector3> verts, // 볼록 껍질을 구성하는 정점 리스트
            ref List<int> tris, // 볼록 껍질을 구성하는 삼각형 인덱스 리스트
            ref List<Vector3> normals) // 볼록 껍질 정점별 법선 벡터 리스트
        {
            if (points.Count < 4)
            {
                switch (points.Count)
                {
                    case 0:
                        verts.Clear();
                        tris.Clear();
                        normals.Clear();
                        break;
                    case 1:
                        // 1개의 점인 경우 처리
                        verts.Clear();
                        verts.Add(points[0]);
                        tris.Clear();
                        normals.Clear();
                        return;
                    case 2:
                        // 2개의 점인 경우 처리
                        verts.Clear();
                        Vector3 center = (points[0] + points[1]) / 2;
                        verts.Add(center);
                        tris.Clear();
                        normals.Clear();
                        return;
                    case 3:
                        // 3개의 점인 경우 처리
                        verts.Clear();
                        Vector3 minExtent = CalculateMinExtent(points[0], points[1], points[2]);
                        verts.Add(minExtent);
                        tris.Clear();
                        normals.Clear();
                        return;
                }
            }

            Initialize(points, foreachtriangle);
            bool bSuccess = GenerateInitialHull(points);

            while (openSetTail >= 0)
            {
                GrowHull(points);
            }

            ExportMesh(points, foreachtriangle, ref verts, ref tris, ref normals); // 볼록 껍질 메쉬 추출
        }
        Vector3 CalculateMinExtent(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float minX = Math.Min(p0.X, Math.Min(p1.X, p2.X));
            float minY = Math.Min(p0.Y, Math.Min(p1.Y, p2.Y));
            float minZ = Math.Min(p0.Z, Math.Min(p1.Z, p2.Z));

            float maxX = Math.Max(p0.X, Math.Max(p1.X, p2.X));
            float maxY = Math.Max(p0.Y, Math.Max(p1.Y, p2.Y));
            float maxZ = Math.Max(p0.Z, Math.Max(p1.Z, p2.Z));

            return new Vector3(minX, minY, minZ);
        }
        void Initialize(List<Vector3> points, bool splitVerts)
        {
            faceCount = 0;
            openSetTail = -1;

            // 데이터 구조 초기화
            if (faces == null)
            {
                faces = new Dictionary<int, ConvexFace>();
                litFaces = new HashSet<int>();
                horizon = new List<HorizonEdge>();
                openSet = new List<PointFace>(points.Count);
            }
            else
            {
                faces?.Clear();
                litFaces?.Clear();
                horizon?.Clear();
                openSet?.Clear();

                if (openSet?.Capacity < points.Count)
                {
                    openSet.Capacity = points.Count;
                }
            }

            if (!splitVerts)
            {
                if (hullVerts == null)
                {
                    hullVerts = new Dictionary<int, int>();
                }
                else
                {
                    hullVerts.Clear();
                }
            }
        }

        /// <summary>
        /// 초기 볼록 껍질 생성 메서드
        /// 입력 점들을 기반으로 초기 볼록 껍질의 면을 생성하고 openSet을 초기화.
        /// </summary>
        /// <param name="points"></param>
        bool GenerateInitialHull(List<Vector3> points)
        {
            // 본 알고리즘 내부의 openSet은 초기 볼록 껍질 알고리즘에서 점들이
            // 어떤 면에 속하는지 추적하고 처리 순서를 관리하는 데 사용됨
            //
            // 1. 점들의 관리: 초기 볼록 껍질 알고리즘에서 아직 처리되지 않은 점들을 관리
            //    openSet에 포함된 점들은 아직 어떤 볼록 껍질 면에도 할당되지 않은 상태.
            // 2. 면에 점 할당:
            //    openSet에 있는 각 점들은 현재 검토 중인 볼록 껍질의 면에 할당될 수 있음
            //    점이 특정 면에 할당되면 해당 면의 인덱스와 점과 면 사이의 거리가 openSet에 저장됨
            // 3. 알고리즘 진행:
            //    초기 볼록 껍질 생성 과정에서 openSet에 있는 각 점들은 순차적으로 처리되며
            //    각 점이 어떤 면에 할당될 수 있는지 검사하고,
            //    할당될 경우 해당 면의 정보가 업데이트됨.
            // 4. INSIDE 표시:
            //    초기 면에 속하는 점들은 openSet에 INSIDE로 표시
            //    이는 해당 점들이 초기 볼록 껍질의 외부에 있음을 나타냄.
            // 5. 업데이트와 제거:
            //    점이 특정 면에 할당되면 openSet에서 해당 점을 제거하거나, 다른 상태로 표시할 수 있음
            // 알고리즘이 진행됨에 따라 openSet에서 처리 대기 중인 점들이 점차 줄어들게 됨.

            // 초기 볼록 껍질을 구성할 점들의 인덱스 찾기.
            FindInitialHullIndices(points, out int b0, out int b1, out int b2, out int b3);

            // b0 ~ b3 중 하나라도 -1인 경우 처리
            if (b0 == -1 || b1 == -1 || b2 == -1 || b3 == -1)
            {
                // 초기 볼록 껍질을 생성할 수 없는 경우에 대한 처리
                return false;
                throw new InvalidOperationException("점의 개수가 충분하지 않거나 초기 볼록 껍질 생성이 불가능한 상황.");
            }

            // 찾은 인덱스에 해당하는 점들을 가져오기.
            Vector3 v0 = points[b0];
            Vector3 v1 = points[b1];
            Vector3 v2 = points[b2];
            Vector3 v3 = points[b3];

            // 초기 면이 위에 있는지 여부를 판단.
            bool above = Vector3.Dot(v3 - v1, Vector3.Cross(v1 - v0, v2 - v0)) > 0.0f;

            faceCount = 0;
            if (above)
            { // 초기 4개의 면 생성, 점들이 위에 있을 경우
                faces.Add(faceCount++, new ConvexFace(b0, b2, b1, 3, 1, 2, NormalPlane(points[b0], points[b2], points[b1])));
                faces.Add(faceCount++, new ConvexFace(b0, b1, b3, 3, 2, 0, NormalPlane(points[b0], points[b1], points[b3])));
                faces.Add(faceCount++, new ConvexFace(b0, b3, b2, 3, 0, 1, NormalPlane(points[b0], points[b3], points[b2])));
                faces.Add(faceCount++, new ConvexFace(b1, b2, b3, 2, 1, 0, NormalPlane(points[b1], points[b2], points[b3])));
            }
            else
            {
                faces.Add(faceCount++, new ConvexFace(b0, b1, b2, 3, 2, 1, NormalPlane(points[b0], points[b1], points[b2])));
                faces.Add(faceCount++, new ConvexFace(b0, b3, b1, 3, 0, 2, NormalPlane(points[b0], points[b3], points[b1])));
                faces.Add(faceCount++, new ConvexFace(b0, b2, b3, 3, 1, 0, NormalPlane(points[b0], points[b2], points[b3])));
                faces.Add(faceCount++, new ConvexFace(b1, b3, b2, 2, 0, 1, NormalPlane(points[b1], points[b3], points[b2])));
            }

            // openSet 초기화
            for (int i = 0; i < points.Count; i++)
            {
                // 초기 면을 구성하는 점들은 제외하고 openSet에 추가.
                if (i == b0 || i == b1 || i == b2 || i == b3)
                {
                    continue;
                }

                openSet.Add(new PointFace(i, UNASSIGNED, 0.0f));
            }

            // 초기 면을 INSIDE로 설정하여 openSetTail과 초기화.
            openSet.Add(new PointFace(b0, INSIDE, float.NaN));
            openSet.Add(new PointFace(b1, INSIDE, float.NaN));
            openSet.Add(new PointFace(b2, INSIDE, float.NaN));
            openSet.Add(new PointFace(b3, INSIDE, float.NaN));

            openSetTail = openSet.Count - 5;

            // openSet을 초기 설정된 면에 할당.
            for (int i = 0; i <= openSetTail; i++)
            {
                bool assigned = false;
                PointFace fp = openSet[i];

                // 각 점을 초기 면에 할당.
                for (int j = 0; j < 4; j++)
                {
                    // 면에 할당될 경우 openSet을 업데이트
                    ConvexFace face = faces[j];
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

                // 할당되지 않은 점은 INSIDE로 표시.
                if (!assigned)
                {
                    fp.Face = INSIDE;
                    fp.Distance = float.NaN;

                    // openSetTail과 교환하여 openSet을 업데이트.
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
            b0 = b1 = b2 = b3 = -1;

            // 초기 볼록 껍질을 생성할 수 있는 최소한의 점 개수는 4개이므로, 4개 미만일 경우 처리
            if (count < 4)
            {
                // 4개 미만일 경우 초기 볼록 껍질 생성이 불가능하므로, 임의의 값을 할당하거나 예외를 던지지 않고 종료
                return; // 혹은 다른 방법으로 처리
            }

            // 초기화
            bool found = false;

            // i0는 0부터 Continuity-4까지 반복
            for (int i0 = 0; i0 < count - 3; i0++)
            {
                Vector3 p0 = points[i0];

                // i1은 i0+1부터 Continuity-3까지 반복
                for (int i1 = i0 + 1; i1 < count - 2; i1++)
                {
                    Vector3 p1 = points[i1];

                    // 동일한 점인지 확인
                    if (p0 == p1)
                    {
                        continue;
                    }

                    // i2는 i1+1부터 Continuity-2까지 반복
                    for (int i2 = i1 + 1; i2 < count - 1; i2++)
                    {
                        Vector3 p2 = points[i2];

                        // 직선상인지 확인
                        if (AreCollinear(p0, p1, p2))
                        {
                            continue;
                        }

                        // i3은 i2+1부터 Continuity-1까지 반복
                        for (int i3 = i2 + 1; i3 < count; i3++)
                        {
                            Vector3 p3 = points[i3];

                            // 동일 평면 상의 점인지 확인
                            if (AreCoplanar(p0, p1, p2, p3))
                            {
                                continue;
                            }

                            // 조건을 만족하는 네 개의 점을 찾은 경우
                            b0 = i0;
                            b1 = i1;
                            b2 = i2;
                            b3 = i3;
                            found = true;
                            break;
                        }

                        if (found)
                        {
                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }
        }

        void GrowHull(List<Vector3> points)
        {
            // 가장 멀리 떨어진 점과 해당 점까지의 거리를 찾음
            int farthestPoint = 0;
            float dist = openSet[0].Distance;

            for (int i = 1; i <= openSetTail; i++)
            {
                if (openSet[i].Distance > dist)
                {
                    farthestPoint = i;
                    dist = openSet[i].Distance;
                }
            }

            // 가장 멀리 떨어진 점을 기준으로 새로운 면을 형성하기 위해 지형선을 찾음
            FindHorizon(
                points,
                points[openSet[farthestPoint].Point],
                openSet[farthestPoint].Face,
                faces[openSet[farthestPoint].Face]);

            // 새로운 지형선을 기반으로 원뿔을 구성하여 볼록 껍질의 현재 상태를 업데이트
            ConstructCone(points, openSet[farthestPoint].Point);

            // 지역화된 점을 재할당하여 새로운 볼록 껍질의 모습을 고려
            ReassignPoints(points);
        }

        void FindHorizon(List<Vector3> points, Vector3 point, int fi, ConvexFace face)
        {
            litFaces.Clear(); // 이미 검사한 면을 저장하는 리스트 초기화
            horizon.Clear();  // 찾은 지형선 엣지를 저장하는 리스트 초기화
            litFaces.Add(fi); // 현재 처리 중인 면 추가

            // face의 반대 면들을 검사하여 현재 점과의 거리를 계산하고, 경계선에 추가 또는 추가 검색
            {
                ConvexFace oppositeFace = faces[face.Opposite0]; // face의 첫 번째 반대 면 가져오기

                // 점과 반대 면 사이의 거리 계산
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    // 점이 반대 면의 평면에 위치하면, 이 면을 지형선에 추가
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite0,
                        Edge0 = face.Vertex1,
                        Edge1 = face.Vertex2,
                    });
                }
                else
                {
                    // 점이 반대 면의 평면 밖에 위치할 경우, 추가 검색 수행
                    SearchHorizon(points, point, fi, face.Opposite0, oppositeFace);
                }
            }

            // face의 두 번째 반대 면 검사
            if (!litFaces.Contains(face.Opposite1))
            {
                ConvexFace oppositeFace = faces[face.Opposite1]; // face의 두 번째 반대 면 가져오기

                // 점과 반대 면 사이의 거리 계산
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    // 점이 반대 면의 평면에 위치하면, 이 면을 지형선에 추가
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite1,
                        Edge0 = face.Vertex2,
                        Edge1 = face.Vertex0,
                    });
                }
                else
                {
                    // 점이 반대 면의 평면 밖에 위치할 경우, 추가 검색 수행
                    SearchHorizon(points, point, fi, face.Opposite1, oppositeFace);
                }
            }

            // face의 세 번째 반대 면 검사
            if (!litFaces.Contains(face.Opposite2))
            {
                ConvexFace oppositeFace = faces[face.Opposite2]; // face의 세 번째 반대 면 가져오기

                // 점과 반대 면 사이의 거리 계산
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    // 점이 반대 면의 평면에 위치하면, 이 면을 지형선에 추가
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite2,
                        Edge0 = face.Vertex0,
                        Edge1 = face.Vertex1,
                    });
                }
                else
                {
                    // 점이 반대 면의 평면 밖에 위치할 경우, 추가 검색 수행
                    SearchHorizon(points, point, fi, face.Opposite2, oppositeFace);
                }
            }
        }

        void SearchHorizon(List<Vector3> points, Vector3 point, int prevFaceIndex, int faceCount, ConvexFace face)
        {
            litFaces.Add(faceCount); // 현재 검색한 면을 litFaces에 추가하여 이미 검색한 면임을 표시

            int nextFaceIndex0;
            int nextFaceIndex1;
            int edge0;
            int edge1;
            int edge2;

            // 이전에 검색한 면(prevFaceIndex)과 현재 face가 가지고 있는 반대 면들을 가져옴
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
            else // prevFaceIndex == face.Opposite2
            {
                nextFaceIndex0 = face.Opposite0;
                nextFaceIndex1 = face.Opposite1;

                edge0 = face.Vertex1;
                edge1 = face.Vertex2;
                edge2 = face.Vertex0;
            }

            // 다음 검색할 면(nextFaceIndex0)이 litFaces에 없으면 검색 수행
            if (!litFaces.Contains(nextFaceIndex0))
            {
                ConvexFace oppositeFace = faces[nextFaceIndex0]; // 다음 검색할 반대 면 가져오기

                // 점과 반대 면 사이의 거리 계산
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    // 점이 반대 면의 평면에 위치하면, 이 면을 지형선(horizon)에 추가
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex0,
                        Edge0 = edge0,
                        Edge1 = edge1,
                    });
                }
                else
                {
                    // 점이 반대 면의 평면 밖에 위치할 경우, 추가 검색 수행
                    SearchHorizon(points, point, faceCount, nextFaceIndex0, oppositeFace);
                }
            }

            // 다음 검색할 면(nextFaceIndex1)이 litFaces에 없으면 검색 수행
            if (!litFaces.Contains(nextFaceIndex1))
            {
                ConvexFace oppositeFace = faces[nextFaceIndex1]; // 다음 검색할 반대 면 가져오기

                // 점과 반대 면 사이의 거리 계산
                float dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    // 점이 반대 면의 평면에 위치하면, 이 면을 지형선(horizon)에 추가
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex1,
                        Edge0 = edge1,
                        Edge1 = edge2,
                    });
                }
                else
                {
                    // 점이 반대 면의 평면 밖에 위치할 경우, 추가 검색 수행
                    SearchHorizon(points, point, faceCount, nextFaceIndex1, oppositeFace);
                }
            }
        }

        void ConstructCone(List<Vector3> points, int farthestPoint)
        {
            foreach (int fi in litFaces)
            {
                faces.Remove(fi); // litFaces에 포함된 모든 면을 제거
            }

            int firstNewFace = faceCount; // 새로운 면의 첫 인덱스 설정

            for (int i = 0; i < horizon.Count; i++)
            {
                int v0 = farthestPoint; // 가장 먼 점을 새로운 면의 첫 번째 정점으로 설정
                int v1 = horizon[i].Edge0; // 지형선의 첫 번째 엣지 정점
                int v2 = horizon[i].Edge1; // 지형선의 두 번째 엣지 정점

                int o0 = horizon[i].Face; // 지형선의 기존 면 인덱스
                int o1 = i == horizon.Count - 1 ? firstNewFace : firstNewFace + i + 1; // 다음 새로운 면의 인덱스
                int o2 = i == 0 ? firstNewFace + horizon.Count - 1 : firstNewFace + i - 1; // 이전 새로운 면의 인덱스

                int fi = faceCount++; // 새로운 면 인덱스 생성

                // 새로운 면 생성 및 faces에 추가
                faces[fi] = new ConvexFace(
                    v0, v1, v2,
                    o0, o1, o2,
                    NormalPlane(points[v0], points[v1], points[v2]));

                ConvexFace horizonFace = faces[horizon[i].Face]; // 기존 지형선 면 가져오기

                // 기존 지형선 면과 새로 생성한 면 사이의 관계 설정
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

                faces[horizon[i].Face] = horizonFace; // 업데이트된 기존 지형선 면을 faces에 다시 설정
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

                    foreach (KeyValuePair<int, ConvexFace> kvp in faces)
                    {
                        int fi = kvp.Key;
                        ConvexFace face = kvp.Value;

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
    ref List<Vector3> verts,
    ref List<int> tris,
    ref List<Vector3> normals)
        {
            // 정점 리스트 초기화
            if (verts == null)
            {
                verts = new List<Vector3>();
            }
            else
            {
                verts.Clear();
            }

            // 삼각형 인덱스 리스트 초기화
            if (tris == null)
            {
                tris = new List<int>();
            }
            else
            {
                tris.Clear();
            }

            // 법선 벡터 리스트 초기화
            if (normals == null)
            {
                normals = new List<Vector3>();
            }
            else
            {
                normals.Clear();
            }

            // 각 면에 대해 처리
            foreach (ConvexFace face in faces.Values)
            {
                int vi0, vi1, vi2;

                // 정점 분리 여부에 따라 처리
                if (splitVerts)
                {
                    // 정점을 분리하여 추가
                    vi0 = verts.Count;
                    verts.Add(points[face.Vertex0]);

                    vi1 = verts.Count;
                    verts.Add(points[face.Vertex1]);

                    vi2 = verts.Count;
                    verts.Add(points[face.Vertex2]);

                    // 각 정점의 법선 벡터 추가
                    normals.Add(face.Normal);
                    normals.Add(face.Normal);
                    normals.Add(face.Normal);
                }
                else
                {
                    // 정점을 분리하지 않고, 이미 추가된 정점이 있는지 확인하고 추가
                    if (!hullVerts.TryGetValue(face.Vertex0, out vi0))
                    {
                        vi0 = verts.Count;
                        hullVerts[face.Vertex0] = vi0;
                        verts.Add(points[face.Vertex0]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex1, out vi1))
                    {
                        vi1 = verts.Count;
                        hullVerts[face.Vertex1] = vi1;
                        verts.Add(points[face.Vertex1]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex2, out vi2))
                    {
                        vi2 = verts.Count;
                        hullVerts[face.Vertex2] = vi2;
                        verts.Add(points[face.Vertex2]);
                    }
                }

                // 삼각형 인덱스 추가
                tris.Add(vi0);
                tris.Add(vi1);
                tris.Add(vi2);
            }
        }

        float PointFaceDistance(Vector3 point, Vector3 pointOnFace, ConvexFace face)
        {
            return Vector3.Dot(face.Normal, point - pointOnFace);
        }

        Vector3 NormalPlane(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        }

        bool AreCoincident(Vector3 a, Vector3 b)
        {
            return (a - b).Length <= EPSILON;
        }

        bool AreCollinear(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(c - a, c - b).Length <= EPSILON;
        }

        bool AreCoplanar(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            // Calculate norID vectors
            Vector3 n1 = Vector3.Cross(c - a, c - b);
            Vector3 n2 = Vector3.Cross(d - a, d - b);

            // Calculate.Lengths of norID vectors
            float m1 = n1.Length;
            float m2 = n2.Length;

            // Check coplanarity conditions
            const float epsilon = 1e-10f;
            if (Math.Abs(m1) < epsilon || Math.Abs(m2) < epsilon)
            {
                return true; // Nearly zero.Length vectors
            }
            return false;
        }

        static bool AreCollinear(Vector3 v1, Vector3 v2)
        {
            // Implement collinearity check
            const float epsilon = 1e-10f;
            return Vector3.Cross(v1, v2).Length < epsilon;
        }
    }
}
