using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using IGX.Geometry.DataStructure; // 기하학적 데이터 구조 포함
using IGX.Geometry.DataStructure.IgxMesh;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics; // 기하학 객체 생성을 위한 빌더 패턴 포함

namespace IGX.Loader
{
    public static class Model3DQuery
    {
        public static Dictionary<string, (int partid, int ebomid, List<int> geometryIDs)> GetUniquePartList(Model3D modeldata)
        {
            Dictionary<string, (int partid, int ebomid, List<int> geometryIDs)> ebomIDs = [];
            int partid = 0;

            foreach (PrimitiveBase g in modeldata.Geometries.Values)
            {
                int eAssemblyID = g.RenderComp.InstanceData.EassemblyID;
                int geometryID = g.RenderComp.InstanceData.GeometryID;
                if (!modeldata.Eassemblies.TryGetValue(eAssemblyID, out IAssembly? assembly)) continue;

                string partName = assembly.Name;
                if (!ebomIDs.TryGetValue(partName, out (int, int, List<int>) existingpart))
                {
                    List<int> geos = [geometryID];
                    ebomIDs[partName] = (partid++, eAssemblyID, geos);
                }
                else
                {
                    ebomIDs[partName].geometryIDs.Add(geometryID);
                }
            }
            return ebomIDs;
        }

        public static Vector4 GetGeometryColor(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.RenderComp.InstanceData.Color : default;
        public static GeometryInstance? GetMeshInstance(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.RenderComp.InstanceData : null;
        public static OOBB3 GetGeometryOOBB(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.Oobb : OOBB3.Empty;
        public static AABB3 GetGeometryAABB(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.Aabb : AABB3.Empty;
        public static int GetAssemblyIndex(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.RenderComp.InstanceData.EassemblyID : -1;
        public static ParaPrimType GetPrimitiveType(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.RenderComp.GeometryType : default;
        public static string? GetGrandPrimitiveType(Model3D modelData, int geometryId) => modelData.Geometries.TryGetValue(geometryId, out var primitive) ? primitive.RenderComp.GrandPrimType : null;
        public static float Thickness(Model3D modelData, int geometryID) => modelData.GetIgxMesh(geometryID)?.Thickness ?? -1;
        public static (List<Vector3> pos, List<Vector3> nor, List<uint> ind) MeshData(Model3D modelData, int geometryID)
        {
            var mesh = modelData.GetIgxMesh(geometryID);
            return (mesh?.Positions ?? [], mesh?.Normals ?? [], mesh?.Indices ?? []);
        }
        public static SurfaceMesh? MoldSurface(Model3D modelData, int geometryID) => modelData.GetIgxMesh(geometryID)?.MoldSurface;
        public static SurfaceMesh? OffsetSurface(Model3D modelData, int geometryID) => modelData.GetIgxMesh(geometryID)?.OffsetSurface;

        public static List<int> GetGeometryIdsInAssemblyTree(Model3D modelData, int assemblyId)
        {
            HashSet<int> collectedGeometryIds = [];
            CollectGeometriesRecursive(modelData, assemblyId, collectedGeometryIds);
            return collectedGeometryIds.ToList();
        }

        private static void CollectGeometriesRecursive(Model3D modelData, int currentAssyId, HashSet<int> collectedGeometryIds)
        {
            if (!modelData.Eassemblies.TryGetValue(currentAssyId, out IAssembly? currentAssembly))
            {
                return;
            }
            if (currentAssembly.GeometryIDs?.Count > 0)
            {
                collectedGeometryIds.UnionWith(currentAssembly.GeometryIDs);
            }
            if (currentAssembly.SubEbom?.Count > 0)
            {
                foreach (int childAssyId in currentAssembly.SubEbom)
                {
                    CollectGeometriesRecursive(modelData, childAssyId, collectedGeometryIds);
                }
            }
        }

        public static List<int> FindSiblingAssemblyIds(Model3D modelData, int assemblyId)
        {
            if (!modelData.Eassemblies.TryGetValue(assemblyId, out IAssembly? targetAssy))
            {
                return [];
            }

            int? parentId = targetAssy.ParentAssyID;
            if (parentId == -1) // 최상위 어셈블리인 경우
            {
                return [];
            }

            return modelData.Eassemblies
                .Where(entry => entry.Value.ParentAssyID == parentId && entry.Key != assemblyId)
                .Select(entry => entry.Key)
                .ToList();
        }

        public static List<int> FindChildAssemblyIds(Model3D modelData, int assemblyId)
        {
            return modelData.Eassemblies
                .Where(entry => entry.Value.ParentAssyID == assemblyId)
                .Select(entry => entry.Key)
                .ToList();
        }

        public static List<int> GetLeafAssemblyIds(Model3D modelData)
        {
            return modelData.Eassemblies
                .Where(entry => entry.Value.SubEbom == null || entry.Value.SubEbom.Count == 0)
                .Select(entry => entry.Key)
                .ToList();
        }

        public static List<int> GetAllUniqueGeometryIds(Model3D modelData)
        {
            return modelData.Eassemblies.Values
                .Where(assembly => assembly.GeometryIDs != null)
                .SelectMany(assembly => assembly.GeometryIDs)
                .Distinct()
                .ToList();
        }



        /// <summary>
        /// AABB를 자식 노드에서 부모 노드로 재귀적으로 전파.
        /// </summary>
        private static AABB3 PropagateAABBRecursive(Model3D modelData, IAssembly assembly)
        {
            AABB3 currentAabb = assembly.AssemblyAABB;
            if (assembly.SubEbom != null)
            {
                foreach (int childId in assembly.SubEbom)
                {
                    if (modelData.Eassemblies.TryGetValue(childId, out IAssembly? childAssembly))
                    {
                        currentAabb = currentAabb.Contain(PropagateAABBRecursive(modelData, childAssembly));
                    }
                }
            }
            assembly.AssemblyAABB = currentAabb;
            return currentAabb;
        }

        private static void CollectGeometriesRecursive(
            Model3D modelData,
            int currentAssyId,
            HashSet<int> collectedGeometryIds,
            HashSet<int> visitedAssemblyIds)
        {
            if (!visitedAssemblyIds.Add(currentAssyId))
            {
                return;
            }

            if (!modelData.Eassemblies.TryGetValue(currentAssyId, out IAssembly? currentAssembly))
            {
                return;
            }

            if (currentAssembly.GeometryIDs?.Count > 0)
            {
                collectedGeometryIds.UnionWith(currentAssembly.GeometryIDs);
            }

            if (currentAssembly.SubEbom?.Count > 0)
            {
                foreach (int childAssyId in currentAssembly.SubEbom)
                {
                    CollectGeometriesRecursive(modelData, childAssyId, collectedGeometryIds, visitedAssemblyIds);
                }
            }
        }
    }
}