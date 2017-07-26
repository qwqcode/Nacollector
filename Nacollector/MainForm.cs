using CefSharp;
using CefSharp.WinForms;
using MaterialSkin;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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

namespace Nacollector
{
    public partial class MainForm : MaterialSkin.Controls.MaterialForm
    {
        public static MainForm _mainForm;
        public static CrBrowser crBrowser;
        public static CrDownloads crDownloads;

        public MainForm()
        {
            _mainForm = this;

            InitSkin(); // 初始化窗体皮肤
            InitializeComponent(); // 初始化控件
            InitBrowser(); // 初始化浏览器
        }

        private void InitSkin()
        {
            MaterialSkinManager skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.DARK;
            skinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue800, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 设置窗体透明
            SetOpacity(0);
        }

        private void InitBrowser()
        {
            // 初始化内置浏览器
            string htmlPath = Utils.GetHtmlResPath("home.html");
            if (string.IsNullOrEmpty(htmlPath))
            {
                Application.Exit(); // 退出程序
            }
            
            crBrowser = new CrBrowser(htmlPath);
            crBrowser.GetBrowser().RegisterAsyncJsObject("TaskController", new TaskControllerForJs());
            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(Browser_FrameLoadEnd); // 浏览器初始化完毕时执行

            crDownloads = new CrDownloads(crBrowser);

            ContentPanel.Controls.Add(crBrowser.GetBrowser());
        }
        
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // 取消设置窗体透明
            SetOpacity(1);
        }

        /// <summary>
        /// 任务控制器
        /// </summary>
        public class TaskControllerForJs
        {
            private Dictionary<string, Thread> taskThreads = new Dictionary<string, Thread>();

            // 创建新任务
            public void createTask(string taskId, string className, string classLabel, string parmsJsonStr)
            {
                crBrowser.RunJS("Task.list[" + taskId + "].log('taskId="+ taskId + ", className="+className+", classLabel="+classLabel+", parmsJsonStr="+parmsJsonStr+"')");
                crBrowser.RunJS("Task.list["+taskId+"].log('Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et"
            + "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
            + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu"
            + "fugiat nulla pariatur.')");
                crBrowser.RunJS("Task.list[" + taskId + "].log('Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et"
            + "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
            + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu"
            + "fugiat nulla pariatur.', 'I')");
                crBrowser.RunJS("Task.list[" + taskId + "].log('Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et"
            + "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
            + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu"
            + "fugiat nulla pariatur.', 'S')");
                crBrowser.RunJS("Task.list[" + taskId + "].log('Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et"
            + "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
            + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu"
            + "fugiat nulla pariatur.', 'W')");
                crBrowser.RunJS("Task.list[" + taskId + "].log('Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et"
            + "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip"
            + "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu"
            + "fugiat nulla pariatur.', 'E')");
                return;
            }
        }

        /// <summary>
        /// 打开一个任务终端
        /// </summary>
        public void NewTaskForm(string callClassName, string actionLabel, string parmsJson)
        {
            if (this.InvokeRequired) { this.Invoke(new NewTaskFormDelegate(NewTaskForm), new object[] { callClassName, actionLabel, parmsJson }); return; }

            var t = new TaskForm(callClassName, actionLabel, parmsJson);
            t.Show();
        }
        public delegate void NewTaskFormDelegate(string callClassName, string actionLabel, string parmsJson);

        /// <summary>
        /// 设置窗体透明度
        /// </summary>
        public void SetOpacity(int value)
        {
            if (this.InvokeRequired) { this.Invoke(new SetOpacityDelegate(SetOpacity), new object[] { value }); return; }

            this.Opacity = value;
        }
        public delegate void SetOpacityDelegate(int value);

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存下载任务列表
            crDownloads.SaveDownloadList();

            // 是否退出弹窗
            string dialogTxt = "确定退出 Nacollector？";

            // 下载任务数统计
            int downloadingTaskNum = Convert.ToInt32(crBrowser.EvaluateScript("downloads.countDownloadingTask();", 0, TimeSpan.FromSeconds(3)).GetAwaiter().GetResult());
            if (downloadingTaskNum > 0)
                dialogTxt = $"有 {downloadingTaskNum} 个下载任务仍在继续！确定结束下载并关闭程序？";

            DialogResult dr = MessageBox.Show(dialogTxt, "退出 Nacollector", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
                e.Cancel = false;
            else
                e.Cancel = true;
        }
    }
}
