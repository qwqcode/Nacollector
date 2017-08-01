using CefSharp;
using Microsoft.Win32;
using Nacollector.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 检查系统版本
            if (!Utils.IsWinVistaOrHigher())
            {
                MessageBox.Show("不支持的操作系统版本，最低需求为 Windows Vista",
                "Nacollector 无法运行", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 检测 .NET Framework 版本
            if (!Utils.IsSupportedRuntimeVersion())
            {
                MessageBox.Show("当前 .NET Framework 版本过低，请升级至 4.6.2 或更新版本",
                "Nacollector 无法运行", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Process.Start(
                    "http://dotnetsocial.cloudapp.net/GetDotnet?tfm=.NETFramework,Version=v4.6.2");
                return;
            }

            Utils.ReleaseMemory(true);
            using (Mutex mutex = new Mutex(false, $"Global\\Nacollector_{Application.StartupPath.GetHashCode()}"))
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.ApplicationExit += Application_ApplicationExit;
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName("Nacollector");
                    if (oldProcesses.Length > 0)
                    {
                        Process oldProcess = oldProcesses[0];
                    }
                    MessageBox.Show("请在任务栏里寻找 Nacollector 图标"
                        + Environment.NewLine
                        + "如果想同时启动多个，可以另外复制一份程序到别的目录");
                    return;
                }

                Directory.SetCurrentDirectory(Application.StartupPath);
                Logging.OpenLogFile();

                // 初始化 CEF
                InitCef();

                // 启动主界面
                Application.Run(new MainForm());
            }
        }

        /// <summary>
        /// 初始化 CEF
        /// </summary>
        private static void InitCef()
        {
            Cef.EnableHighDPISupport();
            var cefBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp");
            var setting = new CefSettings();
            setting.Locale = "zh-CN";
            setting.AcceptLanguageList = "zh-CN,zh";
            setting.LogFile = Utils.GetTempPath("cef.log");
            setting.CachePath = Utils.GetTempPath("cef_cache");
            setting.BrowserSubprocessPath = cefBasePath + @"\CefSharp.BrowserSubprocess.exe";
            setting.LocalesDirPath = cefBasePath + @"\locales\";
            setting.ResourcesDirPath = cefBasePath + @"\";
            Cef.Initialize(setting, true, null);
            // Cef.AddCrossOriginWhitelistEntry("https://", "http", "", true);
        }

        private static int exited = 0;
        
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref exited) == 1)
            {
                string errorMsg = $"异常细节: {Environment.NewLine}{e.Exception}";
                Logging.Error(errorMsg);
                MessageBox.Show(
                    $"意外的错误，Nacollector 将退出，请上QQ告诉我 1149527164 {Environment.NewLine}{errorMsg}",
                    "Nacollector UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref exited) == 1)
            {
                string errMsg = e.ExceptionObject.ToString();
                Logging.Error(errMsg);
                MessageBox.Show(
                    $"意外的错误，Nacollector 将退出，请上QQ告诉我 1149527164 \n {Environment.NewLine}{errMsg}",
                    "Nacollector non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // detach static event handlers
            Application.ApplicationExit -= Application_ApplicationExit;
            Application.ThreadException -= Application_ThreadException;

            Cef.Shutdown();
        }
    }
}
