using IGX.Geometry.Common;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGX.Geometry.DataStructure
{
    public class BVHNode
    {
        public AABB3 Bounds;

        public BVHNode? Left;
        public BVHNode? Right;

        public int Start; // 정렬된 삼각형 인덱스 배열(_triIndices)에서의 시작 위치
        public int Count; // 삼각형 수

        public bool IsLeaf => Left == null && Right == null;

        // Note: 이 메서드는 BVH 순회 중에는 직접 사용되지 않습니다.
        public bool IntersectRay(Ray3f ray, ref float? dist)
        {
            return Bounds.Intersects(ray, ref dist);
        }
    }
    public class BVH
    {
        private readonly MeshGeometry _mesh;
        private int[] _triIndices;
        public BVHNode? Root { get; private set; }

        public BVH(MeshGeometry mesh)
        {
            _mesh = mesh;
            Build();
        }

        public void Build()
        {
            int triCount = _mesh.Indices.Count / 3;
            _triIndices = Enumerable.Range(0, triCount).ToArray(); // 멤버 변수에 할당
            Root = BuildNode(0, triCount);
        }

        private BVHNode BuildNode(int start, int count)
        {
            BVHNode node = new BVHNode();

            // ComputeBounds는 _triIndices 배열을 사용하여 경계 볼륨 계산
            node.Bounds = ComputeBounds(_triIndices, start, count);

            if (count <= 4)
            {
                node.Start = start;
                node.Count = count;
                return node;
            }

            Vector3 size = node.Bounds.max - node.Bounds.min;
            int axis = size.X > size.Y
                ? (size.X > size.Z ? 0 : 2)
                : (size.Y > size.Z ? 1 : 2);

            // _triIndices 배열을 start 위치부터 count 개수만큼 정렬
            Array.Sort(_triIndices, start, count, new TriangleCenterComparer(axis, _mesh));

            int mid = start + count / 2;
            int leftCount = mid - start;
            int rightCount = count - leftCount; // 올바른 계산

            // 재귀 호출
            node.Left = BuildNode(start, leftCount);
            node.Right = BuildNode(mid, rightCount);

            return node;
        }

        private AABB3 ComputeBounds(int[] triIndices, int start, int count)
        {
            AABB3 aabb = AABB3.Empty;

            for (int i = start; i < start + count; i++)
            {
                int tri = triIndices[i]; // 정렬된 배열에서 실제 삼각형 ID를 가져옴
                Triangle3f t = _mesh.GetTriangle(tri, out _);
                aabb.Contain(t.V0);
                aabb.Contain(t.V1);
                aabb.Contain(t.V2);
            }
            return aabb;
        }
        public bool Intersect(Ray3f ray, out float? hitDistance)
        {
            // hitDistance는 ref float? best로 전달되어 가장 가까운 충돌 거리를 저장
            hitDistance = float.MaxValue;
            return IntersectNode(Root, ray, ref hitDistance);
        }
        private bool IntersectNode(BVHNode? node, Ray3f ray, ref float? best)
        {
            if (node == null || best == null) return false;

            bool hit = false;
            if (node.IsLeaf)
            {
                for (int i = 0; i < node.Count; i++)
                {
                    // 정렬된 배열에서 실제 삼각형 ID를 가져옴
                    int sortedTriIndex = _triIndices[node.Start + i];
                    Triangle3f t = _mesh.GetTriangle(sortedTriIndex, out _);

                    if (t.RayIntersectsTriangle(ray, out float d))
                    {
                        if (d < best)
                        {
                            best = d;
                            hit = true;
                        }
                    }
                }
                return hit;
            }

            // --- 비-리프 노드 (자식 순서 정렬 및 가지치기) ---
            float? distL = float.MaxValue;
            bool intersectsL = false;
            if (node.Left != null)
            {
                // AABB 교차 테스트. distL에 광선-AABB 교차 거리가 저장됨.
                intersectsL = node.Left.Bounds.Intersects(ray, ref distL);
            }

            float? distR = float.MaxValue;
            bool intersectsR = false;
            if (node.Right != null)
            {
                // AABB 교차 테스트. distR에 광선-AABB 교차 거리가 저장됨.
                intersectsR = node.Right.Bounds.Intersects(ray, ref distR);
            }

            BVHNode? firstChild = null;
            float? distFirst = null;
            BVHNode? secondChild = null;
            float? distSecond = null;

            // 교차하는 두 노드를 거리가 가까운 순서로 정렬
            if (intersectsL && intersectsR)
            {
                if (distL!.Value < distR!.Value)
                {
                    firstChild = node.Left; distFirst = distL;
                    secondChild = node.Right; distSecond = distR;
                }
                else
                {
                    firstChild = node.Right; distFirst = distR;
                    secondChild = node.Left; distSecond = distL;
                }
            }
            else if (intersectsL)
            {
                firstChild = node.Left; distFirst = distL;
            }
            else if (intersectsR)
            {
                firstChild = node.Right; distSecond = distR; // SecondChild는 null, distSecond는 distR이 됨
            }
            else
            {
                return false; // 둘 다 교차 안 함
            }

            hit = false;

            // 1. 첫 번째 자식 노드 테스트
            if (firstChild != null && distFirst < best)
            {
                hit = IntersectNode(firstChild, ray, ref best) || hit;
            }

            // 2. 두 번째 자식 노드 테스트
            if (secondChild != null && distSecond.HasValue && best.HasValue)
            {
                // 가지치기: 두 번째 노드의 AABB 거리가 현재까지 찾은 가장 가까운 거리(best)보다 작을 때만 호출
                if (distSecond < best)
                {
                    hit = IntersectNode(secondChild, ray, ref best) || hit;
                }
            }

            return hit;
        }
    }
    public class TriangleCenterComparer : IComparer<int>
    {
        private readonly int _axis;
        private readonly MeshGeometry _mesh;

        public TriangleCenterComparer(int axis, MeshGeometry mesh)
        {
            _axis = axis;
            _mesh = mesh;
        }

        public int Compare(int x, int y)
        {
            var t1 = _mesh.GetTriangle(x, out _);
            var t2 = _mesh.GetTriangle(y, out _);

            float c1 = t1.Center[_axis];
            float c2 = t2.Center[_axis];

            return c1.CompareTo(c2);
        }
    }
}
