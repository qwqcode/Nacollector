using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CefSharp.WinForms;
using CefSharp;
using System.Threading;
using Nacollector.Util;
using Nacollector.Browser;
using System.IO;

namespace Nacollector.Spiders
{
    public class Spider
    {
        protected string TaskId = null; // 任务ID
        protected Hashtable parms = new Hashtable(); // 参数哈希表
        protected CrBrowser crBrowser;

        /// <summary>
        /// 1.设置配置
        /// </summary>
        /// <param name="spiderSettings"></param>
        public void importSettings(SpiderSettings spiderSettings)
        {
            TaskId = spiderSettings.TaskId;

            JArray ja = (JArray)JsonConvert.DeserializeObject(spiderSettings.ParmsJsonStr);
            foreach (JObject item in ja)
            {
                string parmName = item["name"].ToString();
                string parmValue = item["value"].ToString();
                parms[parmName] = parmValue;
            }

            crBrowser = spiderSettings.CrBrowser;
        }
        
        /// <summary>
        /// 2.开始工作
        /// </summary>
        public virtual void BeginWork()
        {
            Thread.Sleep(900); // 开始得太快 感觉违和感强...
            Log(string.Format("ThreadID=\"{0}\"; SpiderObj=\"{1}\";", Thread.CurrentThread.ManagedThreadId, this.GetType().ToString()));
            Log("&gt;&gt; 任务执行开始");
            Log("\n");
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="parmName"></param>
        /// <returns></returns>
        public string GetParm(string parmName)
        {
            if (parms[parmName] == null)
                return null;
            return (string)parms[parmName];
        }
        
        public void Log(string content)
        {
            _Log(content);
        }

        public void LogInfo(string content)
        {
            _Log(content, "I");

        }

        public void LogSuccess(string content)
        {
            _Log(content, "S");
        }

        public void LogWarning(string content)
        {
            _Log(content, "W");
        }

        public void LogError(string content)
        {
            _Log(content, "E");
        }

        /// <summary>
        /// 任务日志表显示一条日志
        /// </summary>
        /// <param name="content"></param>
        /// <param name="level"></param>
        public void _Log(string content, string level = "")
        {
            Logging.Info("[" + TaskId + "]" + (!string.IsNullOrEmpty(level)?$"[{level}]":"") + " " + content);
            crBrowser.RunJS($"Task.log('{TaskId}', '{Utils.Base64Encode(content)}', '{level}', '{Utils.GetTimeStamp()}', true);");
        }
        
        /// <summary>
        /// 自动填充 URL Scheme
        /// </summary>
        /// <param name="url"></param>
        /// <param name="schemeIsHttps">是否补全为 https</param>
        /// <returns></returns>
        protected string UrlSchemeFull(string url, bool schemeIsHttps = false)
        {
            return url.Substring(0, 2).ToLower() == "//" ? (schemeIsHttps ? "https:" : "http:") + url : url;
        }

        /// <summary>
        /// 获取临时存放文件夹
        /// </summary>
        protected string GetTempDirPath(string tag = null)
        {
            string path = Utils.GetTempPath("Spider_Temp" + (!string.IsNullOrEmpty(tag) ? $"_{tag}" : ""));
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// 删除临时文件夹
        /// </summary>
        /// <param name="tag"></param>
        protected void DeleteTempDirPath(string tag = null)
        {
            string path = Utils.GetTempPath("Spider_Temp" + (!string.IsNullOrEmpty(tag) ? $"_{tag}" : ""));
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
