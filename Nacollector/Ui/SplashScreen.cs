using CefSharp;
using CefSharp.Wpf;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.JsActions;
using Nacollector.Ui;
using NacollectorUtils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Nacollector.Ui
{
    public partial class FormBase : Form
    {
        protected Form startingForm;

        /// <summary>
        /// 设置程序启动画面
        /// </summary>
        protected void SetIsStarting(bool isStarting)
        {
            if (this.InvokeRequired) { this.Invoke(new SetIsStartingDelegate(SetIsStarting), new object[] { isStarting }); return; }

            if (isStarting)
            {
                startingForm = new Form
                {
                    Size = new Size(640, 400),
                    TopMost = true,
                    ControlBox = false,
                    ShowInTaskbar = false,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    BackgroundImage = Properties.Resources.StartingImg,
                    BackColor = ColorTranslator.FromHtml("#282c34")
                };
                startingForm.Show();
                DropShadowToWindow(startingForm.Handle);
                this.Opacity = 0;
            }
            else
            {
                startingForm.Hide();
                this.Opacity = 1;
            }

        }
        protected delegate void SetIsStartingDelegate(bool isStarting);

        protected void SplashScreen_MainForm_Load(object sender, EventArgs e)
        {
            // 程序启动画面
            BeginInvoke((MethodInvoker)delegate
            {
                DropShadowToWindow(Handle);
            });
            SetIsStarting(true);
        }

        protected void SplashScreen_Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            // 关闭程序启动画面
            SetIsStarting(false);
        }
    }
}
