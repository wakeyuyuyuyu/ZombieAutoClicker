using System;
using System.Drawing;
using System.IO;
using PaddleOCRSharp;
using ZombieAutoClicker.Core.Interfaces;

namespace ZombieAutoClicker.Modules.Vision
{
    /// <summary>
    /// 复合视觉服务
    /// 整合OCR文字识别功能（图像模板匹配暂未实现）
    /// </summary>
    public class CompositeVisionService : IVisionService
    {
        private readonly PaddleOCREngine _ocrEngine;
        private readonly object _lock = new object();

        /// <summary>
        /// OCR识别结果事件
        /// </summary>
        public event Action<VisionOCRResult> OnOcrResult;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CompositeVisionService()
        {
            OCRModelConfig config = null;
            OCRParameter ocrParameter = new OCRParameter();
            _ocrEngine = new PaddleOCREngine(config, ocrParameter);
        }

        /// <summary>
        /// 识别目标中心点
        /// 根据目标字符串后缀自动选择识别方式：
        /// - .png/.jpg/.jpeg: 暂不支持图像模板匹配，返回null
        /// - 其他: OCR文字识别
        /// </summary>
        public System.Drawing.Point? FindTargetCenter(System.Drawing.Bitmap screenBmp, string target)
        {
            if (screenBmp == null) return null;

            // 如果目标为空，仅进行屏幕识别
            if (string.IsNullOrWhiteSpace(target))
            {
                RecognizeScreen(screenBmp);
                return null;
            }

            // 根据文件扩展名判断识别类型
            string extension = Path.GetExtension(target)?.ToLower();
            bool isImageFile = extension == ".png" || extension == ".jpg" || extension == ".jpeg";

            if (isImageFile)
            {
                // 图像模板匹配暂未实现
                Console.WriteLine($"[CompositeVisionService] 图像模板匹配功能暂未实现: {target}");
                return null;
            }
            else
            {
                // OCR文字识别
                return FindTextCenter(screenBmp, target);
            }
        }

        /// <summary>
        /// 仅进行屏幕识别并触发事件
        /// </summary>
        public void RecognizeScreen(System.Drawing.Bitmap screenBmp)
        {
            if (screenBmp == null) return;

            try
            {
                lock (_lock)
                {
                    var paddleResult = _ocrEngine.DetectText(screenBmp);
                    OnOcrResult?.Invoke(ConvertToCustomOCRResult(paddleResult));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompositeVisionService] OCR识别错误: {ex.Message}");
            }
        }

        /// <summary>
        /// OCR文字识别
        /// </summary>
        private System.Drawing.Point? FindTextCenter(System.Drawing.Bitmap screenBmp, string targetText)
        {
            try
            {
                lock (_lock)
                {
                    var paddleResult = _ocrEngine.DetectText(screenBmp);
                    OnOcrResult?.Invoke(ConvertToCustomOCRResult(paddleResult));

                    if (paddleResult != null && paddleResult.TextBlocks != null)
                    {
                        foreach (var block in paddleResult.TextBlocks)
                        {
                            string recognizedText = block.Text.Replace(" ", "");

                            if (recognizedText.Contains(targetText))
                            {
                                if (block.BoxPoints != null && block.BoxPoints.Count >= 4)
                                {
                                    int centerX = (block.BoxPoints[0].X + block.BoxPoints[2].X) / 2;
                                    int centerY = (block.BoxPoints[0].Y + block.BoxPoints[2].Y) / 2;
                                    return new System.Drawing.Point(centerX, centerY);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompositeVisionService] OCR文字识别错误: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 转换PaddleOCR结果到自定义格式
        /// </summary>
        private VisionOCRResult ConvertToCustomOCRResult(PaddleOCRSharp.OCRResult paddleResult)
        {
            if (paddleResult == null) return null;

            var result = new VisionOCRResult();
            if (paddleResult.TextBlocks != null)
            {
                var textBlocks = new VisionOCRTextBlock[paddleResult.TextBlocks.Count];
                for (int i = 0; i < paddleResult.TextBlocks.Count; i++)
                {
                    var paddleBlock = paddleResult.TextBlocks[i];
                    var customBlock = new VisionOCRTextBlock
                    {
                        Text = paddleBlock.Text,
                        BoxPoints = new System.Drawing.Point[paddleBlock.BoxPoints.Count]
                    };

                    for (int j = 0; j < paddleBlock.BoxPoints.Count; j++)
                    {
                        var point = paddleBlock.BoxPoints[j];
                        customBlock.BoxPoints[j] = new System.Drawing.Point(point.X, point.Y);
                    }

                    textBlocks[i] = customBlock;
                }
                result.TextBlocks = textBlocks;
            }

            return result;
        }
    }
}