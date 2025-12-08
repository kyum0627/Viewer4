using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace IGX.Geometry.Distance
{
    public class KDTree<T> where T : struct
    {
        private class Node
        {
            public T Point;
            public Node? Left;
            public Node? Right;
        }

        private Node? root;
        private readonly int dimensions; // 2D 또는 3D 여부
        private readonly Func<T, int, float> getCoordinate;

        public KDTree(int dimensions)
        {
            this.dimensions = dimensions;

            // 2D와 3D를 처리할 수 있는 getCoordinate 설정
            if (dimensions == 2)
            {
                getCoordinate = (point, dim) => ((Vector2)(object)point)[dim];
            }
            else if (dimensions == 3)
            {
                getCoordinate = (point, dim) => ((Vector3)(object)point)[dim];
            }
            else
            {
                throw new ArgumentException("Only 2D and 3D are supported");
            }
        }

        // 점 삽입
        public void Insert(T point)
        {
            root = Insert(root!, point, depth: 0);
        }

        private Node Insert(Node node, T point, int depth)
        {
            Stack<Node> stack = new();
            stack.Push(node);

            while (stack.Count > 0)
            {
                node = stack.Pop();

                if (node == null)
                {
                    return new Node { Point = point };
                }

                int cd = depth % dimensions; // 2D 또는 3D에 따라 분할 차원을 결정

                if (getCoordinate(point, cd) < getCoordinate(node.Point, cd))
                {
                    if (node.Left == null)
                    {
                        node.Left = new Node { Point = point };
                    }
                    else
                    {
                        stack.Push(node.Left);
                    }
                }
                else
                {
                    if (node.Right == null)
                    {
                        node.Right = new Node { Point = point };
                    }
                    else
                    {
                        stack.Push(node.Right);
                    }
                }
            }

            return node;
        }

        // 최근접 점 쌍 찾기
        public (T p1, T p2, float distance) FindClosestPair(T[] points)
        {
            foreach (T point in points)
            {
                Insert(point);
            }

            return FindClosestPair(root!, points[0], float.MaxValue);
        }

        private (T p1, T p2, float distance) FindClosestPair(Node node, T targetPoint, float bestDistance)
        {
            Stack<Node> stack = new();
            stack.Push(node);

            (T Point1, T Point2, float Distance) bestPair = (Point1: targetPoint, Point2: targetPoint, Distance: bestDistance);

            while (stack.Count > 0)
            {
                node = stack.Pop();

                if (node == null)
                {
                    continue;
                }

                float distanceSquared = CalculateDistanceSquared(targetPoint, node.Point);
                if (distanceSquared < bestDistance)
                {
                    bestDistance = distanceSquared;
                    bestPair = (node.Point, targetPoint, bestDistance);
                }

                // 차원에 따라 다음 노드를 결정
                int cd = 0;
                if (dimensions == 2)
                {
                    cd = 2;
                }
                else if (dimensions == 3)
                {
                    cd = 3;
                }

                // 왼쪽 또는 오른쪽 자식으로 갈지 결정
                if (getCoordinate(targetPoint, cd) < getCoordinate(node.Point, cd))
                {
                    stack.Push(node.Left!);
                    stack.Push(node.Right!); // 오른쪽도 확인
                }
                else
                {
                    stack.Push(node.Right!);
                    stack.Push(node.Left!); // 왼쪽도 확인
                }
            }

            return bestPair;
        }

        // 두 점 사이의 거리 제곱 계산
        private float CalculateDistanceSquared(T p1, T p2)
        {
            if (dimensions == 2)
            {
                return Vector2.DistanceSquared((Vector2)(object)p1, (Vector2)(object)p2);
            }
            else
            {
                return dimensions == 3 ? Vector3.DistanceSquared((Vector3)(object)p1, (Vector3)(object)p2) : 0f;
            }
        }
    }
}
