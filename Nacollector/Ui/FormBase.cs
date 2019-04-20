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
    /// <summary>
    /// Based on http://customerborderform.codeplex.com/
    /// </summary>
    public partial class FormBase : Form
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
         );
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        public void DecorationMouseDown(HitTestValues hit, Point p)
        {
            NativeMethods.ReleaseCapture();
            var pt = new POINTS { X = (short)p.X, Y = (short)p.Y };
            NativeMethods.SendMessage(Handle, (int)WindowMessages.WM_NCLBUTTONDOWN, (int)hit, pt);
        }

        public void DecorationMouseDown(HitTestValues hit)
        {
            DecorationMouseDown(hit, MousePosition);
        }

        public void DecorationMouseUp(HitTestValues hit, Point p)
        {
            NativeMethods.ReleaseCapture();
            var pt = new POINTS { X = (short)p.X, Y = (short)p.Y };
            NativeMethods.SendMessage(Handle, (int)WindowMessages.WM_NCLBUTTONUP, (int)hit, pt);
        }

        public void DecorationMouseUp(HitTestValues hit)
        {
            DecorationMouseUp(hit, MousePosition);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected static int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        public void ShowSystemMenu(Point pos)
        {
            NativeMethods.SendMessage(Handle, (int)WindowMessages.WM_SYSMENU, 0, MakeLong((short)pos.X, (short)pos.Y));
        }

        protected override void WndProc(ref Message m)
        {
            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case 174: // ignore magic message number
                    {
                        break;
                    }
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }

        private void SetWindowRegion(IntPtr hwnd, int left, int top, int right, int bottom)
        {
            var hrg = new HandleRef((object)this, NativeMethods.CreateRectRgn(0, 0, 0, 0));
            var r = NativeMethods.GetWindowRgn(hwnd, hrg.Handle);
            RECT box;
            NativeMethods.GetRgnBox(hrg.Handle, out box);
            if (box.left != left || box.top != top || box.right != right || box.bottom != bottom)
            {
                var hr = new HandleRef((object)this, NativeMethods.CreateRectRgn(left, top, right, bottom));
                NativeMethods.SetWindowRgn(hwnd, hr.Handle, NativeMethods.IsWindowVisible(hwnd));
            }
        }

        public FormWindowState MinMaxState
        {
            get
            {
                var s = NativeMethods.GetWindowLong(Handle, NativeConstants.GWL_STYLE);
                var max = (s & (int)WindowStyle.WS_MAXIMIZE) > 0;
                if (max) return FormWindowState.Maximized;
                var min = (s & (int)WindowStyle.WS_MINIMIZE) > 0;
                if (min) return FormWindowState.Minimized;
                return FormWindowState.Normal;
            }
        }

        private void WmWindowPosChanged(ref Message m)
        {
            DefWndProc(ref m);
            UpdateBounds();
            var pos = (WINDOWPOS)Marshal.PtrToStructure(m.LParam, typeof(WINDOWPOS));
            SetWindowRegion(m.HWnd, 0, 0, pos.cx, pos.cy);
            m.Result = NativeConstants.TRUE;
        }

        private void WmNCCalcSize(ref Message m)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowmessages/wm_nccalcsize.asp
            // http://groups.google.pl/groups?selm=OnRNaGfDEHA.1600%40tk2msftngp13.phx.gbl

            var r = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
            var max = MinMaxState == FormWindowState.Maximized;

            if (max)
            {
                var x = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXSIZEFRAME);
                var y = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYSIZEFRAME);
                var p = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXPADDEDBORDER);
                var w = x + p;
                var h = y + p;

                r.left += w;
                r.top += h;
                r.right -= w;
                r.bottom -= h;

                var appBarData = new APPBARDATA();
                appBarData.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
                var autohide = (NativeMethods.SHAppBarMessage(NativeConstants.ABM_GETSTATE, ref appBarData) & NativeConstants.ABS_AUTOHIDE) != 0;
                if (autohide) r.bottom -= 1;

                Marshal.StructureToPtr(r, m.LParam, true);
            }

            m.Result = IntPtr.Zero;
        }

        private void WmNCPaint(ref Message msg)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/pantdraw_8gdw.asp
            // example in q. 2.9 on http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c41c.aspx#q1026q

            // The WParam contains handle to clipRegion or 1 if entire window should be repainted
            //PaintNonClientArea(msg.HWnd, (IntPtr)msg.WParam);

            // we handled everything
            msg.Result = NativeConstants.TRUE;
        }

        private void WmSetText(ref Message msg)
        {
            // allow the system to receive the new window title
            DefWndProc(ref msg);

            // repaint title bar
            //PaintNonClientArea(msg.HWnd, (IntPtr)1);
        }

        private void WmNCActivate(ref Message msg)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/windows/windowreference/windowmessages/wm_ncactivate.asp

            bool active = (msg.WParam == NativeConstants.TRUE);

            if (MinMaxState == FormWindowState.Minimized)
                DefWndProc(ref msg);
            else
            {
                // repaint title bar
                //PaintNonClientArea(msg.HWnd, (IntPtr)1);

                // allow to deactivate window
                msg.Result = NativeConstants.TRUE;
            }
        }

        public FormWindowState ToggleMaximize()
        {
            return WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        public static bool DropShadowToWindow(IntPtr handle)
        {
            try
            {
                var v = 2;
                DwmSetWindowAttribute(handle, 2, ref v, 4);
                MARGINS margins = new MARGINS()
                {
                    bottomHeight = 1,
                    leftWidth = 0,
                    rightWidth = 0,
                    topHeight = 0
                };
                int ret2 = DwmExtendFrameIntoClientArea(handle, ref margins);
                return ret2 == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
