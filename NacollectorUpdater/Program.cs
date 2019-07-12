using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace NacollectorUpdater
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
		private static int exited;

        [STAThread]
        private static void Main(params string[] args)
        {
            // 检测 .NET Framework 版本
            if (!IsSupportedRuntimeVersion())
            {
                MessageBox.Show("当前 .NET Framework 版本过低，请升级至 4.6.2 或更新版本",
                $"{Application.ProductName} 无法运行", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Process.Start(
                    "http://dotnetsocial.cloudapp.net/GetDotnet?tfm=.NETFramework,Version=v4.6.2");
                return;
            }

            using (Mutex mutex = new Mutex(false, $"Global\\Naupdater_{Application.StartupPath.GetHashCode()}"))
            {
#if !DEBUG
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (!mutex.WaitOne(0, false))
                {
                    Process[] oldProcesses = Process.GetProcessesByName(Application.ProductName);
                    if (oldProcesses.Length > 0)
                    {
                        Process oldProcess = oldProcesses[0];
                    }
                    MessageBox.Show($"{Application.ProductName} 已在运行，无需重复打开");
                    return;
                }

                Directory.SetCurrentDirectory(Application.StartupPath);

                // 启动主界面
                Application.Run(new UpdaterForm(args));
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref Program.exited) == 1)
            {
                string errMsg = $"异常细节: {Environment.NewLine}{e.Exception}";
                ErrorCatchAction("UI Error", errMsg, e.Exception.GetType().FullName);
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Interlocked.Increment(ref Program.exited) == 1)
            {
                string errMsg = $"异常细节: {Environment.NewLine}{e.ExceptionObject.ToString()}";
                ErrorCatchAction("non-UI Error", errMsg, e.ExceptionObject.GetType().FullName);
                Application.Exit();
            }
        }

        private static void ErrorCatchAction(string type, string errorMsg, string eType)
        {
            string title = $"{eType}";
            MessageBox.Show(
                $"{title} 程序即将退出，请发起 issue 来反馈，谢谢 {Environment.NewLine}{errorMsg}",
                $"{Application.ProductName} {type}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ReportErrorGithub(type + "\n" + errorMsg, title);
        }

        #region Utils
        public static RegistryKey OpenRegKey(string name, bool writable, RegistryHive hive = RegistryHive.CurrentUser)
        {
            // we are building x86 binary for both x86 and x64, which will
            // cause problem when opening registry key
            // detect operating system instead of CPU
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));
            try
            {
                RegistryKey userKey = RegistryKey.OpenBaseKey(hive,
                        Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
                    .OpenSubKey(name, writable);
                return userKey;
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show("OpenRegKey: " + ae.ToString());
                return null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        // See: https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
        public static bool IsSupportedRuntimeVersion()
        {
            const int minSupportedRelease = 378389; // NET 4.5

            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (var ndpKey = OpenRegKey(subkey, false, RegistryHive.LocalMachine))
            {
                if (ndpKey?.GetValue("Release") != null)
                {
                    var releaseKey = (int)ndpKey.GetValue("Release");

                    if (releaseKey >= minSupportedRelease)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ReportErrorAndExit(string errMsg, bool githubReport = true, string githubBody = null)
        {
            if (Interlocked.Increment(ref exited) == 1)
            {
                MessageBox.Show($"{errMsg}", $"Nacollector 升级失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (githubReport) ReportErrorGithub(githubBody ?? errMsg);

                ExitApp();
            }
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

        public static void ExitApp()
        {
            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }

        public static string FormatBytes(long bytes)
        {
            const long K = 1024L;
            const long M = K * 1024L;
            const long G = M * 1024L;
            const long T = G * 1024L;
            const long P = T * 1024L;
            const long E = P * 1024L;

            if (bytes >= P * 990)
                return (bytes / (double)E).ToString("F5") + "EB";
            if (bytes >= T * 990)
                return (bytes / (double)P).ToString("F5") + "PB";
            if (bytes >= G * 990)
                return (bytes / (double)T).ToString("F5") + "TB";
            if (bytes >= M * 990)
            {
                return (bytes / (double)G).ToString("F4") + "GB";
            }
            if (bytes >= M * 100)
            {
                return (bytes / (double)M).ToString("F1") + "MB";
            }
            if (bytes >= M * 10)
            {
                return (bytes / (double)M).ToString("F2") + "MB";
            }
            if (bytes >= K * 990)
            {
                return (bytes / (double)M).ToString("F3") + "MB";
            }
            if (bytes > K * 2)
            {
                return (bytes / (double)K).ToString("F1") + "KB";
            }
            return bytes.ToString() + "B";
        }

        /// <summary>
        /// 获取 基于A 的 B 路径
        /// </summary>
        /// <param name="basePath">A</param>
        /// <param name="fileName">B</param>
        /// <returns>结果会始终包含 A</returns>
        public static string GetPathBasedOn(string basePath, string fileName)
        {
            string fullPath = GetFullPath(
                startPath: @"\",
                partialPath: fileName
            ); // 处于 @"X:\" 解析相对路径为绝对路径

            fullPath = fullPath.Substring(Path.GetPathRoot(fullPath).Length); // 剔除 @"X:\", @"\", "/"

            return Path.GetFullPath(Path.Combine(basePath, fullPath));
        }

        /// <summary>
        /// 处于指定路径下 解析相对路径为绝对路径
        /// </summary>
        /// <param name="startPath"></param>
        /// <param name="partialPath"></param>
        /// <returns></returns>
        public static string GetFullPath(string startPath, string partialPath)
        {
            partialPath = partialPath.TrimStart('.', '\\', '/'); // 剔除开头字符

            string oldStartPath = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(startPath);

                if (String.IsNullOrWhiteSpace(partialPath))
                    return Path.GetFullPath(".");

                return Path.GetFullPath(partialPath);
            }
            finally { Directory.SetCurrentDirectory(oldStartPath); }
        }

        public static void CopyFilesRecursively(string source, string target)
        {
            DirectoryInfo diSource = new DirectoryInfo(source);
            DirectoryInfo diTarget = new DirectoryInfo(target);

            _CopyFilesRecursively(diSource, diTarget);
        }

        public static void _CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                _CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true); // 覆盖文件
        }
        #endregion
    }
}
