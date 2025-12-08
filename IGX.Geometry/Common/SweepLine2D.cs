using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Threading.Tasks;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 2D 공간에서 가장 가까운 두 점을 찾는 문제를 효율적으로 해결하기 위한 방법으로
    /// 스윕 라인 기법을 사용하여 점을 정렬하고 후보 점을 선택하는 방식으로 구현
    /// - Parallel.ForEach를 사용하여 병렬 처리 및 스레드 안전성까지 고려하여
    /// - 성능과 안정성을 모두 확.
    /// - 각 스레드는 후보 집합과, 최소 거리 업데이트할 때 lock을 사용하여 스레드 안전성을 보장
    /// 정렬 시간은 (O(nID log nID))이므로  점 개수가 많을수록 효율적
    /// - 각 점에 대해 후보 집합을 검사하고 거리를 계산하는 시간은 (O(nID))
    /// </summary>
    public class SweepLine2D
    {
        /// <summary>
        /// 주어진 점 목록에서 가장 가까운 두 점을 찾음
        /// </summary>
        /// <param name="points">점 목록</param>
        /// <returns>가장 가까운 점 쌍</returns>
        static Vector2[] ClosestPair(List<Vector2> points)
        {
            /// ===========================================================================================
            /// 입력: 2D 평면의 점 목록.
            /// 가장 가까운 두 점을 찾고 그 좌표를 반환.
            /// 1. 정렬
            ///    - 입력된 점들을 X좌표에 따라 정렬. 같은 X좌표일 경우 Y좌표에 따라 정렬
            ///    - 이 정렬을 통해 점들을 좌우로 나누어서 효율성을 높임
            /// 2. 최소 거리 초기화
            ///    - 초기 최소 거리를 첫 두 점 간의 거리로 설정
            ///    - 거리를 계산할 때 제곱 거리로 설정하여 제곱근 계산을 회피
            /// 3. 후보 점 집합 생성
            ///    - Y좌표에 따라 정렬된 점들을 저장할 SortedSet을 초기화
            ///    - 이 집합은 현재 점에 대해 가장 가까운 점을 찾는 데 사용
            /// 4. 반복
            ///    - 정렬된 각 점에 대해 다음을 수행
            ///    - 후보 제거: 현재 점과의 거리가 crtMinDistSquared를 초과하는 점들을 후보 집합에서 제거
            ///    - Y축 범위 설정: 현재 점의 Y좌표에서 `crtMinDist`만큼 위아래로 확장
            ///    - 후보 집합에서 점 선택: 설정된 Y축 범위 내의 점들을 후보 집합에서 선택
            ///    - 거리 계산: 선택된 점들에 대해 거리를 계산. 현재 최소 거리가 크다면 최소 거리를 업데이트
            ///      closestPair를 갱신
            ///    - 현재 점 추가: 마지막으로, 현재 점을 후보 집합에 추가하여 이후 점들과의 비교에 활용
            /// 5. 모든 점을 처리한 후, 가장 가까운 두 점을 포함한 배열을 반환
            Vector2[] closestPair = new Vector2[2];
            List<Vector2> sorted = new(points);

            // x 좌표로 정렬
            sorted.Sort((A, B) => A.X != B.X ? A.X.CompareTo(B.X) : A.Y.CompareTo(B.Y));

            // 현재 최소 거리 (제곱 거리)
            float crtMinDistSquared = (sorted[1].X - sorted[0].X) * (sorted[1].X - sorted[0].X);
            int leftMostCandidateIndex = 0;

            // YComparer를 사용하여 Y축으로 정렬된 후보 집합
            SortedSet<Vector2> candidates = new(new YComparer());
            object lockObject = new(); // 스레드 안전성을 위한 잠금 객체

            // 병렬로 각 점을 처리
            Parallel.ForEach(sorted, current =>
            {
                // 현재 점과 멀리 떨어진 점 제거
                while (current.X - sorted[leftMostCandidateIndex].X > Math.Sqrt(crtMinDistSquared))
                {
                    lock (lockObject)
                    {
                        candidates.Remove(sorted[leftMostCandidateIndex]);
                    }
                    leftMostCandidateIndex++;
                }

                // 현재 점의 Y축 범위 설정
                Vector2 head = new() { X = current.X, Y = (float)checked(current.Y - Math.Sqrt(crtMinDistSquared)) };
                Vector2 tail = new() { X = current.X, Y = (float)checked(current.Y + Math.Sqrt(crtMinDistSquared)) };

                // 후보 집합에서 Y축 범위 내의 점들 선택
                SortedSet<Vector2> subset;
                lock (lockObject)
                {
                    subset = candidates.GetViewBetween(head, tail);
                }

                foreach (Vector2 point in subset)
                {
                    float distanceSquared = Vector2.DistanceSquared(current, point);
                    if (distanceSquared < crtMinDistSquared)
                    {
                        lock (lockObject) // 최소 거리 및 점 쌍 업데이트 시 잠금 처리
                        {
                            if (distanceSquared < crtMinDistSquared)
                            {
                                crtMinDistSquared = distanceSquared;
                                closestPair[0] = current;
                                closestPair[1] = point;
                            }
                        }
                    }
                }

                // 현재 점을 후보 집합에 추가
                lock (lockObject)
                {
                    candidates.Add(current);
                }
            });

            return closestPair;
        }

        public class YComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 p1, Vector2 p2)
            {
                return YCompare(p1, p2);
            }

            public static int YCompare(Vector2 p1, Vector2 p2)
            {
                return p1.Y < p2.Y ? -1
                     : p1.Y > p2.Y ? 1
                     : p1.X < p2.X ? -1
                     : p1.X > p2.X ? 1
                     : 0;
            }
        }

        public class XComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 p1, Vector2 p2)
            {
                return XCompare(p1, p2);
            }

            public static int XCompare(Vector2 p1, Vector2 p2)
            {
                return p1.X < p2.X ? -1
                    : p1.X > p2.X ? 1
                    : p1.Y < p2.Y ? -1
                    : p1.Y > p2.Y ? 1
                    : 0;
            }
        }
    }

    public class SweepLine3D
    {
        /// <summary>
        /// 주어진 점 목록에서 가장 가까운 두 점을 찾음.
        /// </summary>
        /// <param name="points">점 목록</param>
        /// <returns>가장 가까운 점 쌍</returns>
        public static Vector3[] ClosestPair(List<Vector3> points)
        {
            Vector3[] closestPair = new Vector3[2];
            List<Vector3> sorted = new(points);

            // X, Y, Z 좌표로 정렬
            sorted.Sort((A, B) => A.X != B.X ? A.X.CompareTo(B.X) :
                                  A.Y != B.Y ? A.Y.CompareTo(B.Y) :
                                  A.Z.CompareTo(B.Z));

            // 현재 최소 거리 (제곱 거리)
            float crtMinDistSquared = ((sorted[1].X - sorted[0].X) * (sorted[1].X - sorted[0].X)) +
                                       ((sorted[1].Y - sorted[0].Y) * (sorted[1].Y - sorted[0].Y)) +
                                       ((sorted[1].Z - sorted[0].Z) * (sorted[1].Z - sorted[0].Z));
            int leftMostCandidateIndex = 0;

            // YComparer와 ZComparer를 사용하여 후보 집합 생성
            SortedSet<Vector3> candidates = new(new YZComparer());
            object lockObject = new(); // 스레드 안전성을 위한 잠금 객체

            // 병렬로 각 점을 처리
            Parallel.ForEach(sorted, current =>
            {
                // 현재 점과 너무 멀리 떨어진 점 제거
                while (current.X - sorted[leftMostCandidateIndex].X > Math.Sqrt(crtMinDistSquared))
                {
                    lock (lockObject)
                    {
                        candidates.Remove(sorted[leftMostCandidateIndex]);
                    }
                    leftMostCandidateIndex++;
                }

                // 현재 점의 Y축 및 Z축 범위 설정
                Vector3 head = new(current.X, current.Y - (float)Math.Sqrt(crtMinDistSquared), current.Z - (float)Math.Sqrt(crtMinDistSquared));
                Vector3 tail = new(current.X, current.Y + (float)Math.Sqrt(crtMinDistSquared), current.Z + (float)Math.Sqrt(crtMinDistSquared));

                // 후보 집합에서 Y축 및 Z축 범위 내의 점들 선택
                SortedSet<Vector3> subset;
                lock (lockObject)
                {
                    subset = candidates.GetViewBetween(head, tail);
                }

                foreach (Vector3 point in subset)
                {
                    float distanceSquared = DistanceSquared(current, point);
                    if (distanceSquared < crtMinDistSquared)
                    {
                        lock (lockObject) // 최소 거리 및 점 쌍 업데이트 시 잠금 처리
                        {
                            if (distanceSquared < crtMinDistSquared)
                            {
                                crtMinDistSquared = distanceSquared;
                                closestPair[0] = current;
                                closestPair[1] = point;
                            }
                        }
                    }
                }

                // 현재 점을 후보 집합에 추가
                lock (lockObject)
                {
                    candidates.Add(current);
                }
            });

            return closestPair;
        }

        // 두 점 간의 제곱 거리 계산
        private static float DistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        public class YZComparer : IComparer<Vector3>
        {
            public int Compare(Vector3 p1, Vector3 p2)
            {
                int yComparison = p1.Y.CompareTo(p2.Y);
                return yComparison != 0 ? yComparison : p1.Z.CompareTo(p2.Z);
            }
        }
    }
}
