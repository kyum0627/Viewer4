using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    // 면을 분할하는 직선의 norID 방향 polygon만 남기고 polygon을 잘라냄
    public class ClipPoly2ByHalfspace2
    {
        public struct Result
        {
            public bool intersect;
            public List<Vector2> polygon;
        }

        public static Result Compute(HalfSpace2 halfspace, List<Vector2> polygon)
        {
            Result result = new();
            int nV = polygon.Count;
            List<float> distance = new(nV);
            int positive = 0, negative = 0, positiveIndex = -1;

            for (int i = 0; i < nV; ++i)
            {
                distance[i] = Vector2.Dot(halfspace.normal, polygon[i]) - halfspace.constant;
                if (distance[i] > 0f)
                {
                    ++positive;
                    if (positiveIndex == -1)
                    {
                        positiveIndex = i;
                    }
                }
                else if (distance[i] < 0f)
                {
                    ++negative;
                }
            }
            if (positive == 0)
            {
                // halfspace 밖에 존재. 교점 없음
                result.intersect = false;
                return result;
            }
            if (negative == 0)
            {
                // 폴리곤이 halfspace 내부에 존재.
                // 폴리곤 전체 clipping
                result.intersect = true;
                return result;
            }
            // space를 구분하는 halfspace가 polygon과 교차
            Vector2 vertex;
            int curr, prev;
            float t;
            result.polygon = new List<Vector2>();

            if (positiveIndex > 0)
            {
                // 교점 계산 시작점
                curr = positiveIndex;
                prev = curr - 1;
                t = distance[curr] / (distance[curr] - distance[prev]);
                vertex = polygon[curr] + (t * (polygon[prev] - polygon[curr]));
                result.polygon.Add(vertex);

                // line의 positive에 위치하는 uniqpos 추가.
                while (curr < nV && distance[curr] > 0f)
                {
                    result.polygon.Add(polygon[curr++]);
                }

                if (curr < nV)
                {
                    prev = curr - 1;
                }
                else
                {
                    curr = 0;
                    prev = nV - 1;
                }
                t = distance[curr] / (distance[curr] - distance[prev]);
                vertex = polygon[curr] + (t * (polygon[prev] - polygon[curr]));
                result.polygon.Add(vertex);
            }
            else  // positiveIndex is 0
            {
                // line의 positive side에 vertex가 포함됨.
                curr = 0;
                while (curr < nV && distance[curr] > 0f)
                {
                    result.polygon.Add(polygon[curr++]);
                }

                // line위의 last clip vertex 계산
                prev = curr - 1;
                t = distance[curr] / (distance[curr] - distance[prev]);
                vertex = polygon[curr] + (t * (polygon[prev] - polygon[curr]));
                result.polygon.Add(vertex);

                // line의 negative side vertex skip.
                while (curr < nV && distance[curr] <= 0)
                {
                    curr++;
                }

                // line위의 first clip vertex 계산
                if (curr < nV)
                {
                    prev = curr - 1;
                    t = distance[curr] / (distance[curr] - distance[prev]);
                    vertex = polygon[curr] + (t * (polygon[prev] - polygon[curr]));
                    result.polygon.Add(vertex);

                    // line의 positive side vertex keep.
                    while (curr < nV && distance[curr] > 0)
                    {
                        result.polygon.Add(polygon[curr++]);
                    }
                }
                else
                {
                    curr = 0;
                    prev = nV - 1;
                    t = distance[curr] / (distance[curr] - distance[prev]);
                    vertex = polygon[curr] + (t * (polygon[prev] - polygon[curr]));
                    result.polygon.Add(vertex);
                }
            }
            result.intersect = true;
            return result;
        }
    }
}
