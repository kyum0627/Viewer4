using IGX.Geometry.Common;
using IGX.Geometry.DataStructure;
using IGX.Geometry.GeometryBuilder;
using IGX.Loader.AMFileLoader;
using Newtonsoft.Json;

namespace IGX.Loader
{
    public abstract class ModelLoader
    {
        public FileInformation Header;
        public Dictionary<int, PrimitiveBase> geometries = [];
        public Dictionary<int, IAssembly> assemblies = [];
        public Dictionary<uint, VolColor> colors = [];
        public AABB3 modelBoundingBox = AABB3.Empty;
        //Encoding utf8 = Encoding.GetEncoding("utf-8");

        public static readonly float continuityAngle = (float)Math.Cos(Math.PI / 6);
        public List<string> progress = [];// List<string>();
        public static bool IsFlat(Vertex[] vertices)
        { // 플랫 여부를 확인하는 메서드
            OpenTK.Mathematics.Vector3 baseNormal = vertices[0].Normal;
            foreach (Vertex vertex in vertices)
            {
                if (baseNormal != vertex.Normal)
                {
                    return false;
                }
            }
            return true;
        }

        public void CalculateAssemblyBox()
        {
            // 1단계: 모든 Assembly의 AABB 초기화
            foreach (var ass in assemblies.Values)
            {
                ass.AssemblyAABB = AABB3.Empty;
            }

            // 2단계: Leaf Assembly (Geometry를 가진)부터 상향식으로 AABB 계산
            // Geometry의 AABB를 먼저 해당 Assembly에 포함
            foreach (var ass in assemblies.Values)
            {
                if (ass.GeometryIDs.Count > 0)
                {
                    foreach (var geoId in ass.GeometryIDs)
                    {
                        if (geometries.TryGetValue(geoId, out var geo))
                        {
                            // Geometry의 AABB가 유효한지 확인
                            if (geo.Aabb != AABB3.Empty)
                            {
                                ass.AssemblyAABB = ass.AssemblyAABB.Contain(geo.Aabb);
                            }
                        }
                    }
                }
            }

            // 3단계: 하위 Assembly의 AABB를 상위로 전파 (Bottom-Up)
            // 깊이 역순으로 정렬하여 자식부터 처리
            var sortedAssemblies = assemblies.Values
                .OrderByDescending(a => GetAssemblyDepth(a))
                .ToList();

            foreach (var ass in sortedAssemblies)
            {
                if (ass.ParentAssyID != -1 && assemblies.TryGetValue(ass.ParentAssyID, out var parent))
                {
                    // 자식 Assembly의 AABB가 유효한 경우만 부모에 포함
                    if (ass.AssemblyAABB != AABB3.Empty)
                    {
                        parent.AssemblyAABB = parent.AssemblyAABB.Contain(ass.AssemblyAABB);
                    }
                }
            }

            // 4단계: 전체 모델 BoundingBox 계산
            modelBoundingBox = AABB3.Empty;
            foreach (var ass in assemblies.Values)
            {
                if (ass.ParentAssyID == -1) // Root Assembly만
                {
                    if (ass.AssemblyAABB != AABB3.Empty)
                    {
                        modelBoundingBox = modelBoundingBox.Contain(ass.AssemblyAABB);
                    }
                }
            }
        }

        /// <summary>
        /// Assembly의 깊이(Depth)를 계산 - Root는 0, 자식은 부모+1
        /// </summary>
        private int GetAssemblyDepth(IAssembly assembly)
        {
            int depth = 0;
            int currentParentId = assembly.ParentAssyID;
            
            while (currentParentId != -1 && assemblies.TryGetValue(currentParentId, out var parent))
            {
                depth++;
                currentParentId = parent.ParentAssyID;
                
                // 무한 루프 방지 (순환 참조)
                if (depth > assemblies.Count)
                {
                    break;
                }
            }
            
            return depth;
        }

        public static List<(int ID, string Name)> EmptyAssemblies(Dictionary<int, Assembly> assemblies)
        {
            // Geometry가 있는 Assembly만 추림
            Dictionary<int, Assembly> hasGeometry = assemblies
                .Where(a => a.Value.GeometryIDs?.Count > 0)
                .ToDictionary(a => a.Key, a => a.Value);

            // hasGeometry에 있는 항목들의 모든 조상들도 포함해야 하므로
            HashSet<int> extended = new(hasGeometry.Keys);

            foreach (Assembly? assembly in hasGeometry.Values)
            {  // 각 항목에 대해 ParentAssyID 체인을 따라 올라가면서 추가
                int parentId = assembly.ParentAssyID;
                while (parentId != -1 && assemblies.ContainsKey(parentId))
                {
                    if (!extended.Add(parentId))
                    {
                        break; // 이미 포함된 경우 중지
                    }

                    parentId = assemblies[parentId].ParentAssyID;
                }
            }

            // 확장된 hasGeometry에 포함되지 않은 나머지 Assembly들을 추림
            List<(int ID, string Name)> emptyAssemblies = assemblies
                .Where(a => !extended.Contains(a.Key))
                .Select(a => (a.Value.ID, a.Value.Name))
                .ToList();

            return emptyAssemblies;
        }

        public virtual string ToJson()
        {
            JsonSerializerSettings settings = new()
            { // 순환 참조 무시
                Formatting = Formatting.Indented,
                Converters = [new Vector2Converter(), new Vector3Converter(), new Vector4Converter()]
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public virtual void FromJson(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }
        public virtual void SaveToFile(string filePath)
        {
            string json = ToJson();
            File.WriteAllText(filePath, json);
        }
        public virtual void LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            FromJson(json);
        }
    }
}
