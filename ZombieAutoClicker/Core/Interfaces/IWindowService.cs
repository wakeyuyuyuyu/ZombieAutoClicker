using System.Drawing;

namespace ZombieAutoClicker.Core.Interfaces
{
    /// <summary>
    /// 窗口控制服务接口
    /// 提供窗口查找、截图和鼠标控制功能
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// 查找窗口句柄
        /// </summary>
        /// <param name="windowTitle">窗口标题</param>
        /// <returns>窗口句柄</returns>
        IntPtr FindWindow(string windowTitle);

        /// <summary>
        /// 截取指定窗口的画面
        /// </summary>
        /// <param name="windowTitle">窗口标题</param>
        /// <param name="windowTopLeft">窗口左上角坐标</param>
        /// <returns>窗口截图</returns>
        Bitmap CaptureWindow(string windowTitle, out Point windowTopLeft);

        /// <summary>
        /// 模拟鼠标左键点击绝对坐标
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        void ClickAbsolute(int x, int y);

        /// <summary>
        /// 将窗口相对坐标转换为屏幕绝对坐标并点击
        /// </summary>
        /// <param name="relativePoint">窗口相对坐标</param>
        /// <param name="windowTopLeft">窗口左上角坐标</param>
        void ClickRelativePoint(Point relativePoint, Point windowTopLeft);

        /// <summary>
        /// 设置窗口为前台窗口
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>是否成功</returns>
        bool SetForegroundWindow(IntPtr hWnd);
    }
}