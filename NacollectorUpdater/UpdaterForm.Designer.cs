namespace NacollectorUpdater
{
    partial class UpdaterForm
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdaterForm));
            this.UpdateProgressBar = new System.Windows.Forms.ProgressBar();
            this.UpdateProgressDesc = new System.Windows.Forms.Label();
            this.UpdatePercentageDesc = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // UpdateProgressBar
            // 
            this.UpdateProgressBar.Location = new System.Drawing.Point(13, 51);
            this.UpdateProgressBar.Name = "UpdateProgressBar";
            this.UpdateProgressBar.Size = new System.Drawing.Size(558, 23);
            this.UpdateProgressBar.TabIndex = 0;
            // 
            // UpdateProgressDesc
            // 
            this.UpdateProgressDesc.AutoSize = true;
            this.UpdateProgressDesc.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UpdateProgressDesc.ForeColor = System.Drawing.SystemColors.ControlText;
            this.UpdateProgressDesc.Location = new System.Drawing.Point(9, 15);
            this.UpdateProgressDesc.Margin = new System.Windows.Forms.Padding(0);
            this.UpdateProgressDesc.MaximumSize = new System.Drawing.Size(565, 20);
            this.UpdateProgressDesc.Name = "UpdateProgressDesc";
            this.UpdateProgressDesc.Size = new System.Drawing.Size(102, 20);
            this.UpdateProgressDesc.TabIndex = 1;
            this.UpdateProgressDesc.Text = "正在初始化中...";
            // 
            // UpdatePercentageDesc
            // 
            this.UpdatePercentageDesc.AutoSize = true;
            this.UpdatePercentageDesc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UpdatePercentageDesc.ForeColor = System.Drawing.SystemColors.ControlText;
            this.UpdatePercentageDesc.Location = new System.Drawing.Point(10, 82);
            this.UpdatePercentageDesc.Margin = new System.Windows.Forms.Padding(0);
            this.UpdatePercentageDesc.MaximumSize = new System.Drawing.Size(565, 20);
            this.UpdatePercentageDesc.Name = "UpdatePercentageDesc";
            this.UpdatePercentageDesc.Size = new System.Drawing.Size(53, 17);
            this.UpdatePercentageDesc.TabIndex = 2;
            this.UpdatePercentageDesc.Text = "请稍等...";
            // 
            // UpdaterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(584, 121);
            this.Controls.Add(this.UpdatePercentageDesc);
            this.Controls.Add(this.UpdateProgressDesc);
            this.Controls.Add(this.UpdateProgressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UpdaterForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nacollector 升级程序";
            this.Load += new System.EventHandler(this.UpdaterForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar UpdateProgressBar;
        private System.Windows.Forms.Label UpdateProgressDesc;
        private System.Windows.Forms.Label UpdatePercentageDesc;
    }
}

