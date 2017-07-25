using CefSharp;
using MaterialSkin;
using Nacollector.Browser;
using Nacollector.Spiders;
using Nacollector.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector
{
    public partial class TaskForm : MaterialSkin.Controls.MaterialForm
    {
        private string TaskId; // 任务ID

        public TaskForm _taskForm;

        private string _spiderClassName;
        private string _actionLabel;
        private string _parmsJson;

        public CrBrowser crBrowser;

        public TaskForm(string spiderClassName, string actionLabel, string parmsJson)
        {
            TaskId = DateTime.Now.ToString("yyyyMMddHHmmss");

            _taskForm = this;
            _spiderClassName = spiderClassName;
            _actionLabel = actionLabel;
            _parmsJson = parmsJson;

            InitializeComponent();
        }

        private void TaskForm_Load(object sender, EventArgs e)
        {
            // 窗体标题
            this.Text = _actionLabel + " 任务ID：" + TaskId;

            Logging.Info($"[{TaskId}] 创建任务 {_actionLabel} {_spiderClassName} Parms: {_parmsJson}");

            // 初始化内置浏览器
            string htmlPath = Utils.GetHtmlResPath("terminal.html");
            if (string.IsNullOrEmpty(htmlPath))
            {
                this.Dispose(); // 流放窗体
            }

            crBrowser = new CrBrowser(htmlPath);
            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(BrowserFrameLoadEnd); // 浏览器初始化完毕时执行
            ContentPanel.Controls.Add(crBrowser.GetBrowser());
        }

        Thread taskThread = null;

        private void BrowserFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // 设置 Nav Title
            crBrowser.RunJS(string.Format("NavBar.titleSet('{0}', true);", Utils.Base64Encode(this.Text)));
            // 创建任务执行线程
            taskThread = new Thread(StartTask);
            taskThread.IsBackground = true;
            taskThread.Start();
        }

        /// <summary>
        /// 开始任务 创建子线程
        /// </summary>
        private void StartTask()
        {
            Spider spider = null;

            // 实例化 Spider 对象
            string typeName = $"{this.GetType().Namespace}.Spiders.{_spiderClassName}";
            try
            {
                spider = (Spider)Activator.CreateInstance(Type.GetType(typeName));
                spider.setParentForm(this); // 设置父窗体
                spider.setTaskConfig(TaskId, _parmsJson); // 配置任务
            }
            catch
            {
                string errorText = "任务新建失败，无法实例化对象 " + typeName;
                Logging.Error(errorText);
                MessageBox.Show(errorText, "Nacollector 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 开始工作
            try
            {
                spider.BeginWork();
            }
            catch (Exception e) { spider.LogError(e.Message); return; }
            
            // 工作结束
            spider.LogInfo("任务执行完毕");
        }

        /// <summary>
        /// 窗口关闭时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭窗口随即终止任务线程
            if (taskThread != null)
                taskThread.Abort(); // 引发错误结束线程

            if (!this.IsDisposed)
                this.Dispose();

            Utils.ReleaseMemory(true);
        }
    }
}
