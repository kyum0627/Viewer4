using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

public static class VectorPairFinder
{
    public static List<Tuple<int, int>> FindOppositePairs(ReadOnlySpan<Vector3> vectors)
    {
        List<Tuple<int, int>> pairs = new();
        HashSet<int> pairedIndices = new(); // 짝이 지어진 인덱스를 추적
        const float epsilon = 0.0001f;

        for (int i = 0; i < vectors.Length; i++)
        {
            if (pairedIndices.Contains(i))
            {
                continue;
            }

            Vector3 normalized_i = vectors[i].Normalized();
            for (int j = i + 1; j < vectors.Length; j++)
            {
                if (pairedIndices.Contains(j))
                {
                    continue;
                }

                Vector3 normalized_j = vectors[j].Normalized();
                float dotProduct = Vector3.Dot(normalized_i, normalized_j);
                if (Math.Abs(dotProduct - (-1.0f)) < epsilon)
                {
                    pairs.Add(Tuple.Create(i, j));
                    pairedIndices.Add(i);
                    pairedIndices.Add(j);
                    break; // 짝을 찾았으므로 내부 루프를 종료하고 다음 i로 이동
                }
            }
        }
        return pairs;
    }
}