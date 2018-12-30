using Nacollector.Browser;
using Nacollector.Spiders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nacollector.JsActions
{
    /// <summary>
    /// 任务控制器操作
    /// </summary>
    class TaskControllerAction
    {
        private MainForm _form;
        private CrBrowser crBrowser;

        public TaskControllerAction(MainForm form, CrBrowser crBrowser)
        {
            this._form = form;
            this.crBrowser = crBrowser;
        }

        private Dictionary<string, Thread> taskThreads = new Dictionary<string, Thread>();

        // 创建新任务
        public void createTask(string taskId, string className, string classLabel, string parmsJsonStr)
        {
            // 配置
            var settings = new SpiderSettings()
            {
                TaskId = taskId,
                ClassName = className,
                ClassLabel = classLabel,
                ParmsJsonStr = parmsJsonStr
            };
            // 创建任务执行线程
            var thread = new Thread(new ParameterizedThreadStart(_form.StartTask))
            {
                IsBackground = true
            };
            thread.Start(settings);
            // 加入 Threads Dictionary
            taskThreads.Add(taskId, thread);
        }

        // 终止任务
        public bool abortTask(string taskId)
        {
            if (!taskThreads.ContainsKey(taskId))
                return false;

            taskThreads[taskId].Abort();
            return true;
        }
    }
}
