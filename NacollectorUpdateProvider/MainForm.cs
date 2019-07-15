using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorUpdateProvider
{
    public partial class MainForm : Form
    {
        private static string WorkFolderPath = null;
        private static bool IsSaved = true; // 数据是否已保存

        private static readonly string JsonFileName = "nacollector-updates.json";
        private static JObject RawJsonObj;
        private static string BaseUrl = "";
        public static JObject ReleaseNotes;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 打开上次的目录
            string lastFolderPath = MainSettings.Default.LastWorkFolderPath;
            if (lastFolderPath != null && Directory.Exists(lastFolderPath))
            {
                BeginInvoke(new Action(() =>
                {
                    LoadWorkFolder(lastFolderPath);
                }));
            }

            modulesBindingSource.ListChanged += ModulesBindingSource_ListChanged;
        }

        private void ModulesBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            IsSaved = false;
        }

        private void OpenWorkFolderBtn_Click(object sender, EventArgs e)
        {
            SeletedWorkFolderDialog();
        }

        private void SeletedWorkFolderDialog()
        {
            if (!IsSaved && MessageBox.Show("是否丢弃数据，打开新目录？", "数据未保存", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            FolderBrowserDialog PathSelDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "请选择更新资料存放根目录"
            };
            if (WorkFolderPath != null)
            {
                PathSelDialog.SelectedPath = WorkFolderPath;
            }

            if (PathSelDialog.ShowDialog() == DialogResult.OK && PathSelDialog.SelectedPath != null)
            {
                BeginInvoke(new Action(() =>
                {
                    LoadWorkFolder(PathSelDialog.SelectedPath);
                }));
            }
        }

        private void LoadWorkFolder(string path)
        {
            string jsonFilePath = Path.Combine(path, JsonFileName);
            if (!File.Exists(jsonFilePath))
            {
                // 若 json 文件不存在，则初始化新的文件
                File.WriteAllText(jsonFilePath, "{}");
            }

            JObject jsonObj;
            try
            {
                jsonObj = JObject.Parse(File.ReadAllText(jsonFilePath, Encoding.UTF8));
            }
            catch (Exception e)
            {
                MessageBox.Show("JSON 文件解析错误" + Environment.NewLine + e, "读取 JSON 文件", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RawJsonObj = jsonObj;
            WorkFolderPath = path;
            WorkFolderTextBox.Text = path;

            LoadRawJsonObj();

            MainSettings.Default.LastWorkFolderPath = path;
            MainSettings.Default.Save();
        }

        public void LoadRawJsonObj()
        {
            modulesBindingSource.Clear();

            if (RawJsonObj.ContainsKey("modules"))
            {
                foreach (var item in RawJsonObj["modules"])
                {
                    modulesBindingSource.Add(new Module()
                    {
                        name = (item["name"] ?? "").ToString(),
                        version = (item["version"] ?? "").ToString(),
                        src = (item["src"] ?? "").ToString(),
                        size = (item["size"] ?? "").ToString()
                    });
                }
            }
            else
            {
                RawJsonObj.Add("modules", new JArray());
            }
            BaseUrl = (RawJsonObj["base_url"] ?? "").ToString();
            ReleaseNotes = (JObject)(RawJsonObj["release_notes"] ?? new JObject());
            baseUrlTextBox.Text = BaseUrl;

            IsSaved = true;
        }

        private void BaseUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            BaseUrl = ((TextBox)sender).Text.Trim();
        }

        private bool IsWorkFolderLoaded()
        {
            bool isLoaded = WorkFolderPath != null && WorkFolderPath.Trim().Length > 0;
            if (!isLoaded)
            {
                MessageBox.Show("请先打开工作根目录");
            }
            return isLoaded;
        }

        private OpenFileDialog OpenModuleSrcSelectDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Title = "选择模块资源文件"
            };
            if (WorkFolderPath != null && WorkFolderPath.Length > 0)
            {
                openFileDialog.InitialDirectory = WorkFolderPath;
            }

            return openFileDialog;
        }

        private string GetFileVersion(string fileName)
        {
            string fileVersion = "";
            try
            {
                if (new List<string> { ".exe", ".dll" }.Contains(Path.GetExtension(fileName)))
                    fileVersion = FileVersionInfo.GetVersionInfo(fileName).ProductVersion;
            }
            catch { return ""; }
            return fileVersion.Trim();
        }

        private void AddModuleBtn_Click(object sender, EventArgs e)
        {
            if (!IsWorkFolderLoaded())
                return;

            var openFileDialog = OpenModuleSrcSelectDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(path);

                // 复制文件到工作目录
                CopyFileToWorkFolder(path);

                modulesBindingSource.Add(new Module()
                {
                    name = Path.GetFileNameWithoutExtension(path),
                    version = GetFileVersion(path),
                    src = BaseUrl.TrimEnd('/') + "/" + fileInfo.Name,
                    size = Program.FormatFileSize(fileInfo.Length)
                });
            }
        }

        private void ModifySrcBtn_Click(object sender, EventArgs e)
        {
            if (!IsWorkFolderLoaded())
                return;

            if (modulesDataGridView.SelectedCells.Count <= 0)
                return;

            var openFileDialog = OpenModuleSrcSelectDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(path);
                var fileVersion = GetFileVersion(path);

                // 复制文件到工作目录
                CopyFileToWorkFolder(path);

                Module module = ((Module)modulesDataGridView.CurrentRow.DataBoundItem);

                if (fileVersion.Length > 0) {
                    module.version = fileVersion;
                }
                module.src = BaseUrl.TrimEnd('/') + "/" + fileInfo.Name;
                module.size = Program.FormatFileSize(fileInfo.Length);

                modulesDataGridView.Refresh();
            }
        }

        private void DelBtn_Click(object sender, EventArgs e)
        {
            DelSelectedRows();
        }

        private void DelSelectedRows()
        {
            var rowsIndexList = new List<int>() { };

            foreach (DataGridViewTextBoxCell item in modulesDataGridView.SelectedCells)
            {
                if (!rowsIndexList.Contains(item.RowIndex))
                    rowsIndexList.Add(item.RowIndex);
            }

            if (rowsIndexList.Count <= 0)
            {
                return;
            }

            if (MessageBox.Show(
                $"是否确定删除已选定的 {rowsIndexList.Count} 个模块", "删除模块",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                foreach (var i in rowsIndexList)
                {
                    try
                    {
                        modulesDataGridView.Rows.RemoveAt(i);
                    } catch { }
                }
            }));
        }

        private Form ProcessingDialog;

        private void CopyFileToWorkFolder(string path)
        {
            if (Path.GetFullPath(Path.GetDirectoryName(path)) == Path.GetFullPath(WorkFolderPath))
            {
                return;
            }

            ProcessingDialog = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "复制文件",
                TopMost = true,
                StartPosition = FormStartPosition.CenterParent,
                ControlBox = false,
                ShowInTaskbar = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = "正在复制文件中...", Width = 460 };
            Button cancel = new Button() { Text = "取消", Left = 360, Width = 120, Top = 70, DialogResult = DialogResult.Cancel };
            ProcessingDialog.Controls.Add(cancel);
            ProcessingDialog.Controls.Add(textLabel);

            var thread = new Thread(() =>
            {
                File.Copy(path, Path.Combine(WorkFolderPath, Path.GetFileName(path)), true);
                ProcessingDialog.Invoke(new Action(() =>
                {
                    ProcessingDialog.Close();
                }));
            });
            cancel.Click += (sender, e) => { thread.Abort();  ProcessingDialog.Close(); };
            thread.Start();
            ProcessingDialog.ShowDialog();
        }

        /// <summary>
        /// 数据保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (!IsWorkFolderLoaded()) return;

            try
            {
                string jsonStr = JObject.FromObject(new
                {
                    modules = modulesBindingSource.List,
                    base_url = BaseUrl ?? null,
                    release_notes = ReleaseNotes ?? new JObject()
                }).ToString(); //.ToString(Newtonsoft.Json.Formatting.None);

                Debug.WriteLine(jsonStr);

                // 备份原版
                string jsonFileFullPath = Path.Combine(WorkFolderPath, JsonFileName);
                File.Copy(jsonFileFullPath, jsonFileFullPath + ".bak", true);

                // 保存
                File.WriteAllText(jsonFileFullPath, jsonStr);

                MessageBox.Show("更新已成功推送", "更新推送", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception err)
            {
                MessageBox.Show($"更新推送失败{Environment.NewLine}{err}", "更新推送", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MakeReleaseNotesBtn_Click(object sender, EventArgs e)
        {
            if (!IsWorkFolderLoaded()) return;
            var form = new ReleaseNotesEditorForm(ReleaseNotes);
            form.ShowDialog();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://qwqaq.com");
        }
    }
}
