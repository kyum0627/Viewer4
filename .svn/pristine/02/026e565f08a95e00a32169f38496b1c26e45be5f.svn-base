using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace IGX.Geometry.Tessellation
{
    // 정점의 index만 가지고 만든 circular doubly linked list와
    // 재귀 호출 방식으로 삼각화 속도를 매우 빠르게 함
    // 아래 site 참고 (JAVA code)하여 C#으로 conversion
    // https://github.com/earcut4j/earcut4j/blob/master/src/main/java/earcut4j/Earcut.java
    // 변수 및 함수 이름이 직관적이지 않아 바꾸고...
    // 오류가 많이 있어서 고침 2022.01.11 HJJO
    // 아직 hashed order 사용 불가
    public class EarClipping
    {
        /// <summary>
        /// only for MeshGeometry generation....
        /// circular doubly linked list를 구현하기 위해 만들어짐
        /// 각 점의 id만 가지고 있어서 상대적으로 매우 가벼움
        /// </summary>
        public class Node
        {
            public int id;   // node MeshID
            public double X; // node coordinate
            public double Y; // node coordinate

            // https://en.wikipedia.org/wiki/Z-order
            // z-order ear clipping 은 아직 구현되지 않음
            // 알고리즘에 오류가 있어서 z-order를 계산해도 사용하지 않음
            public long Z; // node coordinate // for z-order

            //https://en.wikipedia.org/wiki/Steiner_point_(triangle)
            // 삼각형의 외접원의 중심을 O라고 하고 K를 symmedian point라고 할 때
            // 선분 OK를 지름으로하는 원을 brocard circle이라고 하며,
            // 삼각형의 각변에 수직하며 외접원의 원점 O를 지나는 두 직선과
            // brocard circle이 만나는 세 점을 각각 연결하면 
            // brocard circle 내부의 삼각형을 구할 수 있음
            // ....
            // 이렇게 만든 세 직선은 한 점에서 만나는데 이를 steiner point라고 함
            public bool steiner; // 이 점이 steiner point인지 여부

            public Node? prev;
            public Node? next;
            public Node? prevZ; // zorder를 고려한 prev node
            public Node? nextZ; // zorder를 고려한 p1ID node

            public Node(int index, double x, double y)
            {
                id = index;
                X = x;
                Y = y;
                prev = null;
                next = null;
                Z = long.MinValue; // node를 z-order로 정렬하기 위한 목적
                prevZ = null;
                nextZ = null;
                steiner = false;
            }

            public Node(int i, Vector2 v)
            {
                id = i;
                X = v.X;
                Y = v.Y;
                prev = null;
                next = null;
                Z = long.MinValue;
                prevZ = null;
                nextZ = null;
                steiner = false;
            }
        }

        public List<int>? triangles; // tirangluation results
        public int[]? polygon;

        /// <summary>
        /// hole 이 없는 polygon triangulation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dim">data dimension, default = 2</param>
        /// <returns></returns>
        public List<int> Triangulate(double[] data, int dim = 2)
        {
            return Triangulate(data, null, dim);
        }

        /// <summary>
        /// hole이 있는 polygon triangulation
        /// nID = outter polygon을 구성하는 정점의 수 + sum of 내부의 hole 정점들의 수
        /// </summary>
        /// <param name="points">polygon + hole의 좌표, v0, v1, ..., vn </param>
        /// <param name="holeIndices">points중 hole의 index 시작, 끝, 시작, 끝</param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public List<int> Triangulate(List<Vector2> points, int[] holeIndices, int dim = 2)
        {
            double[] data = new double[points.Count * 2];
            for (int i = 0; i < points.Count; i++)
            {
                data[i * 2] = points[i].X;
                data[(i * 2) + 1] = points[i].Y;
            }

            return Triangulate(data, holeIndices, dim);
        }

        /// <summary>
        /// hole이 있는 polygon triangulation
        /// nID = outter polygon을 구성하는 정점의 수 + sum of 내부의 hole 정점들의 수
        /// </summary>
        /// <param name="data">polygon + hole의 flat 좌표, x0, y0, x1, y1, .., xn, yn </param>
        /// <param name="holeIndices">points중 hole의 index 시작, 끝, 시작, 끝</param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public List<int> Triangulate(double[] data, int[]? holeIndices, int dim)
        {
            // holeIndices == null 이면 hole이 없음
            bool hasHoles = holeIndices?.Length > 0;
            int outerLen = hasHoles ? holeIndices![0] * dim : data.Length;

            Node? outerNode = DoubleLinkedList(data, 0, outerLen, dim, true);

#if DEBUG
            //int[] poly = GetPolygon(outerNode);
#endif
            triangles = new List<int>();

            if (outerNode == null || outerNode.next == outerNode.prev)
            {
                return triangles;
            }

            double minX = 0;
            double minY = 0;
            double maxX = 0;
            double maxY = 0;
            double invSize = double.MinValue;

            if (hasHoles)
            {
                outerNode = ElininateAllHoles(data, holeIndices!, outerNode, dim);
            }
#if DEBUG
            //polygon = GetPolygon(outerNode);
#endif
            // 복잡한 contour의 성능 향상을 위해.... hash ...  
            // 오류가 많아 제거
            if (data.Length > 80 * dim)
            {
                minX = maxX = data[0];
                minY = maxY = data[1];

                for (int i = dim; i < outerLen; i += dim)
                {
                    double x = data[i];
                    double y = data[i + 1];
                    if (x < minX)
                    {
                        minX = x;
                    }

                    if (y < minY)
                    {
                        minY = y;
                    }

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }
                invSize = Math.Max(maxX - minX, maxY - minY);
                invSize = invSize != 0.0 ? 1.0 / invSize : 0.0;
            }

            EarCut(outerNode, triangles, dim, minX, minY, invSize, int.MinValue);

            return triangles;
        }

        /// <summary>
        /// data set을 linked list Node로 변환
        /// </summary>
        /// <param name="data">data set = coordinates xi, yi, ...</param>
        /// <param name="start">start MeshID</param>
        /// <param name="end">end MeshID</param>
        /// <param name="dim">dimension</param>
        /// <param name="clockwise">true 이면 데이터 입력 순서 그대로 node indexing & insert</param>
        /// <returns>last Node</returns>
        private static Node? DoubleLinkedList(double[] data, int start, int end, int dim, bool clockwise)
        {
            Node? last = null;
            int k = 0;
            if (clockwise == SignedArea(data, start, end, dim) > 0)
            {
                for (int i = start; i < end; i += dim)
                {
                    last = AddNode(i, data[i], data[i + 1], last);
                    k++;
                }
            }
            else
            {
                for (int i = end - dim; i >= start; i -= dim)
                {
                    last = AddNode(i, data[i], data[i + 1], last);
                    k++;
                }
            }

            if (last != null && Equals(last, last.next))
            {
                //RemoveNode(last);
                last = last.next;
            }

            return last;
        }

        /// <summary>
        /// colinear point를 nodel list에서 거르기, steiner point로 지정하면 거르는 대상에서 제외
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static Node? FilterColinearPoints(Node? start, Node? end)
        {
            if (start == null)
            {
                return start;
            }

            if (end == null)
            {
                end = start;
            }

            Node? p = start;
            bool again;

            do
            {
                again = false;

                if (!p!.steiner && (Equals(p, p.next) || Area(p!.prev!, p, p!.next!) == 0))
                {
                    RemoveNode(p!);
                    p = end = p!.prev;
                    if (p == p!.next)
                    {
                        break;
                    }

                    again = true;
                }
                else
                {
                    p = p.next;
                }
            } while (again || p != end);

            return end;
        }

        /// <summary>
        /// ear vertex이면 triangle을 생성하고 해당 정점을 search 대상에서 차례대로 제외
        /// </summary>
        /// <param name="ear">ear 여부를 검사할 시작 정점</param>
        /// <param name="triangles">tesslation 결과 삼각형 list</param>
        /// <param name="dim"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="invSize"></param>
        /// <param name="pass"></param>
        private void EarCut(Node? ear, List<int> triangles, int dim, double minX, double minY, double invSize, int pass)
        {
            if (ear == null)
            {
                return;
            }

            // z-order indexing & linking
            if (pass == int.MinValue && invSize != double.MinValue)
            {
                IndexCurve(ear, minX, minY, invSize);
            }

            Node? stop = ear;
            // ear를 찾아서 하나씩 제거, 삼각형 생성
            while (ear!.prev != ear.next)
            {
                Node? prev = ear.prev;
                Node? next = ear.next;

                if (IsEar(ear))
                {
                    {
                        // data set이 flat 하게 입력되어 있으므로 node id는 dimension으로 나눈 몫
                        triangles.Add(prev!.id / dim);
                        triangles.Add(ear.id / dim);
                        triangles.Add(next!.id / dim);

                        RemoveNode(ear);

                        // 아주 예리하게 만들어지는 삼각형(sliver contour_vertex_indices)을 회피하기 위해
                        // 건너 뛰면서 탐색
                        // p3가 ear로 제거될 때 p0ID p1ID p4가 ear가 되지만 거의 colinear한 경우
                        // 매우 예리한 silver triangle이 생성될 가능성이 있음
                        //
                        //  p0ID    p1ID   p4       p5       p6
                        //  o------o   o--------o.........o
                        //          \ /
                        //           o
                        //           p3          
                        // ear p3를 제거하면 p0ID, p1ID, p4가 colinear 하므로 ... p5를 먼저 탐색

                        ear = next!.next;
                        stop = next!.next;

                        continue;
                    }
                }

                ear = next;

                // 잔여 points로 구성된 polygon 오류 점검을 통해 더 이상 ear를 찾지 못하면 stop
                if (ear == stop)
                {
                    // 먼저 colinear point가 있는지 조사해서 제거한 후 재 탐색
                    if (pass == int.MinValue)
                    {
                        EarCut(FilterColinearPoints(ear, null), triangles, dim, minX, minY, invSize, 1);
                    }
                    else if (pass == 1)
                    { // 못 찾으면 남은 polygon에 self-intersections이 있는지 검사해서 해당 segment 제거
                        ear = CureLocalIntersections(FilterColinearPoints(ear, null), triangles, dim);
                        EarCut(ear, triangles, dim, minX, minY, invSize, 2);
                    }
                    else if (pass == 2)
                    { // 못 찾으면 남은 polygon을 둘로 분할해서 재탐색
                        Split(ear, triangles, dim, minX, minY, invSize);
                    }

                    break;
                }
            }
        }

        private void EarCutHashed(Node? ear, List<int> triangles, int dim, double minX, double minY, double invSize, int pass)
        {
            if (ear == null)
            {
                return;
            }

            // z-order indexing & linking
            if (pass == int.MinValue && invSize != double.MinValue)
            {
                IndexCurve(ear, minX, minY, invSize);
            }

            //Node stop = ear;
            Node? prev = ear.prev;
            Node? next = ear.prev;
            // ear를 찾아서 하나씩 제거, 삼각형 생성
            while (ear!.prev != ear.next)
            {
                prev = ear.prev;
                next = ear.next;

                //if (invSize != 0 ? IsEarHashed(ear, minX, minY, invSize) : IsEar(ear))
                if (IsEarHashed(ear, minX, minY, invSize))
                {
                    // data set이 flat 하게 입력되어 있으므로 node id는 dimension으로 나눈 몫
                    triangles.Add(prev!.id / dim);
                    triangles.Add(ear!.id / dim);
                    triangles.Add(next!.id / dim);

                    RemoveNode(ear);

                    // 아주 예리하게 만들어지는 삼각형(sliver contour_vertex_indices)을 회피하기 위해
                    // 건너 뛰면서 탐색
                    // p3가 ear로 제거될 때 p0ID p1ID p4가 ear가 되지만 거의 colinear한 경우
                    // 매우 예리한 silver triangle이 생성될 가능성이 있음
                    //
                    //  p0ID    p1ID   p4       p5       p6
                    //  o------o   o--------o.........o
                    //          \ /
                    //           o
                    //           p3          
                    // ear p3를 제거하면 p0ID, p1ID, p4가 colinear 하므로 ... p5를 먼저 탐색

                    ear = next.next;
                    //stop = p1ID.p1ID;

                    continue;
                }
                ear = next;
            }

            if (prev == next)
            {
                return;
            }

            // 먼저 colinear point가 있는지 조사해서 제거한 후 재 탐색
            if (pass == int.MinValue)
            {
                EarCutHashed(FilterColinearPoints(ear, null), triangles, dim, minX, minY, invSize, 1);
            }
            else if (pass == 1)
            { // 못 찾으면 남은 polygon에 self-intersections이 있는지 검사해서 해당 segment 제거
                ear = CureLocalIntersections(FilterColinearPoints(ear, null), triangles, dim);
                EarCutHashed(ear, triangles, dim, minX, minY, invSize, 2);
            }
            else if (pass == 2)
            { // 못 찾으면 남은 polygon을 둘로 분할해서 재탐색
                Split(ear, triangles, dim, minX, minY, invSize);
            }
        }

        private static bool IsConvex(Node ear)
        {
            Node? a = ear.prev, b = ear, c = ear.next;
            if (Area(a!, b, c!) >= 0)
            {
                return false; // reflex
            }

            return true;
        }

        private bool IsEar(Node ear)
        {
            if (!IsConvex(ear))
            {
                return false;
            }

            Node? a = ear.prev, b = ear, c = ear.next;

            // convex이나 이를 중심으로하는 삼각형 내부에 다른 정점이 있으면 ear가 아님
            Node? p = ear.next!.next;
            while (p != ear.prev)
            {
                // 속도향상 목적으로 추가 HJJO
                if (p!.Equals(a) || p.Equals(b) || p.Equals(c))
                {
                    p = p.next;
                }

                if (PointInTriangle(a!.X, a.Y, b.X, b.Y, c!.X, c.Y, p!.X, p.Y) &&
                    Area(p.prev!, p, p.next!) >= 0)
                {
                    return false;
                }

                p = p.next;
            }
            return true;
        }

        private static bool EarCheck(Node? a, Node? b, Node? c, Node? prev, Node? p, Node? next)
        {
            return p!.id != a!.id &&
                p.id != c!.id &&
                PointInTriangle(a.X, a.Y, b!.X, b.Y, c.X, c.Y, p.X, p.Y) &&
                Area(prev!, p, next!) >= 0;
        }

        // hashed ear 오류 많음. 제거 2022.01.11
        private static bool IsEarHashed(Node ear, double minX, double minY, double invSize)
        {
            Node? a = ear.prev;
            Node? b = ear;
            Node? c = ear.next;

            if (Area(a!, b, c!) >= 0)
            {
                return false; // reflex, can't be an ear
            }

            // triangle bbox; min & max are calculated like this for speed
            double minTX = a!.X < b.X ? a.X < c!.X ? a.X : c.X : b.X < c!.X ? b.X : c.X;
            double minTY = a.Y < b.Y ? a.Y < c.Y ? a.Y : c.Y : b.Y < c.Y ? b.Y : c.Y;
            double maxTX = a.X > b.X ? a.X > c.X ? a.X : c.X : b.X > c.X ? b.X : c.X;
            double maxTY = a.Y > b.Y ? a.Y > c.Y ? a.Y : c.Y : b.Y > c.Y ? b.Y : c.Y;

            // z-order range for the current triangle bbox;
            long minZ = Zorder(minTX, minTY, minX, minY, invSize);
            long maxZ = Zorder(maxTX, maxTY, minX, minY, invSize);

            Node? p = ear.prevZ;
            Node? n = ear.nextZ;

            //Node l1 = new Node(ear.MeshID, ear.X, ear.Y);
            // look for ControlPoints inside the triangle in both directions
            while (p != null && p.Z >= minZ && n != null && n.Z <= maxZ)
            {
                if (EarCheck(ear.prev!, ear, ear.next!, p.prev!, p, p.next!))
                {
                    return false;
                }

                p = p.prevZ;
                if (EarCheck(ear.prev!, ear, ear.next!, n.prev!, n, n.next!))
                {
                    return false;
                }

                n = n.nextZ;
            }

            //pID.Z = minZ - 1;
            // look for remaining ControlPoints in decreasing z-order
            while (p != null && p.Z >= minZ)
            {
                if (EarCheck(ear.prev!, ear, ear.next!, p.prev!, p, p.next!))
                {
                    return false;
                }

                p = p.prevZ;
            }

            // look for remaining ControlPoints in increasing z-order
            //pID.Z = minZ + 1;
            while (n != null && n.Z <= maxZ)
            {
                if (EarCheck(ear.prev!, ear, ear.next!, n.next!.prev!, n.next, n.next.next!))
                {
                    return false;
                }

                n = n.nextZ;
            }

            return true;
        }

        private Node? CureLocalIntersections(Node? start, List<int> triangles, int dim)
        {
            Node? p = start;
            do
            {
                Node? a = p!.prev;
                Node? b = p.next!.next;

                if (!Equals(a, b) &&
                    AreIntersects(a!, p, p.next, b!) &&
                    LocallyInside(a!, b!) &&
                    LocallyInside(b!, a!))
                {
                    triangles.Add(a!.id / dim);
                    triangles.Add(p!.id / dim);
                    triangles.Add(b!.id / dim);

                    // 서로 교차하는 두 node 제거
                    RemoveNode(p);
                    RemoveNode(p.next);

                    p = start = b;
                }
                p = p.next;
            } while (p != start);

            return FilterColinearPoints(p, null);
        }

        private void Split(Node? start, List<int> triangles, int dim, double minX, double minY, double size)
        {
            Node? a = start;
            do
            {
                Node b = a!.next!.next!;
                while (b != a.prev)
                {
                    if (a.id != b!.id && IsValidDiagonal(a, b))
                    {
                        Node? c = SplitPolygon(a, b);
                        a = FilterColinearPoints(a, a.next);
                        c = FilterColinearPoints(c, c.next);
                        EarCut(a, triangles, dim, minX, minY, int.MinValue, int.MinValue);
                        EarCut(c, triangles, dim, minX, minY, int.MinValue, int.MinValue);
                        return;
                    }
                    b = b.next!;
                }
                a = a.next;
            } while (a != start);
        }

        private Node ElininateAllHoles(double[] data, int[] holeIndices, Node outerNode, int dim)
        {
            List<Node> queue = new();

            int len = holeIndices.Length;
            int start, end;
            Node? list;
#if DEBUG
            //int[] poly = GetPolygon(outerNode);
#endif
            for (int i = 0; i < len; i++)
            {
                start = holeIndices[i] * dim;
                // 오류 수정 holeIndices[i + 1] --> holeIndices[i + 1] + 1
                end = i < len - 1 ? (holeIndices[i + 1] + 1) * dim : data.Length;
                list = DoubleLinkedList(data, start, end, dim, false);
                if (list == list!.next)
                {
                    list.steiner = true;
                }
                // 오류 수정 2022.01.11
                if (i % 2 == 0)
                {
                    queue.Add(GetLeftMost(list));
                }
            }

            // 오류 수정, y 값도 비교해야 함
            queue.Sort((a, b) => Comp(a, b));

            for (int i = 0; i < queue.Count; i++)
            {
                outerNode = EliminateHole(queue[i], outerNode);
#if DEBUG
                //poly = GetPolygon(outerNode);
#endif
            }

            return outerNode;
        }

        private int Comp(Node a, Node b)
        {
            if (a.X < b.X)
            {
                return -1;
            }
            else if (a.X == b.X)
            {
                if (a.Y < b.Y)
                {
                    return -1;
                }
                else if (a.Y == b.Y)
                {
                    return 0;
                }
            }
            return 1;
        }

        private Node EliminateHole(Node hole, Node outerNode)
        {
            // leftmost point에서 시작....
            Node? onHull = FindBridgeOnPolygon(hole, outerNode);
            if (onHull == null)
            {
                return outerNode;
            }

            Node bridgeReverse = SplitPolygon(onHull, hole);

            return bridgeReverse;
        }

        // hole과 outter polygon간 연결 bridge를 찾기 위한
        // David Eberly's algorithm.
        // 오류 수정 2022.01.11
        // https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
        private static Node? FindBridgeOnPolygon(Node M, Node outerNode)
        {
            Node closestSegP0 = outerNode;
            double hx = M.X;
            double hy = M.Y;
            double qx = -double.MaxValue;
            Node? P = null;

            // hole의 leftmost point에서 시작해서 left로 뻗는 ray와 outter polygon을 구성하는
            // segment가 만나면 potential connection point로 설정
            do
            {
                if (hy <= closestSegP0!.Y && hy >= closestSegP0.next!.Y && closestSegP0.next.Y != closestSegP0.Y)
                { // hole의 y 값이 폴리곤 정점 p와 pnext의 각 정점의 y좌표 범위내에 있고 pID -> pnext를 연결하는 세그먼트가 수평하지 않으면
                    double x = closestSegP0.X + ((hy - closestSegP0.Y) * (closestSegP0.next.X - closestSegP0.X) / (closestSegP0.next.Y - closestSegP0.Y)); // 교차점의 x 좌표
                    if (x <= hx && x > qx)
                    { // 교차점의 x 값이 hole의 leftmost X와 Xmin 사이에 있으면, potentially 선택 가능
                        qx = x; // xmin 재설정, hole과 가장 가까운 polygon의 세그먼트를 찾기 위해서...
                        if (x == hx)
                        {
                            if (hy == closestSegP0.Y)
                            {
                                return closestSegP0; // 선분의 시작점이 hole의 leftmost와 같으면 이 점에서 hole과 polygon이 서로 접함
                            }

                            if (hy == closestSegP0.next.Y)
                            {
                                return closestSegP0.next; // 선분의 끝점이 hole의 leftmost와 같으면 이 점에서 hole과 polygon이 서로 접함
                            }
                        }
                        // 가장 일반적인 case
                        // X 값이 작은 점을 P로 선택
                        P = closestSegP0.X < closestSegP0.next.X ? closestSegP0 : closestSegP0.next;
                    }
                }
                closestSegP0 = closestSegP0.next!;
            } while (closestSegP0 != outerNode);

            if (P == null)
            {
                return null; // hole이 polygon의 내부에 존재하지 않으므로 연결 불가
            }

            if (hx == qx)
            {
                return P; // hole과 polygon이 서로 접함
            }

            // visiable point 찾기
            Node stop = P;
            double mx = P.X;
            double my = P.Y;
            double tanMin = double.MaxValue;
            double tan;

            closestSegP0 = P;
            do
            {
                if (hx >= closestSegP0!.X && closestSegP0.X >= mx && hx != closestSegP0.X &&
                    PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, closestSegP0.X, closestSegP0.Y))
                { // convex point인 경우
                    tan = Math.Abs(hy - closestSegP0.Y) / (hx - closestSegP0.X); // tangential
                    // 내부에 reflex point가 없는지 조사
                    // 내각이 가장 작은 정점을 연결할 정점으로 선택
                    if (LocallyInside(closestSegP0, M) &&
                        (tan < tanMin || (tan == tanMin && (closestSegP0.X > P.X || (closestSegP0.X == P.X && SectorContainsSector(P, closestSegP0))))))
                    {
                        P = closestSegP0;
                        tanMin = tan;
                    }
                }
                closestSegP0 = closestSegP0.next!;
            } while (closestSegP0 != stop);

            // 드디어 찾은 폴리곤 상의 연결점
            return P;
        }

        /// <summary>
        /// Node m과 Node p의 각 요소간에 생성되는 삼각 sector중 작은 것을 찾기 위함
        /// 즉, 내각 비교 목적
        /// </summary>
        /// <param name="m"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool SectorContainsSector(Node m, Node p)
        {
            return Area(m.prev!, m, p.prev!) < 0 && Area(p.next!, m, m.next!) < 0;
        }

        /// <summary>
        /// z-order indexing
        /// </summary>
        /// <param name="start"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="invSize"></param>
        private void IndexCurve(Node start, double minX, double minY, double invSize)
        {
            Node? p = start;
            do
            {
                if (p!.Z == double.MinValue)
                {
                    p.Z = Zorder(p.X, p.Y, minX, minY, invSize);
                }

                p.prevZ = p.prev;
                p.nextZ = p.next;
                p = p.next;
            } while (p != start);

            p.prevZ!.nextZ = null;
            p.prevZ = null;

            SortLinked(p);
        }

        private Node? SortLinked(Node? list)
        {
            int inSize = 1;
            int numMerges;
            do
            {
                Node? p = list;
                list = null;
                Node? tail = null;
                numMerges = 0;

                while (p != null)
                {
                    numMerges++;
                    Node? q = p;
                    int pSize = 0;
                    for (int i = 0; i < inSize; i++)
                    {
                        pSize++;
                        q = q.nextZ;
                        if (q == null)
                        {
                            break;
                        }
                    }
                    int qSize = inSize;
                    while (pSize > 0 || (qSize > 0 && q != null))
                    {
                        Node e;
                        if (pSize != 0 && (qSize == 0 || q == null || p!.Z <= q.Z))
                        {
                            e = p!;
                            p = p!.nextZ;
                            pSize--;
                        }
                        else
                        {
                            e = q!;
                            q = q!.nextZ;
                            qSize--;
                        }

                        if (tail != null)
                        {
                            tail.nextZ = e;
                        }
                        else
                        {
                            list = e;
                        }

                        e.prevZ = tail;
                        tail = e;
                    }

                    p = q;
                }

                tail!.nextZ = null;
                inSize *= 2;
            } while (numMerges > 1);

            return list;
        }

        private bool IsValidDiagonal(Node a, Node b)
        {
            return
                a.next!.id != b.id &&
                a.prev!.id != b.id &&
                !IntersectPolygon(a, b) && // 다른 edge와 교차하지 않고
                    ((LocallyInside(a, b) &&
                    LocallyInside(b, a) &&
                    MiddleInside(a, b) && // locally visible하며
                    (Area(a.prev, a, b.prev!) != 0 ||
                    Area(a, b.prev!, b) != 0)) || // 다른 sector를 만들지 않으며
                    (Equals(a, b) &&
                    Area(a.prev, a, a.next) > 0 &&
                    Area(b.prev!, b, b.next!) > 0)); // 두 점이 겹치지 않으면
        }

        private bool IntersectPolygon(Node a, Node b)
        {
            Node p = a;
            do
            {
                if (p.id != a.id &&
                    p.next!.id != a.id &&
                    p.id != b.id &&
                    p.next.id != b.id &&
                    AreIntersects(p, p.next, a, b))
                {
                    return true;
                }
                p = p.next!;
            } while (p != a);

            return false;
        }

        private bool AreIntersects(Node p1, Node q1, Node p2, Node q2)
        {
            if ((Equals(p1, p2) && Equals(q1, q2)) || (Equals(p1, q2) && Equals(p2, q1)))
            {
                return true;
            }

            double o1 = Math.Sign(Area(p1, q1, p2));
            double o2 = Math.Sign(Area(p1, q1, q2));
            double o3 = Math.Sign(Area(p2, q2, p1));
            double o4 = Math.Sign(Area(p2, q2, q1));

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == 0 && P2IsOnSegP1P3(p1, p2, q1))
            {
                return true;
            }

            if (o2 == 0 && P2IsOnSegP1P3(p1, q2, q1))
            {
                return true;
            }

            if (o3 == 0 && P2IsOnSegP1P3(p2, p1, q2))
            {
                return true;
            }

            return o4 == 0 && P2IsOnSegP1P3(p2, q1, q2);
        }

        private bool P2IsOnSegP1P3(Node p1, Node p2, Node p3)
        {
            return
                p2.X <= Math.Max(p1.X, p3.X) &&
                p2.X >= Math.Min(p1.X, p3.X) &&
                p2.Y <= Math.Max(p1.Y, p3.Y) &&
                p2.Y >= Math.Min(p1.Y, p3.Y);
        }

        // point의 각 좌표 값을 bit화하여 blending하여 z-order 값 생성
        private static long Zorder(double x, double y, double minX, double minY, double invSize)
        {
            long lx = (long)((x - minX) * invSize);
            long ly = (long)((y - minY) * invSize);

            long xy = (lx << 32) | ly;
            xy = (xy | (xy << 8)) & 0x00FF00FF00FF00FF;
            xy = (xy | (xy << 4)) & 0x0F0F0F0F0F0F0F0F;
            xy = (xy | (xy << 2)) & 0x3333333333333333;
            xy = (xy | (xy << 1)) & 0x5555555555555555;
            return (xy >> 32) | (ly << 1);
        }

        private static bool Equals(Node? p1, Node? p2)
        {
            return p1?.X == p2?.X && p1?.Y == p2?.Y;
        }

        private static double Area(Node p, Node q, Node r)
        {
            return ((q.Y - p.Y) * (r.X - q.X)) - ((q.X - p.X) * (r.Y - q.Y));
        }

        private static Node SplitPolygon(Node a, Node b)
        {
            // 0 ~ 5 까지 index된 6개의 points로 구성된 circulat doubly linked list를 가정
            // 0 1 2 3 4 5 0 1 2 ...
            // 1과 4를 기준으로 두 개의 list로 자르면 다음과 같은 두개의 circulat doubly linked list가 생성되어야 함...
            // ... 4 5 0 1, 4 5 0 1...
            // ... 1 2 3 4, 1 2 3 4...
            // 그러려면
            // 1. 첫째 cycle은 4 5 0 1 ...
            // 2. 둘째 cycle은
            //    index 4와 1을 복제한 새로운 노드(4-->6, 1-->7)가 두 개 더 필요
            //    즉, ... 2 3 6(4) 7(1) 2 3 6 7

            Node a2 = new(a.id, a.X, a.Y);
            Node b2 = new(b.id, b.X, b.Y);
            Node an = a.next!;
            Node bp = b.prev!;

            a.next = b;
            b.prev = a;

            a2.next = an;
            an.prev = a2;

            b2.next = a2;
            a2.prev = b2;

            bp.next = b2;
            b2.prev = bp;

            return b2;
        }

        private static bool LocallyInside(Node a, Node b)
        {
            return Area(a.prev!, a, a.next!) < 0 ?
                Area(a, b, a.next!) >= 0 && Area(a, a.prev!, b) >= 0 :
                Area(a, b, a.prev!) < 0 || Area(a, a.next!, b) < 0;
        }

        private bool MiddleInside(Node a, Node b)
        {
            Node p = a;
            bool inside = false;
            double px = (a.X + b.X) / 2;
            double py = (a.Y + b.Y) / 2;
            do
            {
                if (p.Y > py != p.next!.Y > py && px < ((p.next.X - p.X) * (py - p.Y) / (p.next.Y - p.Y)) + p.X)
                {
                    inside = !inside;
                }

                p = p.next;
            } while (p != a);

            return inside;
        }

        private static bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
        {
            return
                ((cx - px) * (ay - py)) >= ((ax - px) * (cy - py)) &&
                ((ax - px) * (by - py)) >= ((bx - px) * (ay - py)) &&
                ((bx - px) * (cy - py)) >= ((cx - px) * (by - py));
        }

        private static Node GetLeftMost(Node start)
        {
            Node p = start;
            Node leftmost = start;
            do
            {
                if (p.X < leftmost.X ||
                    (p.X == leftmost.X && p.Y < leftmost.Y))
                {
                    leftmost = p;
                }

                p = p.next!;
            } while (p != start);
            return leftmost;
        }

        private static void RemoveNode(Node p)
        {
            p.next!.prev = p.prev;
            p.prev!.next = p.next;

            if (p.prevZ != null)
            {
                p.prevZ.nextZ = p.nextZ;
            }
            if (p.nextZ != null)
            {
                p.nextZ.prevZ = p.prevZ;
            }
        }

        private static Node AddNode(int i, double x, double y, Node? last)
        {
            Node p = new(i, x, y);

            if (last == null)
            {
                p.prev = p;
                p.next = p;
            }
            else
            {
                p.next = last.next;
                p.prev = last;
                last.next!.prev = p;
                last.next = p;
            }
            return p;
        }

        public double Deviation(double[] data, int[] holeIndices, int dim, List<int> triangles)
        {
            bool hasHoles = holeIndices?.Length > 0;
            int outerLen = hasHoles ? holeIndices![0] * dim : data.Length;

            double polygonArea = Math.Abs(SignedArea(data, 0, outerLen, dim));
            if (hasHoles)
            {
                for (int i = 0, len = holeIndices!.Length; i < len; i++)
                {
                    int start = holeIndices[i] * dim;
                    int end = i < len - 1 ? holeIndices[i + 1] * dim : data.Length;
                    polygonArea -= Math.Abs(SignedArea(data, start, end, dim));
                }
            }

            double trianglesArea = 0;
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = triangles[i] * dim;
                int b = triangles[i + 1] * dim;
                int c = triangles[i + 2] * dim;
                trianglesArea += Math.Abs(
                        ((data[a] - data[c]) * (data[b + 1] - data[a + 1])) -
                                ((data[a] - data[b]) * (data[c + 1] - data[a + 1])));
            }

            return polygonArea == 0 && trianglesArea == 0 ? 0 :
                    Math.Abs((trianglesArea - polygonArea) / polygonArea);
        }

        private static double SignedArea(double[] data, int start, int end, int dim)
        {
            double sum = 0;
            int j = end - dim;
            for (int i = start; i < end; i += dim)
            {
                sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
                j = i;
            }
            return sum;
        }

        public int[] GetPolygon(Node start)
        {
            Node p = start;
            //Node end = start;

            List<int> tmp = new();

            do
            {
                tmp.Add(p.id / 2);
                p = p.next!;
            } while (p != start);

            return tmp.ToArray();
        }
    }
}