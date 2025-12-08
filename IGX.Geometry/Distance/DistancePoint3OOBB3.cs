using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Distance
{
    class DistancePoint3OOBB3
    {
        Vector3 point;
        OOBB3 box;

        public DistancePoint3OOBB3(Vector3 p, OOBB3 b)
        {
            point = p;
            box = b;
        }
        /// <summary>
        /// 점 P와 Object Oriented Bounding Cube 사이의 가장 가까운 거리
        /// </summary>
        /// <param name="point"></param>
        /// <param name="box"></param>
        /// <returns>최단거리, box위의 최단거리점</returns>
        public Result3f xCompute()
        {
            Result3f result = new();
            Vector3 diff = point - box.center;

            float sqrDistance = 0;
            float delta;
            Vector3 closest = Vector3.Zero;
            int i;
            for (i = 0; i < 3; ++i)
            {
                closest[i] = diff.Dot(box.Axis(i));
                if (closest[i] < -box.extent[i])
                {
                    delta = closest[i] + box.extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = -box.extent[i];
                }
                else if (closest[i] > box.extent[i])
                {
                    delta = closest[i] - box.extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = box.extent[i];
                }
            }

            result.closest[0] = box.center;
            for (i = 0; i < 3; ++i)
            {
                result.closest[0] += closest[i] * box.Axis(i);
            }

            result.sqrDistance = sqrDistance;
            result.distance = (float)Math.Sqrt(sqrDistance);
            return result;
        }
        public Result3f Compute()
        {
            Result3f result = new();
            Vector3 diff = point - box.center;
            Vector3 closestPointInLocalSpace = Vector3.Zero;
            float sqrDistance = 0;

            Vector3[] axes = { box.axisX, box.axisY, box.axisZ };
            float[] extents = { box.extent.X, box.extent.Y, box.extent.Z };

            for (int i = 0; i < 3; ++i)
            {
                // 1. OBB 축에 대한 점의 로컬 위치를 계산
                float localCoord = Vector3.Dot(diff, axes[i]);

                // 2. 해당 축의 반너비(extent) 내로 클램핑
                float clampedCoord = Math.Clamp(localCoord, -extents[i], extents[i]);

                // 3. OBB 경계 밖일 경우, 거리 제곱에 기여하는 값을 계산
                float delta = localCoord - clampedCoord;
                sqrDistance += delta * delta;

                // 4. 클램핑된 로컬 위치로 월드 공간 벡터를 구성
                closestPointInLocalSpace += clampedCoord * axes[i];
            }

            // 5. 최종 결과 설정
            result.closest[0] = box.center + closestPointInLocalSpace;
            result.sqrDistance = sqrDistance;
            result.distance = (float)Math.Sqrt(sqrDistance);
            return result;
        }
    }
}
