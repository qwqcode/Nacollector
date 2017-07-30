using CefSharp;
using CefSharp.WinForms;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector
{
    /// <summary>
    /// 浏览器下载管理器
    /// 配合 JS
    /// </summary>
    public class CrDownloads
    {
        public CrBrowser crBrowser;
        public static DownloadHandler downloadHandler;

        public CrDownloads(CrBrowser _crBrowser)
        {
            crBrowser = _crBrowser;

            downloadHandler = new DownloadHandler();
            downloadHandler.OnBeforeDownloadFired += Browser_OnBeforeDownloadFired;
            downloadHandler.OnDownloadUpdatedFired += Browser_OnDownloadUpdatedFired;
            crBrowser.GetBrowser().DownloadHandler = downloadHandler;
            
            crBrowser.GetBrowser().RegisterAsyncJsObject("CrDownloadsCallBack", new CrDownloadsCallBack());
        }

        public class CrDownloadsCallBack
        {
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
            public void downloadingTaskAction(string key, int action)
            {
                dlTaskAction[key] = action;
            }
        }
        
        private void Browser_OnBeforeDownloadFired(object sender, BeforeDownloadUpdatedEventArgs e)
        {
            DownloadDo("add", crBrowser.GetBrowser(), e);
        }

        private void Browser_OnDownloadUpdatedFired(object sender, DownloadUpdatedEventArgs e)
        {
            DownloadDo("update", crBrowser.GetBrowser(), e);
        }

        private static Dictionary<int, string> dlTaskIndex = new Dictionary<int, string>();
        private static Dictionary<string, int> dlTaskAction = new Dictionary<string, int>();

        private void DownloadDo(string doType, ChromiumWebBrowser browser, EventArgs e)
        {
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
    }
}
