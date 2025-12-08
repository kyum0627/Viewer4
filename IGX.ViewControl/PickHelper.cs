using IGX.Geometry.Common;
using IGX.Loader;
using IGX.ViewControl.Render;
using OpenTK.GLControl;
using OpenTK.Mathematics;
namespace IGX.ViewControl
{
    public partial class PickHelper
    {
        /// <summary>
        /// 키보드 및 마우스 입력을 기반으로 3D 객체의 선택/선택 해제 로직을 처리
        /// Shift 키와 마우스 버튼 조합에 따라 단일/다중 선택 및 선택 해제 기능을 제공
        /// </summary>
        /// <param name="keyboard">현재 눌린 키보드 키 상태</param>
        /// <param name="e">마우스 이벤트 인자</param>
        /// <param name="glControl">렌더링이 이루어지는 <see cref="GLControl"/> 인스턴스</param>
        /// <param name="invVP">카메라의 _viewMatrix * _projectionMatrix 행렬의 역행렬 월드 좌표 변환에 사용됨.</param>
        /// <param name="vc">뷰어의 API (<see cref="IIgxViewAPI"/>) 인스턴스 모델 데이터 및 렌더링 제어에 사용됨.</param>
        /// <param name="bMouseMove">마우스 이동 이벤트인지 여부 true이면 마우스 이동, false이면 클릭 이벤트</param>
        public static void PickUnPick(Keys keyboard, MouseEventArgs e, GLControl glControl, IgxViewAPI vc, bool bMouseMove)
        {
            var models = vc.ModelManager.Models.ToArray();
            var byPart = vc.SelectionManager.Selection.ByPart;
            var selection = vc.SelectionManager;
            var camera = vc.SceneParameter.Camera;

            Matrix4 invVP = camera.InvVP;

            if (keyboard.HasFlag(Keys.Shift))
            {
                if (e.Button == MouseButtons.Left)
                {
                    (int? modelid, int? partid, List<int>? matched) = HitTest(e, glControl, invVP, models, byPart);
                    if (modelid.HasValue && partid.HasValue && matched != null)
                    {
                        selection.UpdatePickedList(modelid!.Value, matched!);
                    }
                }
                if (e.Button == MouseButtons.Right && selection.PickedItems.Count > 0)
                {
                    PickedGeometry lastPicked = selection.PickedItems.Last().Key;
                    int partid = ModelQuery.GetPartID(models[lastPicked.ModelID], lastPicked.GeometryID);
                    List<int> geoIDs = ModelQuery.GetGeometryIdsOfThePart(models[lastPicked.ModelID], partid);
                    selection.UpdatePickedList(lastPicked.ModelID, geoIDs);
                    
                    foreach (int geo in geoIDs)
                    {
                        PickedGeometry p = new(lastPicked.ModelID, geo);
                        selection.Remove(p);
                    }
                }
            }
            else if (!bMouseMove)
            {
                if (e.Button == MouseButtons.Left)
                {
                    selection.RevertAll();
                    (int? modelid, int? partid, List<int>? matched) = HitTest(e, glControl, invVP, models, byPart);
                    if (modelid.HasValue && partid.HasValue && matched != null)
                    {
                        selection.UpdatePickedList(modelid.Value, matched);
                    }
                }
            }
        }

        /// <summary>
        /// 주어진 모델 ID와 지오메트리 키 목록 중 현재 PickedItems에 포함된 항목이 있는지 확인
        /// </summary>
        /// <param name="modelid">모델의 ID</param>
        /// <param name="matchingGeometryKeys">검사할 지오메트리 키(ID) 목록</param>
        /// <returns>주어진 목록 중 하나라도 선택된 항목에 포함되어 있으면 true, 그렇지 않으면 false</returns>
        public bool ContainsPicked(int modelid, List<int> matchingGeometryKeys, Dictionary<PickedGeometry, (Vector4 Color, SelectTo Mode)> pickedItems)
        {
            return matchingGeometryKeys.Any(geoKey => pickedItems.ContainsKey(new PickedGeometry(modelid, geoKey)));
        }

