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
    /// <summary>
    /// Based on http://customerborderform.codeplex.com/
    /// </summary>
    public partial class FormBase : Form
    {
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

            // 启动 Drop Shaow
            this.BeginInvoke((MethodInvoker)delegate
            {
                DropShadowToWindow(this.Handle);
            });
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
                case (int)WindowMessages.WM_NCCALCSIZE:
                    {
                        // Provides new coordinates for the window client area.
                        WmNCCalcSize(ref m);
                        break;
                    }
                case (int)WindowMessages.WM_WINDOWPOSCHANGED:
                    {
                        WmWindowPosChanged(ref m);
                        break;
                    }
                case (int)WindowMessages.WM_NCPAINT:
                    {
                        // Here should all our painting occur, but...
                        WmNCPaint(ref m);
                        break;
                    }
                case (int)WindowMessages.WM_NCACTIVATE:
                    {
                        // ... WM_NCACTIVATE does some painting directly 
                        // without bothering with WM_NCPAINT ...
                        WmNCActivate(ref m);
                        break;
                    }
                case (int)WindowMessages.WM_SETTEXT:
                    {
                        // ... and some painting is required in here as well
                        WmSetText(ref m);
                        break;
                    }
                
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
            m.Result = NativeConstants.TRUE;
        }

        private void WmNCCalcSize(ref Message m)
        {
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

        private void WmNCPaint(ref Message m)
        {
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/pantdraw_8gdw.asp
            // example in q. 2.9 on http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c41c.aspx#q1026q

            // The WParam contains handle to clipRegion or 1 if entire window should be repainted
            //PaintNonClientArea(msg.HWnd, (IntPtr)msg.WParam);

            // we handled everything

            // msg.Result = NativeConstants.FALSE; // 会让 DropShadow 丢失

            base.WndProc(ref m);
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
    }
}
