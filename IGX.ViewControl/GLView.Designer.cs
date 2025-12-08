using System.ComponentModel;

namespace IGX.ViewControl
{
    partial class GLView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private GLViewToolbar glViewToolbar1;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glViewToolbar1 = new IGX.ViewControl.GLViewToolbar();
            this.SuspendLayout();
            // 
            // glViewToolbar1
            // 
            this.glViewToolbar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.glViewToolbar1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.glViewToolbar1.Location = new System.Drawing.Point(985, 0);
            this.glViewToolbar1.Name = "glViewToolbar1";
            this.glViewToolbar1.Size = new System.Drawing.Size(40, 648);
            this.glViewToolbar1.TabIndex = 0;
            this.glViewToolbar1.Text = "GLViewToolbar";
            this.glViewToolbar1.ViewDirectionClicked += new System.EventHandler(this.SetViewDirection);
            this.glViewToolbar1.RenderModeToggled += new System.EventHandler(this.ToggleRenderMode);
            this.glViewToolbar1.SelectToggled += new System.EventHandler(this.ToggleSelect);
            this.glViewToolbar1.FitModelClicked += new System.EventHandler(this.FitModel_Click);
            this.glViewToolbar1.ClipBoxToggled += new System.EventHandler(this.ToggleClipBoxMode);
            // 
            // GLView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.glViewToolbar1);
            this.Name = "GLView";
            this.Size = new System.Drawing.Size(1025, 648);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
