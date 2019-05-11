using CefSharp;
using CefSharp.WinForms;
using Nacollector.Browser;
using Nacollector.Browser.Handler;
using Nacollector.Ui;
using NacollectorUtils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Nacollector.Ui
{
    public class SplashScreen
    {
        private Form parentForm;
        protected Form startingForm;

        public SplashScreen(Form parentForm)
        {
            this.parentForm = parentForm;
        }
        
        public void Show()
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
            FormBase.DropShadowToWindow(startingForm.Handle);
            startingForm.Show();
        }

        public void Hide()
        {
            startingForm.Invoke((MethodInvoker)delegate
            {
                startingForm.Hide();
            });
        }
    }
}
