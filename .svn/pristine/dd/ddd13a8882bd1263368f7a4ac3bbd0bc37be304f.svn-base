using System.ComponentModel;

namespace IGX.ViewControl
{
    [ToolboxItem(false)]
    public class GLViewToolbar : ToolStrip
    {
        private ToolStripButton? LookAfter;
        private ToolStripButton? LookForward;
        private ToolStripButton? LookStarboard;
        private ToolStripButton? LookPort;
        private ToolStripButton? LookBottom;
        private ToolStripButton? LookTop;
        private ToolStripButton? LookISO;
        private ToolStripSeparator? toolStripSeparator1;
        private ToolStripButton? Xray;
        private ToolStripButton? toolStripButton3;
        private ToolStripButton? PICK;
        private ToolStripButton? ClipBoxButton;

        public event EventHandler? ViewDirectionClicked;
        public event EventHandler? RenderModeToggled;
        public event EventHandler? SelectToggled;
        public event EventHandler? FitModelClicked;
        public event EventHandler? ClipBoxToggled;

        public GLViewToolbar()
        {
            if (!DesignMode)
            {
                InitializeComponent();
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager? resources = null;
            
            try
            {
                resources = new ComponentResourceManager(typeof(GLView));
            }
            catch
            {
                // Designer mode에서 리소스를 찾을 수 없을 경우 무시
            }
            
            LookAfter = new ToolStripButton();
            LookForward = new ToolStripButton();
            LookStarboard = new ToolStripButton();
            LookPort = new ToolStripButton();
            LookBottom = new ToolStripButton();
            LookTop = new ToolStripButton();
            LookISO = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            Xray = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            PICK = new ToolStripButton();
            ClipBoxButton = new ToolStripButton();
            
            SuspendLayout();
            
            // toolStrip configuration
            Dock = DockStyle.Right;
            ImageScalingSize = new Size(20, 20);
            Items.AddRange(new ToolStripItem[]
            {
                LookAfter, LookForward, LookStarboard, LookPort, LookBottom, LookTop, LookISO,
                toolStripSeparator1, Xray, toolStripButton3, PICK, ClipBoxButton
            });
            Location = new Point(985, 0);
            Name = "GLViewToolbar";
            Size = new Size(40, 648);
            TabIndex = 0;
            Text = "GLViewToolbar";
            
            // LookAfter
            LookAfter.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookAfter.Image = (Image?)resources.GetObject("LookAfter.Image");
            LookAfter.ImageTransparentColor = Color.Magenta;
            LookAfter.Name = "LookAfter";
            LookAfter.Size = new Size(37, 24);
            LookAfter.Tag = "LookAfter";
            LookAfter.Text = "LookAfter";
            LookAfter.Click += OnViewDirectionClick;
            
            // LookForward
            LookForward.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookForward.Image = (Image?)resources.GetObject("LookForward.Image");
            LookForward.ImageTransparentColor = Color.Magenta;
            LookForward.Name = "LookForward";
            LookForward.Size = new Size(37, 24);
            LookForward.Tag = "LookForward";
            LookForward.Text = "LookForward";
            LookForward.Click += OnViewDirectionClick;
            
            // LookStarboard
            LookStarboard.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookStarboard.Image = (Image?)resources.GetObject("LookStarboard.Image");
            LookStarboard.ImageTransparentColor = Color.Magenta;
            LookStarboard.Name = "LookStarboard";
            LookStarboard.Size = new Size(37, 24);
            LookStarboard.Tag = "LookStarboard";
            LookStarboard.Text = "LookStarboard";
            LookStarboard.Click += OnViewDirectionClick;
            
            // LookPort
            LookPort.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookPort.Image = (Image?)resources.GetObject("LookPort.Image");
            LookPort.ImageTransparentColor = Color.Magenta;
            LookPort.Name = "LookPort";
            LookPort.Size = new Size(37, 24);
            LookPort.Tag = "LookPort";
            LookPort.Text = "LookPort";
            LookPort.Click += OnViewDirectionClick;
            
            // LookBottom
            LookBottom.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookBottom.Image = (Image?)resources.GetObject("LookBottom.Image");
            LookBottom.ImageTransparentColor = Color.Magenta;
            LookBottom.Name = "LookBottom";
            LookBottom.Size = new Size(37, 24);
            LookBottom.Tag = "LookBottom";
            LookBottom.Text = "LookBottom";
            LookBottom.Click += OnViewDirectionClick;
            
            // LookTop
            LookTop.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookTop.Image = (Image?)resources.GetObject("LookTop.Image");
            LookTop.ImageTransparentColor = Color.Magenta;
            LookTop.Name = "LookTop";
            LookTop.Size = new Size(37, 24);
            LookTop.Tag = "LookTop";
            LookTop.Text = "LookTop";
            LookTop.Click += OnViewDirectionClick;
            
            // LookISO
            LookISO.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                LookISO.Image = (Image?)resources.GetObject("LookISO.Image");
            LookISO.ImageTransparentColor = Color.Magenta;
            LookISO.Name = "LookISO";
            LookISO.Size = new Size(37, 24);
            LookISO.Tag = "LookISO";
            LookISO.Text = "LookISO";
            LookISO.Click += OnViewDirectionClick;
            
            // toolStripSeparator1
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(37, 6);
            
            // Xray
            Xray.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                Xray.Image = (Image?)resources.GetObject("Xray.Image");
            Xray.ImageTransparentColor = Color.Magenta;
            Xray.Name = "Xray";
            Xray.Size = new Size(37, 24);
            Xray.Tag = "Xray";
            Xray.Text = "Xray";
            Xray.Click += OnRenderModeToggle;
            
            // toolStripButton3
            if (resources != null)
                toolStripButton3.Image = (Image?)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(37, 24);
            toolStripButton3.Tag = "Pick";
            toolStripButton3.Click += OnSelectToggle;
            
            // PICK
            PICK.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (resources != null)
                PICK.Image = (Image?)resources.GetObject("PICK.Image");
            PICK.ImageTransparentColor = Color.Magenta;
            PICK.Name = "PICK";
            PICK.Size = new Size(37, 24);
            PICK.Tag = "Fit";
            PICK.Text = "Fit";
            PICK.Click += OnFitModelClick;

            // ClipBoxButton
            ClipBoxButton = new ToolStripButton();
            ClipBoxButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ClipBoxButton.Name = "ClipBoxButton";
            ClipBoxButton.Size = new Size(37, 24);
            ClipBoxButton.Text = "C";
            ClipBoxButton.ToolTipText = "Toggle ClipBox (Shift+C)";
            ClipBoxButton.Click += OnClipBoxToggle;
            
            ResumeLayout(false);
            PerformLayout();
        }

        private void OnViewDirectionClick(object? sender, EventArgs e)
        {
            ViewDirectionClicked?.Invoke(sender, e);
        }

        private void OnRenderModeToggle(object? sender, EventArgs e)
        {
            RenderModeToggled?.Invoke(sender, e);
        }

        private void OnSelectToggle(object? sender, EventArgs e)
        {
            SelectToggled?.Invoke(sender, e);
        }

        private void OnFitModelClick(object? sender, EventArgs e)
        {
            FitModelClicked?.Invoke(sender, e);
        }

        private void OnClipBoxToggle(object? sender, EventArgs e)
        {
            ClipBoxToggled?.Invoke(sender, e);
        }
    }
}
