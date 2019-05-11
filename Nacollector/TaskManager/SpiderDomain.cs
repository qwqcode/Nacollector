using Nacollector.Ui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Windows.Forms;
using NacollectorUtils.Settings;
using System.Diagnostics;
using NacollectorSpiders;

namespace Nacollector.TaskManager
{
    /// <summary>
    /// 执行爬虫任务，新的 AppDomain 中
    /// </summary>
    public class SpiderDomain : MarshalByRefObject
    {
        Assembly assembly = null;
        
        public void LoadAssembly(string assemblyPath)
        {
            try
            {
                assembly = Assembly.LoadFile(assemblyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public void NewTask(string fullClassName, SpiderSettings settings, SpiderCallback callback)
        {
            Type tp = assembly.GetType(fullClassName);
            Spider spider = (Spider)Activator.CreateInstance(tp);
            spider.NewTask(settings, callback);
        }
    }
}
