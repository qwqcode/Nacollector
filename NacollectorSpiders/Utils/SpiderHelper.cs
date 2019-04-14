using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorSpiders.Utils
{
    class SpiderHelper
    {
        private static string _tempPath = null;

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
                    //Logging.Error(e);
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
    }
}
