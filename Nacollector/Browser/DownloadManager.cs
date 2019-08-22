using CefSharp;
using CefSharp.WinForms;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using NacollectorUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Browser
{
    /// <summary>
    /// 浏览器 - 下载管理器
    /// </summary>
    class DownloadManager
    {
        private CrBrowser _crBrowser;
        private ChromiumWebBrowser browser;

        private Dictionary<int, DownloadTask> dlTaskDict = new Dictionary<int, DownloadTask>(); // key 为 dlItem.id

        /// NOTE: 任务ID (taskId, 前端使用) 区别于 下载ID (dlItem.Id)

        // 前端状态
        public enum Status
        {
            Downloading = 1, // 下载中
            Pause = 2, // 暂停
            Done = 3, // 下载完毕
            Cancelled = 4, // 已取消
            Fail = 5, // 下载错误
        }

        // 前端操作
        public enum Action
        {
            Pause = 1, // 暂停
            Resume = 2, // 恢复
            Cancel = 3 // 取消
        }

        public DownloadManager(CrBrowser crBrowser)
        {
            _crBrowser = crBrowser;
            browser = _crBrowser.GetBrowser();

            var handler = new DownloadHandler();
            handler.OnBeforeDownloadFired += (s, e) => OnBeforeDownload(e, e.downloadItem);
            handler.OnDownloadUpdatedFired += (s, e) => OnDownloadUpdated(e, e.downloadItem);

            _crBrowser.GetBrowser().DownloadHandler = handler;
            _crBrowser.GetBrowser().RegisterAsyncJsObject("CrDownloadsCallBack", new CrDownloadsCallBack(this));
        }

        // 每个 dlItem 仅会调用一次；不一定在 OnDownloadUpdated 被调用前调用
        private void OnBeforeDownload(BeforeDownloadEventArgs e, DownloadItem dlItem)
        {
            DownloadTask dlTask = TryGetDlTask(dlItem);
        }

        private void OnDownloadUpdated(DownloadUpdatedEventArgs e, DownloadItem dlItem)
        {
            DownloadTask dlTask = TryGetDlTask(dlItem);
            dlTask.Update(dlItem, e.callback);
        }

        /// <summary>
        /// 获取 dlTask，若为 新dlItem，则通知前端并创建 新dlTask
        /// </summary>
        private DownloadTask TryGetDlTask(DownloadItem dlItem)
        {
            // 若不存在，则实例化一个新的
            if (!dlTaskDict.ContainsKey(dlItem.Id))
                dlTaskDict[dlItem.Id] = new DownloadTask(this, dlItem);

            return dlTaskDict[dlItem.Id];
        }

        /// <summary>
        /// 下载任务
        /// </summary>
        public class DownloadTask
        {
            private readonly DownloadManager DownloadManager;

            public string Id { get; }

            public DownloadItem DlItem { get; set; }

            public IDownloadItemCallback Callback { get; set; } = null; // Callback 用于执行 Cancel, Pause 等操作

            public Status Status { get; set; } // set 过后记得调用 CallFrontendUpdate

            /// <summary>
            /// 初始化
            /// </summary>
            public DownloadTask(DownloadManager downloadManager, DownloadItem dlItem)
            {
                DownloadManager = downloadManager;
                Id = Utils.GetTimeStamp();
                DlItem = dlItem;
                CallFrontendCreate();
            }

            /// <summary>
            /// 更新
            /// </summary>
            public void Update(DownloadItem dlItem = null, IDownloadItemCallback callback = null, Status ?status = null)
            {
                if (dlItem != null) DlItem = dlItem;
                if (callback != null) Callback = callback;

                if (status == null)
                {
                    if (dlItem.IsInProgress) Status = Status.Downloading;
                    else if (dlItem.IsComplete) Status = Status.Done;
                    else if (dlItem.IsCancelled) Status = Status.Cancelled;
                    else Status = Status.Fail;
                }
                else Status = (Status)status;

                CallFrontendUpdate();
            }

            /// <summary>
            /// 操作
            /// </summary>
            public void Handle(Action action)
            {
                if (action == Action.Cancel)
                {
                    Callback.Cancel();
                    Update(status: Status.Cancelled);
                }
                else if (action == Action.Pause)
                {
                    Callback.Pause();
                    Update(status: Status.Pause);
                }
                else if (action == Action.Resume)
                {
                    Callback.Resume(); // 命令仅需下达一次
                    Update(status: Status.Downloading);
                }
            }

            /// <summary>
            /// 通知前端 更新任务
            /// </summary>
            public void CallFrontendUpdate()
            {
                string callbackObj = JsonConvert.SerializeObject(new
                {
                    key = Id,
                    receivedBytes = DlItem.ReceivedBytes,
                    currentSpeed = DlItem.CurrentSpeed,
                    status = Status,
                    fullPath = DlItem.FullPath,
                    downloadUrl = DlItem.Url
                });
                DownloadManager.browser.ExecuteScriptAsync($"Downloads.updateTask({callbackObj})");
            }

            /// <summary>
            /// 通知前端 创建新任务
            /// </summary>
            public void CallFrontendCreate()
            {
                string callbackObj = JsonConvert.SerializeObject(new
                {
                    key = Id,
                    fullPath = DlItem.SuggestedFileName,
                    downloadUrl = DlItem.OriginalUrl,
                    totalBytes = DlItem.TotalBytes,
                });
                DownloadManager.browser.ExecuteScriptAsync($"Downloads.addTask({callbackObj})");
            }
        }

        /// <summary>
        /// 暴露给前端的 methods
        /// </summary>
        public class CrDownloadsCallBack
        {
            private DownloadManager _DownloadManager;

            public CrDownloadsCallBack(DownloadManager downloadManager)
            {
                _DownloadManager = downloadManager;
            }

            // 文件启动
            public bool FileLaunch(string fileFullPath)
            {
                if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath)) return false;
                Process.Start(fileFullPath);
                return true;
            }

            // 文件在资源管理器中显示
            public bool FileShowInExplorer(string fileFullPath)
            {
                if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath)) return false;
                Process.Start("explorer.exe", "/select, \"" + fileFullPath + "\"");
                return true;
            }

            // URL 在系统默认浏览器中打开
            public void UrlOpenInDefaultBrowser(string url)
            {
                Process.Start("explorer.exe", url);
            }

            // 下达任务操作命令
            public void DownloadingTaskAction(string dlTaskId, Action action)
            {
                DownloadTask dlTask = _DownloadManager.dlTaskDict.Where(o => o.Value.Id.Equals(dlTaskId)).FirstOrDefault().Value;
                if (dlTask != null) {
                    dlTask.Handle(action);
                }
            }
        }
    }
}
