using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector.Browser.Handler
{
    public class MenuHandler : IContextMenuHandler
    {
        private bool _showReload = false;

        public MenuHandler(bool showReload = false)
        {
            _showReload = showReload;
        }

        private const int ShowDevTools = 26501;
        private const int CloseDevTools = 26502;
        private const int SaveImage = 26503;
        private const int SaveLink = 26504;
        private const int CopyLink = 26505;
        private const int LinkOpenDefaultBrowser = 26506;
        private const int LinkToZneiatProject = 26507;
        private const int FeedbackProject = 26508;

        void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            // To disable the menu then call clear
            model.Clear();
            // Removing existing menu item
            // model.Remove(CefMenuCommand.ViewSource); // Remove "View Source" option
            // Add new custom menu items
            // model.AddItem((CefMenuCommand)CloseDevTools, "Close DevTools");

            string selectionText = parameters.SelectionText;
            bool selectedText = !selectionText.Equals("");
            // 不在input中 勾选内容 右键
            if (!parameters.IsEditable && selectedText)
            {
                model.AddItem(CefMenuCommand.Copy, "复制 (Ctrl+C)");
            }
            // 在input中
            else if (parameters.IsEditable)
            {
                model.AddItem(CefMenuCommand.Cut, "剪切 (Ctrl+X)");
                model.AddItem(CefMenuCommand.Copy, "复制 (Ctrl+C)");
                model.AddItem(CefMenuCommand.Paste, "粘贴 (Ctrl+V)");
                model.AddItem(CefMenuCommand.Delete, "删除 (Delete)");
                model.AddSeparator();
                model.AddItem(CefMenuCommand.SelectAll, "全选 (Ctrl+A)");
                if (!selectedText)
                {
                    // 若没有勾选内容，则禁止点击 Cut Copy
                    model.SetEnabled(CefMenuCommand.Cut, false);
                    model.SetEnabled(CefMenuCommand.Copy, false);
                    model.SetEnabled(CefMenuCommand.Delete, false);
                }
            }
            else if (parameters.MediaType == ContextMenuMediaType.Image)
            {
                model.AddItem((CefMenuCommand)SaveImage, "下载图片");
            }
            else if (parameters.UnfilteredLinkUrl != "")
            {
                
                model.AddItem((CefMenuCommand)CopyLink, "复制链接");
                // 当 LinkUrl 是 JavaScript:; 的时候，下载会出问题
                if (!Regex.IsMatch(parameters.UnfilteredLinkUrl, @"(?s)(?i)^(javascript:)"))
                {
                    model.AddItem((CefMenuCommand)SaveLink, "下载链接内容");
                    model.AddItem((CefMenuCommand)LinkOpenDefaultBrowser, "默认浏览器打开");
                }
            }
            else
            {
                if (_showReload)
                    model.AddItem(CefMenuCommand.ReloadNoCache, "刷新 (ReloadNoCache)");
                model.AddItem((CefMenuCommand)FeedbackProject, "反馈问题");
                model.AddItem((CefMenuCommand)LinkToZneiatProject, "开源项目");
                // model.AddItem((CefMenuCommand)ShowDevTools, "检查 (ShowDevTools)");
            }
        }

        bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            switch ((int)commandId)
            {
                case ShowDevTools:
                    browser.ShowDevTools();
                    break;

                case CloseDevTools:
                    browser.CloseDevTools();
                    break;

                case SaveImage:
                    ExecuteSaveFileByUrl(parameters.SourceUrl.ToString(), frame);
                    break;

                case SaveLink:
                    ExecuteSaveFileByUrl(parameters.UnfilteredLinkUrl, frame);
                    break;

                case CopyLink:
                    Clipboard.SetDataObject(parameters.UnfilteredLinkUrl);
                    break;

                case LinkOpenDefaultBrowser:
                    System.Diagnostics.Process.Start("explorer.exe", parameters.UnfilteredLinkUrl);
                    break;
                case LinkToZneiatProject:
                    System.Diagnostics.Process.Start("https://github.com/Zneiat/Nacollector");
                    break;
                case FeedbackProject:
                    System.Diagnostics.Process.Start("https://github.com/Zneiat/Nacollector/issues");
                    break;
            }

            return false;
        }

        /// <summary>
        /// 执行通过URL保存文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="frame"></param>
        void ExecuteSaveFileByUrl(string url, IFrame frame)
        {
            frame.ExecuteJavaScriptAsync("var a = document.createElement('a');a.setAttribute('href', \"" + url + "\");a.setAttribute('download','');a.click();");
        }

        void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }

        bool IContextMenuHandler.RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}