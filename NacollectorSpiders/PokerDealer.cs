using NacollectorUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NacollectorUtils.Settings;

namespace NacollectorSpiders
{
    public class PokerDealer
    {
        public void NewTask(SpiderSettings settings)
        {
            DateTime beforWorkDt = DateTime.Now;

            string typeName = $"{this.GetType().Namespace}.{settings.ClassName}";

            // 实例化 Spider 对象
            var spider = (Spider)Activator.CreateInstance(Type.GetType(typeName));
            spider.importSettings(settings);

            // 开始任务工作
#if !DEBUG
            try
            {
#endif
            spider.BeginWork();
#if !DEBUG
        }
            catch (Exception e)
            {
                // 任务执行中抛出的错误被接住了...
                spider.LogError(e.Message);
                Logging.Error(e.ToString()); // 保存错误详情
            }
#endif

            // 任务执行完毕
            DateTime afterWorkDt = DateTime.Now;
            double timeSpent = afterWorkDt.Subtract(beforWorkDt).TotalSeconds;
            spider.Log("\n");
            spider.Log($"&gt;&gt; 任务执行完毕 （执行耗时：{timeSpent.ToString()}s）");
            settings.BrowserJsRunFunc($"Task.get('{settings.TaskId}').taskIsEnd();"); // 报告JS任务结束

            Utils.ReleaseMemory(true);
        }
    }
}
