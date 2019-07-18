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
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net;

namespace Nacollector.Browser.JsActions
{
    /// <summary>
    /// 升级操作
    /// </summary>
    class UpdateAction
    {
        private MainForm _form;
        private CrBrowser _crBrowser;
        private static readonly string UpdateFileDir = Path.Combine(Utils.GetTempPath(), "./UpdateTemp/");

        public UpdateAction(MainForm form, CrBrowser crBrowser)
        {
            this._form = form;
            this._crBrowser = crBrowser;
        }

        // 获取升级参数
        public string GetUpdateParms()
        {
            return JsonConvert.SerializeObject(new
            {
                updateCheckUrl = GlobalConstant.UpdateCheckUrl,
                updateCheckToken = GlobalConstant.UpdateCheckToken
            });
        }

        // 获取文件版本号
        public string GetFileVersion(string path)
        {
            try { return FileVersionInfo.GetVersionInfo(Path.Combine(Application.StartupPath, path)).ProductVersion; }
            catch { return ""; }
        }

        // 获取所有模块的版本号
        public Dictionary<string, string> GetAllModuleVersion()
        {
            return new Dictionary<string, string>
            {
                { "Nacollector", GetFileVersion(Process.GetCurrentProcess().MainModule.FileName) },
                { "NacollectorSpiders", GetFileVersion("NacollectorSpiders.dll") },
                { "NacollectorUpdater", GetFileVersion("NacollectorUpdater.exe") },
                { "CefSharp", GetFileVersion(Path.Combine(Program.CefBasePath, "./CefSharp.dll")) }
            };
        }

        // 执行任务下载
        public void StartUpdateWork(string updateModulesJsonStr)
        {
            var updateModules = JArray.Parse(updateModulesJsonStr);

            var updaterUnpackFileList = new List<string> { };
            var updaterCopyFileList = new List<string> { };

            int i = 0;
            // 创建下载
            foreach (var module in updateModules)
            {
                try
                {
                    if (i == 0)
                    {
                        // 清理升级文件路径
                        SetUpdateProgress(0, "正在清理升级文件路径...");
                        if (Directory.Exists(UpdateFileDir))
                        {
                            new DirectoryInfo(UpdateFileDir).GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(file => file.Delete());
                        }
                        else
                        {
                            Directory.CreateDirectory(UpdateFileDir);
                        }
                    }

                    string filePath = StartUpdateDownload(module);

                    if (!File.Exists(filePath))
                        throw new Exception($"未找到下载的升级文件: ${filePath}");

                    string moduleName = module["name"].ToString();
                    if (moduleName == "NacollectorSpiders" || moduleName == "NacollectorUpdater")
                    {
                        // 直接复制文件并覆盖
                        File.Copy(filePath, Path.Combine(Application.StartupPath, moduleName + Path.GetExtension(filePath)), true);

                        // 刷新 SpiderList
                        if (moduleName == "NacollectorSpiders")
                            MainForm.taskRunner.RefreshFrontendSpiderList();
                    }
                    else
                    {
                        // 使用外部升级程序
                        if (new string[] { ".zip", ".7z", ".rar", ".gz", ".bz2", ".xz", ".tar", ".tar.gz", ".tar.bz2", ".tar.xz" }.Contains(Path.GetExtension(filePath)))
                        { // 压缩包
                            updaterUnpackFileList.Add(filePath);
                        }
                        else
                        {
                            updaterCopyFileList.Add(filePath);
                        }
                    }
                }
                catch (Exception e)
                {
                    UpdateError($"模块 {module["name"]} 更新失败：{e}");
                    return;
                }
                i++;
            }

            if (updaterCopyFileList.Count > 0 || updaterUnpackFileList.Count > 0)
            {
                string arguments = "";

                if (updaterCopyFileList.Count > 0) {
                    arguments += "-c";
                    foreach (string path in updaterCopyFileList)
                        arguments += $" \"{path}\"";
                }
                if (updaterUnpackFileList.Count > 0)
                {
                    arguments += " -p";
                    foreach (string path in updaterUnpackFileList)
                        arguments += $" \"{path}\"";
                }

                // 调用外部升级程序
                if (File.Exists(Path.Combine(Application.StartupPath, "NacollectorUpdater.exe")))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "NacollectorUpdater.exe";
                    process.StartInfo.WorkingDirectory = Application.StartupPath;
                    process.StartInfo.Arguments = arguments;
                    process.Start();
                }
                else
                {
                    UpdateError($"模块 NacollectorUpdater 丢失，无法升级");
                    return;
                }
            }
            else
            {
                // 直接结束更新
                UpdateSuccess();
            }
        }

        private string StartUpdateDownload(JToken module)
        {
            string moduleSrc = (string)module["src"];
            Uri SrcUri = new Uri(moduleSrc);
            string DownloadFilePath = Path.Combine(UpdateFileDir, Path.GetFileName(SrcUri.AbsolutePath)); // 保存本地文件名
            string progPreText = $"正在下载 {Path.GetFileName(moduleSrc)}";

            SetUpdateProgress(0, progPreText + " 连接服务器中...");

            if (File.Exists(DownloadFilePath))
                File.Delete(DownloadFilePath);

            // Init
            Stopwatch sw = new Stopwatch();
            WebClient wc = new WebClient();

            // 代理请求
            if (Utils.GetIsUseIeProxyReq())
            {
                wc.Proxy = WebRequest.GetSystemWebProxy();
            }

            // Events
            wc.DownloadProgressChanged += (s, e) =>
            {
                double receive = double.Parse(e.BytesReceived.ToString());
                double total = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = (total > 0) ? ((receive / total) * 100) : 0;

                string speed = string.Format("{0} KB/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
                SetUpdateProgress(percentage, $"{progPreText}  {string.Format("{0:0.##}", percentage)}%  速度 {speed}");
            };
            wc.DownloadFileCompleted += (s, e) => {
                sw.Reset();
            };

            // Go
            sw.Start();
            Task.Run(async () =>
            {
                await wc.DownloadFileTaskAsync(SrcUri, DownloadFilePath);
            }).Wait();
            return DownloadFilePath;
        }

        private void UpdateSuccess(string desc = "")
        {
            _crBrowser.RunJS($"AppUpdate.panel.setSuccess(\"{PurifyStrForJS(desc)}\")");
        }

        private void UpdateError(string reason = "")
        {
            _crBrowser.RunJS($"AppUpdate.panel.setError(\"{PurifyStrForJS(reason)}\")");
        }

        private void SetUpdateProgress(double percentage, string statusText)
        {
            _crBrowser.RunJS($"AppUpdate.panel.setProgress({string.Format("{0:0.##}", percentage)}, \"{PurifyStrForJS(statusText)}\")");
        }

        private string PurifyStrForJS(string str)
        {
            return str.Replace("\"", "\\\"").Replace(" ", "\\&nbsp;").Replace(Environment.NewLine, "<br/>");
        }
    }
}
