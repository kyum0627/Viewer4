using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using IGX.Geometry.DataStructure;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGX.Geometry.GeometryBuilder
{

    public class TransformComponent
    {
        public Matrix4 Trans = Matrix4.Identity;
        public Vector3 Translation => Trans.ExtractTranslation();
        public Quaternion Rotation => Trans.ExtractRotation();
        public Vector3 Scale { get; set; } = Vector3.One;

        public void ApplyTransform(Matrix4 matrix) => Trans = matrix;
    }

    public class RenderComponent
    {
        public int GeometryID;
        public ParaPrimType GeometryType = ParaPrimType.None;
        public uint ColorID = 1;
        public string GrandPrimType = string.Empty;
        public GeometryInstance InstanceData = new();
    }

    public class CollisionComponent
    {
        private readonly MeshGeometry _mesh;
        private readonly BVH _bvh;

        public CollisionComponent(MeshGeometry mesh)
        {
            _mesh = mesh;
            _bvh = new BVH(mesh);
        }

        public AABB3 LocalAabb => _mesh.Aabb;

        public bool CollideBruteForce(Ray3f ray, out float? hitDistance)
        {
            hitDistance = float.MaxValue;
            bool hit = false;
            int numTriangles = _mesh.Indices.Count / 3;

            for (int j = 0; j < numTriangles; j++)
            {
                Triangle3f triangle = _mesh.GetTriangle(j, out AABB3 triangleAabb);
                if (triangleAabb.Intersects(ray, ref hitDistance))
                {
                    if (triangle.RayIntersectsTriangle(ray, out float intersectionDistance))
                    {
                        if (intersectionDistance < hitDistance)
                        {
                            hitDistance = intersectionDistance;
                            hit = true;
                        }
                    }
                }
            }
            return hit;
        }

        public bool Collide(Ray3f rayWorld, Matrix4 worldToLocal, out float? hitDistance)
        {
            Ray3f localRay = rayWorld.Transform(worldToLocal);
            return Collide(localRay, out hitDistance);
        }
        public bool Collide(Ray3f localRay, out float? hitDistance)
        {
            if (_bvh.Root == null && _mesh.Indices.Count > 0)
            {
                _bvh.Build();
            }
            return _bvh.Intersect(localRay, out hitDistance);
        }
    }

    public abstract class PrimitiveBase : IPrimitive, IEquatable<PrimitiveBase>
    {
        public abstract bool IsTransformable { get; }
        public MeshGeometry Mesh = new();
        public TransformComponent TransformComp { get; } = new();
        public RenderComponent RenderComp { get; } = new();
        public CollisionComponent CollisionComp { get; private set; }
        public void ApplyTransform(Matrix4 matrix) => TransformComp.ApplyTransform(matrix);

        public PrimitiveBase()
        {
            CollisionComp = new CollisionComponent(Mesh);
        }
        protected void InitializeComponents()
        {
            CollisionComp = new CollisionComponent(Mesh);
        }
        public uint ColorID { get => RenderComp.ColorID; set => RenderComp.ColorID = value; }
        public int GeometryID { get => RenderComp.GeometryID; set => RenderComp.GeometryID = value; }
        public string GrandPrimType = "PRIM";
        public ParaPrimType GeometryType { get => RenderComp.GeometryType; set => RenderComp.GeometryType = value; }
        public GeometryInstance InstanceData { get => RenderComp.InstanceData; set => RenderComp.InstanceData = value; }
        
        public List<Vector3> Positions => Mesh.Vertices;
        public List<Vector3> Normals => Mesh.Normals;
        public List<uint> Indices => Mesh.Indices;

        public Matrix4 Transform => TransformComp.Trans;
        public Vector3 Scale => TransformComp.Scale;
        public Quaternion Rotation =>  TransformComp.Rotation;
        public Vector3 Translation => TransformComp.Translation;

        public AABB3 Aabb => Mesh.Aabb;
        public OOBB3 Oobb => Mesh.Oobb;

        public bool CollideLocal(Ray3f ray, out float? hitDistance)
        {
            // BVH를 사용하여 로컬 충돌 테스트
            return CollisionComp.Collide(ray, out hitDistance);
        }
        public bool Collide(Ray3f rayWorld, out float? hitDistance)
        {
            // 월드 행렬 역변환
            Matrix4 worldToLocal = Transform.Inverted();
            return CollisionComp.Collide(rayWorld, worldToLocal, out hitDistance);
        }
        public bool CollideBruteForce(Ray3f rayLocal, out float? hitDistance)
        {
            return CollisionComp.CollideBruteForce(rayLocal, out hitDistance);
        }
        public bool Collide(Ray3f rayWorld)
        {
            return Collide(rayWorld, out _);
        }
        public abstract ParaPrimType GetParametricType();

        public bool Equals(PrimitiveBase? other)
        {
            if (other == null) return false;
            if (GeometryType != other.GeometryType) return false;

            if (!Positions.SequenceEqual(other.Positions)) return false;
            if (!Normals.SequenceEqual(other.Normals)) return false;
            if (!Indices.SequenceEqual(other.Indices)) return false;

            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + RenderComp.GeometryType.GetHashCode();
                hash = hash * 23 + Positions.Aggregate(0, (acc, p) => acc ^ p.GetHashCode());
                hash = hash * 23 + Normals.Aggregate(0, (acc, n) => acc ^ n.GetHashCode());
                hash = hash * 23 + Indices.Aggregate(0, (acc, i) => acc ^ i.GetHashCode());
                return hash;
            }
        }
    }
}
