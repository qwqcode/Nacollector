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
                    "https://www.microsoft.com/zh-cn/download/details.aspx?id=53344");
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

        // The subfolder, where the cefsharp files will be moved to
        public static readonly string CefBasePath = @"Resources\cef_sharp";
        // If the assembly resolver loads cefsharp from another folder, set this to true
        private static bool resolved = false;

        /// <summary>
        /// Will attempt to load missing assemblys from subfolder
        /// </summary>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                resolved = true; // Set to true, so BrowserSubprocessPath will be set

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
            //settings.CefCommandLineArgs.Add("--allow-file-access-from-files", "");
            settings.CefCommandLineArgs.Add("--disable-web-security", "");
            settings.Locale = "zh-CN";
            settings.AcceptLanguageList = "zh-CN,zh";
            settings.LogFile = Utils.GetTempPath("cef.log");
            settings.CachePath = Utils.GetTempPath("cef_cache");
            settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\CefSharp.BrowserSubprocess.exe");
            settings.LocalesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\locales\");
            settings.ResourcesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\cef_sharp\");
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true; // default is false, see https://github.com/cefsharp/CefSharp/issues/2359
            //settings.RemoteDebuggingPort = 51228;
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
                string errMsg = $"异常细节: {Environment.NewLine}{e.Exception}";
                ErrorCatchAction("UI Error", errMsg, e.Exception.GetType().FullName);
                Logging.Error(errMsg);
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref Program.exited) == 1)
            {
                string errMsg = $"异常细节: {Environment.NewLine}{e.ExceptionObject.ToString()}";
                ErrorCatchAction("non-UI Error", errMsg, e.ExceptionObject.GetType().FullName);
                Logging.Error(errMsg);
                Application.Exit();
            }
        }

        private static void ErrorCatchAction(string type, string errorMsg, string eType)
        {
            string title = $"Nacollector 意外错误 {eType}";
            MessageBox.Show(
                $"{title} 程序即将退出， {Environment.NewLine}{errorMsg}",
                $"{Application.ProductName} {type}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ReportErrorGithub(type + "\n" + errorMsg, eType);
        }

        public static void ReportErrorGithub(string body, string title = null)
        {
            DialogResult dr = MessageBox.Show("是否将错误信息提交到 GitHub？", "提交 issue 以反馈错误", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) return;

            title = $"[{Application.ProductName}] 意外错误 " + title;

            Process.Start(
                "https://github.com/qwqcode/Nacollector/issues/new"
                + $"?title={ HttpUtility.UrlEncode(title, Encoding.UTF8) }"
                + $"&body={ HttpUtility.UrlEncode(body + "\n\n---\n" + $"{Application.ProductName} v{Application.ProductVersion}", Encoding.UTF8) }");
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