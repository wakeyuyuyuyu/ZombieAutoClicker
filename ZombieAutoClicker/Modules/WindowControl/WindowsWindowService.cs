using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using ZombieAutoClicker.Core.Interfaces;

namespace ZombieAutoClicker.Modules.WindowControl
{
    /// <summary>
    /// Windows窗口控制服务
    /// 封装Windows API，提供窗口查找、截图和鼠标控制功能
    /// </summary>
    public class WindowsWindowService : IWindowService
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// 查找窗口句柄
        /// </summary>
        public IntPtr FindWindow(string windowTitle)
        {
            return FindWindow(null, windowTitle);
        }

        /// <summary>
        /// 截取指定窗口的画面
        /// </summary>
        public Bitmap? CaptureWindow(string windowTitle, out Point windowTopLeft)
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
        public void ClickAbsolute(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 将窗口相对坐标转换为屏幕绝对坐标并点击
        /// </summary>
        public void ClickRelativePoint(Point relativePoint, Point windowTopLeft)
        {
            int absoluteX = windowTopLeft.X + relativePoint.X;
            int absoluteY = windowTopLeft.Y + relativePoint.Y;
            ClickAbsolute(absoluteX, absoluteY);
        }

        /// <summary>
        /// 设置窗口为前台窗口
        /// </summary>
        bool IWindowService.SetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }
    }
}