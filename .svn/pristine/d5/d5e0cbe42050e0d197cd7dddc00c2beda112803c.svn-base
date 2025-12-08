using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using System.Linq;
namespace IGX.Geometry.GeometryBuilder
{
    public static class GeometryFactory
    {
        private static T? Build<T>(float[] parameters, Func<T> creator, params Func<float[], bool>[] conditions) where T : class
        {
            foreach (Func<float[], bool> condition in conditions)
            {
                if (!condition(parameters))
                {
                    Debug.WriteLine("Provided parameters do not meet the required conditions for creating the geometry.");
                    return null;
                }
            }
            return creator();
        }
        public static (AABB3 Aabb, OOBB3 Oobb) UpdateAabbOobb(Matrix4 matrix, AABB3 bBoxLocal)
        {
            Matrix4 Rot = Matrix4.CreateFromQuaternion(matrix.ExtractRotation());
            Vector3 Trn = matrix.ExtractTranslation();
            Vector3 Extents = bBoxLocal.Extents;

            OOBB3 Oobb = OOBB3.Empty;
            Oobb.center = Trn + bBoxLocal.Center.Transform(Rot);
            Oobb.extent = Extents;
            Oobb.axisX = Rot.Row0.Xyz;
            Oobb.axisY = Rot.Row1.Xyz;
            Oobb.axisZ = Rot.Row2.Xyz;
            AABB3 Aabb = AABB3.Empty;

            Vector3 worldExtents = new Vector3(
                Extents.X * MathF.Abs(Oobb.axisX.X) + Extents.Y * MathF.Abs(Oobb.axisY.X) + Extents.Z * MathF.Abs(Oobb.axisZ.X),
                Extents.X * MathF.Abs(Oobb.axisX.Y) + Extents.Y * MathF.Abs(Oobb.axisY.Y) + Extents.Z * MathF.Abs(Oobb.axisZ.Y),
                Extents.X * MathF.Abs(Oobb.axisX.Z) + Extents.Y * MathF.Abs(Oobb.axisY.Z) + Extents.Z * MathF.Abs(Oobb.axisZ.Z));

            Aabb = new AABB3(Oobb.center - worldExtents, Oobb.center + worldExtents);
            return (Aabb, Oobb);
        }
        public static Cube? BuildBox(Matrix4 matrix, AABB3 bBoxLocal, float[] xyz) =>
            Build(
                xyz, () => new Cube(matrix, bBoxLocal, xyz[0], xyz[1], xyz[2]), p => p.Length >= 3, p => p[0] > 0 && p[1] > 0 && p[2] > 0);

        public static Cylinder? BuildCylinder(Matrix4 matrix, AABB3 bBoxLocal, float[] d_h) =>
            Build(
                d_h,
                () => new Cylinder(matrix, bBoxLocal, d_h[0], d_h[1]),
                p => p.Length >= 2,
                p => p[0] > 0 && p[1] > 0
            );

        public static Sphere? BuildSphere(Matrix4 matrix, AABB3 bBoxLocal, float[] diameter) =>
            Build(
                diameter,
                () => new Sphere(matrix, bBoxLocal, diameter[0]),
                p => p.Length >= 1,
                p => p[0] > 0
            );

        public static VolLine? BuildVolLine(Matrix4 matrix, AABB3 bBoxLocal, float[] start_end) =>
            Build(
                start_end,
                () => new VolLine(matrix, bBoxLocal, start_end[0], start_end[1]),
                p => p.Length >= 2,
                p => !(p[0] == 0 && p[1] == 0));

        public static CircularTorus? BuildCircularTorus(Matrix4 matrix, AABB3 bBoxLocal, float[] rorohalf_ri_ang) =>
            Build(
                rorohalf_ri_ang,
                () => new CircularTorus(matrix, bBoxLocal, rorohalf_ri_ang[0], rorohalf_ri_ang[1], rorohalf_ri_ang[2]),
                p => p.Length >= 3,
                p => p[1] > 0);

        public static RectangularTorus? BuildRectangularTorus(Matrix4 matrix, AABB3 bBoxLocal, float[] ri_ro_h_ang) =>
            Build(
                ri_ro_h_ang,
                () => new RectangularTorus(matrix, bBoxLocal, ri_ro_h_ang[0], ri_ro_h_ang[1], ri_ro_h_ang[2], ri_ro_h_ang[3]),
                p => p.Length >= 4,
                p => p[0] > 0 && p[1] > 0 && p[2] > 0);

        public static EllipticalDish? BuildElipticalDish(Matrix4 matrix, AABB3 bBoxLocal, float[] r_h_zoffset_zscale) =>
            Build(
                r_h_zoffset_zscale,
                () => new EllipticalDish(matrix, bBoxLocal, r_h_zoffset_zscale[0], r_h_zoffset_zscale[1]),
                p => p.Length >= 2,
                p => p[0] > 0 && p[1] > 0);

        public static SphericalDish? BuildSpericalDish(Matrix4 matrix, AABB3 bBoxLocal, float[] d_h) =>
            Build(
                d_h,
                () => new SphericalDish(matrix, bBoxLocal, d_h[0], d_h[1]),
                p => p.Length >= 2,
                p => p[0] > 0 && p[1] > 0);

        public static Pyramid? BuildPyramid(Matrix4 matrix, AABB3 bBoxLocal, float[] p) =>
            Build(
                p,
                () => new Pyramid(matrix, bBoxLocal, p[0], p[1], p[2], p[3], p[4], p[5], p[6]),
                p => p.Length >= 7,
                p => p[0] > 0 && p[1] > 0 && p[2] > 0 && p[3] > 0 && p[6] > 0);

        public static Snout? BuildSnout(Matrix4 matrix, AABB3 bBoxLocal, float[] baseParams, float[] topParams)
        {
            return Build(
                baseParams.Concat(topParams).ToArray(),
                () => new Snout(
                    matrix, bBoxLocal,
                    baseParams[0], baseParams[1], baseParams[2], baseParams[3], baseParams[4],
                    topParams[0], topParams[1], topParams[2], topParams[3]),
                p => p.Length >= 9//, // baseParams 5 + topParams 4 = 9
            );
        }
    }
}