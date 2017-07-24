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
        public static DownloadHandler crDlHandler;

        public static string dlHistoryPath = Utils.GetTempPath("dl_history.json");

        public MainForm()
        {
            _mainForm = this;

            InitSkin(); // 初始化窗体皮肤
            InitializeComponent(); // 初始化控件
            InitBrowser(); // 初始化浏览器
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 设置窗体透明
            SetOpacity(0);
        }

        private void InitSkin()
        {
            MaterialSkinManager skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.DARK;
            skinManager.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
        }

        #region 浏览器
        private void InitBrowser()
        {
            // 初始化内置浏览器
            string htmlFilePath = string.Format(@"{0}\html_res\home.html", Application.StartupPath);
            if (!File.Exists(htmlFilePath))
            {
                MessageBox.Show("由于文件丢失，主界面无法正常显示\n文件路：" + htmlFilePath, "文件丢失", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit(); // 退出程序
            }

            // 下载管理
            crDlHandler = new DownloadHandler();
            crDlHandler.OnBeforeDownloadFired += Browser_onBeforeDownloadFired;
            crDlHandler.OnDownloadUpdatedFired += Browser_onDownloadUpdatedFired;

            crBrowser = new CrBrowser(htmlFilePath, crDlHandler);
            crBrowser.GetBrowser().RegisterAsyncJsObject("MainFormCallBack", new JsCallbackObj());
            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(BrowserFrameLoadEnd); // 浏览器初始化完毕时执行\

            ContentPanel.Controls.Add(crBrowser.GetBrowser());
        }

        /// 浏览器回调JS对象
        public class JsCallbackObj
        {
            public void taskNew(string callClassName, string actionLabel, string parmsJson)
            {
                // MessageBox.Show(" " + key + ": " + formContent.ToString());
                _mainForm.NewTaskForm(callClassName, actionLabel, parmsJson);
            }

            // 文件在资源管理器中显示
            public bool fileShowInExplorer(string fileFullPath)
            {
                if (string.IsNullOrEmpty(fileFullPath))
                    return false;

                if (!File.Exists(fileFullPath))
                    return false;

                string argument = "/select, \"" + fileFullPath + "\"";
                Process.Start("explorer.exe", argument);
                return true;
            }

            // 文件启动
            public bool fileLaunch(string fileFullPath)
            {
                if (string.IsNullOrEmpty(fileFullPath))
                    return false;

                if (!File.Exists(fileFullPath))
                    return false;

                Process.Start(fileFullPath);
                return true;
            }

            // URL 在系统默认浏览器中打开
            public void urlOpenInDefaultBrowser(string url)
            {
                Process.Start("explorer.exe", url);
            }

            // 下达任务操作命令
            public void downloadTaskAction(string key, int action)
            {
                dlTaskAction[key] = action;
            }
        }

        private void BrowserFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // 取消设置窗体透明
            SetOpacity(1);

            // 下载管理器下载记录导入
            string downloadsListJson = File.ReadAllText(dlHistoryPath);
            crBrowser.RunJS($"downloads.appLoadEvent({downloadsListJson});");
        }

        private void Browser_onBeforeDownloadFired(object sender, BeforeDownloadUpdatedEventArgs e)
        {
            DownloadDo("add", crBrowser.GetBrowser(), e);
        }

        private void Browser_onDownloadUpdatedFired(object sender, DownloadUpdatedEventArgs e)
        {
            DownloadDo("update", crBrowser.GetBrowser(), e);
        }

        private static Dictionary<int, string> dlTaskIndex = new Dictionary<int, string>();
        private static Dictionary<string, int> dlTaskAction = new Dictionary<string, int>();

        private void DownloadDo(string doType, ChromiumWebBrowser browser, EventArgs e)
        {
            if (this.InvokeRequired) { this.Invoke(new DownloadDoDelegate(DownloadDo), new object[] { doType, browser, e }); return; }
            
            DownloadItem downloadItem = null;
            if (doType == "add")
                downloadItem = ((BeforeDownloadUpdatedEventArgs)e).downloadItem;
            else if (doType == "update")
                downloadItem = ((DownloadUpdatedEventArgs)e).downloadItem;
            
            // Key
            string key;
            if (!dlTaskIndex.ContainsKey(downloadItem.Id) && doType == "update")
            { // 若在索引中找不到 并且 doType = update
                return;
            }
            else if (!dlTaskIndex.ContainsKey(downloadItem.Id) && doType == "add")
            {
                key = Utils.GetTimeStamp();
                dlTaskIndex.Add(downloadItem.Id, key);
            }
            else
            {
                key = dlTaskIndex[downloadItem.Id];
            }
            
            // 状态
            var statusList = new
            {
                downloading = 1, // 下载中
                pause = 2, // 暂停
                done = 3, // 下载完毕
                cancelled = 4, // 已取消
                fail = 5, // 下载错误
            };

            int status;
            if (downloadItem.IsInProgress)
                status = statusList.downloading;
            else if (downloadItem.IsComplete)
                status = statusList.done;
            else if (downloadItem.IsCancelled)
                status = statusList.cancelled;
            else
                status = statusList.fail;
            
            // 任务操作
            if (doType == "update")
            {
                var actionList = new
                {
                    pause = 1,
                    resume = 2,
                    cancel = 3
                };
                
                var callback = ((DownloadUpdatedEventArgs)e).callback;

                int action = 0;
                if (dlTaskAction.ContainsKey(key))
                {
                    action = dlTaskAction[key];
                }

                if (action == actionList.pause) // 暂停
                {
                    status = statusList.pause;
                    callback.Pause();
                }

                if (action == actionList.resume) // 恢复
                {
                    status = statusList.downloading;
                    callback.Resume();
                    dlTaskAction[key] = 0; // 命令下达一次就够了！
                }

                if (action == actionList.cancel) // 取消
                {
                    status = statusList.cancelled;
                    callback.Cancel();
                }
            }

            // 回调
            if (doType == "add")
            {
                string callbackObj = JsonConvert.SerializeObject(new
                {
                    key = key,
                    fullPath = downloadItem.SuggestedFileName,
                    downloadUrl = downloadItem.OriginalUrl,
                    totalBytes = downloadItem.TotalBytes,
                });
                browser.ExecuteScriptAsync($"downloads.addTask({callbackObj})");
            }
            else if (doType == "update")
            {
                string callbackObj = JsonConvert.SerializeObject(new
                {
                    key = key,
                    receivedBytes = downloadItem.ReceivedBytes,
                    currentSpeed = downloadItem.CurrentSpeed,
                    status = status,
                    fullPath = downloadItem.FullPath,
                    downloadUrl = downloadItem.Url
                });
                browser.ExecuteScriptAsync($"downloads.updateTask({callbackObj})");
            }
        }
        public delegate void DownloadDoDelegate(string doType, ChromiumWebBrowser browser, EventArgs e);
        #endregion

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
            string downloadsListJsonStr = crBrowser.EvaluateScript("downloads.appExitEvent();", 0, TimeSpan.FromSeconds(3)).GetAwaiter().GetResult().ToString();
            File.WriteAllText(dlHistoryPath, downloadsListJsonStr);

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
