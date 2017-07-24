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

        public event EventHandler<BeforeDownloadUpdatedEventArgs> OnBeforeDownloadFired;

        public event EventHandler<DownloadUpdatedEventArgs> OnDownloadUpdatedFired;

        // 下载之前（单个下载任务 只会执行一次，从这里获取 downloadItem.SuggestedFileName）
        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            var handler = OnBeforeDownloadFired;
            if (handler != null)
            {
                var eventArgs = new BeforeDownloadUpdatedEventArgs();
                eventArgs.downloadItem = downloadItem; // DownloadItem 用来获取下载任务信息至关重要
                eventArgs.callback = callback;

                // 调用事件
                handler(this, eventArgs);
            }

            string downloadPath = @"";
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadPath + downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        // 当下载任务信息更新
        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            var handler = OnDownloadUpdatedFired;
            if (handler != null)
            {
                var eventArgs = new DownloadUpdatedEventArgs();
                eventArgs.downloadItem = downloadItem;
                eventArgs.callback = callback; // IDownloadItemCallback 可以控制下载任务，用于暂停下载任务、取消下载任务等

                handler(this, eventArgs);
            }
        }
    }
}
