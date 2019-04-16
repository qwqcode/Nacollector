using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorUtils
{
    public class Utils
    {
        private static IniFile _iniFile = null;
        private static string _tempPath = null;
        
        public static IniFile GetIniFile()
        {
            if (_iniFile == null)
            {
                _iniFile = new IniFile();
            }
            return _iniFile;
        }

        // return path to store temporary files
        public static string GetTempPath()
        {
            if (_tempPath == null)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "NacollectorTemp"));
                    // don't use "/", it will fail when we call explorer /select xxx/NacollectorTemp\xxx.log
                    _tempPath = Path.Combine(Application.StartupPath, "NacollectorTemp");
                }
                catch (Exception e)
                {
                    Logging.Error(e);
                    throw;
                }
            }
            return _tempPath;
        }

        // return a full path with filename combined which pointed to the temporary directory
        public static string GetTempPath(string filename)
        {
            return Path.Combine(GetTempPath(), filename);
        }

        public static bool GetIsUseIeProxyReq()
        {
            return GetIniFile().Read("ReqByIeProxy", "Request") == "1";
        }

        public static HttpResult GetPageByUrl(string reqUrl, Dictionary<string, string> headers=null, Dictionary<string, string> postData=null, Encoding encoding = null)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = reqUrl,
                Method = "GET",
                Timeout = 100000, // 连接超时时间 ms
                ReadWriteTimeout = 30000, // 写入Post数据超时时间
                IsToLower = false, // 得到的HTML代码是否转成小写,可选项默认转小写
                Cookie = "", // 字符串
                UserAgent = GlobalConstant.HttpReqUserAgent,
                Accept = "text/html, application/xhtml+xml, */*",
                ContentType = "text/html", //返回类型
                Allowautoredirect = true, // 是否根据301跳转
                MaximumAutomaticRedirections = 10
                //ProxyIp = "192.168.1.105", // 代理服务器ID
                //ProxyPwd = "123456", // 代理服务器密码
                //ProxyUserName = "administrator", // 代理服务器账户名
                //ResultType = ResultType.String, // 返回数据类型，是Byte还是String
            };

            // 请求头
            if (headers != null && headers.Count > 0)
            {
                foreach (var key in headers.Keys)
                {
                    if (key.ToLower() == "referer")
                    {
                        item.Referer = headers[key];
                    }
                    else if (key.ToLower() == "cookie")
                    {
                        item.Cookie = headers[key];
                    }
                    else
                    {
                        item.Header.Add(key, headers[key]);
                    }
                }
            }

            if (postData != null)
            {
                item.Method = "POST";
                item.Postdata = string.Join("&", postData.Select(d => Uri.EscapeDataString(d.Key) + "=" + Uri.EscapeDataString(d.Value)));
                item.PostDataType = PostDataType.String;
                item.ContentType = "application/x-www-form-urlencoded";
            }

            if (encoding != null)
                item.Encoding = encoding; // 例如 Encoding.GetEncoding("gb2312");
            else
                item.Encoding = Encoding.GetEncoding("UTF-8");
            
            // 是否使用IE代理
            if (GetIsUseIeProxyReq())
                item.ProxyIp = "ieproxy";

            HttpResult result = http.GetHtml(item);
            return result;
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="url">图片URL</param>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        public static void DownloadImgByUrl(string url, string dirPath, string fileName)
        {
            Dictionary<string, string> extLookup = new Dictionary<string, string>()
            {
                {"image/jpeg", "jpg"}, {"image/webp", "webp"}, {"image/gif", "gif"},
                {"image/png", "png"}, {"image/bmp", "bmp"}, {"image/x-icon", "ico"},
                {"image/tiff", "tif"}, {"image/svg+xml", "svg"}, {"image/x-xbitmap", "xbm"}
            };

            using (WebClient wc = new WebClient())
            {
                // 是否使用IE代理
                if (!GetIsUseIeProxyReq())
                    wc.Proxy = null;

                byte[] fileBytes = wc.DownloadData(url);
                string fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];

                if (fileType != null && extLookup.ContainsKey(fileType))
                {
                    string ext = extLookup[fileType];
                    File.WriteAllBytes(Path.Combine(dirPath, $"{fileName}.{ext}"), fileBytes);
                }
            }
        }
        
        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 获取HTML资源路径
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetHtmlResPath(string filename)
        {
            string path = Path.Combine(Application.StartupPath, "Resources/html_res", filename);

            if (!File.Exists(path))
            {
                string errorText = "由于文件丢失，程序界面无法正常显示"
                    + Environment.NewLine
                    + $"路径：{path}";

                Logging.Error(errorText);
                MessageBox.Show(errorText, "Nacollector 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return path;
        }

        /// <summary>
        /// Base 64 编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 合并两个 JSON 字符串
        /// </summary>
        /// <param name="json1"></param>
        /// <param name="json2"></param>
        /// <returns></returns>
        public static string MergeJsonString(string json1, string json2)
        {
            JArray obj;
            if (!string.IsNullOrEmpty(json1))
            {
                obj = JArray.Parse(json1);
            }
            else
            {
                obj = JArray.Parse("[]");
            }
            JArray obj2 = JArray.Parse($"[{json2}]");

            obj.Merge(obj2, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
            return obj.ToString();
        }
        
        /// <summary>
        /// 输入对话框
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static string InputDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                TopMost = true,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, Width = 460 };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 460 };
            Button confirmation = new Button() { Text = "完成", Left = 360, Width = 120, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        public static void ReleaseMemory(bool removePages)
        {
            // release any unused pages
            // making the numbers look good in task manager
            // this is totally nonsense in programming
            // but good for those users who care
            // making them happier with their everyday life
            // which is part of user experience
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            if (removePages)
            {
                // as some users have pointed out
                // removing pages from working set will cause some IO
                // which lowered user experience for another group of users
                //
                // so we do 2 more things here to satisfy them:
                // 1. only remove pages once when configuration is changed
                // 2. add more comments here to tell users that calling
                //    this function will not be more frequent than
                //    IM apps writing chat logs, or web browsers writing cache files
                //    if they're so concerned about their disk, they should
                //    uninstall all IM apps and web browsers
                //
                // please open an issue if you're worried about anything else in your computer
                // no matter it's GPU performance, monitor contrast, audio fidelity
                // or anything else in the task manager
                // we'll do as much as we can to help you
                //
                // just kidding
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
                                         (UIntPtr)0xFFFFFFFF,
                                         (UIntPtr)0xFFFFFFFF);
            }
        }


        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process,
            UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);

        public static string FormatBytes(long bytes)
        {
            const long K = 1024L;
            const long M = K * 1024L;
            const long G = M * 1024L;
            const long T = G * 1024L;
            const long P = T * 1024L;
            const long E = P * 1024L;

            if (bytes >= P * 990)
                return (bytes / (double)E).ToString("F5") + "EiB";
            if (bytes >= T * 990)
                return (bytes / (double)P).ToString("F5") + "PiB";
            if (bytes >= G * 990)
                return (bytes / (double)T).ToString("F5") + "TiB";
            if (bytes >= M * 990)
            {
                return (bytes / (double)G).ToString("F4") + "GiB";
            }
            if (bytes >= M * 100)
            {
                return (bytes / (double)M).ToString("F1") + "MiB";
            }
            if (bytes >= M * 10)
            {
                return (bytes / (double)M).ToString("F2") + "MiB";
            }
            if (bytes >= K * 990)
            {
                return (bytes / (double)M).ToString("F3") + "MiB";
            }
            if (bytes > K * 2)
            {
                return (bytes / (double)K).ToString("F1") + "KiB";
            }
            return bytes.ToString() + "B";
        }

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
                Logging.LogUsefulException(e);
                return null;
            }
        }

        public static bool IsWinVistaOrHigher()
        {
            return Environment.OSVersion.Version.Major > 5;
        }

        // See: https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx
        public static bool IsSupportedRuntimeVersion()
        {
            /*
             * +-----------------------------------------------------------------+----------------------------+
             * | Version                                                         | Value of the Release DWORD |
             * +-----------------------------------------------------------------+----------------------------+
             * | .NET Framework 4.6.2 installed on Windows 10 Anniversary Update | 394802                     |
             * | .NET Framework 4.6.2 installed on all other Windows OS versions | 394806                     |
             * +-----------------------------------------------------------------+----------------------------+
             */
            const int minSupportedRelease = 394802;

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
    }
}
