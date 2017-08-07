using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClass, string lpWindowName);
        [DllImport("user32.dll",EntryPoint = "GetWindowRect")]
        private static extern IntPtr GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
        [DllImport("user32.dll", EntryPoint = "SetParent")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        private SystemInfo cm;
        private NetMonitorCore net;

        private void Form1_Load(object sender, EventArgs e)
        {
            InsetTaskBar();
            net = new NetMonitorCore();
            net.InitNetMonitorCore(2);
            timer1.Start();
            cm = new SystemInfo();
        }
        /// <summary>
        /// 嵌入任务栏
        /// 参考http://blog.csdn.net/autumn20080101/article/details/8813550  
        ///     http://www.cnblogs.com/sunthx/p/3470743.html
        /// </summary>
        private void InsetTaskBar()
        {
            IntPtr _Ptr = FindWindow("Shell_TrayWnd", null);//得到任务栏窗口句柄
            IntPtr _ChildHWnd = FindWindowEx(_Ptr, IntPtr.Zero, "ReBarWindow32", null);
            IntPtr _MSTaskHwnd = FindWindowEx(_ChildHWnd, IntPtr.Zero, "MSTaskSwWClass", null);
            Rect rect_ChildHWnd = new Rect();
            GetWindowRect(_MSTaskHwnd, out rect_ChildHWnd);
            rect_MSTaskHwnd = new Rect();
            GetWindowRect(_MSTaskHwnd, out rect_MSTaskHwnd);
            MoveWindow(_MSTaskHwnd, 0, 0, rect_MSTaskHwnd.Right - rect_MSTaskHwnd.Left - this.Width, rect_MSTaskHwnd.Bottom - rect_MSTaskHwnd.Top, true);
            SetParent(this.Handle, _ChildHWnd);
            this.Height = 35;
            //this.Width = 116;
            MoveWindow(this.Handle, rect_MSTaskHwnd.Right - rect_MSTaskHwnd.Left - this.Width, (rect_ChildHWnd.Bottom - rect_ChildHWnd.Top - this.Height) / 2, this.Width, this.Height, true);
        }
        private void RecoveryTaskBar()
        {
            IntPtr _Ptr = FindWindow("Shell_TrayWnd", null);//得到任务栏窗口句柄
            IntPtr _ChildHWnd = FindWindowEx(_Ptr, IntPtr.Zero, "ReBarWindow32", null);
            IntPtr _MSTaskHwnd = FindWindowEx(_ChildHWnd, IntPtr.Zero, "MSTaskSwWClass", null);
            MoveWindow(_MSTaskHwnd, 0, 0, rect_MSTaskHwnd.Right - rect_MSTaskHwnd.Left, rect_MSTaskHwnd.Bottom - rect_MSTaskHwnd.Top, true);
        }
        private Rect rect_MSTaskHwnd;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RecoveryTaskBar();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = "↑:" + net.GetNetSend();
            label2.Text = "↓:" + net.GetNetRecv();
            label3.Text = "C:" + cm.CpuLoad.ToString("0") + "%";
            label4.Text = "M:" + cm.MemoryUsage.ToString("0") + "%";
            string[] sysInfo = cm.CpuTemperature;
            label5.Text = "CT:" + sysInfo[0].ToString() + "℃";
            //label6.Text = "HT:" + sysInfo[1].ToString() + "℃";
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
       
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecoveryTaskBar();
            System.Environment.Exit(0);
        }
    }
}
