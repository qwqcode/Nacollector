using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Browser.Handler
{
    public class LoadHandler : ILoadHandler
    {
        public void OnFrameLoadEnd(IWebBrowser browserControl, FrameLoadEndEventArgs frameLoadEndArgs)
        {
            // browserControl.ExecuteScriptAsync("");
        }

        public void OnFrameLoadStart(IWebBrowser browserControl, FrameLoadStartEventArgs frameLoadStartArgs)
        {
            // Console.WriteLine("Start Load: " + browserControl.Address);
        }

        public void OnLoadError(IWebBrowser browserControl, LoadErrorEventArgs loadErrorArgs)
        {

        }

        public void OnLoadingStateChange(IWebBrowser browserControl, LoadingStateChangedEventArgs loadingStateChangedArgs)
        {

        }
    }
}