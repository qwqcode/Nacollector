namespace NacollectorUpdateProvider
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.OpenWorkFolderBtn = new System.Windows.Forms.Button();
            this.WorkFolderTextBox = new System.Windows.Forms.TextBox();
            this.modulesDataGridView = new System.Windows.Forms.DataGridView();
            this.delBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.addModuleBtn = new System.Windows.Forms.Button();
            this.publicUrlLabel = new System.Windows.Forms.Label();
            this.baseUrlTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.modifySrcBtn = new System.Windows.Forms.Button();
            this.makeReleaseNotesBtn = new System.Windows.Forms.Button();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.versionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.srcDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modulesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.modulesDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.modulesBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // OpenWorkFolderBtn
            // 
            this.OpenWorkFolderBtn.Location = new System.Drawing.Point(422, 10);
            this.OpenWorkFolderBtn.Name = "OpenWorkFolderBtn";
            this.OpenWorkFolderBtn.Size = new System.Drawing.Size(105, 23);
            this.OpenWorkFolderBtn.TabIndex = 0;
            this.OpenWorkFolderBtn.Text = "打开根目录";
            this.OpenWorkFolderBtn.UseVisualStyleBackColor = true;
            this.OpenWorkFolderBtn.Click += new System.EventHandler(this.OpenWorkFolderBtn_Click);
            // 
            // WorkFolderTextBox
            // 
            this.WorkFolderTextBox.Location = new System.Drawing.Point(77, 11);
            this.WorkFolderTextBox.Name = "WorkFolderTextBox";
            this.WorkFolderTextBox.ReadOnly = true;
            this.WorkFolderTextBox.Size = new System.Drawing.Size(339, 21);
            this.WorkFolderTextBox.TabIndex = 1;
            // 
            // modulesDataGridView
            // 
            this.modulesDataGridView.AutoGenerateColumns = false;
            this.modulesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.modulesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.versionDataGridViewTextBoxColumn,
            this.srcDataGridViewTextBoxColumn,
            this.sizeDataGridViewTextBoxColumn});
            this.modulesDataGridView.DataSource = this.modulesBindingSource;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.modulesDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.modulesDataGridView.Location = new System.Drawing.Point(13, 79);
            this.modulesDataGridView.Name = "modulesDataGridView";
            this.modulesDataGridView.RowTemplate.Height = 23;
            this.modulesDataGridView.Size = new System.Drawing.Size(514, 306);
            this.modulesDataGridView.TabIndex = 2;
            // 
            // delBtn
            // 
            this.delBtn.Location = new System.Drawing.Point(533, 149);
            this.delBtn.Name = "delBtn";
            this.delBtn.Size = new System.Drawing.Size(105, 23);
            this.delBtn.TabIndex = 3;
            this.delBtn.Text = "删除";
            this.delBtn.UseVisualStyleBackColor = true;
            this.delBtn.Click += new System.EventHandler(this.DelBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(533, 342);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(105, 43);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "推送更新";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // addModuleBtn
            // 
            this.addModuleBtn.Location = new System.Drawing.Point(533, 79);
            this.addModuleBtn.Name = "addModuleBtn";
            this.addModuleBtn.Size = new System.Drawing.Size(105, 23);
            this.addModuleBtn.TabIndex = 6;
            this.addModuleBtn.Text = "新增模块";
            this.addModuleBtn.UseVisualStyleBackColor = true;
            this.addModuleBtn.Click += new System.EventHandler(this.AddModuleBtn_Click);
            // 
            // publicUrlLabel
            // 
            this.publicUrlLabel.AutoSize = true;
            this.publicUrlLabel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.publicUrlLabel.Location = new System.Drawing.Point(12, 44);
            this.publicUrlLabel.Name = "publicUrlLabel";
            this.publicUrlLabel.Size = new System.Drawing.Size(59, 17);
            this.publicUrlLabel.TabIndex = 7;
            this.publicUrlLabel.Text = "外部地址:";
            // 
            // baseUrlTextBox
            // 
            this.baseUrlTextBox.Location = new System.Drawing.Point(77, 42);
            this.baseUrlTextBox.Name = "baseUrlTextBox";
            this.baseUrlTextBox.Size = new System.Drawing.Size(450, 21);
            this.baseUrlTextBox.TabIndex = 8;
            this.baseUrlTextBox.TextChanged += new System.EventHandler(this.BaseUrlTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 17);
            this.label1.TabIndex = 9;
            this.label1.Text = "根目录:";
            // 
            // modifySrcBtn
            // 
            this.modifySrcBtn.Location = new System.Drawing.Point(533, 120);
            this.modifySrcBtn.Name = "modifySrcBtn";
            this.modifySrcBtn.Size = new System.Drawing.Size(105, 23);
            this.modifySrcBtn.TabIndex = 10;
            this.modifySrcBtn.Text = "更换文件";
            this.modifySrcBtn.UseVisualStyleBackColor = true;
            this.modifySrcBtn.Click += new System.EventHandler(this.ModifySrcBtn_Click);
            // 
            // makeReleaseNotesBtn
            // 
            this.makeReleaseNotesBtn.Location = new System.Drawing.Point(533, 313);
            this.makeReleaseNotesBtn.Name = "makeReleaseNotesBtn";
            this.makeReleaseNotesBtn.Size = new System.Drawing.Size(105, 23);
            this.makeReleaseNotesBtn.TabIndex = 11;
            this.makeReleaseNotesBtn.Text = "撰写版本说明";
            this.makeReleaseNotesBtn.UseVisualStyleBackColor = true;
            this.makeReleaseNotesBtn.Click += new System.EventHandler(this.MakeReleaseNotesBtn_Click);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "模块名";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.Width = 150;
            // 
            // versionDataGridViewTextBoxColumn
            // 
            this.versionDataGridViewTextBoxColumn.DataPropertyName = "version";
            this.versionDataGridViewTextBoxColumn.HeaderText = "版本";
            this.versionDataGridViewTextBoxColumn.Name = "versionDataGridViewTextBoxColumn";
            this.versionDataGridViewTextBoxColumn.Width = 90;
            // 
            // srcDataGridViewTextBoxColumn
            // 
            this.srcDataGridViewTextBoxColumn.DataPropertyName = "src";
            this.srcDataGridViewTextBoxColumn.HeaderText = "资源";
            this.srcDataGridViewTextBoxColumn.Name = "srcDataGridViewTextBoxColumn";
            this.srcDataGridViewTextBoxColumn.Width = 170;
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            this.sizeDataGridViewTextBoxColumn.DataPropertyName = "size";
            this.sizeDataGridViewTextBoxColumn.HeaderText = "大小";
            this.sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            this.sizeDataGridViewTextBoxColumn.ReadOnly = true;
            this.sizeDataGridViewTextBoxColumn.Width = 60;
            // 
            // modulesBindingSource
            // 
            this.modulesBindingSource.DataSource = typeof(NacollectorUpdateProvider.Module);
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.Red;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkLabel1.LinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabel1.Location = new System.Drawing.Point(549, 32);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(76, 16);
            this.linkLabel1.TabIndex = 12;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "(c) qwqaq.com";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.DodgerBlue;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(652, 397);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.makeReleaseNotesBtn);
            this.Controls.Add(this.modifySrcBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.baseUrlTextBox);
            this.Controls.Add(this.publicUrlLabel);
            this.Controls.Add(this.addModuleBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.delBtn);
            this.Controls.Add(this.modulesDataGridView);
            this.Controls.Add(this.WorkFolderTextBox);
            this.Controls.Add(this.OpenWorkFolderBtn);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nacollector 更新推送工具";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.modulesDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.modulesBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenWorkFolderBtn;
        private System.Windows.Forms.TextBox WorkFolderTextBox;
        private System.Windows.Forms.DataGridView modulesDataGridView;
        private System.Windows.Forms.BindingSource modulesBindingSource;
        private System.Windows.Forms.Button delBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button addModuleBtn;
        private System.Windows.Forms.Label publicUrlLabel;
        private System.Windows.Forms.TextBox baseUrlTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button modifySrcBtn;
        private System.Windows.Forms.Button makeReleaseNotesBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn versionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn srcDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

