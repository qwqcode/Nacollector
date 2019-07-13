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
    /// 升级操作
    /// </summary>
    class UpdateAction
    {
        private MainForm _form;
        private CrBrowser _crBrowser;

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
    }
}
