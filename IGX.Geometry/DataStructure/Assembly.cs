using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;
using IGX.Geometry.GeometryBuilder;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace IGX.Geometry.DataStructure
{
    public interface IAssembly
    {
        // 모든 속성을 자동 구현 속성으로 변경 가능하도록 public get/set 유지
        int ID { get; set; }
        string Name { get; set; }
        int ModelID { get; set; }
        int ParentAssyID { get; set; } // 부모가 없으면 -1
        uint Version { get; set; }
        AABB3 AssemblyAABB { get; set; }
        Vector3 Translation { get; set; }
        List<int> GeometryIDs { get; set; } // BufferGeometry의 MeshID 목록
        Matrix4 BaseMatrix { get; set; } // 변환 전 고유의 모델 행렬
        Matrix4 TransformedMatrix { get; set; } // S * Radius * T (변환된 최종 행렬)
        Matrix4 Transform { get; set; } // S * Radius * T (변환된 최종 행렬)
        uint Color { get; set; } // 고유 색상
        Dictionary<string, string> Attributes { get; set; } // 추가 속성 저장을 위한 딕셔너리
        List<int> SubEbom { get; set; } // 'subAssemblies'를 'SubEbom'로 변경 (C# 명명 규칙)

        Vector3 AssyNormal { get; set; }

        // 인터페이스에 메서드도 포함
        void AddGeometry(PrimitiveBase mesh);
        void RemoveGeometry(int geometryID);
        void ApplyTransformation(Matrix4 transformation);
        void ContainAndLock(AABB3 otherAabb);
    }

    public class Assembly : IAssembly
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty; // 기본값 초기화
        public int ModelID { get; set; }
        public int ParentAssyID { get; set; } = -1; // 부모가 없으면 -1
        public uint Version { get; set; }
        public AABB3 AssemblyAABB { get; set; } = AABB3.Empty;
        public OOBB3 OOBB { get; set; } = OOBB3.Empty; // 기본값 초기화
        public Vector3 Translation { get; set; } = Vector3.Zero; // 기본값 초기화
        public List<int> GeometryIDs { get; set; } = new(); // 기본값 초기화
        public Matrix4 BaseMatrix { get; set; } = Matrix4.Identity; // 변환 전 고유의 모델 행렬
        public Matrix4 TransformedMatrix { get; set; } = Matrix4.Identity; // S * Radius * T (최종 변환 행렬)
        public Matrix4 Transform { get; set; } = Matrix4.Identity;
        public Vector3 AssyNormal { get; set; } = Vector3.UnitZ; // 기본값 초기화 (명명 규칙 준수)
        public uint Color { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new(); // 기본값 초기화
        public List<int> SubEbom { get; set; } = new(); 
        public Assembly(int parentBomID = -1)
        {
            ParentAssyID = parentBomID;
            GeometryIDs = new List<int>();
            BaseMatrix = Matrix4.Identity; // 기본적으로 단위 행렬로 초기화
            Translation = Vector3.Zero;
            Color = 0;
            Name = string.Empty;
        }
        // 이름, 변환 및 재료 ID로 초기화하는 생성자
        public Assembly(int parent, string name, Vector3 translation, uint materialcolor)
        {
            ParentAssyID = parent;
            GeometryIDs = new List<int>();
            BaseMatrix = Matrix4.Identity; // 기본적으로 단위 행렬로 초기화
            Name = name; // 시스템 이름 설정
            Translation = translation; // 위치 변환 설정
            Color = materialcolor;
        }
        public void AddGeometry(PrimitiveBase mesh)
        {
            if (!GeometryIDs.Contains(mesh.GeometryID))//InstanceData.MeshID))
            {
                GeometryIDs.Add(mesh.GeometryID);// InstanceData.MeshID);
                AssemblyAABB = AssemblyAABB.Contain(mesh.Aabb);
            }
        }
        public void AddGeometry(int geoID)
        {
            if (!GeometryIDs.Contains(geoID))
            {
                GeometryIDs.Add(geoID);
            }
        }
        public void RemoveGeometry(int geometryID)
        {
            GeometryIDs.Remove(geometryID);
        }
        public void ApplyTransformation(Matrix4 transformation)
        {
            BaseMatrix *= transformation; // 기존 변환 행렬에 새 변환을 적용
        }
        public override string ToString()
        {
            return $"Assembly(MeshID: {ID}, name: {Name}, _parentID: {ParentAssyID}, GeometryCount: {GeometryIDs.Count}, Color: {Color}, Model: {BaseMatrix})";
        }
        private readonly object _aabbLock = new object();
        public void ContainAndLock(AABB3 otherAabb)
        {
            lock (_aabbLock)
            {
                AssemblyAABB = AssemblyAABB.Contain(otherAabb);
            }
        }
    }

}