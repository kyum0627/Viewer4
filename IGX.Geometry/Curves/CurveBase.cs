//using IGX.Geometry.Common;
//using IGX.Geometry.ConvexHull;
//using IGX.Geometry.Curves;
//using IGX.Geometry.GeometryBuilder;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//public enum CurveType
//{
//    ArcCurve,
//    BezierCurve,
//    BSplineCurve,
//    EllipseCurve,
//    LeastSquareFitCurve,
//    LineCurve3
//}
//public abstract class CurveBase : IPrimitive, IBoundable
//{
//    public Vector3 Scale { get; set; } = Vector3.One;
//    public Quaternion Rotation { get; set; } = Quaternion.Identity;
//    public Vector3 Translation { get; set; } = Vector3.Zero;
//    public Matrix4 ModelMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Translation);
//    public List<Vector3> ControlPoints { get; protected set; } = new(); // UI 편집 친화적

//    public List<int> ControlPointIDs { get; protected set; } = new();
//    public PrimitiveType GLprimitive => PrimitiveType.LineStrip;
//    public AABB3 Aabb { get; set; } = AABB3.Empty;
//    public OOBB3 Oobb { get; set; } = OOBB3.Empty;

//    public float[]? CumulativeLengths { get; protected set; }
//    public int VertexCount => Vertices?.Count ?? 0;

//    public abstract void GenerateVertices();
//    public abstract void UpdateControlPoints(IEnumerable<Vector3> newControls);
//    public float Length => CumulativeLengths?.Last() ?? 0f;
//    protected void UpdateCumulativeLengths()
//    {
//        if (Vertices == null || Vertices.Count < 2)
//        {
//            CumulativeLengths = null;
//            return;
//        }

//        CumulativeLengths = new float[Vertices.Count];
//        CumulativeLengths[0] = 0f;
//        float total = 0f;
//        for (int i = 1; i < Vertices.Count; i++)
//        {
//            total += (Vertices[i] - Vertices[i - 1]).Length;
//            CumulativeLengths[i] = total;
//        }
//    }
//    protected void UpdateNormals()
//    {
//        Normals.Clear();
//        for (int i = 0; i < Vertices.Count(); i++)
//        {
//            Vector3 tangent;
//            if (i == 0) tangent = Vertices[1] - Vertices[0];
//            else if (i == Vertices.Count() - 1) tangent = Vertices[^1] - Vertices[^2];
//            else tangent = Vertices[i + 1] - Vertices[i - 1];

//            tangent = Vector3.Normalize(tangent);

//            // Z-up 기준으로 곡률 중심 방향 (법선) 계산
//            Vector3 Normal = new Vector3(-tangent.Y, tangent.X, 0);
//            Normals.Add(Vector3.Normalize(Normal));
//        }
//    }
//    public Vector3 GetPointAtLength(float length)
//    {
//        if (Vertices.Count() < 2 || CumulativeLengths == null)
//            return Vector3.Zero;

//        if (length >= CumulativeLengths[^1]) return Vertices[^1];

//        int idx = Array.BinarySearch(CumulativeLengths, length);
//        if (idx < 0) idx = ~idx - 1;
//        float t = (length - CumulativeLengths[idx]) / (CumulativeLengths[idx + 1] - CumulativeLengths[idx]);
//        return Vector3.Lerp(Vertices[idx], Vertices[idx + 1], t);
//    }
//    private int BinarySearchCumulativeLength(float length)
//    {
//        int lo = 0, hi = CumulativeLengths!.Length - 1;
//        while (lo < hi)
//        {
//            int mid = (lo + hi) / 2;
//            if (CumulativeLengths[mid] < length) lo = mid + 1;
//            else hi = mid;
//        }
//        return Math.Max(lo - 1, 0);
//    }
//    protected void UpdateBounds()
//    {
//        if (Vertices == null || Vertices.Count == 0)
//        {
//            Aabb = AABB3.Empty;
//            Oobb = OOBB3.Empty;
//            return;
//        }
//        foreach (var p in Vertices)
//        {
//            Aabb.Contain(p);
//            Oobb.Contain(p); // Convexhull로 고쳐야 함.
//        }
//    }
//    public Vector3 GetPointAtLengthFromStart(float length)
//    {
//        if (Vertices == null || Vertices.Count < 2 || CumulativeLengths == null)
//            return Vector3.Zero;

//        if (length >= Length) return Vertices[^1];

//        int idx = BinarySearchCumulativeLength(length);
//        float segStart = CumulativeLengths[idx];
//        float segLength = CumulativeLengths[idx + 1] - segStart;
//        float t = (length - segStart) / segLength;
//        return Vector3.Lerp(Vertices[idx], Vertices[idx + 1], t);
//    }
//    public Vector3 GetPointAtLengthFromEnd(float lengthFromEnd)
//    {
//        float target = Length - lengthFromEnd;
//        return GetPointAtLengthFromStart(Math.Max(target, 0f));
//    }
//    public void GenerateTubeMesh(float radius = 0.1f, uint circleSegments = 12, bool bCap = true)
//    {
//        if (Vertices == null || Vertices.Count < 2)
//            throw new InvalidOperationException("Points가 충분하지 않아 튜브를 생성할 수 없습니다.");

//        var (vertices, normals, indices) = TubeMeshBuilder.BuildTubeMeshOptimized(this, radius, circleSegments, bCap);

//        Vertices = vertices;
//        Normals = normals;
//        Indices = indices;

//        UpdateBounds();
//    }

//    public void Draw()
//    {
//        throw new NotImplementedException();
//    }
//}