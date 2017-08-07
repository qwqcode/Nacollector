using CefSharp;
using CefSharp.Internals;
using CefSharp.WinForms;
using Nacollector.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector.Browser
{
    public class CrBrowserCookieGetter
    {
        public static List<string> visitedAddress = new List<string>(); // static 不管实例化多少次对象，每次值都是一样的

        public string StartUrl { get; set; }
        public string EndUrlRegPattern { get; set; }

        private bool isUseAutoCompleteInput = false;
        private static Dictionary<string, List<string>> AutoCompleteInput = new Dictionary<string, List<string>>();

        private ChromiumWebBrowser browser = null;
        private Form form = null;

        private string cookie = null;

        public CrBrowserCookieGetter(string caption = "")
        {
            // 新建一个 form
            form = new Form()
            {
                ClientSize = new Size(1150, 698),
                ShowIcon = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = caption,
            };
            // 当窗口关闭时执行
            form.FormClosing += (s, e) =>
            {
                //try
                //{
                //    browser.CloseDevTools();
                //    browser.GetBrowser().CloseBrowser(true);
                //    if (browser != null)
                //    {
                //        browser.Dispose();
                //    }
                //}
                //catch { }
            };
            form.FormClosed += (s, e) =>
            {
                // 至于有没有用，以后再试
                form.Close();
                form.Dispose();
            };
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        private void InitBrowser()
        {
            // 删除浏览器中所有 Cookie
            // Cef.GetGlobalCookieManager().DeleteCookies(null, null);
            // 删除已访问过网页的 Cookie
            ClearCookies();
            // 初始化浏览器
            browser = new ChromiumWebBrowser(StartUrl)
            {
                Dock = DockStyle.Fill,
            };
            browser.BrowserSettings = new BrowserSettings()
            {
                WebSecurity = CefState.Enabled,
                FileAccessFromFileUrls = CefState.Disabled,
                UniversalAccessFromFileUrls = CefState.Disabled,
                DefaultEncoding = "UTF-8",
                BackgroundColor = (uint)ColorTranslator.FromHtml("#333333").ToArgb()
            };
            // 右键菜单
            browser.MenuHandler = new MenuHandler(this);

            browser.FrameLoadStart += (s, e) =>
            {
                var address = ((ChromiumWebBrowser)s).Address;
                if (!visitedAddress.Contains(address))
                    visitedAddress.Add(address);
            };
            // AddressChanged || FrameLoadEnd (可获得JS后添加的Cookie)
            var pageLoadTimes = 0;
            browser.AddressChanged += (s, e) => {
                pageLoadTimes++;
                if (pageLoadTimes == 1) return; // 页面第一次加载的时候怎么可能获取 Cookie ?? So... 第一次不获取

                var address = ((ChromiumWebBrowser)s).Address;
                if (Regex.IsMatch(address, EndUrlRegPattern))
                {
                    EndWork(address);
                }
            };
            
            form.Controls.Add(browser);

            AutoCompleteInputInit();
        }

        /// <summary>
        /// 开始工作（显示窗体）
        /// </summary>
        public void BeginWork()
        {
            if (browser == null)
                InitBrowser();

            form.ShowDialog();
        }

        /// <summary>
        /// 结束工作（关闭窗体）
        /// </summary>
        public async void EndWork(string cookieAddress, string cookieStr = null)
        {
            string cookieHeader = null;
            if (!string.IsNullOrEmpty(cookieAddress) && cookieStr == null)
            {
                var visitor = new CookieCollector();
                Cef.GetGlobalCookieManager().VisitUrlCookies(cookieAddress, true, visitor);

                var cookiesList = await visitor.Task; // AWAIT !!!!!!!!!
                cookieHeader = CookieCollector.GetCookieHeader(cookiesList);
            }
            else if (cookieAddress == null && cookieStr != null)
            {
                cookieHeader = cookieStr;
            }

            // 关键的赋值
            cookie = cookieHeader;

            // 关闭窗体
            form.Invoke(new Action(() => {
                if (form == null || form.IsDisposed)
                    return;

                form.Close();
            }));
        }

        /// <summary>
        /// 设置表单自动填充
        /// </summary>
        public void UseAutoCompleteInput(string forPageUrlRegPattern, List<string> inputCssSelector)
        {
            if (!AutoCompleteInput.ContainsKey(forPageUrlRegPattern))
            {
                AutoCompleteInput.Add(forPageUrlRegPattern, inputCssSelector);
            }
            isUseAutoCompleteInput = true;
        }

        /// <summary>
        /// 初始化表单自动填充
        /// </summary>
        public void AutoCompleteInputInit()
        {
            if (!isUseAutoCompleteInput)
                return;

            foreach (var pattern in AutoCompleteInput.Keys)
            {
                MessageBox.Show(pattern);

                MessageBox.Show(browser.Address); /// 需要将 function 放到当页面每次加载时执行

                if (!Regex.IsMatch(browser.Address, pattern))
                    continue;
                
                List<string> selectors = AutoCompleteInput[pattern];
                string jsCodeStr = "";
                foreach (var sel in selectors)
                {
                    jsCodeStr += @"document.querySelector('"+ sel + "').onblur = function () {  console.log(this.value); };";
                }

                MessageBox.Show(jsCodeStr);

                browser.ExecuteScriptAsync(jsCodeStr);

                break;
            }
        }

        /// <summary>
        /// 获取 Cookie 字符串
        /// </summary>
        /// <returns>this.cookie 为空返回 ""</returns>
        public string GetCookieStr()
        {
            if (string.IsNullOrEmpty(cookie))
                return "";

            return cookie;
        }

        /// <summary>
        /// 清理已访问过网页的全部Cookie
        /// </summary>
        /// <returns></returns>
        public static void ClearCookies()
        {
            // string test = "";
            foreach (var address in visitedAddress)
            {
                // test += address + Environment.NewLine;
                // Cef.GetGlobalCookieManager().DeleteCookies(address, null); // 不能用
                var visitor = new CookieCollector(deleteAllCookie: true);
                Cef.GetGlobalCookieManager().VisitUrlCookies(address, true, visitor);
                visitor.Task.Wait();
            }
            // MessageBox.Show(test);
        }

        /// <summary>
        /// 用于获取 Cookie 的
        /// </summary>
        private class CookieCollector : ICookieVisitor
        {
            private bool _deleteAllCookie = false;
            public CookieCollector(bool deleteAllCookie = false)
            {
                _deleteAllCookie = deleteAllCookie;
            }

            // https://github.com/amaitland/CefSharp.MinimalExample/blob/ce6e579ad77dc92be94c0129b4a101f85e2fd75b/CefSharp.MinimalExample.WinForms/ListCookieVisitor.cs
            // CefSharp.MinimalExample.WinForms ListCookieVisitor 

            private readonly TaskCompletionSource<List<Cookie>> _source = new TaskCompletionSource<List<Cookie>>();

            public Task<List<Cookie>> Task => _source.Task;

            private readonly List<Cookie> _cookies = new List<Cookie>();

            public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
            {
                _cookies.Add(cookie);

                if (count == (total - 1))
                {
                    _source.SetResult(_cookies);
                }

                if (_deleteAllCookie)
                    deleteCookie = true;

                return true;
            }

            public static string GetCookieHeader(List<Cookie> cookies)
            {
                StringBuilder cookieString = new StringBuilder();
                string delimiter = string.Empty;

                foreach (var cookie in cookies)
                {
                    cookieString.Append(delimiter);
                    cookieString.Append(cookie.Name);
                    cookieString.Append('=');
                    cookieString.Append(cookie.Value);
                    delimiter = "; ";
                }

                return cookieString.ToString();
            }
            
            public void Dispose() {}
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        private class MenuHandler : IContextMenuHandler
        {
            private const int GetCurrentCookie = 26501;
            private const int InputCookie = 26502;
            private const int ShowDevTools = 26503;

            private CrBrowserCookieGetter _this;

            public MenuHandler(CrBrowserCookieGetter thisObj)
            {
                _this = thisObj;
            }

            void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.AddSeparator();
                model.AddItem((CefMenuCommand)GetCurrentCookie, "获取当前 Cookie");
                model.AddItem((CefMenuCommand)InputCookie, "手动输入 Cookie");
                model.AddSeparator();
                model.AddItem((CefMenuCommand)ShowDevTools, "检查 (ShowDevTools)");
            }

            bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                switch ((int)commandId)
                {
                    case GetCurrentCookie:
                        _this.EndWork(browserControl.Address);
                        break;

                    case InputCookie:
                        string cookieStr = "";
                        _this.form.Invoke(new Action(() => {
                            cookieStr = Utils.InputDialog("输入 Cookie 字符串 (通过 Chrome EditThisCookie 扩展完整获取)", "手动输入 Cookie");
                        }));
                        _this.EndWork(null, cookieStr);
                        break;

                    case ShowDevTools:
                        browser.ShowDevTools();
                        break;
                }

                return false;
            }

            void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame) { }

            bool IContextMenuHandler.RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback) { return false; }
        }
    }
}
