using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;

namespace IGX.Loader.AMFileLoader
{
    public static class PrimitiveBinaryDataReader
    {
        /// <summary>
        /// Primitive Type별 Parsing 함수
        /// </summary>
        public static readonly Dictionary<ParaPrimType, Func<MemoryStream, Matrix4, AABB3, PrimitiveBase?>> PrimParseFunctionMap = new()
        {
            { ParaPrimType.Pyramid, ReadPyramidRvm },
            { ParaPrimType.Cube, ReadBoxRvm },
            { ParaPrimType.RectangularTorus, ReadRectangularTorusRvm },
            { ParaPrimType.CircularTorus, ReadCircularTorusRvm },
            { ParaPrimType.EllipticalDish, ReadElipticalDishRvm },
            { ParaPrimType.SphericalDish, ReadSphericalDishRvm },
            { ParaPrimType.Snout, ReadSnoutRvm },
            { ParaPrimType.Cylinder, ReadCylinderRvm },
            { ParaPrimType.Sphere, ReadSphereRvm },
            { ParaPrimType.Line, ReadVolLineRvm },
            { ParaPrimType.FacetVolume, ReadFacetVolumeRvm }
        };
        private static Pyramid? ReadPyramidRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // xb, yb, xt, yt, xo, yo, h
            float[] p = BinaryDataReader.ParseFloatsb(stream, 7);
            return GeometryFactory.BuildPyramid(matrix, bBoxLocal, p);
        }
        private static Cube? ReadBoxRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // x, y, z
            float[] p = BinaryDataReader.ParseFloatsb(stream, 3);
            return GeometryFactory.BuildBox(matrix, bBoxLocal, p);
        }
        private static RectangularTorus? ReadRectangularTorusRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // r_in, r_out, h, angle
            float[] p = BinaryDataReader.ParseFloatsb(stream, 4);
            return GeometryFactory.BuildRectangularTorus(matrix, bBoxLocal, p);
        }
        private static CircularTorus? ReadCircularTorusRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // (r_out - r_in) /2, OffsetSurface(r_in), angle
            float[] p = BinaryDataReader.ParseFloatsb(stream, 3);
            return GeometryFactory.BuildCircularTorus(matrix, bBoxLocal, p);
        }
        private static EllipticalDish? ReadElipticalDishRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // base_r, h, z_offset, scale_z
            float[] p = BinaryDataReader.ParseFloatsb(stream, 2);
            return GeometryFactory.BuildElipticalDish(matrix, bBoxLocal, p);
        }
        private static SphericalDish? ReadSphericalDishRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // dia, h, 
            float[] p = BinaryDataReader.ParseFloatsb(stream, 2);
            return GeometryFactory.BuildSpericalDish(matrix, bBoxLocal, p);
        }
        private static Snout? ReadSnoutRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // r_b, r_t, xo, yo, h, xb_s, yb_s, xt_s, yt_s
            float[] p = BinaryDataReader.ParseFloatsb(stream, 9);
            float[] firstFive = [.. p.Take(5)];//pID.Take(5).ToArray();
            float[] lastFour = [.. p.Skip(5)];//pID.Skip(5).ToArray();
            return GeometryFactory.BuildSnout(matrix, bBoxLocal, firstFive, lastFour);
        }
        private static Cylinder? ReadCylinderRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // dia, h
            float[] p = BinaryDataReader.ParseFloatsb(stream, 2);
            return GeometryFactory.BuildCylinder(matrix, bBoxLocal, p);
        }
        private static Sphere? ReadSphereRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        { // dia
            float[] diameter = BinaryDataReader.ParseFloatsb(stream, 1);
            return GeometryFactory.BuildSphere(matrix, bBoxLocal, diameter);
        }
        private static VolLine? ReadVolLineRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        {
            float[] p = BinaryDataReader.ParseFloatsb(stream, 2);
            return GeometryFactory.BuildVolLine(matrix, bBoxLocal, p);
        }
        private static FacetVolume? ReadFacetVolumeRvm(MemoryStream stream, Matrix4 matrix, AABB3 bBoxLocal)
        {
            // 1단계: 면(Facet)의 총 개수 읽기
            uint noOffacets = BinaryDataReader.ReadUint32(stream);
            List<Facet3> facets = new((int)noOffacets);
            bool b_all_triangles = true;

            Dictionary<Facet3, int> temporary = new Dictionary<Facet3, int>();
            // 2단계: 모든 면을 순차적으로 파싱
            for (int i = 0; i < noOffacets; i++)
            {
                Facet3 facet = new();

                // 아웃라인(outer contour)과 홀(hole)의 개수 읽기
                uint noOfHoles = BinaryDataReader.ReadUint32(stream) - 1;

                // 아웃라인의 정점 개수 및 정점 데이터 읽기
                uint outerVerticesCount = BinaryDataReader.ReadUint32(stream);
                Vertex[] outerVertices = BinaryDataReader.ParseVerticesb(stream, outerVerticesCount);

                // 삼각형 여부 확인
                if (outerVerticesCount > 3)
                {
                    b_all_triangles = false;
                }
                facet.Outter = new Loop3(outerVertices);

                // 홀이 있으면 파싱
                if (noOfHoles > 0)
                {
                    facet.Holes = new List<Loop3>((int)noOfHoles);
                    for (int k = 0; k < noOfHoles; k++)
                    {
                        uint holeVerticesCount = BinaryDataReader.ReadUint32(stream);
                        Vertex[] holeVertices = BinaryDataReader.ParseVerticesb(stream, holeVerticesCount);

                        if (holeVerticesCount > 3)
                        {
                            b_all_triangles = false;
                        }

                        Loop3 hole = new Loop3(holeVertices) { IsHole = true };
                        facet.Holes.Add(hole);
                    }
                }
                facets.Add(facet);
            }
            if (facets.Count == 0) return null;
            return new FacetVolume(matrix, bBoxLocal, facets, b_all_triangles);
        }
    }
}