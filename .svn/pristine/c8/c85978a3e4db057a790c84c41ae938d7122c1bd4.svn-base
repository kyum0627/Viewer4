using IGX.Geometry.GeometryBuilder;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    public class SelectionManager
    {
        private readonly Model3dDataManager _modelManager;
        private readonly Model3dBufferManager _rendererManager;

        public SelectionOptions Selection = new SelectionOptions();
        public PickHelper Pick = new PickHelper();
        private readonly Dictionary<PickedGeometry, (Vector4 Color, SelectTo Mode)> _pickedList = [];
        public Dictionary<PickedGeometry, (Vector4 Color, SelectTo Mode)> PickedItems => _pickedList;
        
        public List<GLVertex> InstancedVectors = [];
        public List<BasicInstance> InstancedBoxes = [];

        public SelectionManager(Model3dDataManager modelManager, Model3dBufferManager rendererManager)
        {
            _modelManager = modelManager;
            _rendererManager = rendererManager;
        }

        public void Add(PickedGeometry item, Vector4 color, SelectTo selectMode) => _pickedList[item] = (color, selectMode);
        public bool Remove(PickedGeometry item) => _pickedList.Remove(item);
        public void Clear() => _pickedList.Clear();

        private void RevertBuffer(int mid, int gid)
        {
            PickedGeometry itemToRemove = new(mid, gid);
            _pickedList.Remove(itemToRemove);
            MeshInstanceGL instanceData = ModelQuery.GetMeshInstance(_modelManager.Models[mid], gid);
            if (_rendererManager._geometryBuffers[mid] is IInstancedGeometryBuffer<MeshInstanceGL> inst)
            {
                inst.UpdateSingleInstance(gid, instanceData);
            }
        }

        public void UpdatePickedList(int modelid, List<int> toUpdate)
        {
            bool bExist = Pick.ContainsPicked(modelid, toUpdate, PickedItems);

            if (bExist)
            {
                RemovePickedList(modelid, toUpdate);
            }
            else
            {
                AppendPickedList(modelid, toUpdate);
            }
            SelectedObjectsDetails(true);
        }

        public void RevertAll()
        {
            _pickedList.Keys.ToList().ForEach(item => RevertBuffer(item.ModelID, item.GeometryID));
            _pickedList.Clear();
        }

        private void AppendPickedList(int modelid, List<int> matchingGeometryKeys)
        {
            matchingGeometryKeys.ForEach(geoKey =>
            {
                PickedGeometry itemToAdd = new(modelid, geoKey);
                if (!_pickedList.ContainsKey(itemToAdd))
                {
                    Vector4 color = Selection.Color;
                    if (Selection.DoWhat == SelectTo.Transparent)
                    {
                        color.W = 0;
                    }
                    _pickedList[itemToAdd] = (color, Selection.DoWhat);
                    MeshInstanceGL inst = ModelQuery.GetMeshInstance(_modelManager.Models[itemToAdd.ModelID], itemToAdd.GeometryID);
                    _rendererManager.Highlight(inst, itemToAdd.ModelID, itemToAdd.GeometryID, Selection.DoWhat, color);
                }
            });
        }

        private void RemovePickedList(int? modelid, List<int> matchingGeometryKeys)
        {
            if (modelid.HasValue)
            {
                matchingGeometryKeys.ForEach(geoKey => RevertBuffer(modelid.Value, geoKey));
            }
        }

        public void SelectedObjectsDetails(bool BoxIsOOBB)
        {
            InstancedBoxes.Clear();
            InstancedVectors.Clear();

            foreach (var p in _pickedList)
            {
                if (p.Key.ModelID < 0 || p.Key.ModelID >= _modelManager.Models.Count) continue;

                Matrix4 boxMatrix = BoxIsOOBB
                    ? _modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID].Oobb.ToMatrix()
                    : _modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID].Aabb.ToMatrix();
                var mci = new BasicInstance();
                mci.Model = boxMatrix;
                mci.Color = new Vector4(0,0,1,1);
                InstancedBoxes.Add(mci);

                var vectorPosition = _modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID].Oobb.center;
                var vectorNormal = _modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID].Oobb.axisZ;
                if (_modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID].GeometryType == ParaPrimType.FacetVolume)
                {
                    var vol = _modelManager.Models[p.Key.ModelID].Geometries[p.Key.GeometryID] as FacetVolume;
                    vectorPosition = vol!.Volume.Surfaces[0].Median.Position;
                    vectorNormal = vol.Volume.Surfaces[0].Median.Normal;
                }
                InstancedVectors.Add(new GLVertex(vectorPosition, vectorNormal));
            }
        }
    }
}