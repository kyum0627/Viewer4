namespace IGX.ViewControl
{
    public struct PickedGeometry(int modelId, int geometryId)
    {
        public int ModelID { get; } = modelId;// 선택된 지오메트리가 속한 모델의 고유 식별자(ID)를 가져옴.
        public int GeometryID { get; } = geometryId;// 선택된 지오메트리의 고유 식별자(ID)를 가져옴.
    }
}