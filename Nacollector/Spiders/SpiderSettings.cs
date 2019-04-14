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
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 任务调用类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 任务调用类标题
        /// </summary>
        public string ClassLabel { get; set; }

        /// <summary>
        /// 任务参数 JSON 字符串
        /// </summary>
        public string ParmsJsonStr { get; set; }

        /// <summary>
        /// 浏览器（调用执行JS日志显示）
        /// </summary>
        public CrBrowser CrBrowser { get; set; }
    }
}