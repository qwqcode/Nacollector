using Fclp;
using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorUpdater
{
    public partial class UpdaterForm : Form
    {
        private static readonly string AppProcessName = "Nacollector";

        private static readonly string AppRootPath = Application.StartupPath; // 程序根目录
        private static List<string> DelFiles; // 待删除的文件
        private static List<string> PkgFiles; // 待解压压缩包
        private static List<string> CopyFiles; // 待复制的文件
        private static readonly List<string> ErrMsgs = new List<string> { }; // 错误信息

        public UpdaterForm(params string[] args)
        {
            var p = new FluentCommandLineParser();

            p.Setup<List<string>>('d', "del-files")
                .Callback(items => DelFiles = items)
                .SetDefault(new List<string> { })
                .WithDescription("待删除的文件");

            p.Setup<List<string>>('p', "pkgs")
                .Callback(items => PkgFiles = items)
                .SetDefault(new List<string> { })
                .WithDescription("待解压压缩包");

            p.Setup<List<string>>('c', "copy-files")
                .Callback(items => CopyFiles = items)
                .SetDefault(new List<string> { })
                .WithDescription("待复制的文件");

            p.SetupHelp("?", "help").Callback(text =>{
                MessageBox.Show(text);
                Environment.Exit(0);
            });

            var result = p.Parse(args);

            if (result.HasErrors)
                Environment.Exit(0);

            if (PkgFiles.Count == 0 && DelFiles.Count == 0 && CopyFiles.Count == 0)
                Environment.Exit(0);

            InitializeComponent();

            // 杀掉相关进程
            Program.KillProcess(AppProcessName);
        }

        private void UpdaterForm_Load(object sender, EventArgs e)
        {
            var thread = new Thread(BeginWork)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public void BeginWork()
        {
            // 删除文件/文件夹
            if (DelFiles.Count > 0)
            {
                SetProgressDesc("清理文件中...");
                SetCurrentProgram(0, "请稍后...");

                int deleted = 0;
                foreach (string path in DelFiles)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path); // 文件
                        else if (Directory.Exists(path))
                            Directory.Delete(path, true); // 文件夹
                        /*else
                            throw new Exception("路径不存在");*/
                            
                    }
                    catch (Exception e)
                    {
                        ErrMsgs.Add($"[删除路径: {path}] {e}");
                    }

                    deleted++;

                    double percentage = ((double)deleted / DelFiles.Count) * 100;
                    SetCurrentProgram(percentage, $"已完成 {string.Format("{0:0.##}", percentage)}% - {Path.GetFileName(path)}");
                }
            }
            
            // 解压压缩包
            if (PkgFiles.Count > 0)
            {
                SetProgressDesc("准备更新文件中...");
                SetCurrentProgram(0, "请稍后...");

                int extractedPkg = 0;
                foreach (string path in PkgFiles)
                {
                    try
                    {
                        using (ArchiveFile archiveFile = new ArchiveFile(path))
                        {
                            string ProcessText = $"正在解压 [{extractedPkg+1}/{PkgFiles.Count}] - {Path.GetFileName(path)}";

                            // 更新进度
                            SetProgressDesc(ProcessText);

                            int extracted = 0;
                            int entriesTotal = archiveFile.Entries.Count;

                            foreach (Entry entry in archiveFile.Entries)
                            {
                                // 提取文件
                                Console.WriteLine($"准备提取文件 {entry.FileName}");
                                if (!String.IsNullOrWhiteSpace(entry.FileName))
                                    entry.Extract(Program.GetPathBasedOn(AppRootPath, entry.FileName));
                                Console.WriteLine($"文件 {entry.FileName} 已提取");

                                extracted++;

                                double percentage = (entriesTotal > 0) ? (((double)extracted / entriesTotal) * 100) : 0;
                                SetCurrentProgram(percentage, $"已完成 {string.Format("{0:0.##}", percentage)}% - {Path.GetFileName(entry.FileName)} {Program.FormatBytes(long.Parse(entry.Size.ToString()))}");
                            }
                        }
                    }
                    catch (SevenZipException e)
                    {
                        ErrMsgs.Add($"[解包: {path}] {e}");
                    }
                    extractedPkg++;
                }
            }

            // 复制文件
            if (CopyFiles.Count > 0)
            {
                SetProgressDesc("应用文件中...");
                SetCurrentProgram(0, "请稍后...");

                int copyed = 0;
                foreach (string path in CopyFiles)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Copy(path, Path.Combine(AppRootPath, Path.GetFileName(path)), true); // 文件
                        else if (Directory.Exists(path))
                            Program.CopyFilesRecursively(path, AppRootPath); // 文件夹
                        /*else
                            throw new Exception("路径不存在");*/
                    }
                    catch (Exception e)
                    {
                        ErrMsgs.Add($"[复制路径: {path}] {e}");
                    }

                    copyed++;

                    double percentage = ((double)copyed / CopyFiles.Count) * 100;
                    SetCurrentProgram(percentage, $"已完成 {string.Format("{0:0.##}", percentage)}% - {Path.GetFileName(path)}");
                }
            }

            // 汇报错误
            if (ErrMsgs.Count > 0)
            {
                Program.ReportErrorAndExit(string.Join("\n", ErrMsgs.ToArray()));
            }

            LauchProgram();
            Environment.Exit(0);
        }

        /// <summary>
        /// 运行程序
        /// </summary>
        private void LauchProgram()
        {
            string path = Path.Combine(Application.StartupPath, $"{AppProcessName}.exe");
            if (!File.Exists(path))
            {
                string str = $"无法启动程序，未找到程序文件: {path}";
                Program.ReportErrorAndExit(str);
                return;
            }
            Process.Start(path);
        }

        /// <summary>
        /// 设置当前进展
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="percentageDesc"></param>
        public void SetCurrentProgram(double percentage, string percentageDesc)
        {
            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate { SetCurrentProgram(percentage, percentageDesc); });
                return;
            }

            if (percentage > 0)
            {
                UpdateProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                UpdateProgressBar.Style = ProgressBarStyle.Continuous;
            }
            else
            {
                UpdateProgressBar.Style = ProgressBarStyle.Marquee;
            }
            UpdatePercentageDesc.Text = percentageDesc;
        }

        /// <summary>
        /// 设置进度描述
        /// </summary>
        /// <param name="progressDesc"></param>
        public void SetProgressDesc(string progressDesc)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { SetProgressDesc(progressDesc); });
                return;
            }

            UpdateProgressDesc.Text = progressDesc;
        }
    }
}