        /// <summary>
        /// 마우스 이벤트 정보, GLControl, 역 _viewMatrix-_projectionMatrix 행렬, 그리고 뷰어 API를 사용하여
        /// 화면상의 마우스 위치에 해당하는 3D 객체를 픽킹(Hit Test)
        /// </summary>
        /// <param name="e">마우스 이벤트 인자</param>
        /// <param name="glControl">렌더링이 이루어지는 <see cref="GLControl"/> 인스턴스</param>
        /// <param name="invVP">카메라의 _viewMatrix * _projectionMatrix 행렬의 역행렬</param>
        /// <param name="vc">뷰어의 API (<see cref="IIgxViewAPI"/>) 인스턴스</param>
        /// <returns>픽킹된 객체의 모델 ID, 파트 ID, 그리고 해당 파트에 속하는 지오메트리 ID 목록을 포함하는 튜플
        /// 객체가 픽킹되지 않았다면 null 값을 가질 수 있음.</returns>
        private static (int? modelid, int? partid, List<int>? matched) HitTest(MouseEventArgs e, GLControl glControl, Matrix4 invVP, ReadOnlySpan<Model3D> models, bool byPart)
        {
            (int? modelid, int? geoid, int? partid) = PickObjectOnScreen(glControl, invVP, models, e.X, e.Y);
            List<int> matched = [];
            if (modelid.HasValue && geoid.HasValue && partid.HasValue)
            {
                matched = ModelQuery.GetGeometryIdsOfThePart(models[modelid.Value], partid!.Value);

            }
            return (modelid, partid, matched);
        }

        /// <summary>
        /// 화면상의 2D 마우스 좌표를 3D 월드 공간의 레이(Ray)로 변환하고,
        /// 이 레이와 충돌하는 가장 가까운 3D 객체를 찾아 그 정보를 반환
        /// </summary>
        /// <param name="glControl">렌더링이 이루어지는 <see cref="GLControl"/> 인스턴스</param>
        /// <param name="invVP">카메라의 _viewMatrix * _projectionMatrix 행렬의 역행렬</param>
        /// <param name="vc">뷰어의 API (<see cref="IIgxViewAPI"/>) 인스턴스</param>
        /// <param name="mouseX">마우스 커서의 X 화면 좌표</param>
        /// <param name="mouseY">마우스 커서의 Y 화면 좌표</param>
        /// <returns>픽킹된 객체의 모델 ID, 지오메트리 ID, 파트 ID를 포함하는 튜플
        /// 객체가 픽킹되지 않았다면 null 값을 가질 수 있음.</returns>
        private static (int? modelIndex, int? meshIndex, int? partIndex) PickObjectOnScreen(GLControl glControl, Matrix4 invVP, ReadOnlySpan<Model3D> models, int mouseX, int mouseY)
        {
            float x = (2.0f * (mouseX + 0.5f) / glControl.Width) - 1.0f;
            float y = 1.0f - (2.0f * (mouseY + 0.5f) / glControl.Height);
            Vector4 clipNear = new(x, y, -1.0f, 1.0f);
            Vector4 clipFar = new(x, y, 1.0f, 1.0f);


            Vector4 worldNear4 = clipNear.Transform(invVP);
            Vector4 worldFar4 = clipFar.Transform(invVP);
            Vector3 worldNear = new Vector3(worldNear4.X, worldNear4.Y, worldNear4.Z) / worldNear4.W;
            Vector3 worldFar = new Vector3(worldFar4.X, worldFar4.Y, worldFar4.Z) / worldFar4.W;

            Vector3 direction = Vector3.Normalize(worldFar - worldNear);

            Ray3f ray = new(worldNear, direction);

            (int? modelIndex, int? meshIndex, int? partIndex) = HitTestHelper.FindClosestObjectHitByRay(ray, models);
            return (modelIndex, meshIndex, partIndex);
        }
    }
}