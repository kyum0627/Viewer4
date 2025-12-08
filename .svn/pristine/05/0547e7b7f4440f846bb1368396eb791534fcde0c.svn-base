using IGX.Geometry.Common;
using IGX.Loader;
using IGX.Loader.AMFileLoader;

namespace ViewerTest
{
    /// <summary>
    /// 모델 데이터 로딩 및 관리를 전담.
    /// </summary>
    public class ModelFileLoader
    {
        private readonly Dictionary<string, Model3D> models = [];
        public AABB3 TotalBoundingBox = AABB3.Empty;

        /// <summary>
        /// 지정된 파일 경로에서 모델을 로드하고, 전체 바운딩 박스를 갱신.
        /// </summary>
        /// <param name="fileNames">로드할 파일 경로 배열</param>
        public void LoadFiles(string[] fileNames)
        {
            foreach (string filePath in fileNames)
            {
                if (models.ContainsKey(filePath))
                {
                    continue;
                }

                Model3D? newModel = LoadSingleFile(filePath);

                if (newModel != null)
                {
                    models[filePath] = newModel;
                    //TotalBoundingBox = TotalBoundingBox.Contain(newModel.ModelAABB);
                }
            }
        }

        /// <summary>
        /// 단일 파일 로딩을 위한 내부 헬퍼 메서드.
        /// </summary>
        /// <param name="filePath">로드할 파일 경로</param>
        /// <returns>로드된 Model3D 객체</returns>
        private static Model3D? LoadSingleFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".rev")
            {
                RevLoader loader = new(filePath);
                return new(loader.Header.Name, loader.assemblies, loader.geometries, loader.colors, loader.modelBoundingBox);
            }
            else if (fileExtension == ".rvm")
            {
                RvmLoader loader = new(filePath);
                return new(loader.Header.Name, loader.assemblies, loader.geometries, loader.colors, loader.modelBoundingBox);
            }

            return null;
        }

        /// <summary>
        /// 로드된 모든 모델 데이터를 지우고 바운딩 박스를 초기화.
        /// </summary>
        public void ClearAll()
        {
            models.Clear();
            TotalBoundingBox = AABB3.Empty;
        }

        /// <summary>
        /// 현재 로드된 모델의 딕셔너리를 반환.
        /// </summary>
        /// <returns>모델 딕셔너리</returns>
        public Dictionary<string, Model3D> GetModels()
        {
            return models;
        }
    }
}