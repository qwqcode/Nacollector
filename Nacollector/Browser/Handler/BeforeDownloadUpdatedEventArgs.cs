using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Browser
{
    public class BeforeDownloadUpdatedEventArgs : EventArgs
    {
        public DownloadItem downloadItem { get; set; }
        public IBeforeDownloadCallback callback { get; set; }
    }
}
