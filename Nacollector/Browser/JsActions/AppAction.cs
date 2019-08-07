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
using Newtonsoft.Json;

namespace Nacollector.Browser.JsActions
{
    /// <summary>
    /// App 杂项操作
    /// </summary>
    class AppAction
    {
        private MainForm _form;
        private CrBrowser _crBrowser;

        public AppAction(MainForm form, CrBrowser crBrowser)
        {
            this._form = form;
            this._crBrowser = crBrowser;
        }

        public void AppClose()
        {
            _form.Invoke((MethodInvoker)delegate {
                Application.Exit();
            });
        }

        public void AppMaxMini()
        {
            _form.Invoke((MethodInvoker)delegate {
                _form.ToggleMaximize();
            });
        }

        public void AppMin()
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
        public string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void ShowDevTools()
        {
            _crBrowser.GetBrowser().ShowDevTools();
        }

        // 采集是否使用IE代理请求
        public void _utilsReqIeProxy(bool isEnable)
        {
            Utils.GetIniFile().Write("ReqByIeProxy", isEnable ? "1" : "0", "Request");
        }

        // 日志文件清理
        public void LogFileClear()
        {
            Logging.Clear();
        }

        // 调用浏览器下载文件
        public void DownloadUrl(string url)
        {
            _crBrowser.DownloadUrl(url);
        }

        // 另存为文件
        public void SaveLocalFile(string path, string downloadKey)
        {
            _form.BeginInvoke((MethodInvoker)delegate
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = Path.GetFileName(path);
                savefile.Filter = $"*{Path.GetExtension(path)} | *.*";
                string dStatus;

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(path, savefile.FileName, true);
                    dStatus = "3";
                }
                else
                {
                    dStatus = "4";
                }
                _crBrowser.RunJS($"Downloads.updateTask({{ key: \"{downloadKey}\", receivedBytes: 0, currentSpeed: 0, status: {dStatus}, fullPath: String.raw`{savefile.FileName}`, downloadUrl: \"\" }})");
            });
        }

        // 升级操作
        public void AppUpdateAction(string srcUrl, string updateType)
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
