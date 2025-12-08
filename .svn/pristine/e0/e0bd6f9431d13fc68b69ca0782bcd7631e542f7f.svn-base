using IGX.Geometry.DataStructure;
using IGX.Loader;

namespace ViewerTest
{
    public static class TreeViewHelper
    {
        public static void InitializeTreeView(TreeView treeView, Dictionary<string, Model3D> models)//, bool buildAssy = true)
        {
            treeView.Nodes.Clear();
            foreach (Model3D model in models.Values)
            {
                TreeNode node = new(model.Filename);
                treeView.Nodes.Add(node);
                BuildTree(node, model, true); // Build assembly tree, if true.
            }
            treeView.CheckBoxes = true;
        }

        private static void BuildTree(TreeNode treeNode, Model3D model, bool buildAssy = true)
        {
            Dictionary<int, TreeNode> nodeLookup = [];
            foreach (KeyValuePair<int, IAssembly> kvp in model.Eassemblies)
            {
                IAssembly assembly = kvp.Value;
                if (!buildAssy && assembly.GeometryIDs.Count == 0)
                {
                    continue;
                }
                // Geometry가 없으면 선택 불가 표시
                string name = $"{assembly.Name}[{assembly.ParentAssyID}, {assembly.ID}]";
                TreeNode newNode = new(name) { Tag = assembly.ID };
                if (assembly.GeometryIDs.Count == 0)
                {
                    newNode.ForeColor = Color.Gray; // 시각적 표시
                    newNode.Tag = new { assembly.ID, Selectable = false };
                }
                else
                {
                    newNode.ForeColor = Color.Blue; // 시각적 표시
                    newNode.Tag = new { assembly.ID, Selectable = true };
                }
                nodeLookup[assembly.ID] = newNode;

                if (assembly.ParentAssyID == -1)
                {
                    treeNode.Nodes.Add(newNode);
                }
                else
                {
                    if (nodeLookup.TryGetValue(assembly.ParentAssyID, out TreeNode? parentNode))
                    {
                        parentNode.Nodes.Add(newNode);
                    }
                    else
                    {
                        treeNode.Nodes.Add(newNode);
                    }
                }
            }
        }
    }
}