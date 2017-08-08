using CefSharp;
using CefSharp.WinForms;
using MaterialSkin;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.Spiders;
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
            string htmlPath = Utils.GetHtmlResPath("app.html");
            if (string.IsNullOrEmpty(htmlPath))
            {
                Application.Exit(); // 退出程序
            }
            
            crBrowser = new CrBrowser(htmlPath);
            crBrowser.GetBrowser().RegisterAsyncJsObject("AppAction", new AppActionForJs());
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
        /// 程序JS操作
        /// </summary>
        public class AppActionForJs
        {
            // 获取程序版本
            public string getVersion()
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            // 采集是否使用IE代理请求
            public void _utilsReqIeProxy(bool isEnable)
            {
                Utils.ReqIeProxy = isEnable;
            }

            // 日志文件清理
            public void logFileClear()
            {
                Logging.Clear();
            }
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
                // 配置
                var settings = new SpiderSettings()
                {
                    TaskId = taskId,
                    ClassName = className,
                    ClassLabel = classLabel,
                    ParmsJsonStr = parmsJsonStr
                };
                // 创建任务执行线程
                var thread = new Thread(new ParameterizedThreadStart(_mainForm.StartTask));
                thread.IsBackground = true;
                thread.Start(settings);
                // 加入 Threads Dictionary
                taskThreads.Add(taskId, thread);
            }

            // 终止任务
            public bool abortTask(string taskId)
            {
                if (!taskThreads.ContainsKey(taskId))
                    return false;

                taskThreads[taskId].Abort();
                return true;
            }
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <param name="obj"></param>
        public void StartTask(object obj)
        {
            SpiderSettings settings = (SpiderSettings)obj;
            settings.CrBrowser = crBrowser;

            Spider spider = null;

            // 实例化 Spider 对象
            string typeName = $"{this.GetType().Namespace}.Spiders.{settings.ClassName}";
            try
            {
                spider = (Spider)Activator.CreateInstance(Type.GetType(typeName));
                spider.importSettings(settings); // 导入配置
            }
            catch
            {
                string errorText = "任务新建失败，无法实例化对象 " + typeName;
                Logging.Error(errorText);
                MessageBox.Show(errorText, "Nacollector 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 开始任务工作
            DateTime beforWorkDt = DateTime.Now;

            try
            {
                spider.BeginWork();
            }
            catch (Exception e)
            {
                // 任务执行中抛出的错误被接住了...
                spider.LogError(e.Message);
                Logging.Error(e.ToString()); // 保存错误详情
#if DEBUG
                spider.Log(e.ToString());
#endif
            }

            // 任务执行完毕
            DateTime afterWorkDt = DateTime.Now;
            double timeSpent = afterWorkDt.Subtract(beforWorkDt).TotalSeconds;
            spider.Log("\n");
            spider.Log($"&gt;&gt; 任务执行完毕 （执行耗时：{timeSpent.ToString()}s）");

            // 报告JS任务结束
            crBrowser.RunJS($"Task.get('{settings.TaskId}').taskIsEnd();");

            Utils.ReleaseMemory(true);
        }

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