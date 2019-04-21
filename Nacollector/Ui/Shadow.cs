using CefSharp;
using CefSharp.WinForms;
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
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public struct Margins
        {
            public int Bottom;
            public int Left;
            public int Right;
            public int Top;
        }
        
        public static bool DropShadowToWindow(IntPtr hwnd)
        {
            try
            {
                int val = 2;
                int ret1 = DwmSetWindowAttribute(hwnd, 2, ref val, 4);

                if (ret1 == 0)
                {
                    Margins m = new Margins { Bottom = 1, Left = 0, Right = 0, Top = 0 };
                    int ret2 = DwmExtendFrameIntoClientArea(hwnd, ref m);
                    return ret2 == 0;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }
        }
    }
}
