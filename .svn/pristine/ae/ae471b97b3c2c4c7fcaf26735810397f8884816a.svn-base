//using IGX.Geometry.DataStructure;
//using IGX.Geometry.GeometryBuilder;
//using IGX.Loader;
//using IGX.ViewControl;
//using OpenTK.Mathematics;

//namespace ViewerTest
//{
//    public static class SceneGraphHelper
//    {
//        public static SceneGraph BuildSceneGraph(Dictionary<string, Model3D> models, IDrawCommandManager geometryManager)
//        {
//            SceneGraph sceneGraph = new();
//            foreach (Model3D model in models.Values)
//            {
//                SceneNode rootNode = new(model.Filename, 0)
//                {
//                    LocalTransform = Matrix4.Identity,
//                    BoundingBox = model.ModelAABB
//                };
//                BuildSceneNodeTree(rootNode, model, geometryManager);
//                sceneGraph.AddRootNode(rootNode);
//            }
//            sceneGraph.UpdateAllTransforms();
//            return sceneGraph;
//        }

//        private static void BuildSceneNodeTree(SceneNode parentSceneNode, Model3D model, IDrawCommandManager geometryManager)
//        {
//            Dictionary<int, SceneNode> nodeLookup = new();
//            foreach (var kvp in model.Eassemblies)
//            {
//                IAssembly assembly = kvp.Value;
//                SceneNode newNode = new(assembly.Name, assembly.ID)
//                {
//                    LocalTransform = assembly.Transform,
//                    BoundingBox = assembly.AssemblyAABB
//                };
//                if (assembly.GeometryIDs.CommandCount > 0)
//                {
//                    bool anyVisibleGeometry = false;
//                    foreach (int geometryId in assembly.GeometryIDs)
//                    {
//                        if (model.Geometries.TryGetValue(geometryId, out PrimitiveBase? primitive))
//                        {
//                            CommandAllocationInfo allocation = geometryManager.FindOrRegister(primitive, assembly);
//                            newNode.AddGeometry(
//                                commandIndex: allocation.DrawCommandIndex,
//                                baseInstance: allocation.BaseInstance,
//                                instanceCount: 1//, // 각 어셈블리는 1개의 변환을 가진 단일 인스턴스
//                                //materialName: primitive.RenderComp.Material.Name
//                            );

//                            geometryManager.UpdateInstanceTransform(allocation.BaseInstance, assembly.Transform);

//                            anyVisibleGeometry = true;
//                        }
//                    }
//                    newNode.Visible = anyVisibleGeometry;
//                }
//                else
//                {
//                    newNode.Visible = false; // Geometry가 없는 그룹핑 노드는 비가시
//                }
//                nodeLookup[assembly.ID] = newNode;
//            }

//            foreach (var kvp in nodeLookup)
//            {
//                IAssembly assembly = model.Eassemblies[kvp.Key];
//                SceneNode currentNode = kvp.Value;

//                if (assembly.ParentAssyID == -1)
//                {
//                    parentSceneNode.AddChild(currentNode);
//                }
//                else
//                {
//                    if (nodeLookup.TryGetValue(assembly.ParentAssyID, out SceneNode? parentNode))
//                    {
//                        parentNode.AddChild(currentNode);
//                        parentNode.BoundingBox = parentNode.BoundingBox.Contain(currentNode.BoundingBox);
//                    }
//                    else
//                    {
//                        parentSceneNode.AddChild(currentNode);
//                        System.Diagnostics.Debug.WriteLine($"Warning: Parent Assembly ID {assembly.ParentAssyID} not found for {assembly.Name} (ID: {assembly.ID}). Attached to root.");
//                    }
//                }
//            }
//        }
//    }
//}