using Nacollector.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Spiders
{
    /// <summary>
    /// 蜘蛛配置
    /// </summary>
    public class SpiderSettings
    {
        private string taskId;
        private string className;
        private string classLabel;
        private string parmsJsonStr;
        private CrBrowser crBrowser;

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId
        {
            get { return taskId; }
            set { taskId = value; }
        }

        /// <summary>
        /// 任务调用类名
        /// </summary>
        public string ClassName
        {
            get { return className; }
            set { className = value; }
        }

        /// <summary>
        /// 任务调用类标题
        /// </summary>
        public string ClassLabel
        {
            get { return classLabel; }
            set { classLabel = value; }
        }

        /// <summary>
        /// 任务参数 JSON 字符串
        /// </summary>
        public string ParmsJsonStr
        {
            get { return parmsJsonStr; }
            set { parmsJsonStr = value; }
        }

        /// <summary>
        /// 浏览器（调用执行JS日志显示）
        /// </summary>
        public CrBrowser CrBrowser
        {
            get { return crBrowser; }
            set { crBrowser = value; }
        }
    }
}
