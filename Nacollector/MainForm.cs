using CefSharp;
using Nacollector.Browser;
using Nacollector.JsActions;
using Nacollector.Ui;
using System;
using System.Windows.Forms;

namespace Nacollector
{
    public partial class MainForm : FormBase
    {
        public static MainForm _mainForm;
        public static CrBrowser crBrowser;
        public static CrDownloads crDownloads;
        public static CrBrowserCookieGetter crCookieGetter;

        public MainForm()
        {
            _mainForm = this;

            InitializeComponent(); // 初始化控件
            InitBrowser(); // 初始化浏览器
        }

        private void InitBrowser()
        {
            // 初始化内置浏览器
#warning 记得修改
#if DEBUG
            string htmlPath = "nacollector://html_res/index.html";
#else
            string htmlPath = "http://127.0.0.1:8080";
#endif
            crBrowser = new CrBrowser(this, htmlPath);

            // Need Update: https://github.com/cefsharp/CefSharp/issues/2246

            //For legacy biding we'll still have support for
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            crBrowser.GetBrowser().RegisterAsyncJsObject("AppAction", new AppAction(this, crBrowser));
            crBrowser.GetBrowser().RegisterAsyncJsObject("TaskController", new TaskControllerAction(this, crBrowser));

            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(SplashScreen_Browser_FrameLoadEnd); // 浏览器初始化完毕时执行

            crDownloads = new CrDownloads(crBrowser);
            
            ContentPanel.Controls.Add(crBrowser.GetBrowser());

            crCookieGetter = new CrBrowserCookieGetter();
        }

        public CrBrowser GetCrBrowser()
        {
            return crBrowser;
        }

        public CrBrowserCookieGetter GetCrCookieGetter()
        {
            return crCookieGetter;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 是否退出弹窗
            string dialogTxt = "确定退出 Nacollector？";

            // 下载任务数统计
            int downloadingTaskNum = Convert.ToInt32(crBrowser.EvaluateScript("Downloads.countDownloadingTask();", 0, TimeSpan.FromSeconds(3)).GetAwaiter().GetResult());
            if (downloadingTaskNum > 0)
                dialogTxt = $"有 {downloadingTaskNum} 个下载任务仍在继续！确定结束下载并关闭程序？";

            DialogResult dr = MessageBox.Show(dialogTxt, "退出 Nacollector", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}