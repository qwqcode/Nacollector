using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorUpdateProvider
{
    public partial class ReleaseNotesEditorForm : Form
    {
        public ReleaseNotesEditorForm(JObject releaseNotes)
        {
            InitializeComponent();
            if (releaseNotes != null) {
                textBox.Text = JObject.FromObject(releaseNotes).ToString();
            } else
            {
                textBox.Text = @"{}";
            }
            textBox.SelectionStart = textBox.Text.Length;
        }

        private void ReleaseNotesEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            JObject jobj;
            try
            {
                jobj = JObject.Parse(textBox.Text);
            }
            catch (Exception err)
            {
                DialogResult dr = MessageBox.Show($"JSON 格式错误{Environment.NewLine}点击 \"是\" 继续修改，点击 \"否\" 放弃修改，不保存{Environment.NewLine}{Environment.NewLine}{err}", "JSON 格式错误", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    e.Cancel = true; // 不退出
                else
                    e.Cancel = false; // 退出
                return;
            }

            // 保存 JObj
            MainForm.ReleaseNotes = jobj;

            e.Cancel = false; // 退出
        }
    }
}
