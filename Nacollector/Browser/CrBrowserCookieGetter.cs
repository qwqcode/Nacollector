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
        public static List<string> visitedAddress = new List<string>(); // 所有已访问过的页面地址
        public static Dictionary<string, string> historyCookie = new Dictionary<string, string>(); // 所有 已得到的 历史Cookie 字符串

        public string StartUrl { get; } // 初始页 URL
        public string EndUrlReg { get; } // 获取Cookie页 Url 正则表达式

        private Form form = null;
        private ChromiumWebBrowser browser = null;
        private InputAutoComplete inputAutoComplete = null;

        private string cookie = null;

        public CrBrowserCookieGetter(string startUrl, string endUrlReg, string caption = "")
        {
            StartUrl = startUrl;
            EndUrlReg = endUrlReg;

            // 是否使用上一次得到的 Cookie
            if (historyCookie.ContainsKey(EndUrlReg) && (MessageBox.Show("上一次已经获取过 Cookie，是否继续使用？" + Environment.NewLine + "如果刚刚才干过这件事 请点是", caption, MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                // 使用上一次的 Cookie
                EndWork(null, historyCookie[EndUrlReg]);
                return;
            }
            
            // 新建一个 form
            form = new Form()
            {
                ClientSize = new Size(1300, 700),
                ShowIcon = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = caption,
            };
            form.FormClosing += ((s, e) =>
            {
                form.DialogResult = DialogResult.OK;
            });

            // 浏览器 初始化
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
            browser.MenuHandler = new MenuHandler(this); // 右键菜单
            // 当页面加载开始
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
                if (pageLoadTimes == 1) return; // 页面第一次加载的时候怎么可能获取 Cookie ?? So... 第一次加载不获取

                var address = ((ChromiumWebBrowser)s).Address;
                if (Regex.IsMatch(address, EndUrlReg))
                {
                    EndWork(address);
                }
            };
        }

        /// <summary>
        /// 1. 使用输入自动填充
        /// </summary>
        /// <param name="pageUrlReg">生效页面地址正则表达式</param>
        /// <param name="inputElemCssSelectors">输入框元素CSS选择器</param>
        public void UseInputAutoComplete(string pageUrlReg, List<string> inputElemCssSelectors)
        {
            if (browser == null) return;
            inputAutoComplete = new InputAutoComplete(browser, pageUrlReg, inputElemCssSelectors);
        }

        /// <summary>
        /// 2. 开始工作（显示窗体）
        /// </summary>
        public void BeginWork()
        {
            if (browser == null) return;
            form.Controls.Add(browser);
            MainForm._mainForm.Invoke(new Action(() =>
            {
                form.ShowDialog();
            }));
        }

        /// <summary>
        /// 3. 结束工作（关闭窗体）
        /// </summary>
        /// <param name="cookieAddress">Cookie 地址</param>
        /// <param name="cookieStr">Cookie 字符串</param>
        /// <param name="closeForm">是否关闭窗体</param>
        public async void EndWork(string cookieAddress, string cookieStr = null)
        {
            string cookieHeader = null;
            if (!string.IsNullOrEmpty(cookieAddress) && cookieStr == null)
            {
                var visitor = new CookieCollector();
                Cef.GetGlobalCookieManager().VisitUrlCookies(cookieAddress, true, visitor);

                var cookiesList = await visitor.Task; // AWAIT !!!!!!!!!
                cookieHeader = GetCookieStr(cookiesList);
            }
            else if (cookieAddress == null && cookieStr != null)
            {
                cookieHeader = cookieStr;
            }

            // 关键的赋值
            cookie = cookieHeader;
            historyCookie[EndUrlReg] = cookieHeader;

            // 关闭窗体
            if (form != null && !form.IsDisposed)
            {
                MainForm._mainForm.Invoke(new Action(() =>
                {
                    form.Close();
                }));
            }

            // 清理 Cookie
            if (visitedAddress.Count > 0)
            {
                // 删除已访问过网页的 Cookie
                foreach (var address in visitedAddress)
                {
                    var visitor = new CookieCollector(deleteAllCookie: true);
                    Cef.GetGlobalCookieManager().VisitUrlCookies(address, true, visitor);
                }
            }
        }

        private class InputAutoComplete
        {
            private ChromiumWebBrowser browser;
            private static Dictionary<string, AutoInputConfig> pages = new Dictionary<string, AutoInputConfig>(); // 静态：所有输入框自动填充配置；静态字段 无论实例化对象多少次，字段的值不变

            public InputAutoComplete(ChromiumWebBrowser parmBrowser, string pageUrlReg, List<string> inputElemCssSelectors)
            {
                if (!pages.ContainsKey(pageUrlReg))
                {
                    var inputConfig = new AutoInputConfig() { inputElemSelectors = inputElemCssSelectors };
                    pages.Add(pageUrlReg, inputConfig);
                }

                browser = parmBrowser;
                browser.RegisterJsObject("_cr_browser_InputRecord", new InputRecordObjForJs() { PageUrlReg = pageUrlReg });
                browser.FrameLoadStart += (s, e) =>
                {
                    var currentAddress = ((ChromiumWebBrowser)s).Address;
                    
                    foreach (var dic in pages)
                    {
                        if (!Regex.IsMatch(currentAddress, dic.Key))
                            continue;

                        var config = dic.Value;
                        List<string> selectors = config.inputElemSelectors;

                        string jsCodeStr = "";
                        foreach (var sel in selectors)
                        {
                            jsCodeStr += @"document.querySelector('" + sel + "').oninput = function () { _cr_browser_InputRecord.record('" + sel + "', this.value); };";
                            string data = config.GetInputValue(sel);
                            if (!string.IsNullOrEmpty(data))
                                jsCodeStr += $"document.querySelector('{sel}').value = '{data.Replace(@"\", @"\\").Replace("'", @"\'")}';";
                        }

                        // MessageBox.Show(jsCodeStr);

                        browser.ExecuteScriptAsync(jsCodeStr);

                        break;
                    }
                };
            }
            
            // 输入记录对象 拿给JS用的
            private class InputRecordObjForJs
            {
                public string PageUrlReg { get; set; } // 存值时需要的 页面地址正则表达式
                
                public void record(string inputElemSel, string value)
                {
                    if (!pages.ContainsKey(PageUrlReg))
                        return;

                    pages[PageUrlReg].SaveInputValue(inputElemSel, value);
                }
            }

            private class AutoInputConfig
            {
                public List<string> inputElemSelectors { get; set; } // 输入框元素选择器
                public Dictionary<string, string> inputValues = new Dictionary<string, string>(); // 输入值

                // 存储输入值，对应一个 输入框元素选择器
                public void SaveInputValue(string inputSel, string val)
                {
                    if (inputValues.ContainsKey(inputSel))
                        inputValues.Remove(inputSel); // 字典使用 Add 不会覆盖原有值，所以先删除原有值

                    inputValues.Add(inputSel, val); // 也可以使用 inputValues[inputSel] = val; 代替这行代码，直接覆盖原有值
                }

                // 获取输入值
                public string GetInputValue(string inputSel)
                {
                    if (!inputValues.ContainsKey(inputSel)) return "";
                    return inputValues[inputSel];
                }
            }
        }
        
        /// <summary>
        /// 4. 获取 Cookie 字符串
        /// </summary>
        /// <returns>this.cookie 为空返回 ""</returns>
        public string GetCookieStr()
        {
            if (string.IsNullOrEmpty(cookie))
                return "";

            return cookie;
        }

        /// <summary>
        /// List 转 Cookie 字符串
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public string GetCookieStr(List<Cookie> cookies)
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

        /// <summary>
        /// Cookie 操作
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
                        _this.form.Invoke(new Action(() =>
                        {
                            cookieStr = Utils.InputDialog("输入 Cookie 字符串 (通过 Chrome EditThisCookie 扩展完整获取)", "手动输入 Cookie");
                        }));
                        if (!string.IsNullOrEmpty(cookieStr))
                            _this.EndWork(null, cookieStr);
                        else
                            MessageBox.Show("Cookie 字符串不能为空", "手动输入 Cookie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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