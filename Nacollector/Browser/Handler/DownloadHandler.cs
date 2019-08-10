using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Browser.Handler
{
    public class DownloadHandler : IDownloadHandler
    {

        public event EventHandler<BeforeDownloadEventArgs> OnBeforeDownloadFired;

        public event EventHandler<DownloadUpdatedEventArgs> OnDownloadUpdatedFired;

        /// <summary>
        /// 下载之前（一个下载任务 只会执行一次，从这里获取 downloadItem.SuggestedFileName）
        /// </summary>
        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, new BeforeDownloadEventArgs
            {
                downloadItem = downloadItem, // DownloadItem 用来获取下载任务信息至关重要
                callback = callback
            });

            // 下载前显示对话框
            string downloadPath = @"";
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadPath + downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        /// <summary>
        /// 当下载任务信息更新
        /// </summary>
        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, new DownloadUpdatedEventArgs
            {
                downloadItem = downloadItem,
                callback = callback // IDownloadItemCallback 可以控制下载任务，用于暂停下载任务、取消下载任务等
            });
        }
    }
}