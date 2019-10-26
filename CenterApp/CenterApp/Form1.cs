using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// TODO: add this to Windows Shell Extension
//       try to intercept all executibles and run them through this command
//      https://www.howtogeek.com/107965/how-to-add-any-application-shortcut-to-windows-explorers-context-menu/
//
namespace CenterApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int GetTopWindow();
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [DllImport("User32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("User32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint flags);


        [DllImport("User32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool IsWindowVisible(IntPtr hWnd);
        public const int WM_COMMAND = 0x0112;
        public const int WM_CLOSE = 0xF060;

        private static void Start(string fName)
        {
            const int MAX_WAIT = 200;
            int counter = 0;
            using (Process p = Process.Start(fName))
            {
                while ((p.MainWindowHandle == IntPtr.Zero) || !IsWindowVisible(p.MainWindowHandle))
                {
                    System.Threading.Thread.Sleep(10);
                    p.Refresh();
                    counter++;
                    if (counter > MAX_WAIT)
                        return;
                }
                p.WaitForInputIdle(10000);
                //MoveWindow(p.MainWindowHandle, 0, 0, 400, 400, true);

                // Calculate center of window
                RECT r = new RECT();
                GetWindowRect(p.MainWindowHandle, out r);

                SetWindowPos(p.MainWindowHandle, (IntPtr)0, (1920/2) - (r.Width/2), (1080/2) - (r.Height/2), 0, 0, 0x0040 | 0x0004 | 0x0001);
            }
        }

        private static void StartCallback(IAsyncResult ar)
        {
            System.Runtime.Remoting.Messaging.AsyncResult result = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
            Action<string> del = (Action<string>)result.AsyncDelegate;
            try
            {
                del.EndInvoke(ar);
            }
            catch { }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();

            if (diag.ShowDialog() == DialogResult.OK)
            {
                new Action<string>(Start).BeginInvoke(diag.FileName, new AsyncCallback(StartCallback), null);
            }

            diag.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

        public int X
        {
            get { return Left; }
            set { Right -= (Left - value); Left = value; }
        }

        public int Y
        {
            get { return Top; }
            set { Bottom -= (Top - value); Top = value; }
        }

        public int Height
        {
            get { return Bottom - Top; }
            set { Bottom = value + Top; }
        }

        public int Width
        {
            get { return Right - Left; }
            set { Right = value + Left; }
        }

        public System.Drawing.Point Location
        {
            get { return new System.Drawing.Point(Left, Top); }
            set { X = value.X; Y = value.Y; }
        }

        public System.Drawing.Size Size
        {
            get { return new System.Drawing.Size(Width, Height); }
            set { Width = value.Width; Height = value.Height; }
        }

        public static implicit operator System.Drawing.Rectangle(RECT r)
        {
            return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator RECT(System.Drawing.Rectangle r)
        {
            return new RECT(r);
        }

        public static bool operator ==(RECT r1, RECT r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(RECT r1, RECT r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(RECT r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        public override bool Equals(object obj)
        {
            if (obj is RECT)
                return Equals((RECT)obj);
            else if (obj is System.Drawing.Rectangle)
                return Equals(new RECT((System.Drawing.Rectangle)obj));
            return false;
        }

        public override int GetHashCode()
        {
            return ((System.Drawing.Rectangle)this).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
        }
    }

}