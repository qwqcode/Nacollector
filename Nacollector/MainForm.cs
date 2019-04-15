using CefSharp;
using CefSharp.WinForms;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.JsActions;
using Nacollector.Ui;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NacollectorUtils;

namespace Nacollector
{
    public partial class MainForm : FormBase
    {
        public static MainForm _mainForm;
        public static CrBrowser crBrowser;
        public static CrDownloads crDownloads;

        public MainForm()
        {
            _mainForm = this;

            InitializeComponent(); // 初始化控件
            InitBrowser(); // 初始化浏览器
        }

        private void InitBrowser()
        {
            // 初始化内置浏览器
#if !DEBUG
            string htmlPath = Utils.GetHtmlResPath("index.html");
            if (string.IsNullOrEmpty(htmlPath))
            {
                Application.Exit(); // 退出程序
            }
#else
            string htmlPath = "http://127.0.0.1:8080";
#endif
            crBrowser = new CrBrowser(this, htmlPath);

            // Need Update: https://github.com/cefsharp/CefSharp/issues/2246

            //For legacy biding we'll still have support for
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            crBrowser.GetBrowser().RegisterAsyncJsObject("AppAction", new AppAction(this, crBrowser));
            crBrowser.GetBrowser().RegisterAsyncJsObject("TaskController", new TaskControllerAction(this, crBrowser));

            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(SplashScreen_Browser_FrameLoadEnd); // 浏览器初始化完毕时执行

            crDownloads = new CrDownloads(crBrowser);
            
            ContentPanel.Controls.Add(crBrowser.GetBrowser());
        }

        public CrBrowser GetCrBrowser()
        {
            return crBrowser;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 是否退出弹窗
            string dialogTxt = "确定退出 Nacollector？";

            // 下载任务数统计
            int downloadingTaskNum = Convert.ToInt32(crBrowser.EvaluateScript("Downloads.countDownloadingTask();", 0, TimeSpan.FromSeconds(3)).GetAwaiter().GetResult());
            if (downloadingTaskNum > 0)
                dialogTxt = $"有 {downloadingTaskNum} 个下载任务仍在继续！确定结束下载并关闭程序？";

            DialogResult dr = MessageBox.Show(dialogTxt, "退出 Nacollector", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        ///
        /// 任务执行
        ///

        public AppDomain _spiderRawDomain = null;
        public SpiderDomain _spiderDomain = null;

        public SpiderDomain GetLoadSpiderDomain()
        {
            if (this._spiderDomain != null)
            {
                return this._spiderDomain;
            }

            // 创建新的 AppDomain
            AppDomainSetup domaininfo = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory
            };
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            AppDomain rawDomain = AppDomain.CreateDomain("NacollectorSpiders", adevidence, domaininfo);

            // 动态加载 dll
            Type type = typeof(SpiderDomain);
            var spiderDomain = (SpiderDomain)rawDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            spiderDomain.LoadAssembly(Path.Combine(Application.StartupPath, "NacollectorSpiders.dll"));

            this._spiderRawDomain = rawDomain;
            this._spiderDomain = spiderDomain;
            return spiderDomain;
        }

        public void UnloadSpiderDomain()
        {
            if (_spiderRawDomain == null)
            {
                return;
            }

            // 卸载 dll
            AppDomain.Unload(_spiderRawDomain);
            _spiderRawDomain = null;
            _spiderDomain = null;
        }

        /// <summary>
        /// 任务线程
        /// </summary>
        public Dictionary<string, Thread> taskThreads = new Dictionary<string, Thread>();

        public bool AbortTask(string taskId)
        {
            if (!taskThreads.ContainsKey(taskId))
                return false;

            // 主线程委托执行，防止 Abort() 后面的代码无效
            this.BeginInvoke((MethodInvoker)delegate
            {
                taskThreads[taskId].Abort();
                taskThreads.Remove(taskId);
                if (taskThreads.Count <= 0)
                {
                    UnloadSpiderDomain();
                }
            });
            
            return true;
        }

        public void NewTaskThread(Dictionary<string, object> settings)
        {
            // 创建任务执行线程
            var thread = new Thread(new ParameterizedThreadStart(StartTask))
            {
                IsBackground = true
            };
            thread.Start(settings);

            // 加入 Threads Dictionary
            taskThreads.Add((string)settings["TaskId"], thread);
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        /// <param name="obj"></param>
        public void StartTask(object obj)
        {
            var spiderDomain = GetLoadSpiderDomain();

            // 调用目标函数
#if !DEBUG
            try
            {
#endif
            var settings = (Dictionary<string, object>)obj;
            spiderDomain.Invoke("NacollectorSpiders.PokerDealer", "NewTask", settings, new Action<string>((string str) => {
                this.GetCrBrowser().RunJS(str);
            }));
#if !DEBUG
            }
            catch
            {
                string errorText = "任务新建失败,无法调用 NacollectorSpiders.PokerDealer.NewTask";
                Logging.Error(errorText);
                MessageBox.Show(errorText, "Nacollector 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
#endif

            AbortTask((string)settings["TaskId"]);
        }

        /// <summary>
        /// 执行爬虫任务，新的 AppDomain 中
        /// </summary>
        public class SpiderDomain : MarshalByRefObject
        {
            Assembly assembly = null;

            public void LoadAssembly(string assemblyPath)
            {
                try
                {
                    assembly = Assembly.LoadFile(assemblyPath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.Message);
                }
            }

            public bool Invoke(string fullClassName, string methodName, params object[] args)
            {
                if (assembly == null)
                    return false;
                Type tp = assembly.GetType(fullClassName);
                if (tp == null)
                    return false;
                MethodInfo method = tp.GetMethod(methodName);
                if (method == null)
                    return false;
                object obj = Activator.CreateInstance(tp);
                method.Invoke(obj, args);
                return true;
            }
        }
    }
}