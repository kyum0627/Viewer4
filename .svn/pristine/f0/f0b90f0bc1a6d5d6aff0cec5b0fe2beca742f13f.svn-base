//using IGX.Geometry.Common;
//using IGX.Geometry.DataStructure.IgxMesh;
//using IGX.Geometry.GeometryBuilder;
//using OpenTK.Mathematics;
//using IGX.Geometry.DataStructure.MeshDecomposer;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace IGX.Geometry.DataStructure
//{
//    public enum ContactType
//    {
//        Fillet, Butt, Overlap
//    }

//    public record ContactProperty(SurfaceMesh surface1, SurfaceMesh surface2, float distance, float angle, ContactType type);

//    public static partial class ContactAnalysisHelper
//    {
//        public static List<ContactProperty> ContactAnalysis(FacetVolume volume1, FacetVolume volume2)
//        {
//            Dictionary<(SurfaceMesh, SurfaceMesh), ContactProperty> contactDict = new();

//            if (AreAABBCollide(volume1, volume2) && AreOOBBCollide(volume1, volume2))
//            {
//                FindContactPairs(volume1, volume2, contactDict);
//            }
//            return contactDict.Values.ToList();// Dictionary의 값들만 List로 변환하여 반환
//        }
//        public static bool AreAABBCollide(FacetVolume volume1, FacetVolume volume2)
//        {
//            return volume1.Aabb.Collide(volume2.Aabb, AllowableTollerance(volume1, volume2));
//        }

//        public static bool AreOOBBCollide(FacetVolume volume1, FacetVolume volume2)
//        {
//            return volume1.Oobb.Collide(volume2.Oobb, AllowableTollerance(volume1, volume2));
//        }

//        public static float AllowableTollerance(FacetVolume volume1, FacetVolume volume2)
//        {
//            return Math.Min(volume1.Thickness, volume2.Thickness) * 0.5f;
//        }

//        public static (float distance, float angle) GetSurfaceContactProperties(SurfaceMesh s1, SurfaceMesh s2)
//        {
//            float dist = s1.Normal.Dot(s1.Centroid - s2.Centroid);
//            float angle = s1.Normal.Dot(s2.Normal);
//            return (dist, angle);
//        }

//        public static bool AreOpposite(SurfaceMesh s1, SurfaceMesh s2)
//        {
//            return s1.Normal.Dot(s2.Normal) < -0.9f;
//        }

//        private static void FindContactPairs(FacetVolume vol1, FacetVolume vol2, Dictionary<(SurfaceMesh, SurfaceMesh), ContactProperty> contactDict)
//        {
//            float appliedTollerance = AllowableTollerance(vol1, vol2);

//            foreach (var s1 in vol1.Surfaces)
//            {
//                FindContactBetweenSurfaceMesh(vol1, vol2, contactDict, s1, appliedTollerance);
//            }
//        }

//        private static void FindContactBetweenSurfaceMesh(FacetVolume vol1, FacetVolume vol2, Dictionary<(SurfaceMesh, SurfaceMesh), ContactProperty> contactDict, SurfaceMesh s1, float appliedTollerance)
//        {
//            foreach (var s2 in vol2.Surfaces)
//            {
//                if (!s1.Aabb.Collide(s2.Aabb, appliedTollerance))
//                {
//                    continue;
//                }
//                FineContactBetweenTriangles(vol1, vol2, contactDict, s1, s2, appliedTollerance);
//            }
//        }
//        private static void FineContactBetweenTriangles(
//            FacetVolume vol1, FacetVolume vol2,
//            Dictionary<(SurfaceMesh, SurfaceMesh), ContactProperty> contactDict,
//            SurfaceMesh s1, SurfaceMesh s2,
//            float appliedTollerance
//        )
//        {
//            foreach (var t1 in s1.TriangleIDs)
//            {
//                Triangle3f rt1 = vol1.Volume.GetTriangle(t1);

//                foreach (var t2 in s2.TriangleIDs)
//                {
//                    Triangle3f rt2 = vol2.Volume.GetTriangle(t2);

