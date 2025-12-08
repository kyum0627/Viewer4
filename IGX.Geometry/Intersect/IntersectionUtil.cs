using System.Collections.Generic;
using OpenTK.Mathematics;
//using IGX.Geometry.Operations.MinimumBox;
using IGX.Geometry.Common;
/// =============================================================================================
/// 다양한 기하 형상간의 교차점, 선 면을 계산하기 위해 필요한 utility (직선과 삼각형 및 구)
/// =============================================================================================
///
/// <summary>
/// 참고: https://www.geometrictools.com/Samples/Intersection.html
/// 상기 site에서 하기와 같은 샘플 애플리케이션들(CPP, C#, PS)을 볼 수 있음
/// </summary>
///
namespace IGX.Geometry.Intersect
{
    //Different boolean operations
    public enum BooleanOperation
    {
        Intersection,
        Difference,
        ExclusiveOr,
        Union
    }

    /// <summary>
    /// 가장 일반적인 intersection 유형별로 intersection
    /// 1. line2 - line2 : 2차원 평면내에서의 line 간 교차
    /// 2. line - triangle
    /// 3. line - sphere
    /// 4. ray - sphere (line-sphere와 동일)
    /// 5. ...
    /// 기하 형상간의 교차계산 결과를 저장
    /// 교차 형태는 점, 선분, 무한직선, 다각형, 평면 혹은 기타 정의되지 않은 형상이 될 수 있음
    /// </summary>
    public enum IntersectionResult // 계산 결과/현황 요약
    {
        NOTCOMPUTED,    // 아직 계산 안함
        INTERSECT,      // 교차함
        NOTINTERSECT,   // 교차하지 않음
        INVALID         // 교차계산 불가
    }

    public enum IntersectionType // 교차 유형
    {
        EMPTY,      // 교차하지 않음
        POINT,      // 점(s)으로 만남
        SEGMENT,    // 선분으로 만남 (겹치는 두 선분이 특정 부분에서 만남)
        RAY,        // 
        LINE,       // 선으로 만남 (서로 다른 무한 평면간 교차)
        PLANE,      // 면으로 만남 (무한면)
        POLYLINE,   // polyline으로 만남
        POLYGON,    // 다각형으로 만남 (동일 평면상에 있는 다각형간 교차)
        POLYHEDRON, // 다면체
        ETC,        // 정의되지 않은 형태, 예) 3D Volume
        NONE
    }

    public struct IntersectionResult2 // 교차계산결과
    {
        public bool intersects;             // 교차여부
        public int quantity;                // 교차횟수 0, 1, ...
        public Interval parameter1;          // 파라메터 t-values
        public Interval parameter2;          // 파라메터 t-values
        public IntersectionType type;       // 교차유형
        public IntersectionResult status;   //
        public List<Vector2> points;
    }

    public struct IntersectionResult3      // 교차계산결과
    {
        public bool intersects;             // 교차여부
        public int quantity;        // 교차횟수 0, 1, ...
        public Interval parameter1;          // 파라메터 t-values
        public Interval parameter2;
        public IntersectionType type;
        public IntersectionResult status;
        public List<Vector3> points;
    }

    public struct IntersectionResult4
    {
        public bool intersects;
        public int quantity; // 0 : no intersect, 1 : 한 점에서 만남, integer.max = 겹침
        public List<float> line0Parameter; // [2]
        public List<float> line1Parameter; // [2]
        public Vector2 point;
    }
}
