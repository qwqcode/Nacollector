using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorUtils.Settings
{
    [Serializable]
    public class SpiderCallback
    {
        public delegate string OnCookieGetterBrowserDelegate(CookieGetterSettings settings);
        public OnCookieGetterBrowserDelegate OnCookieGetterBrowser;

        public delegate void OnJsRunDelegate(string code);
        public OnJsRunDelegate OnJsRun;
    }
}