//                    // 1차 AABB 충돌 검사: 넓은 단계 (Broad Phase)
//                    if (!vol1.Volume.TriangleIndices[t1].aabb.Collide(vol2.Volume.TriangleIndices[t2].aabb, appliedTollerance))
//                    {
//                        continue;
//                    }

//                    if (!Triangle3f.IsCollide(rt1, rt2, appliedTollerance))
//                    {
//                        continue;
//                    }

//                    // GJK/EPA 알고리즘을 사용하여 정밀한 충돌 검사 및 거리 계산
//                    var result = GJKEPAAlgorithm.GetClosestPoints(rt1, rt2);

//                    // 결과에 따라 로직 처리
//                    if (result.intersects || result.distance <= appliedTollerance)
//                    {
//                        // 두 삼각형이 겹치거나 허용 오차 내에 있는 경우
//                        // contactDict에 접촉 정보를 추가하는 함수 호출
//                        ContactCheckBetweenTriangle(vol1, vol2, contactDict, s1, s2, t1, t2, result.distance);
//                    }
//                }
//            }
//        }

//        private static void ContactCheckBetweenTriangle(
//            FacetVolume vol1, FacetVolume vol2,
//            Dictionary<(SurfaceMesh, SurfaceMesh), ContactProperty> contactDict,
//            SurfaceMesh s1, SurfaceMesh s2,
//            int t1, int t2,
//            float minDistance
//        )
//        {
//            var tri1_prop = vol1.Volume.TriangleIndices[t1];
//            var tri2_prop = vol2.Volume.TriangleIndices[t2];
//            float angleDot = tri1_prop.Normal.Normalized().Dot(tri2_prop.Normal.Normalized());
//            ContactType contactType = DefineContactTypeFromTriangles(tri1_prop, tri2_prop, angleDot);

//            var key = (s1, s2);
//            if (!contactDict.ContainsKey(key))
//            {
//                // 새로운 접촉 쌍이면 Dictionary에 추가
//                contactDict[key] = new ContactProperty(s1, s2, minDistance, angleDot, contactType);
//            }
//            else
//            {
//                // 이미 존재하는 접촉 쌍이면 최단 거리를 가진 정보로 갱신
//                if (minDistance < contactDict[key].distance)
//                {
//                    contactDict[key] = contactDict[key] with { distance = minDistance, angle = angleDot, type = contactType };
//                }
//            }
//        }

//        private static ContactType DefineContactTypeFromTriangles(TrianglesAdjacency tri1, TrianglesAdjacency tri2, float angleDot)
//        {
//            float angleTolerance = 0.1f; // 약 84도 ~ 96도 범위

//            if (angleDot > 1.0f - angleTolerance) // 노말이 거의 같은 방향 (0도)
//            {
//                return ContactType.Butt;
//            }
//            else if (angleDot < -1.0f + angleTolerance) // 노말이 거의 반대 방향 (180도)
//            {
//                return ContactType.Overlap;
//            }
//            else // 그 외의 각도 (90도에 가까움)
//            {
//                return ContactType.Fillet;
//            }
//        }

//        /// <summary>
//        /// GJKEPAAlgorithm/EPA 알고리즘을 사용해 두 삼각형 간의 최단 거리를 계산
//        /// </summary>
//        /// <param name="t1"></param>
//        /// <param name="t2"></param>
//        /// <returns></returns>
//        private static float GetMinDistanceBetweenTriangles(Triangle3f t1, Triangle3f t2)
//        {
//            // GJKEPAAlgorithm 알고리즘으로 두 삼각형이 겹치는지 판단
//            Vector3 closestPoint1, closestPoint2;
//            float distance;

//            // 최단 거리. 만약 겹치면 거리는 0, closestPoint1과 closestPoint2는 교차점 중 하나.
//            var res = GJKEPAAlgorithm.GetClosestPoints(t1, t2);

//            if (res.intersects)
//            {// 겹쳤다면, 거리는 0.
//                return 0.0f;
//            }
//            else
//            {// 겹치지 않았다면
//                return res.distance;
//            }
//        }
//    }
//}