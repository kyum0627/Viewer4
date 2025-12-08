using OpenTK.Mathematics;
using IGX.ViewControl;
using IGX.ViewControl.Render;

namespace ViewerTest
{
    public partial class IgxViewer : Form
    {
        private IgxViewAPI _api;
        private readonly ModelFileLoader _fileLoader = new();
        private readonly List<GLView> _viewManagers = new List<GLView>();
        public IgxViewer()
        {
            InitializeComponent();
            _api = IgxViewAPI.CreateDefault();
            
            // ModelsLoaded 이벤트 구독 - 모델 로딩 후 자동 갱신
            _api.ModelsLoaded += OnModelsLoaded;
            
            var view1 = CreateNewView();
            view1.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(view1);
            //DefaultSettingd();
        }

        private void OnModelsLoaded(object? sender, EventArgs e)
        {
            // 모든 뷰 갱신
            foreach (var view in _viewManagers)
            {
                if (view._glControl.InvokeRequired)
                {
                    view._glControl.Invoke(() => view._glControl.Invalidate());
                }
                else
                {
                    view._glControl.Invalidate();
                }
            }
        }

        public GLView CreateNewView()
        {
            var viewManager = new GLView(_api);
            _viewManagers.Add(viewManager);
            return viewManager;
        }

        private void OpenSecondView()
        {
            var context = _viewManagers[0]._glControl.Context;
        }

        private void DefaultSettingd()
        {
            _api.Drawing.ObjectBox = true;
            _api.Drawing.BackFace = true;
            _api.Drawing.BoxIsOOBB = true;
            _api.Drawing.Coordinates = true;
            _api.Drawing.CompanyLogo = true;
            _api.SceneParameter.CoordinatePosition = CoordinateDrawPosition.LeftDown;
            _api.SceneParameter.VectorSize = 1.0f;
            _api.SceneParameter.VectorColor = new Vector4(1, 0, 1, 0.5f);
            _api.SelectionManager.Selection.ByPart = true;
            _api.SelectionManager.Selection.Color = new Vector4(1, 0, 0, 1);
            _api.SelectionManager.Selection.DoWhat = SelectTo.ColorChange;
            _api.Shading.EdgeThickness = 1f;
            _api.Shading.EdgeColor = Vector3.Zero;
            _api.Shading.DisplayEdge = true;
            _api.Shading.Mode = ShadeMode.Phong;
        }
        private void LoadAdditionalModels(object sender, EventArgs e)
        {
            LoadNewModelsFromFile();
        }
        private void LoadNewModels(object sender, EventArgs e)
        {
            UnloadModelData();
            LoadNewModelsFromFile();
        }
        private void LoadNewModelsFromFile()
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "REV and RVM Files|*.rev;*.rvm|All Files|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _fileLoader.LoadFiles(openFileDialog.FileNames);
                _api.NewModels(_fileLoader.GetModels());
                UpdateTreeView();
            }
        }

        private void UpdateTreeView()
        {
            treeView1.Nodes.Clear();
            TreeViewHelper.InitializeTreeView(treeView1, _fileLoader.GetModels());
        }
        private void UnloadModelData()
        {
            _fileLoader.ClearAll();
            _api.UnloadModelData();
            treeView1.Nodes.Clear();
        }
        private void UnLoadAll(object sender, EventArgs e)
        {
            UnloadModelData();
        }
        private void drawToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void subViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSecondView();
        }
    }
}
