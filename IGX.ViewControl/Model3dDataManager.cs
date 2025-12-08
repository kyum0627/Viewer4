using IGX.Geometry.Common;
using IGX.Loader;
using IGX.ViewControl.Render;

namespace IGX.ViewControl
{
    public class Model3dDataManager
    {
        public IReadOnlyList<Model3D> Models = new List<Model3D>();
        public List<MeshedModel> Buffers = new List<MeshedModel>();

        public AABB3 TotalAabb = AABB3.Empty;
        private readonly object _lockObject = new();

        public void NewModel(Dictionary<string, Model3D> source)
        {
            Models = new List<Model3D> (source.Count);
            Buffers = [];
            TotalAabb = AABB3.Empty;

            if (source == null) return;
            lock (_lockObject)
            {
                Clear();
                Models = source.Values.ToList();
                MakeModel3dBufferData();
            }
        }

        public void Clear()
        {
            TotalAabb = AABB3.Empty;
        }

        public void MakeModel3dBufferData()
        {
            lock (_lockObject)
            {
                int startModelID = 0;
                foreach (Model3D model in Models)
                {
                    if (model == null) { continue; }
                    var renderData = MeshedModelBuilder.Build(startModelID, model);
                    if (renderData.Vertices.Length == 0 || renderData.TriIndices.Length == 0) { continue; }
                    Buffers.Add(renderData);
                    TotalAabb = TotalAabb.Contain(renderData.ModelAabb);
                    startModelID++;
                }
            }
        }
    }
}