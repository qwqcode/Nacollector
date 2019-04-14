using CefSharp;
using Nacollector.Browser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NacollectorUtils;

namespace Nacollector.JsActions
{
    /// <summary>
    /// App 杂项操作
    /// </summary>
    class AppAction
    {
        private MainForm _form;
        private CrBrowser crBrowser;

        public AppAction(MainForm form, CrBrowser crBrowser)
        {
            this._form = form;
            this.crBrowser = crBrowser;
        }

        public void appClose()
        {
            _form.Invoke((MethodInvoker)delegate {
                Application.Exit();
            });
        }

        public void appMaxMini()
        {
            _form.Invoke((MethodInvoker)delegate {
                _form.ToggleMaximize();
            });
        }

        public void appMin()
        {
            _form.Invoke((MethodInvoker)delegate {
                if (!_form.ShowInTaskbar)
                {
                    _form.Hide();
                }
                else
                {
                    _form.WindowState = FormWindowState.Minimized;
                }
            });
        }

        // 获取程序版本
        public string getVersion()
        {
            crBrowser.RunJS($"AppConfig.updateCheckUrl=\"{GlobalConstant.UpdateCheckUrl}\";AppConfig.updateCheckToken=\"{GlobalConstant.UpdateCheckToken}\"");
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void showDevTools()
        {
            crBrowser.GetBrowser().ShowDevTools();
        }

        // 采集是否使用IE代理请求
        public void _utilsReqIeProxy(bool isEnable)
        {
            Utils.GetIniFile().Write("ReqByIeProxy", isEnable ? "1" : "0", "Request");
        }

        // 日志文件清理
        public void logFileClear()
        {
            Logging.Clear();
        }

        // 调用浏览器下载文件
        public void downloadUrl(string url)
        {
            crBrowser.DownloadUrl(url);
        }

        // 升级操作
        public void appUpdateAction(string srcUrl, string updateType)
        {
            if (!File.Exists(Path.Combine(Application.StartupPath, "Naupdater.exe")))
            {
                MessageBox.Show("升级 Naupdater.exe 模块丢失，无法升级", "未找到 Naupdater.exe", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process process = new Process();
            process.StartInfo.FileName = "Naupdater.exe";
            process.StartInfo.WorkingDirectory = Application.StartupPath;
            process.StartInfo.Arguments = $"-{updateType} \"{srcUrl}\"";
            // MessageBox.Show($"-{updateType} \"{srcUrl}\"");
            process.Start();
        }
    }
}
