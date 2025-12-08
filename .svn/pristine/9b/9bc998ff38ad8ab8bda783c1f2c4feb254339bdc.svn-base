using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 정렬을 위한 비교 함수
    /// </summary>
    public class Segment3fComparer : IComparer<Segment3f>
    {
        public int Compare(Segment3f s1, Segment3f s2)
        {
            Vector3 baseSeg = new(-1000000f, 0f, -1000000f);
            float d1 = s1.Distance(baseSeg);
            float d2 = s2.Distance(baseSeg);

            return d1.CompareTo(d2);

            //hschoi : 20240502 : 원형으로 생기는 용접장에 대해 순차적 정렬을 할 수 있는가?
            //// Calculate the distance between the end point of segment1 and the start point of segment2
            //double distance1 = s1.P1.GetDistanceToPoint(s2.P0);
            //// Calculate the distance between the end point of segment2 and the start point of segment2
            //double distance2 = s2.P1.GetDistanceToPoint(s2.P0);
            //// Compare distances
            //return distance1.CompareTo(distance2);
        }
    }
    /// <summary>
    /// 정렬 수행하기 위한 클래스
    /// </summary>
    public class SortSegments
    {
        public List<Segment3f> segList = new();
        public SortSegments(List<Segment3f> segs)
        {
            segList.Clear();

            //Base 기준으로 시작점 끝점을 정렬
            Vector3 baseSeg = new(-1000000f, 0f, -1000000f);
            foreach (Segment3f seg in segs)
            {
                float d1 = seg.P0.DistanceTo(baseSeg);
                float d2 = seg.P1.DistanceTo(baseSeg);

                if (d1.CompareTo(d2) < 0)
                {
                    // d1 이 더 baseSeg에 가까움
                    // 그대로
                    segList.Add(seg);
                }
                else
                {
                    // d2가 더 가까움
                    // P0, P1 바꿈
                    Segment3f newSeg = new(seg.P1, seg.P0);
                    segList.Add(newSeg);
                }
            }

            //Base를 기준으로 Seg를 정렬
            segList.Sort(new Segment3fComparer().Compare);

            ////hschoi : 20240502 : 원형으로 생기는 용접장에 대해 순차적 정렬을 할 수 있는가?
            //segList.Sort((segment1, segment2) =>
            //{
            //    double minDistance1 = FindMinDistance(segment1.P1, segList);
            //    double minDistance2 = FindMinDistance(segment2.P1, segList);
            //    return minDistance1.CompareTo(minDistance2);
            //});
        }
        double FindMinDistance(Vector3 point, List<Segment3f> segments)
        {
            double minDistance = double.MaxValue;
            foreach (Segment3f segment in segments)
            {
                if (segment.P0 != point) // 시작점과 끝점이 같으면 거리를 계산하지 않음.
                {
                    double distance = segment.Distance(point);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }
            return minDistance;
        }

        /// <summary>
        /// Segments를 단순화(동일선상에서 이어진 Segs를 병합하는 것)
        /// 반드시 SortsSegments 수행 후 사용할 것
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        public List<Segment3f> SimplifySegments(List<Segment3f> segs)
        {
            List<Segment3f> simplified_segs = new();

            bool bSimplified = false;
            Segment3f si = segs[0];//그냥 초기화.
            for (int i = 0; i < segs.Count - 1; i++)
            {
                if (!bSimplified)
                {
                    si = segs[i];
                }

                Segment3f sj = segs[i + 1];

                //연결된 Seg인지 검사
                float d = si.P1.DistanceTo(sj.P0);
                if (d < 1.0f)//동일한 점이라고 본다.
                {
                    //방향벡터가 같은지 검사
                    float ang = si.direction.AngleDeg(sj.direction);
                    if (Math.Abs(ang) < 1.0f || Math.Abs(Math.Abs(ang) - 180) < 1.0f)//같은 방향이라고 본다.
                    {
                        si = new Segment3f(si.P0, sj.P1);
                        bSimplified = true;

                        //마지막 세그먼트인데 합쳐진 경우면 다음 테스트케이스가 없으므로 여기서 add 해야 한다.
                        if (i == segs.Count - 2)
                        {
                            simplified_segs.Add(si);
                        }
                    }
                    else
                    {
                        simplified_segs.Add(si); bSimplified = false;
                        //hschoi : 20240502 마지막 세그먼트인데 병합이 안된거면 마지막 세그먼트를 추가해야 한다.
                        if (i + 1 == segs.Count - 1) { simplified_segs.Add(sj); }
                    }
                }
                else
                {
                    simplified_segs.Add(si); bSimplified = false;
                    if (i + 1 == segs.Count - 1) { simplified_segs.Add(sj); }
                }
            }

            return simplified_segs;
        }
    }

    /// <summary>
    /// 세그먼트들이 닫혀있는지를 체크하는 클래스
    /// </summary>
    public class ClosedSegments
    {
        public Dictionary<Segment3f, Segment3f> segsMap = new();
        public List<Segment3f> mSegs = new();

        /// <summary>
        /// 부동소숫점에서 반올림하여 공차를 조절한다.
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="digits"></param>
        public ClosedSegments(List<Segment3f> segments, int digits = 1)
        {
            foreach (Segment3f seg in segments)
            {
                Vector3 p0 = new();
                p0.X = (float)Math.Round(seg.P0.X, digits);
                p0.Y = (float)Math.Round(seg.P0.Y, digits);
                p0.Z = (float)Math.Round(seg.P0.Z, digits);

                Vector3 p1 = new();
                p1.X = (float)Math.Round(seg.P1.X, digits);
                p1.Y = (float)Math.Round(seg.P1.Y, digits);
                p1.Z = (float)Math.Round(seg.P1.Z, digits);

                Segment3f mseg = new(p0, p1);
                if (!segsMap.ContainsKey(mseg))
                {
                    segsMap[mseg] = seg;
                    mSegs.Add(mseg);
                }
            }
        }

        /// <summary>
        /// 공차가 맞춰진 상태에서 Closed 여부를 판단한다.
        /// </summary>
        /// <returns></returns>
        public bool IsClosed()
        {
            Dictionary<Vector3, List<Vector3>> pointMap = new();

            // 각 점과 연결된 점을 딕셔너리에 추가
            foreach (Segment3f segment in mSegs)
            {
                if (!pointMap.ContainsKey(segment.P0))
                {
                    pointMap[segment.P0] = new List<Vector3>();
                }
                if (!pointMap.ContainsKey(segment.P1))
                {
                    pointMap[segment.P1] = new List<Vector3>();
                }
                pointMap[segment.P0].Add(segment.P1);
                pointMap[segment.P1].Add(segment.P0);
            }

            // DFS를 사용하여 경로가 닫혀있는지 확인
            HashSet<Vector3> visited = new();
            Stack<Vector3> stack = new();
            Vector3 startPoint = mSegs[0].P0;
            stack.Push(startPoint);

            while (stack.Count > 0)
            {
                Vector3 current = stack.Pop();
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    foreach (Vector3 neighbor in pointMap[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }

            // 모든 점이 방문되었는지 확인
            return visited.Count == pointMap.Count;
        }

        /// <summary>
        /// 정렬은 잘 안되겠다.. 나중에 더 고민하자..
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public List<Segment3f> SortSegments(List<Segment3f> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                return new List<Segment3f>();
            }

            List<Segment3f> sortedSegments = new();
            HashSet<Segment3f> usedSegments = new();
            Segment3f currentSegment = segments[0];
            sortedSegments.Add(currentSegment);
            usedSegments.Add(currentSegment);

            while (sortedSegments.Count < segments.Count)
            {
                Segment3f closestSegment = new();
                Vector3 closestPoint = new();
                double closestDistance = double.MaxValue;

                // HMD 닷넷버전 다운으로 인해...
                // 4.6.2 버전으로 낮춤
                foreach (Segment3f segment in segments)
                {
                    if (usedSegments.Contains(segment))
                    {
                        continue;
                    }

                    List<Tuple<Vector3, double>> distances = new()
                    {
                        new(segment.P0, currentSegment.P1.DistanceTo(segment.P0)),
                        new(segment.P1, currentSegment.P1.DistanceTo(segment.P1)),
                        new(segment.P0, currentSegment.P0.DistanceTo(segment.P0)),
                        new(segment.P1, currentSegment.P0.DistanceTo(segment.P1))
                    };

                    foreach (Tuple<Vector3, double> distanceTuple in distances)
                    {
                        Vector3 point = distanceTuple.Item1;
                        double distance = distanceTuple.Item2;

                        if (distance < closestDistance)
                        {
                            closestSegment = segment;
                            closestPoint = point;
                            closestDistance = distance;
                        }
                    }
                }
                /*
                foreach (var segment in segments)
                {
                    if (usedSegments.Contains(segment))
                        continue;

                    var distances = new List<(Vector3 point, double distance)>
                {
                    (segment.P0, currentSegment.P1.GetDistanceToPoint(segment.P0)),
                    (segment.P1, currentSegment.P1.GetDistanceToPoint(segment.P1)),
                    (segment.P0, currentSegment.P0.GetDistanceToPoint(segment.P0)),
                    (segment.P1, currentSegment.P0.GetDistanceToPoint(segment.P1))
                };

                    foreach (var (point, distance) in distances)
                    {
                        if (distance < closestDistance)
                        {
                            closestSegment = segment;
                            closestPoint = point;
                            closestDistance = distance;
                        }
                    }
                }
                */

                if (closestSegment.P0 != default)
                {
                    sortedSegments.Add(closestSegment);
                    usedSegments.Add(closestSegment);
                    currentSegment = closestSegment;
                }
            }

            return sortedSegments;
        }
    }
}
