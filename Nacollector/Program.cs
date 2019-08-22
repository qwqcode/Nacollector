using CefSharp;
using CefSharp.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NacollectorUtils;
using System.Web;
using System.Text;
using System.Runtime.InteropServices;

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
                Process.Start("https://www.microsoft.com/zh-cn/download/details.aspx?id=53344");
                return;
            }

            Utils.ReleaseMemory(true);

            using (Mutex mutex = new Mutex(false, $"Global\\Nacollector_{Application.StartupPath.GetHashCode()}"))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

# if !DEBUG
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
# endif

                Application.ApplicationExit += Application_ApplicationExit;
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 检测程序是否已在运行
                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName("Nacollector");
                    if (oldProcesses.Length > 0)
                    {
                        Process oldProcess = oldProcesses[0];
                        Utils.ShowRunningInstance(oldProcess); // 显示已运行的程序
                    }
                    return;
                }

                Directory.SetCurrentDirectory(Application.StartupPath);
                Logging.OpenLogFile(); // 初始化日志
                InitCef(); // 初始化 CEF

                // 启动主界面
                Application.Run(new MainForm());
            }
        }

        // Cef 依赖文件存放目录
        public static readonly string CefBasePath = @"Resources\cef_sharp";

        /// <summary>
        /// 从文件载入依赖
        /// </summary>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string subfolderPath = Path.Combine(Application.StartupPath, CefBasePath, assemblyName);
                return File.Exists(subfolderPath) ? Assembly.LoadFile(subfolderPath) : null;
            }

            return null;
        }

        /// <summary>
        /// 初始化 CEF
        /// </summary>
        private static void InitCef()
        {
            Cef.EnableHighDPISupport();
            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("allow-file-access-from-files", "");
            settings.CefCommandLineArgs.Add("disable-web-security", "");
            settings.Locale = "zh-CN";
            settings.AcceptLanguageList = "zh-CN,zh";
            settings.LogFile = Utils.GetTempPath("cef.log");
            settings.CachePath = Utils.GetTempPath("cef_cache");
            settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\CefSharp.BrowserSubprocess.exe");
            settings.LocalesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\locales\");
            settings.ResourcesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\");
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true; // default is false, see https://github.com/cefsharp/CefSharp/issues/2359
            //settings.RemoteDebuggingPort = 51228;

            // 注册 "nacollector://"
            settings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ResourceSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ResourceSchemeHandlerFactory()
            });

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
        
        private static int exited = 0;

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref Program.exited) == 1)
            {
                Utils.ErrorHandlingAction("UI Error", $"异常细节: {Environment.NewLine}{e.Exception}", e.Exception.GetType().FullName);

                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref Program.exited) == 1)
            {
                Utils.ErrorHandlingAction("non-UI Error", $"异常细节: {Environment.NewLine}{e.ExceptionObject.ToString()}", e.ExceptionObject.GetType().FullName);

                Application.Exit();
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // detach static event handlers
            Application.ApplicationExit -= Application_ApplicationExit;
            Application.ThreadException -= Application_ThreadException;

            // kill cef process
            Cef.Shutdown();
        }
    }
}