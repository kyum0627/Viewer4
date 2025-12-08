using IGX.Geometry.Common;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
namespace IGX.Geometry.GeometryBuilder
{
    public class Cube : PrimitiveBase
    {
        #region
        public override bool IsTransformable => true;
        public Vector3 Size = new Vector3(1, 1, 1);

        public override ParaPrimType GetParametricType() => ParaPrimType.Cube;
        #endregion
        public Cube(Matrix4 matrix, AABB3 bBoxLocal, float x = 1, float y = 1, float z = 1)
        {
            Size.X = x;
            Size.Y = y;
            Size.Z = z;
            TransformComp.Scale = Size / 2;
            TransformComp.Trans = matrix;
        }
        //public Cube() : base()
        //{
        //    GeometryType = ParaPrimType.Cube;
        //    GenerateMesh();
        //}
        //public void GenerateMesh()
        //{
        //    Mesh.Vertices.Clear();
        //    Mesh.Normals.Clear();
        //    Mesh.Indices.Clear();

        //    // 8 vertices of a cube
        //    var hx = Size.X / 2;
        //    var hy = Size.Y / 2;
        //    var hz = Size.Z / 2;

        //    var vertices = new List<Vector3>
        //    {
        //        new Vector3(-hx, -hy, -hz),
        //        new Vector3( hx, -hy, -hz),
        //        new Vector3( hx,  hy, -hz),
        //        new Vector3(-hx,  hy, -hz),
        //        new Vector3(-hx, -hy,  hz),
        //        new Vector3( hx, -hy,  hz),
        //        new Vector3( hx,  hy,  hz),
        //        new Vector3(-hx,  hy,  hz)
        //    };
        //    Mesh.Vertices.AddRange(vertices);

        //    // Normals (simplified, one per vertex)
        //    for (int i = 0; i < vertices.Count; i++)
        //        Mesh.Normals.Add(Vector3.Normalize(vertices[i]));

        //    // 12 triangles (2 per face)
        //    uint[] indices = {
        //        0,2,1, 0,3,2, // back
        //        4,5,6, 4,6,7, // front
        //        0,1,5, 0,5,4, // bottom
        //        2,3,7, 2,7,6, // top
        //        0,4,7, 0,7,3, // left
        //        1,2,6, 1,6,5  // right
        //    };
        //    Mesh.Indices.AddRange(indices);

        //    // Collision & Bounds 갱신
        //    InitializeComponents();
        //}
    }
}
