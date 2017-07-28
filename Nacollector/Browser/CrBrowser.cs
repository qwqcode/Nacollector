using CefSharp;
using CefSharp.WinForms;
using Nacollector.Browser.Handler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector.Browser
{
    public class CrBrowser
    {
        private ChromiumWebBrowser browser;

        public CrBrowser(string address)
        {
            // 初始化浏览器
            browser = new ChromiumWebBrowser(address);

            // BrowserSettings 必须在 Controls.Add 之前
            BrowserSettings browserSettings = new BrowserSettings();
            // browserSettings.WebSecurity = CefState.Disabled
            browserSettings.FileAccessFromFileUrls = CefState.Enabled; // 必须 Enabled 不然 AJAX 请求 file:// 会显示 Cross origin requests are only supported for protocol schemes: http, data, chrome, chrome-extension, https.
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            browserSettings.DefaultEncoding = "UTF-8";
            browserSettings.BackgroundColor = (uint)ColorTranslator.FromHtml("#333333").ToArgb();
            browser.BrowserSettings = browserSettings;
            
            browser.MenuHandler = new MenuHandler();
            browser.LifeSpanHandler = new LifeSpanHandler();
            browser.LoadHandler = new LoadHandler();

            browser.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(Browser_onFrameLoadEnd);
            browser.IsBrowserInitializedChanged += new EventHandler<IsBrowserInitializedChangedEventArgs>(Browser_onIsBrowserInitializedChanged);
        }
        
        // Frame 加载完毕时执行
        private void Browser_onFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // MessageBox.Show(((ChromiumWebBrowser)sender).Address);
        }
        
        // 浏览器初始化完毕时执行
        private void Browser_onIsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
        }

        /// <summary>
        /// 获取 ChromiumWebBrowser this.browser
        /// </summary>
        /// <returns></returns>
        public ChromiumWebBrowser GetBrowser()
        {
            return browser;
        }

        /// <summary>
        /// 浏览器执行JS代码
        /// </summary>
        /// <param name="jsCodeStr"></param>
        public void RunJS(string jsCodeStr)
        {
            if (browser.InvokeRequired) { browser.Invoke(new runJSDelegate(RunJS), new object[] { jsCodeStr }); return; }

            if (browser == null || !browser.IsBrowserInitialized || browser.IsDisposed || browser.Disposing) { return; }
            
            browser.ExecuteScriptAsync(jsCodeStr);
        }
        public delegate void runJSDelegate(string jsCodeStr);

        /// <summary>
        /// 浏览器执行JS代码获取返回值
        /// </summary>
        /// <param name="script"></param>
        /// <param name="defaultValue"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <example>同步：EvaluateScript("5555 * 19999 + 88888", 0, TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();</example>
        public async Task<object> EvaluateScript(string script, object defaultValue, TimeSpan timeout)
        {
            object result = defaultValue;
            if (browser.IsBrowserInitialized && !browser.IsDisposed && !browser.Disposing)
            {
                try
                {
                    var task = browser.EvaluateScriptAsync(script, timeout);
                    await task.ContinueWith(res => {
                        if (!res.IsFaulted)
                        {
                            var response = res.Result;
                            result = response.Success ? (response.Result ?? "null") : response.Message;
                        }
                    }).ConfigureAwait(false); // <-- This makes the task to synchronize on a different context
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }
            return result;
        }
    }
}
