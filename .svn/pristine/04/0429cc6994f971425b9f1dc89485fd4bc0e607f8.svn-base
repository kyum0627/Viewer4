namespace IGX.ViewControl.GLDataStructure
{
    public class VertexListComparer : IEqualityComparer<List<GLVertex>>
    {
        public bool Equals(List<GLVertex>? x, List<GLVertex>? y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x.Count != y.Count) return false;

            // 리스트 내의 모든 Vertex를 순서대로 비교
            for (int i = 0; i < x.Count; i++)
            { // GLVertex 객체의 동등성을 직접 비교 (예: 좌표 등)
                if (!x[i].Equals(y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(List<GLVertex> obj)
        {
            // 모든 Vertex의 해시 코드를 조합하여 하나의 해시 코드 생성
            unchecked // 오버플로를 무시
            {
                int hash = 19;
                foreach (var vertex in obj)
                {
                    hash = hash * 31 + vertex.GetHashCode();
                }
                return hash;
            }
        }
    }
}
