using NacollectorUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NacollectorSpiders.SpiderSettings;

namespace NacollectorSpiders
{
    public class Class1
    {
        public void HelloWorld(Dictionary<string, object> taskSettings, Action<string> BrowserJsRunFunc)
        {
            //Form f1 = new Form();
            //f1.ShowDialog();
            
            //MessageBox.Show(NacollectorUtils.Utils.GetTempPath());

            //MessageBox.Show(NacollectorUtils.Utils.GetIsUseIeProxyReq().ToString());

            SpiderSettings settings = new SpiderSettings
            {
                TaskId = (string)taskSettings["TaskId"],
                ClassName = (string)taskSettings["ClassName"],
                ClassLabel = (string)taskSettings["ClassLabel"],
                ParmsJsonStr = (string)taskSettings["ParmsJsonStr"],
                BrowserJsRunFunc = BrowserJsRunFunc
            };

            BrowserJsRunFunc($"Task.log('{settings.TaskId}', '{Utils.Base64Encode("23333")}', 'i', '{Utils.GetTimeStamp()}', true);");
            var spider = new Business.CollItemDescImg();
            spider.importSettings(settings);
            spider.BeginWork();
            BrowserJsRunFunc($"Task.get('{settings.TaskId}').taskIsEnd();");
            Utils.ReleaseMemory(true);
        }
    }
}
