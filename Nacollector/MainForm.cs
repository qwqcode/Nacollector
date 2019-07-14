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
#if !DEBUG
            string htmlPath = "nacollector://html_res/index.html";
#else
            string htmlPath = "http://127.0.0.1:8080";
#endif
            crBrowser = new CrBrowser(this, htmlPath);
            
            crBrowser.GetBrowser().FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>((obj, e) => {
                string url = e.Frame.Url;
                if (crBrowser.CheckIsAppUrl(url))
                {
                    // 获取并前端执行表单生成代码
                    _mainForm.BeginInvoke((MethodInvoker)delegate
                    {
                        var spiderDomain = taskRunner.GetLoadSpiderDomain();
                        crBrowser.RunJS(spiderDomain.GetFormGenJsCode());
                        taskRunner.UnloadSpiderDomain();
                    });
                }
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

        }
    }
}