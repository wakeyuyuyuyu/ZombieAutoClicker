using System;
using System.Drawing;
using PaddleOCRSharp;

namespace ZombieAutoClicker.Services
{
    /// <summary>
    /// 提供实时的屏幕文字识别服务 (基于 PaddleOCR 深度学习文字识别)
    /// </summary>
    public static class VisionService
    {
        // 静态 AI 引擎实例，保持在内存中常驻，避免每次截图都重新加载模型
        private static PaddleOCREngine _ocrEngine;

        /// <summary>
        /// 新增：OCR识别结果事件
        /// </summary>
        public static event Action<OCRResult> OnOcrResult;


        /// <summary>
        /// 初始化 AI 引擎
        /// </summary>
        private static void InitOcrEngine()
        {
            if (_ocrEngine == null)
            {
                OCRModelConfig config = null;
                OCRParameter ocrParameter = new OCRParameter();
                _ocrEngine = new PaddleOCREngine(config, ocrParameter);
            }
        }

        /// <summary>
        /// 飞桨 PaddleOCR 文字识别：极高准确率，无视背景干扰
        /// </summary>
        public static System.Drawing.Point? FindTextCenter(Bitmap screenBmp, string targetText)
        {
            if (screenBmp == null) return null; // 允许 targetText 为空，以便仅进行屏幕识别

            try
            {
                InitOcrEngine();
                OCRResult ocrResult = _ocrEngine.DetectText(screenBmp);

                // 触发事件，将原始识别结果广播出去，用于屏幕显示
                OnOcrResult?.Invoke(ocrResult);


                // 如果目标文字为空，则仅识别并显示，不进行后续查找点击
                if (string.IsNullOrWhiteSpace(targetText))
                {
                    return null;
                }

                if (ocrResult != null && ocrResult.TextBlocks != null)
                {
                    foreach (var block in ocrResult.TextBlocks)
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
            catch (Exception)
            {
                return null;
            }

            return null;
        }
    }
}