using CefSharp;
using Nacollector.Browser;
using Nacollector.TaskManager;
using Nacollector.Ui;
using System;
using System.Windows.Forms;
using NacollectorUtils.Settings;
using System.Collections.Generic;

namespace Nacollector
{
    public partial class MainForm : FormBase
    {
        public MainForm _mainForm;
        public SplashScreen _splashScreen;
        public CrBrowser crBrowser;
        public CookieGetterBrowser cookieGetterBrowser;
        public TaskRunner taskRunner;

        public MainForm()
        {
            _mainForm = this;

            this.Opacity = 0;
            _splashScreen = new SplashScreen(this);
            _splashScreen.Show();

            InitializeComponent(); // 初始化控件

            InitBrowser();
            InitTaskRunner();
        }

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        private void InitBrowser()
        {
#warning 记得修改
#if DEBUG
            string htmlPath = "nacollector://html_res/index.html";
#else
            string htmlPath = "http://127.0.0.1:8080";
#endif
            crBrowser = new CrBrowser(this, htmlPath);
            
            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>((obj, args) => {
                _splashScreen.Hide();
                this.Invoke((MethodInvoker)delegate
                {
                    this.Opacity = 1;
                });
            }); // 浏览器初始化完毕时执行
            
            ContentPanel.Controls.Add(crBrowser.GetBrowser());
            cookieGetterBrowser = new CookieGetterBrowser(this);
        }

        /// <summary>
        /// 初始化任务管理器
        /// </summary>
        private void InitTaskRunner()
        {
            taskRunner = new TaskRunner(this, new SpiderCallback
            {
                OnCookieGetterBrowser = new SpiderCallback.OnCookieGetterBrowserDelegate((CookieGetterSettings cgSettings) =>
                {
                    cookieGetterBrowser.CreateNew(cgSettings.StartUrl, cgSettings.EndUrlReg, cgSettings.Caption);

                    if (cgSettings.InputAutoCompleteConfig != null)
                    {
                        cookieGetterBrowser.UseInputAutoComplete(
                            (string)cgSettings.InputAutoCompleteConfig["pageUrlReg"],
                            (List<string>)cgSettings.InputAutoCompleteConfig["inputElemCssSelectors"]
                        );
                    }

                    cookieGetterBrowser.BeginWork();

                    return cookieGetterBrowser.GetCookieStr();
                }),

                OnJsRun = new SpiderCallback.OnJsRunDelegate((code) =>
                {
                    crBrowser.RunJS(code);
                })
            });
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