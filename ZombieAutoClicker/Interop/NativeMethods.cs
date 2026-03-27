using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZombieAutoClicker.Interop
{
    /// <summary>
    /// 封装 Windows 底层 API，用于窗口捕获和鼠标控制
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// 截取指定标题的窗口画面
        /// </summary>
        public static Bitmap? CaptureWindow(string windowTitle, out Point windowTopLeft)
        {
            windowTopLeft = Point.Empty;
            IntPtr hWnd = FindWindow(null, windowTitle);
            if (hWnd == IntPtr.Zero) return null;

            GetWindowRect(hWnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0) return null;
            windowTopLeft = new Point(rect.Left, rect.Top);

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
            }
            return bmp;
        }

        /// <summary>
        /// 模拟鼠标左键点击绝对坐标
        /// </summary>
        public static void ClickAbsolute(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}