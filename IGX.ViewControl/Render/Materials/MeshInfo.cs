namespace IGX.ViewControl.Render.Materials
{
    public struct MeshInfo
    {
        public int BaseVertex;      // VBO에서 해당 메쉬의 시작 정점 인덱스
        public int IndexCount;      // EBO에서 해당 메쉬의 인덱스 개수
        public int EboHandle;       // 해당 메쉬의 EBO 핸들
    }
}
