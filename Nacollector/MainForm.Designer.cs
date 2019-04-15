namespace Nacollector
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ContentPanel
            // 
            this.ContentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentPanel.Location = new System.Drawing.Point(0, 0);
            this.ContentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(1150, 698);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 698);
            this.ControlBox = false;
            this.Controls.Add(this.ContentPanel);
            this.Icon = global::Nacollector.Properties.Resources.app_ico;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MainForm";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nacollector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.SplashScreen_MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel ContentPanel;
    }
}

