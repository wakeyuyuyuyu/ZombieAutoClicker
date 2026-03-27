using System;
using System.Drawing;

namespace ZombieAutoClicker.Core.Interfaces
{
    /// <summary>
    /// 视觉识别服务接口
    /// 提供文字识别和图像模板匹配功能
    /// </summary>
    public interface IVisionService
    {
        /// <summary>
        /// OCR识别结果事件
        /// </summary>
        event Action<VisionOCRResult> OnOcrResult;

        /// <summary>
        /// 识别目标中心点
        /// </summary>
        /// <param name="screenBmp">屏幕截图</param>
        /// <param name="target">目标文字或图片路径</param>
        /// <returns>目标中心点坐标，如果未找到则返回null</returns>
        Point? FindTargetCenter(Bitmap screenBmp, string target);

        /// <summary>
        /// 仅进行屏幕识别并触发事件，不进行目标查找
        /// </summary>
        /// <param name="screenBmp">屏幕截图</param>
        void RecognizeScreen(Bitmap screenBmp);
    }

    /// <summary>
    /// OCR识别结果（自定义类型，避免与PaddleOCRSharp.OCRResult冲突）
    /// </summary>
    public class VisionOCRResult
    {
        public VisionOCRTextBlock[]? TextBlocks { get; set; }
    }

    /// <summary>
    /// OCR文本块
    /// </summary>
    public class VisionOCRTextBlock
    {
        public string? Text { get; set; }
        public Point[]? BoxPoints { get; set; }
    }
}