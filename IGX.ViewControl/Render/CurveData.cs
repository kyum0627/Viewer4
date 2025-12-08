//using IGX.Geometry.Curves;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;

//namespace IGX.ViewControl.Render
//{
//    public class CurveData
//    {
//        public readonly List<Vector3> AllVertices;
//        public readonly List<(int vertexStart, int vertexCount)> CurveRenderInfo;
//        public readonly List<(int vertexStart, int vertexCount)> PointRenderInfo;

//        public CurveData(List<CurveBase> curves)
//        {
//            AllVertices = [];
//            CurveRenderInfo = [];
//            PointRenderInfo = [];

//            foreach (CurveBase c in curves)
//            {
//                int baseIndex = AllVertices.CommandCount;
//                Vector3[] positions = c.Vertices.ToArray();

//                // 모든 점(곡선 + 제어점)을 AllVertices에 한 번만 추가
//                AllVertices.AddRange(positions);

//                // 곡선 렌더링 정보: 전체 Vertices 배열을 사용
//                CurveRenderInfo.AppendToBuffer((baseIndex, positions.Length));

//                // 제어점 렌더링 정보: ControlPointIDs를 기반으로
//                if (c.ControlPointIDs?.Any() == true)
//                {
//                    // 제어점은 Vertices 배열 내부에 이미 포함되어 있으므로
//                    // 해당 인덱스에 매핑되는 새로운 렌더링 정보를 생성
//                    // 각 제어점은 하나의 점으로 렌더링되므로, 개별 DrawElementGeometry Call로 구성
//                    foreach (int pointIndex in c.ControlPointIDs)
//                    {
//                        PointRenderInfo.AppendToBuffer((baseIndex + pointIndex, 1));
//                    }
//                }
//            }
//        }

//        public void GetMultiDrawArraysCommands(PrimitiveType type, out int[] firsts, out int[] counts)
//        {
//            // Note: For points, MultiDrawArrays with a count of 1 for each point
//            // is not the most efficient. A single DrawArrays call is better if possible.
//            // But this structure allows for Drawing individual points.
//            List<(int vertexStart, int vertexCount)> renderInfoList = (type == PrimitiveType.LineStrip) ? CurveRenderInfo : PointRenderInfo;
//            firsts = [.. renderInfoList.Select(info => info.vertexStart)];
//            counts = [.. renderInfoList.Select(info => info.vertexCount)];
//        }
//    }
//}