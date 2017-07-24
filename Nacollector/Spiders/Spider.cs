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

namespace Nacollector.Spiders
{
    public class Spider
    {
        protected TaskForm ParentForm;

        /// <summary>
        /// 1.设置父窗体
        /// </summary>
        /// <param name="pf"></param>
        public void setParentForm(TaskForm pf)
        {
            this.ParentForm = pf;
        }
        
        Hashtable parms = new Hashtable(); // 参数哈希表

        /// <summary>
        /// 2.设置参数
        /// </summary>
        /// <param name="parmsJsonStr"></param>
        public void SetParms(string parmsJsonStr)
        {
            JArray ja = (JArray)JsonConvert.DeserializeObject(parmsJsonStr);
            foreach (JObject item in ja)
            {
                string parmName = item["name"].ToString();
                string parValue = item["value"].ToString();
                parms[parmName] = parValue;
            }
        }

        

        /// <summary>
        /// 3.开始工作
        /// </summary>
        public virtual void BeginWork()
        {
            Thread.Sleep(200);
            Log(string.Format("ThreadID=\"{0}\"; SpiderObj=\"{1}\";", Thread.CurrentThread.ManagedThreadId, this.GetType().ToString()));
            LogInfo("任务执行开始");
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
            _Log(content, "normal");
        }

        public void LogInfo(string content)
        {
            _Log(content, "info");

        }

        public void LogSuccess(string content)
        {
            _Log(content, "success");
        }

        public void LogWarning(string content)
        {
            _Log(content, "warning");
        }

        public void LogError(string content)
        {
            _Log(content, "error");
        }

        /// <summary>
        /// 终端显示一条日志
        /// </summary>
        /// <param name="content"></param>
        /// <param name="type"></param>
        public void _Log(string content, string type = "normal")
        {
            string jsCode = string.Format("Te.log(\"{0}\", Te.lt['{1}'], \"{2}\");", Utils.Base64Encode(content), type, Utils.GetTimeStamp());
            ParentForm.crBrowser.RunJS(jsCode);
            // TaskTerminal.browserRunJS(string.Format("console.log(\"{0}\");", jsCode.Replace("\\", "\\\\").Replace("\"", "\\\"")));
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
    }
}
