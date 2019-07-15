using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorUpdateProvider
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string FormatFileSize(long b)
        {
            const int GB = 1024 * 1024 * 1024;
            const int MB = 1024 * 1024;
            const int KB = 1024;

            if (b / GB >= 1)
            {
                return Math.Round(b / (float)GB, 2) + "GB";
            }

            if (b / MB >= 1)
            {
                return Math.Round(b / (float)MB, 2) + "MB";
            }

            if (b / KB >= 1)
            {
                return Math.Round(b / (float)KB, 2) + "KB";
            }

            return b + "B";
        }
    }
}
