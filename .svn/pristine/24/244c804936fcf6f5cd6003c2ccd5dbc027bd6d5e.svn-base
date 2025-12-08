using IGX.Geometry.Common;
using IGX.Geometry.GeometryBuilder;
using IGX.Loader;
namespace IGX.ViewControl
{
    public partial class PickHelper
    {
        public static class HitTestHelper
        {
            /// <summary>
            /// 주어진 레이(Ray)와 3D 씬 내의 모든 모델 및 지오메트리 간의 교차를 검사하여,
            /// 레이와 가장 가까이 충돌하는 객체를 찾아 그 정보를 반환
            /// </summary>
            /// <param name="ray">충돌을 검사할 3D 레이</param>
            /// <param name="models">뷰어의 API (<see cref="IIgxViewAPI"/>) 인스턴스</param>
            /// <returns>가장 가까이 충돌한 객체의 모델 ID, 지오메트리 ID, 파트 ID를 포함하는 튜플
            /// 충돌한 객체가 없으면 null 값을 가질 수 있음.</returns>
            public static (int? modelIndex, int? meshIndex, int? partIndex) FindClosestObjectHitByRay(Ray3f ray, ReadOnlySpan<Model3D> models)
            {
                float? bestDistance = null;
                float? candidateDistance = null;
                int? bestModelid = null;
                int? bestGeometryid = null;
                int? bestPartid = null;
                for (int i = 0; i < models.Length; i++)
                {
                    var amodel = models[i];

                    for (int instanceIndex = 0; instanceIndex < amodel.Geometries.Count; instanceIndex++)
                    {
                        PrimitiveBase geometry = amodel.Geometries[instanceIndex];
                        if (geometry.InstanceData.SelectionMode == SelectTo.Hide)
                        {
                            continue;
                        }

                        if (geometry.Oobb.Intersects(ray, out candidateDistance))
                        {
                            int numTriangles = geometry.Indices.Count / 3;
                            CheckTriangleMesh(ray, ref bestDistance, ref candidateDistance, ref bestModelid, ref bestGeometryid, ref bestPartid, i, instanceIndex, geometry, numTriangles);
                        }
                    }
                }

                return (bestModelid, bestGeometryid, bestPartid);
            }

            /// <summary>
            /// 특정 지오메트리(메시) 내의 각 삼각형과 레이의 교차를 검사하여 가장 가까운 충돌 지점을 찾고,
            /// 이 메서드는 <see cref="FindClosestObjectHitByRay"/>에서 호출됨.
            /// </summary>
            /// <param name="ray">충돌을 검사할 3D 레이</param>
            /// <param name="bestDistance">현재까지 가장 가까운 충돌 거리에 대한 참조</param>
            /// <param name="candidateDistance">현재 검사 중인 객체의 Oobb 충돌 거리에 대한 참조</param>
            /// <param name="bestBufferIndex">가장 가까운 객체의 모델 ID에 대한 참조</param>
            /// <param name="bestInstanceIndex">가장 가까운 객체의 지오메트리 ID에 대한 참조</param>
            /// <param name="bestPartIndex">가장 가까운 객체의 파트 ID에 대한 참조</param>
            /// <param name="i">현재 모델의 인덱스</param>
            /// <param name="instanceIndex">현재 지오메트리의 인스턴스 인덱스</param>
            /// <param name="geometry">현재 검사 중인 지오메트리(메시)</param>
            /// <param name="numTriangles">현재 지오메트리의 총 삼각형 개수</param>
            private static void CheckTriangleMesh(Ray3f ray,
                ref float? bestDistance,
                ref float? candidateDistance,
                ref int? bestBufferIndex,
                ref int? bestInstanceIndex,
                ref int? bestPartIndex,
                int i,
                int instanceIndex,
                PrimitiveBase geometry,
                int numTriangles)
            {
                for (int j = 0; j < numTriangles; j++)
                {
                    Triangle3f triangle = geometry.Mesh.GetTriangle(j, out AABB3 triangleAabb);
                    if (triangleAabb.Intersects(ray, ref candidateDistance))
                    {
                        if (triangle.RayIntersectsTriangle(ray, out float intersectionDistance))
                        {
                            if (!bestDistance.HasValue || intersectionDistance < bestDistance)
                            {
                                bestDistance = intersectionDistance;
                                bestBufferIndex = i;
                                bestInstanceIndex = instanceIndex;
                                bestPartIndex = geometry.InstanceData.EassemblyID;
                            }
                        }
                    }
                }
            }
        }
    }
}