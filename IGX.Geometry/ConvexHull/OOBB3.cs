using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.ConvexHull
{
    /// <summary>
    /// 박스의 중심과 세 방향의 길이/2로 3차원 공간상의 Cube 정의
    /// local 좌표계 기준 및 world 좌표계 기준으로 둘 다 가능
    /// 3차원 임의의 좌표계 기준 박스 정의 및 3차원 형상간의 교차 계산을 위한 bounding box 정의에 사용
    /// </summary>
    public struct OOBB3 : IEquatable<OOBB3>
    {
        // 박스 무게중심 C, local x축 U[0], y축 U[1], z축 U[2]
        // x, y, z 각 방향으로 길이/2, 폭/2, 높이 e[0]/2, e[1], and e[2] 
        // 각각의 e[] 값은 모두 >= 0

        public Vector3 center; // bounding box의 중심
        public Vector3 extent; // bounding box의 L, B, D의 1/2 값을 나타내는 Vector, 양수
        public Vector3 axisX;  // bounding box를 정의하는 좌표계의 x 방향 벡터
        public Vector3 axisY;  // bounding box를 정의하는 좌표계의 y 방향 벡터
        public Vector3 axisZ;  // bounding box를 정의하는 좌표계의 z 방향 벡터
        public static readonly OOBB3 Empty = new(float.NaN);
        public static readonly OOBB3 UnitZeroCentered = new(Vector3.Zero, 0.5f * Vector3.One);
        public static readonly OOBB3 UnitPositive = new(0.5f * Vector3.One, 0.5f * Vector3.One);

        #region Constructor

        //public Polygon3 GetRectangle(Vector3 Direction)
        //{
        //    Direction = Vector3.Normalize(Direction);

        //    // direction이 어느 축과 평행한지 점검
        //    float dotX = Vector3.Dot(Direction, axisX);
        //    float dotY = Vector3.Dot(Direction, axisY);
        //    float dotZ = Vector3.Dot(Direction, axisZ);
        //    float absDotX = Math.Abs(dotX);
        //    float absDotY = Math.Abs(dotY);
        //    float absDotZ = Math.Abs(dotZ);

        //    Vector3 faceNormal; // 선택된 노말 face
        //    Vector3 uAxis, vAxis; // 선택된 face 평면의 두 축
        //    float uExtent, vExtent; // Rectangle3 크기

        //    if (absDotX >= absDotY && absDotX >= absDotZ)
        //    { // 가장 큰 축, 주어진 방향과 일치하는 면이 u축
        //        faceNormal = Math.Sign(dotX) * axisX;
        //        uAxis = axisY;
        //        vAxis = axisZ;
        //        uExtent = extent.Y;
        //        vExtent = extent.Z;
        //    }
        //    else if (absDotY >= absDotX && absDotY >= absDotZ)
        //    {// 가장 큰 축, 주어진 방향과 일치하는 면이 v축
        //     // Face is aligned with axisY
        //        faceNormal = Math.Sign(dotY) * axisY;
        //        uAxis = axisX;
        //        vAxis = axisZ;
        //        uExtent = extent.X;
        //        vExtent = extent.Z;
        //    }
        //    else
        //    {// 가장 큰 축, 주어진 방향과 일치하는 면이 w축
        //     // Face is aligned with axisZ
        //        faceNormal = Math.Sign(dotZ) * axisZ;
        //        uAxis = axisX;
        //        vAxis = axisY;
        //        uExtent = extent.X;
        //        vExtent = extent.Y;
        //    }

        //    // 면의 중심 계산
        //    // The calculation for faceCenter needs to be corrected.
        //    // It should be 'center + faceNormal * selected_extent'.
        //    // The original two lines for faceCenter calculation seem redundant and potentially incorrect.
        //    // Let's assume 'extent' is a Vector3 representing half-extents along local axes.
        //    float selectedExtentAlongNormal = 0;
        //    if (faceNormal == axisX || faceNormal == -axisX) selectedExtentAlongNormal = extent.X;
        //    else if (faceNormal == axisY || faceNormal == -axisY) selectedExtentAlongNormal = extent.Y;
        //    else if (faceNormal == axisZ || faceNormal == -axisZ) selectedExtentAlongNormal = extent.Z;

        //    Vector3 faceCenter = center + faceNormal * selectedExtentAlongNormal;

        //    // 평면의 4점 계산
        //    Vector3[] p = new Vector3[4];
        //    p[0] = faceCenter + uAxis * uExtent + vAxis * vExtent;
        //    p[1] = faceCenter - uAxis * uExtent + vAxis * vExtent;
        //    p[2] = faceCenter - uAxis * uExtent - vAxis * vExtent;
        //    p[3] = faceCenter + uAxis * uExtent - vAxis * vExtent;
        //    Plane3f plane = new(p[0], p[1], p[2]);
        //    return new Polygon3(plane, p.ToList());
        //}

        public OOBB3(float f)
        { // 중심이 (0,0,0) 박스 크기가 f 인 기본 박스 생성 
            center = Vector3.Zero;
            axisX = Vector3.UnitX; // default : global axis X
            axisY = Vector3.UnitY; // default : global axis Y
            axisZ = Vector3.UnitZ; // default : global axis Z
            extent = new Vector3(f, f, f);
        }

        /// <summary>
        /// 길이, 폭 및 높이가 정의되지 않은, 즉 세변의 길이가 0 인 박스 생성
        /// </summary>
        /// <param name="center"></param>
        ///
        public OOBB3(Vector3 center)
        {
            this.center = center;
            axisX = Vector3.UnitX; // default : global axis X
            axisY = Vector3.UnitY; // default : global axis Y
            axisZ = Vector3.UnitZ; // default : global axis Z
            extent = Vector3.Zero;
        }
        public OOBB3(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            center = translation;

            // OBB 축을 회전된 기본 축으로 초기화
            Vector3[] rotatedAxes = {
                rotation * Vector3.UnitX,
                rotation * Vector3.UnitY,
                rotation * Vector3.UnitZ
            };

            // scale의 절대값을 배열로 저장
            float[] scales = {
                MathF.Abs(scale.X),
                MathF.Abs(scale.Y),
                MathF.Abs(scale.Z)
            };

            // 가장 작은 scale 값을 가진 인덱스 찾기
            int minIndex = 0;
            if (scales[1] < scales[0])
            {
                minIndex = 1;
            }

            if (scales[2] < scales[minIndex])
            {
                minIndex = 2;
            }

            // 축과 반너비를 재정렬
            Vector3[] finalAxes = new Vector3[3];
            float[] finalExtents = new float[3];

            // 가장 작은 scale에 해당하는 축과 반너비를 OBB의 Z축으로 할당
            finalAxes[2] = rotatedAxes[minIndex];
            finalExtents[2] = scales[minIndex];

            // 나머지 두 축을 순환적으로 할당
            finalAxes[0] = rotatedAxes[(minIndex + 1) % 3];
            finalAxes[1] = rotatedAxes[(minIndex + 2) % 3];
            finalExtents[0] = scales[(minIndex + 1) % 3];
            finalExtents[1] = scales[(minIndex + 2) % 3];

            // 최종 결과 할당 및 정규화
            axisX = Vector3.Normalize(finalAxes[0]);
            axisY = Vector3.Normalize(finalAxes[1]);
            axisZ = Vector3.Normalize(finalAxes[2]);

            extent = new Vector3(finalExtents[0], finalExtents[1], finalExtents[2]);
        }
        /// <summary>
        /// world 좌표계 기준 박스 정의
        /// </summary>
        /// <param name="center"></param>
        /// <param name="hal.Length"></param>
        public OOBB3(Vector3 center, Vector3 halfLength)
        {
            this.center = center;
            extent = halfLength;
            axisX = Vector3.UnitX;
            axisY = Vector3.UnitY;
            axisZ = Vector3.UnitZ;
        }

        //public OOBB3(Vector3 center, Vector3 x, Vector3 y, Vector3 z, Vector3 extent)
        //{
        //    this.center = center;
        //    axisX = x; axisY = y; axisZ = z;
        //    this.extent = extent;
        //}

        /// <summary>
        /// bounding 박스 크기의 박스 정의
        /// </summary>
        /// <param name="boundingBox"></param>
        public OOBB3(AABB3 boundingBox)
        {
            extent = new Vector3(boundingBox.Extents[0], boundingBox.Extents[1], boundingBox.Extents[2]);
            center = boundingBox.Center;
            axisX = Vector3.UnitX;
            axisY = Vector3.UnitY;
            axisZ = Vector3.UnitZ;
        }

        public OOBB3(Segment3f seg, float radius = 1e-05f)
        {
            center = seg.center;
            axisZ = seg.direction;
            //axisZ.MakePerpVectors(out axisX, out axisY);
            axisZ.MakeUVnormalsFromW(out axisX, out axisY);
            extent = new Vector3(radius, radius, seg.Extent);
        }

        #endregion

        #region Get, Set

        ///// <summary>
        ///// bounding box의 대각선 벡터
        ///// </summary>
        //public Vector3 Diagonal
        //{
        //    get
        //    {
        //        return e2.X * axisX + e2.Y * axisY + e2.Z * axisZ
        //            - (-e2.X * axisX - e2.Y * axisY - e2.Z * axisZ);
        //    }
        //}
        public readonly float MaxExtent => Math.Max(extent.X, Math.Max(extent.Y, extent.Z));
        public readonly float MinExtent => Math.Min(extent.X, Math.Min(extent.Y, extent.Z));
        public readonly float Volume => 8f * extent.X * extent.Y * extent.Z;
        //public Vector3 OOBB3normal => axisZ;

        #endregion
        public Vector3 Axis(int i) => i == 0 ? axisX : i == 1 ? axisY : axisZ;

        /// <summary>
        /// W가 최단축이 되도록 좌표축 및 길이 변환
        /// </summary>
        /// <summary>
        /// W가 최단축이 되도록 좌표축 및 길이 변환
        /// </summary>
        public void AxisOrder()
        {
            // 최단축 찾기
            float[] extents = { extent.X, extent.Y, extent.Z };
            int shortestIndex = 0;
            if (extents[1] < extents[shortestIndex])
            {
                shortestIndex = 1;
            }
            if (extents[2] < extents[shortestIndex])
            {
                shortestIndex = 2;
            }

            Vector3 newU, newV, newW;
            float newExtU, newExtV, newExtW;

            if (shortestIndex == 0) // X-axis is the shortest
            {
                newW = axisX; newExtW = extent.X;
                newU = axisY; newExtU = extent.Y;
                newV = axisZ; newExtV = extent.Z;
            }
            else if (shortestIndex == 1) // Y-axis is the shortest
            {
                newW = axisY; newExtW = extent.Y;
                newU = axisZ; newExtU = extent.Z;
                newV = axisX; newExtV = extent.X;
            }
            else // Z-axis is the shortest
            {
                newW = axisZ; newExtW = extent.Z;
                newU = axisX; newExtU = extent.X;
                newV = axisY; newExtV = extent.Y;
            }

            axisX = newU;
            axisY = newV;
            axisZ = newW;
            extent = new Vector3(newExtU, newExtV, newExtW);
            //if (axisZ.Y * center.Y > 0)
            //{
            //    axisX *= -1;
            //    axisY *= -1;
            //    axisZ *= -1;
            //}
        }

        ///// <summary>
        ///// Approximation 방법으로 OOBB를 생성한 후 minimum.Length axis를 중심으로 rotating calippers 알고리즘 적용
        ///// </summary>
        ///// <param name="inputpoints"></param>
        ///// <param name="epsilon"></param>
        //public void GetMinimumBox(Vector3[] inputpoints, float epsilon = float.Epsilon)
        //{
        //    ConvexHullProcessor cvh = new();// Convex Hull Processor를 사용하여 Oobb 계산
        //    cvh.ProcessNonPlanarPoints(inputpoints.ToList());
        //    this = cvh.Oobb;
        //    MakeZaxisToMinimum();
        //}

        public bool Collide(OOBB3 box1, float Tolerance = float.Epsilon)
        {
            Vector3 C0 = center;
            Vector3[] A0 = new Vector3[3] { axisX, axisY, axisZ };
            Vector3 E0 = extent;

            Vector3 C1 = box1.center;
            Vector3[] A1 = new Vector3[3] { box1.axisX, box1.axisY, box1.axisZ };
            Vector3 E1 = box1.extent;

            float cutoff = 1 - Tolerance; // JHJ
            bool existsParallelPair = false;

            Vector3 D = C1 - C0; // 박스 센터간 벡터

            float[,] dot01 = new float[3, 3];// dot01[i,j] = Dot(A[i],B[j]) = B[j,i]
            float[,] absDot01 = new float[3, 3];

            CalculateDotProducts(A0, A1, dot01, absDot01, cutoff, ref existsParallelPair);

            return
                TestSeparationOnAxes(D, A0, E0, E1, absDot01) ||
                TestSeparationOnAxes(D, A1, E0, E1, absDot01) ||
                TestSeparationOnCrossProducts(D, A0, A1, E0, E1, absDot01);
        }

        ///// <summary>
        ///// hschoi ; 22040710 기준축에 2개의 OOBB3를 프로젝션하고 크기를 비교함
        ///// </summary>
        ///// <param name="box1"></param>
        ///// <param name="box2"></param>
        ///// <param name="axis"></param>
        ///// <returns></returns>
        //public bool OverlapOnAxis(OOBB3 box1, OOBB3 box2, Vector3 axis)
        //{
        //    axis = Vector3.Normalize(axis);
        //    // Project both boxes onto the axis
        //    float min1, max1, min2, max2;
        //    ProjectBox(box1, axis, out min1, out max1);
        //    ProjectBox(box2, axis, out min2, out max2);

        //    // Check for overlap
        //    return (min1 <= max2 && max1 >= min2);
        //}
        ///// <summary>
        ///// hschoi : 20240710 기준축에 OOBB3를 프로젝션하고 결과 min/max를 리턴함
        ///// </summary>
        ///// <param name="box"></param>
        ///// <param name="axis"></param>
        ///// <param name="min"></param>
        ///// <param name="max"></param>
        //private void ProjectBox(OOBB3 box, Vector3 axis, out float min, out float max)
        //{
        //    Vector3[] axes = new Vector3[3];
        //    axes[0] = box.axisX;
        //    axes[1] = box.axisY;
        //    axes[2] = box.axisZ;

        //    // Project the center
        //    float centerProjection = Vector3.Dot(box.center, axis);

        //    // Calculate the e2 of the projection
        //    float extent = 0;
        //    for (int i = 0; i < 3; i++)
        //    {
        //        extent += Math.Abs(Vector3.Dot(axes[i], axis) * box.extent[i]);
        //    }

        //    min = centerProjection - extent;
        //    max = centerProjection + extent;
        //}
        private static void CalculateDotProducts(Vector3[] A0, Vector3[] A1, float[,] dot01, float[,] absDot01, float cutoff, ref bool existsParallelPair)
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    dot01[i, j] = Vector3.Dot(A0[i], A1[j]);
                    absDot01[i, j] = Math.Abs(dot01[i, j]);

                    if (absDot01[i, j] > cutoff)
                    {
                        existsParallelPair = true;
                    }
                }
            }
        }
        private static bool TestSeparationOnAxes(Vector3 D, Vector3[] A, Vector3 E0, Vector3 E1, float[,] absDot01)
        {
            float R, R0plusR1;
            float[] dotDA = new float[3];
            for (int i = 0; i < 3; ++i)
            {
                dotDA[i] = Vector3.Dot(D, A[i]);
                R = Math.Abs(dotDA[i]);

                // Compute radii for separation test
                float R1 = (E1[0] * absDot01[i, 0]) + (E1[1] * absDot01[i, 1]) + (E1[2] * absDot01[i, 2]);
                R0plusR1 = E0[i] + R1;

                if (R > R0plusR1)
                {
                    return false; // Separation found
                }
            }
            return true; // No separation found on this axis
        }

        private static bool TestSeparationOnCrossProducts(Vector3 D, Vector3[] A0, Vector3[] A1, Vector3 E0, Vector3 E1, float[,] absDot01)
        {
            float R, R0, R1, R0plusR1;
            float[,] dotDA0 = new float[3, 3];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    R = Math.Abs((dotDA0[i, 2] * absDot01[j, 0]) - (dotDA0[i, 1] * absDot01[j, 1]));
                    R0 = (E0[1] * absDot01[j, 0]) + (E0[2] * absDot01[j, 1]);
                    R1 = (E1[1] * absDot01[i, 0]) + (E1[2] * absDot01[i, 1]);
                    R0plusR1 = R0 + R1;

                    if (R > R0plusR1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public readonly Vector3[] ComputeVertices()//Vector3[] vertex)
        {
            Vector3[] vertex = new Vector3[8];
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;
            vertex[0] = center - extaxis0 - extaxis1 - extaxis2;
            vertex[1] = center + extaxis0 - extaxis1 - extaxis2;
            vertex[2] = center + extaxis0 + extaxis1 - extaxis2;
            vertex[3] = center - extaxis0 + extaxis1 - extaxis2;
            vertex[4] = center - extaxis0 - extaxis1 + extaxis2;
            vertex[5] = center + extaxis0 - extaxis1 + extaxis2;
            vertex[6] = center + extaxis0 + extaxis1 + extaxis2;
            vertex[7] = center - extaxis0 + extaxis1 + extaxis2;
            return vertex;
        }

        /// <summary>
        /// 정점 v를 포함하는 box로 e2
        /// </summary>
        /// <param name="v"></param>
        public void Contain(Vector3 v)
        {
            Vector3 lv = v - center;
            Vector3[] axes = { axisX, axisY, axisZ };

            for (int k = 0; k < 3; k++)
            {
                float t = Vector3.Dot(lv, axes[k]);
                float halfExtent = extent[k];

                if (MathF.Abs(t) > halfExtent)
                {
                    float centerOffset = (MathF.Abs(t) - halfExtent) / 2.0f;// 중심 이동 거리 계산: t와 기존 반너비의 차이의 절반
                    float newHalfExtent = MathF.Abs(t);// 새로운 반너비 계산: 확장된 점 t의 절댓값이 새로운 반너비

                    if (t > 0)
                    { // 중심을 확장된 방향으로 이동
                        center += axes[k] * centerOffset;
                    }
                    else
                    {
                        center -= axes[k] * centerOffset;
                    }
                    extent[k] = newHalfExtent;// 새로운 반너비 할당
                }
            }
        }

        public void Contain(List<Vector3> vtxs)
        {
            foreach(Vector3 v in vtxs)
            {
                Contain(v);
            }
        }

        ///// <summary>
        ///// points를 둘러싸는 최소 arbitrary axis bounding box 생성
        ///// </summary>
        ///// <param name="points"></param>
        //public void Contain(List<Vector3> points)
        //{
        //    IEnumerator<Vector3> points_itr = points.GetEnumerator();

        //    points_itr.MoveNext();
        //    Vector3 diff = points_itr.Current - center;
        //    Vector3 pmin = new(Vector3.Dot(diff, axisX), Vector3.Dot(diff, axisY), Vector3.Dot(diff, axisZ));
        //    Vector3 pmax = pmin;

        //    while (points_itr.MoveNext())
        //    {
        //        diff = points_itr.Current - center;

        //        float dotx = Vector3.Dot(diff, axisX);
        //        if (dotx < pmin[0])
        //            pmin[0] = dotx;
        //        else if (dotx > pmax[0])
        //            pmax[0] = dotx;

        //        float doty = Vector3.Dot(diff, axisY);
        //        if (doty < pmin[1])
        //            pmin[1] = doty;
        //        else if (doty > pmax[1])
        //            pmax[1] = doty;

        //        float dotz = Vector3.Dot(diff, axisZ);
        //        if (dotz < pmin[2])
        //            pmin[2] = dotz;
        //        else if (dotz > pmax[2])
        //            pmax[2] = dotz;
        //    }
        //    for (int j = 0; j < 3; ++j)
        //    {
        //        center += (float)0.5 * (pmin[j] + pmax[j]) * Axis(j);
        //        extent[j] = (float)0.5 * (pmax[j] - pmin[j]);
        //    }
        //}

        /// <summary>
        /// box o를 포함하는 box로 e2
        /// </summary>
        /// <param name="o"></param>
        public void Contain(OOBB3 o)
        {
            Vector3[] v = o.Vertices();
            for (int k = 0; k < 8; ++k)
            {
                Contain(v[k]);
            }
        }

        public bool Contains(OOBB3 o)
        {
            Vector3[] v = o.Vertices();
            for (int k = 0; k < 8; ++k)
            {
                if (!Contains(v[k]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// box가 정점 v를 포함하는지 검토
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Contains(Vector3 v)
        {
            Vector3 lv = v - center;
            return Math.Abs(Vector3.Dot(lv, axisX)) <= extent.X &&
                    Math.Abs(Vector3.Dot(lv, axisY)) <= extent.Y &&
                    Math.Abs(Vector3.Dot(lv, axisZ)) <= extent.Z;
        }

        /// <summary>
        /// box를 각 방향으로 길이 2*f 만큼 늘림/줄임(f가 음수이면)
        /// </summary>
        /// <param name="f"></param>
        public void Expand(float f)
        {
            extent.X += f;
            extent.Y += f;
            extent.Z += f;
        }

        ///// <summary>
        ///// points를 둘러싸는 최소 arbitrary axis bounding box approximation
        /////  - box center = average of the ControlPoints
        /////  - box axes   = eigenvectors of the covariance CombinedMatrix.
        ///// </summary>
        ///// <param name="points"></param>
        ///// <param name="box"></param>
        ///// <returns></returns>
        //public bool ApproximateMinimumBox(Vector3[] points, ref OOBB3 box, float tolerance = 0)
        //{
        //    // 점들을 가우시안 분포에 맞춰 피팅
        //    ApprGaussian3 fitter = new();
        //    fitter.Fit(points);

        //    if (fitter.GetShape() == ShapeType.Dimension3)
        //    {//3D 형태
        //        box = fitter.GetParameters();
        //        Vector3 pmin = Vector3.Zero, pmax = Vector3.Zero; // 각 축에 대한 최소, 최대값 계산
        //        foreach (var point in points)
        //        {
        //            Vector3 diff = point - box.center;
        //            for (int j = 0; j < 3; ++j)
        //            {
        //                float dot = Vector3.Dot(diff, box.Axis(j));
        //                pmin[j] = Math.Min(pmin[j], dot);
        //                pmax[j] = Math.Max(pmax[j], dot);
        //            }
        //        }

        //        // 경계 박스의 중심과 크기 계산
        //        for (int j = 0; j < 3; ++j)
        //        {
        //            box.center += 0.5f * (pmin[j] + pmax[j]) * box.Axis(j);
        //            box.extent[j] = 0.5f * (pmax[j] - pmin[j]) + tolerance;
        //        }

        //        // 박스를 회전시키는 행렬 적용
        //        box.extent = box.extent * new Matrix3(box.axisX, box.axisY, box.axisZ);

        //        return true;
        //    }
        //    else if (fitter.GetShape() == ShapeType.Dimension2)
        //    {
        //        // 2D인 경우, z축은 사용하지 않음
        //        box = fitter.GetParameters();

        //        // 각 축에 대한 최소값과 최대값 계산 (2D)
        //        Vector3 pmin = Vector3.Zero, pmax = Vector3.Zero;

        //        foreach (var point in points)
        //        {
        //            Vector3 diff = point - box.center;

        //            for (int j = 0; j < 2; ++j)  // x, y 축만 사용
        //            {
        //                float dot = Vector3.Dot(diff, box.Axis(j));

        //                pmin[j] = Math.Min(pmin[j], dot);
        //                pmax[j] = Math.Max(pmax[j], dot);
        //            }
        //        }

        //        // 경계 박스의 중심과 크기 계산 (2D)
        //        for (int j = 0; j < 2; ++j)
        //        {
        //            box.center += 0.5f * (pmin[j] + pmax[j]) * box.Axis(j);
        //            box.extent[j] = 0.5f * (pmax[j] - pmin[j]) + tolerance;
        //        }

        //        // 2D인 경우 z축은 무시하고 2D 평면에서만 크기를 적용
        //        box.extent[2] = 0f;  // z축 크기는 0으로 설정

        //        return true;
        //    }

        //    return false;  // 3D도 아니고 2D도 아닌 경우
        //}

        //public bool InContainer(Vector3 point, OOBB3 box)
        //{
        //    Vector3 diff = point - box.center;

        //    // 각 축에서 점이 박스 범위 내에 있는지 확인
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        float coeff = Vector3.Dot(diff, box.Axis(i));

        //        // 범위를 벗어나면 바로 false 반환
        //        if (Math.Abs(coeff) > box.extent[i])
        //            return false;
        //    }

        //    // 모든 축에서 범위 내에 있으면 true 반환
        //    return true;
        //}

        /// <summary>
        /// box를 v만큼 이동
        /// </summary>
        /// <param name="v"></param>
        public void Translate(Vector3 v)
        {
            center += v;
        }

        //public OOBB3 Trans(Matrix4 transform)
        //{
        //    Vector3 newCenter = Vector3.TransformPosition(this.center, transform);

        //    Matrix4 rotationScale = new(
        //            transform.M11, transform.M12, transform.M13, 0,
        //            transform.M21, transform.M22, transform.M23, 0,
        //            transform.M31, transform.M32, transform.M33, 0,
        //            0, 0, 0, 1
        //        );

        //    Vector3 newAxisX_rot = Vector3.TransformNormal(this.axisX, rotationScale).Normalized();
        //    Vector3 newAxisY_rot = Vector3.TransformNormal(this.axisY, rotationScale).Normalized();
        //    Vector3 newAxisZ_rot = Vector3.TransformNormal(this.axisZ, rotationScale).Normalized();

        //    // 각 축 방향으로의 스케일 팩터를 계산 (예: 원래 축을 변환했을 때 얼마나 길어졌는지)
        //    float scaleX = Vector3.TransformNormal(this.axisX, rotationScale).Length;
        //    float scaleY = Vector3.TransformNormal(this.axisY, rotationScale).Length;
        //    float scaleZ = Vector3.TransformNormal(this.axisZ, rotationScale).Length;

        //    Vector3 newExtent = new(
        //        this.extent.X * scaleX,
        //        this.extent.Y * scaleY,
        //        this.extent.Z * scaleZ
        //    );

        //    // 최종 Oobb 생성
        //    return new OOBB3(newCenter, newExtent, newAxisX_rot, newAxisY_rot, newAxisZ_rot);
        //}

        public AABB3 ToAABB()
        {
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;
            AABB3 result = AABB3.Empty;
            result.Contain(center - extaxis0 - extaxis1 - extaxis2);
            result.Contain(center + extaxis0 - extaxis1 - extaxis2);
            result.Contain(center + extaxis0 + extaxis1 - extaxis2);
            result.Contain(center - extaxis0 + extaxis1 - extaxis2);
            result.Contain(center - extaxis0 - extaxis1 + extaxis2);
            result.Contain(center + extaxis0 - extaxis1 + extaxis2);
            result.Contain(center + extaxis0 + extaxis1 + extaxis2);
            result.Contain(center - extaxis0 + extaxis1 + extaxis2);
            return result;
        }

        // corners [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z
        //
        //         7-----6 +z           or        3-----2 -z
        //        /|    /|                       /|    /|
        //       / 4---/-5                      / 0---/-1
        //      3-/---2 /                      7-/---6 / 
        //      |/    |/                       |/    |/
        //      0-----1  -z                    4-----5  +z
        //     -x     +x
        public Vector3 Corner(int i)
        {
            Vector3 result = center;

            if ((i & 1) != 0)
            {
                result += extent.X * axisX;
            }
            else
            {
                result -= extent.X * axisX;
            }

            if ((i & 2) != 0)
            {
                result += extent.Y * axisY;
            }
            else
            {
                result -= extent.Y * axisY;
            }

            if ((i & 4) != 0)
            {
                result += extent.Z * axisZ;
            }
            else
            {
                result -= extent.Z * axisZ;
            }

            return result;
        }

        // halfedgeSet
        //              
        //           *----10- -* +z         
        //          /|        /|   
        //         / |11     / |9
        //        6  |      5  |
        //       /   *---8-/---*    
        //      *---/-2---*   /     
        //      |  7      |  4 
        //     3| /       |1/
        //      |/        |/
        //      *----0----*       
        //          
        // face center
        //               
        // (2, 3)      *---------*        
        //  y         /|        /|   
        //  ^        / |    5  / |
        //  |       /  |      /  |
        //  |      / 0 *-----/---*    
        //  |     *---/-----* 1 /     
        //  |     |  /      |  /
        //  |  z  | /  4    | /
        //  | /   |/        |/
        //  |/    *---------*       
        //  ------------------->x (0, 1)       

        public Vector3 xFaceCenter(int i)
        {
            int axisIndex = i / 2;
            float sign = (i % 2 == 0) ? -1.0f : 1.0f;
            Vector3 result = center;

            switch (axisIndex)
            {
                case 0: // X-axis
                    result += sign * extent.X * axisX;
                    break;
                case 1: // Y-axis
                    result += sign * extent.Y * axisY;
                    break;
                case 2: // Z-axis
                    result += sign * extent.Z * axisZ;
                    break;
            }
            return result;
        }
        public readonly Vector3 FaceCenter(int i)
        {
            Vector3[] axes = { axisX, axisY, axisZ };
            float[] extents = { extent.X, extent.Y, extent.Z };
            int axisIndex = i / 2;
            float sign = (i % 2 == 0) ? -1.0f : 1.0f;
            return center + (sign * extents[axisIndex] * axes[axisIndex]);
        }
        //public void Flip()
        //{
        //    axisX.Y *= -1;
        //    axisY.Y *= -1;
        //    axisZ.Y *= -1;
        //    axisX *= -1;
        //    center.Y *= -1;
        //}

        //public void Scale(Vector3 s)
        //{
        //    extent.X *= s.X;
        //    extent.Y *= s.Y;
        //    extent.Z *= s.Z;

        //    //// HJJO 250814
        //    //center = Vector3.Cross(center, s);
        //    //extent = Vector3.Cross(extent, s);
        //    //axisX = Vector3.Cross(axisX, s); axisX = Vector3.Normalize(axisX);
        //    //axisY = Vector3.Cross(axisY, s); axisY = Vector3.Normalize(axisY);
        //    //axisZ = Vector3.Cross(axisZ, s); axisZ = Vector3.Normalize(axisZ);
        //}

        //public void ScaleExtents(Vector3 s)
        //{
        //    extent = Vector3.Cross(extent, s);
        //}

        public float DistanceSquared(Vector3 v)
        {
            if (Contains(v))
            {
                return 0f; // OOBB내부에 있으면 
            }

            Distance.DistancePoint3OOBB3 ddd = new(v, this);
            Distance.Result3f res = ddd.Compute();
            return res.sqrDistance;
        }
        //public float DistanceSquared(Vector3 v)
        //{
        //    Distance.DistancePoint3OOBB3 ddd = new Distance.DistancePoint3OOBB3(v, this);
        //    ddd.Compute();
        //    // Work in the box's coordinate system.
        //    v -= center;

        //    // Compute squared DistanceManager and closest point on box.
        //    float sqrDistance = 0;
        //    float delta;
        //    Vector3 closest = new();
        //    int i;
        //    for (i = 0; i < 3; ++i)
        //    {
        //        closest[i] = Vector3.Dot(Axis(i), v);
        //        if (closest[i] < -extent[i])
        //        {
        //            delta = closest[i] + extent[i];
        //            sqrDistance += delta * delta;
        //            closest[i] = -extent[i];
        //        }
        //        else if (closest[i] > extent[i])
        //        {
        //            delta = closest[i] - extent[i];
        //            sqrDistance += delta * delta;
        //            closest[i] = extent[i];
        //        }
        //    }

        //    return sqrDistance;
        //}
        public readonly Vector3 ClosestPoint(Vector3 v)
        {
            Vector3 p = v - center;// OBB의 로컬 공간으로 점 변환
            float distOnX = Vector3.Dot(p, axisX);// x축
            float clampedX = Math.Clamp(distOnX, -extent.X, extent.X);

            float distOnY = Vector3.Dot(p, axisY);// y축
            float clampedY = Math.Clamp(distOnY, -extent.Y, extent.Y);

            float distOnZ = Vector3.Dot(p, axisZ);// z축
            float clampedZ = Math.Clamp(distOnZ, -extent.Z, extent.Z);

            // 클램핑된 좌표를 월드 공간으로 다시 변환
            return center + (clampedX * axisX) + (clampedY * axisY) + (clampedZ * axisZ);
        }
        public Vector3 xClosestPoint(Vector3 v)
        {
            v -= center;

            float sqrDistance = 0;
            float delta;
            Vector3 closest = new();
            for (int i = 0; i < 3; ++i)
            {
                closest[i] = Vector3.Dot(Axis(i), v);
                float ext = extent[i];
                if (closest[i] < -ext)
                {
                    delta = closest[i] + ext;
                    sqrDistance += delta * delta;
                    closest[i] = -ext;
                }
                else if (closest[i] > ext)
                {
                    delta = closest[i] - ext;
                    sqrDistance += delta * delta;
                    closest[i] = ext;
                }
            }

            return center + (closest.X * axisX) + (closest.Y * axisY) + (closest.Z * axisZ);
        }

        /// <summary>
        /// 박스의 각 꼭지점(8개) 좌표를 계산해서 리턴
        /// </summary>
        /// <returns></returns>
        public Vector3[] Vertices()
        {
            Vector3[] v = new Vector3[8];
            Vertices(v);
            return v;
        }
        private void Vertices(Vector3[] vertex)
        {
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;

            for (int i = 0; i < 8; i++)
            {
                vertex[i] = center;
                if ((i & 1) != 0)
                {
                    vertex[i] += extaxis0;
                }
                else
                {
                    vertex[i] -= extaxis0;
                }

                if ((i & 2) != 0)
                {
                    vertex[i] += extaxis1;
                }
                else
                {
                    vertex[i] -= extaxis1;
                }

                if ((i & 4) != 0)
                {
                    vertex[i] += extaxis2;
                }
                else
                {
                    vertex[i] -= extaxis2;
                }
            }
        }
        private void xVertices(Vector3[] vertex)
        {
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;

            vertex[0] = center - extaxis0 - extaxis1 - extaxis2;
            vertex[1] = center + extaxis0 - extaxis1 - extaxis2;
            vertex[2] = center + extaxis0 + extaxis1 - extaxis2;
            vertex[3] = center - extaxis0 + extaxis1 - extaxis2;
            vertex[4] = center - extaxis0 - extaxis1 + extaxis2;
            vertex[5] = center + extaxis0 - extaxis1 + extaxis2;
            vertex[6] = center + extaxis0 + extaxis1 + extaxis2;
            vertex[7] = center - extaxis0 + extaxis1 + extaxis2;
        }

        #region Operetor, Copmapre, ...
        public IEnumerable<Vector3> VerticesItr()
        {
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;

            for (int i = 0; i < 8; i++)
            {
                Vector3 vertex = center;

                if ((i & 1) != 0)
                {
                    vertex += extaxis0;
                }
                else
                {
                    vertex -= extaxis0;
                }

                if ((i & 2) != 0)
                {
                    vertex += extaxis1;
                }
                else
                {
                    vertex -= extaxis1;
                }

                if ((i & 4) != 0)
                {
                    vertex += extaxis2;
                }
                else
                {
                    vertex -= extaxis2;
                }

                yield return vertex;
            }
        }
        public IEnumerable<Vector3> xVerticesItr()
        {
            Vector3 extaxis0 = extent.X * axisX;
            Vector3 extaxis1 = extent.Y * axisY;
            Vector3 extaxis2 = extent.Z * axisZ;
            yield return center - extaxis0 - extaxis1 - extaxis2;
            yield return center + extaxis0 - extaxis1 - extaxis2;
            yield return center + extaxis0 + extaxis1 - extaxis2;
            yield return center - extaxis0 + extaxis1 - extaxis2;
            yield return center - extaxis0 - extaxis1 + extaxis2;
            yield return center + extaxis0 - extaxis1 + extaxis2;
            yield return center + extaxis0 + extaxis1 + extaxis2;
            yield return center - extaxis0 + extaxis1 + extaxis2;
        }

        //---------------------------- for IEquatable<Box3f> ------------------
        public override bool Equals(object? obj)
        { // 20250212 HJJO
            return obj is OOBB3 other && Equals((OOBB3?)other);
        }

        public bool Equals(OOBB3 obj)
        {
            return obj is OOBB3 other
                && IsEqual(center.X, other.center.X) &&
                        IsEqual(center.Y, other.center.Y) &&
                        IsEqual(center.Z, other.center.Z) &&
                        IsEqual(axisX.X, other.axisX.X) &&
                        IsEqual(axisX.Y, other.axisX.Y) &&
                        IsEqual(axisX.Z, other.axisX.Z) &&
                        IsEqual(axisY.X, other.axisY.X) &&
                        IsEqual(axisY.Y, other.axisY.Y) &&
                        IsEqual(axisY.Z, other.axisY.Z) &&
                        IsEqual(axisZ.X, other.axisZ.X) &&
                        IsEqual(axisZ.Y, other.axisZ.Y) &&
                        IsEqual(axisZ.Z, other.axisZ.Z) &&
                        IsEqual(extent.X, other.extent.X) &&
                        IsEqual(extent.Y, other.extent.Y) &&
                        IsEqual(extent.Z, other.extent.Z);
        }
        public readonly bool Intersects(in Ray3f ray, out float? distance)
        {
            distance = 0;

            Vector3[] axes = { axisX, axisY, axisZ };
            float[] extents = { extent.X, extent.Y, extent.Z };

            Vector3 localOrigin = ray.origin - center;
            Vector3 localDirection = ray.direction;

            float tMin = float.MinValue;
            float tMax = float.MaxValue;

            for (int i = 0; i < 3; ++i)
            {
                // 로컬 좌표계에서의 레이 원점 및 방향 계산
                float localOriginComponent = Vector3.Dot(localOrigin, axes[i]);
                float localDirectionComponent = Vector3.Dot(localDirection, axes[i]);

                // 광선이 슬래브(slab)와 평행한 경우 처리
                if (MathF.Abs(localDirectionComponent) < float.Epsilon)
                {
                    if (MathF.Abs(localOriginComponent) > extents[i])
                    {
                        return false;
                    }
                }
                else
                {
                    float invD = 1.0f / localDirectionComponent;
                    float t0 = (-extents[i] - localOriginComponent) * invD;
                    float t1 = (extents[i] - localOriginComponent) * invD;

                    if (invD < 0.0f)
                    {
                        (t0, t1) = (t1, t0); // C# 튜플을 이용한 값 교환
                    }

                    tMin = MathF.Max(tMin, t0);
                    tMax = MathF.Min(tMax, t1);

                    if (tMax < tMin)
                    {
                        return false;
                    }
                }
            }

            // 교차점이 레이의 시작점 뒤에 있는지 확인
            if (tMax < 0.0f)
            {
                return false;
            }

            distance = tMin;
            return true;
        }

        //public bool Intersects(in Ray3f ray, ref float? distance)
        //{
        //    distance = null;
        //    float tMin = 0.0f;
        //    float tMax = float.MaxValue;

        //    Vector3 p = center - ray.Point;

        //    // X축 Slab 테스트
        //    float e = Vector3.Dot(axisX, p);
        //    float f = Vector3.Dot(axisX, ray.Direction);

        //    if (MathF.Abs(f) > 1e-6f)
        //    {
        //        float t1 = (e + extent.X) / f; // * 0.5f 제거
        //        float t2 = (e - extent.X) / f; // * 0.5f 제거

        //        if (t1 > t2) Swap(ref t1, ref t2);

        //        tMin = MathF.Max(tMin, t1);
        //        tMax = MathF.Min(tMax, t2);

        //        if (tMin > tMax) return false;
        //    }
        //    else if (MathF.Abs(e) > extent.X) // * 0.5f 제거
        //    {
        //        return false;
        //    }

        //    // Y축 Slab 테스트
        //    e = Vector3.Dot(axisY, p);
        //    f = Vector3.Dot(axisY, ray.Direction);

        //    if (MathF.Abs(f) > 1e-6f)
        //    {
        //        float t1 = (e + extent.Y) / f; // * 0.5f 제거
        //        float t2 = (e - extent.Y) / f; // * 0.5f 제거

        //        if (t1 > t2) Swap(ref t1, ref t2);

        //        tMin = MathF.Max(tMin, t1);
        //        tMax = MathF.Min(tMax, t2);

        //        if (tMin > tMax) return false;
        //    }
        //    else if (MathF.Abs(e) > extent.Y) // * 0.5f 제거
        //    {
        //        return false;
        //    }

        //    // Z축 Slab 테스트
        //    e = Vector3.Dot(axisZ, p);
        //    f = Vector3.Dot(axisZ, ray.Direction);

        //    if (MathF.Abs(f) > 1e-6f)
        //    {
        //        float t1 = (e + extent.Z) / f; // * 0.5f 제거
        //        float t2 = (e - extent.Z) / f; // * 0.5f 제거

        //        if (t1 > t2) Swap(ref t1, ref t2);

        //        tMin = MathF.Max(tMin, t1);
        //        tMax = MathF.Min(tMax, t2);

        //        if (tMin > tMax) return false;
        //    }
        //    else if (MathF.Abs(e) > extent.Z) // * 0.5f 제거
        //    {
        //        return false;
        //    }

        //    distance = tMin;
        //    return true;
        //}

        //private static void Swap<T>(ref T a, ref T b)
        //{
        //    (a, b) = (b, a);
        //}

        // NaN 값을 처리하기 위한 비교 메서드
        private bool IsEqual(float a, float b)
        {// 2025.02.12 HJJO
            return float.IsNaN(a) && float.IsNaN(b) || a == b;
        }
        public Matrix4 ToMatrix()
        {
            Vector3 x = axisX * extent.X * 2;
            Vector3 y = axisY * extent.Y * 2;
            Vector3 z = axisZ * extent.Z * 2;
            Vector3 t = center;

            return new Matrix4(
                x.X, x.Y, x.Z, 0f,  // 첫 번째 행 (X축 방향)
                y.X, y.Y, y.Z, 0f,  // 두 번째 행 (Y축 방향)
                z.X, z.Y, z.Z, 0f,  // 세 번째 행 (Z축 방향)
                t.X, t.Y, t.Z, 1f   // 네 번째 행 (평행이동)
            );
        }

        //public void MakeZaxisToMinimum()
        //{
        //    OOBB3 minz = Empty;
        //    Vector3 ax = axisX;
        //    Vector3 ay = axisY;
        //    Vector3 az = axisZ;
        //    float dx = extent[0];
        //    float dy = extent[1];
        //    float dz = extent[2];

        //    float mind = extent[0];
        //    int mini = 0;

        //    for (int i = 0; i < 3; i++)
        //    {
        //        if (extent[i] < mind)
        //        {
        //            mind = extent[i];
        //            mini = i;
        //        }
        //    }
        //    if (mini == 2)
        //    {
        //        return;
        //    }
        //    if (mini == 0)
        //    {
        //        axisZ = ax;
        //        axisX = ay;
        //        axisY = az;
        //        extent[2] = dx;
        //        extent[0] = dy;
        //        extent[1] = dz;
        //    }
        //    else
        //    {
        //        axisZ = ay;
        //        axisX = az;
        //        axisY = ax;
        //        extent[2] = dy;
        //        extent[0] = dz;
        //        extent[1] = dx;
        //    }
        //}
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ center.GetHashCode();
                hash = (hash * 16777619) ^ axisX.GetHashCode();
                hash = (hash * 16777619) ^ axisY.GetHashCode();
                hash = (hash * 16777619) ^ axisZ.GetHashCode();
                hash = (hash * 16777619) ^ extent.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(OOBB3 a, OOBB3 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(OOBB3 a, OOBB3 b)
        {
            return !a.Equals(b);
        }
        public Vector3[][] GetLargestFaceCorners(OOBB3 box)
        {
            // OBB의 축과 반너비를 배열로 저장
            Vector3[] axes = { box.axisX, box.axisY, box.axisZ };
            float[] extents = { box.extent.X, box.extent.Y, box.extent.Z };

            // 가장 넓은 면을 한 번의 순회로 찾기
            int normalAxisIndex = 0;
            float maxArea = extents[0] * extents[1];

            if (extents[1] * extents[2] > maxArea)
            {
                maxArea = extents[1] * extents[2];
                normalAxisIndex = 0; // X축이 법선
            }
            if (extents[2] * extents[0] > maxArea)
            {
                normalAxisIndex = 1; // Y축이 법선
            }

            // 가장 넓은 면을 정의하는 두 축과 법선 축을 결정
            int tangentAxisIndex1, tangentAxisIndex2;
            if (normalAxisIndex == 0) // X축이 법선인 경우 (YZ 평면)
            {
                tangentAxisIndex1 = 1; tangentAxisIndex2 = 2;
            }
            else if (normalAxisIndex == 1) // Y축이 법선인 경우 (ZX 평면)
            {
                tangentAxisIndex1 = 2; tangentAxisIndex2 = 0;
            }
            else // Z축이 법선인 경우 (XY 평면)
            {
                tangentAxisIndex1 = 0; tangentAxisIndex2 = 1;
            }

            Vector3 normalAxis = axes[normalAxisIndex];
            Vector3 tangentAxis1 = axes[tangentAxisIndex1];
            Vector3 tangentAxis2 = axes[tangentAxisIndex2];

            float normalExtent = extents[normalAxisIndex];
            float tangentExtent1 = extents[tangentAxisIndex1];
            float tangentExtent2 = extents[tangentAxisIndex2];

            // 꼭짓점을 저장할 2차원 배열 초기화
            Vector3[][] corners = new Vector3[2][];
            corners[0] = new Vector3[4]; // 음의 방향 면
            corners[1] = new Vector3[4]; // 양의 방향 면

            // 루프를 통해 두 면의 모든 꼭짓점을 계산
            for (int i = 0; i < 4; i++)
            {
                Vector3 tangent1 = ((i & 1) == 0) ? -tangentExtent1 * tangentAxis1 : tangentExtent1 * tangentAxis1;
                Vector3 tangent2 = ((i & 2) == 0) ? -tangentExtent2 * tangentAxis2 : tangentExtent2 * tangentAxis2;

                // 음의 방향 면
                Vector3 faceOffsetNeg = normalAxis * -normalExtent;
                corners[0][i] = box.center + tangent1 + tangent2 + faceOffsetNeg;

                // 양의 방향 면
                Vector3 faceOffsetPos = normalAxis * normalExtent;
                corners[1][i] = box.center + tangent1 + tangent2 + faceOffsetPos;
            }

            return corners;
        }
        public List<Vector3> xGetLargestFaceCorners(OOBB3 box)
        {
            // 가장 넓은 면의 꼭지점 4개 가져오기
            float areaXY = box.extent.X * box.extent.Y;
            float areaYZ = box.extent.Y * box.extent.Z;
            float areaZX = box.extent.Z * box.extent.X;

            // 가장 넓은 면 찾기
            int faceType = 0; // 0: XY, 1: YZ, 2: ZX
            if (areaYZ > areaXY && areaYZ > areaZX)
            {
                faceType = 1;
            }
            else if (areaZX > areaXY && areaZX > areaYZ)
            {
                faceType = 2;
            }

            // 각 면에 해당하는 꼭짓점 인덱스
            int[][] faceCornerIndices = new int[3][]
            {
                new int[] { 0, 1, 2, 3 }, // XY 평면 (Z -)
                new int[] { 1, 2, 6, 5 }, // YZ 평면 (X +)
                new int[] { 2, 3, 7, 6 }  // ZX 평면 (Y +)
            };

            int[] indices = faceCornerIndices[faceType];

            return indices.Select(i => box.Corner(i)).ToList();
        }

        #endregion

        public Matrix4 ToMtarix()
        { // scale * rotation * translation
            Matrix4 rot = new(new Vector4(axisX), new Vector4(axisY), new Vector4(axisZ), Vector4.UnitW);
            return Matrix4.CreateScale(new Vector3(2 * extent)) * rot * Matrix4.CreateTranslation(center);
        }
    }
}