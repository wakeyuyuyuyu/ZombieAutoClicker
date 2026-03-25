using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ZombieAutoClicker.Services
{
    /// <summary>
    /// 提供图文双修的识别服务 (OpenCV 图像匹配 + Windows 原生 OCR)
    /// </summary>
    public static class VisionService
    {
        /// <summary>
        /// [保留] 在屏幕截图中寻找目标图片（模板匹配）
        /// 用于精确识别无文字或艺术字效太强的技能图标
        /// </summary>
        public static System.Drawing.Point? FindImageCenter(Bitmap screenBmp, string templatePath, double threshold = 0.85)
        {
            if (screenBmp == null || !File.Exists(templatePath)) return null;

            // ImreadModes.Color 强制将本地模板图读取为 3 通道 (BGR) 格式，忽略其可能带有的透明度
            using (Mat screenMatRaw = BitmapConverter.ToMat(screenBmp))
            using (Mat templateMat = Cv2.ImRead(templatePath, ImreadModes.Color))
            using (Mat screenMat = new Mat())
            using (Mat result = new Mat())
            {
                if (screenMatRaw.Empty() || templateMat.Empty() || screenMatRaw.Width < templateMat.Width || screenMatRaw.Height < templateMat.Height)
                    return null;

                // 统一通道数
                if (screenMatRaw.Channels() == 4)
                {
                    Cv2.CvtColor(screenMatRaw, screenMat, ColorConversionCodes.BGRA2BGR);
                }
                else
                {
                    screenMatRaw.CopyTo(screenMat);
                }

                // 执行模板匹配
                Cv2.MatchTemplate(screenMat, templateMat, result, TemplateMatchModes.CCoeffNormed);

                // 获取匹配结果
                Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

                if (maxVal >= threshold)
                {
                    int centerX = maxLoc.X + (templateMat.Width / 2);
                    int centerY = maxLoc.Y + (templateMat.Height / 2);
                    return new System.Drawing.Point(centerX, centerY);
                }
            }
            return null;
        }

        /// <summary>
        /// [新增] 在屏幕截图中寻找包含指定文字的区域中心点 (基于 Windows 原生 OCR)
        /// 用于快速识别简单的流程按钮 (如 "开始"、"确定"、"跳过")
        /// </summary>
        /// <param name="screenBmp">屏幕截图</param>
        /// <param name="targetText">要寻找的文字</param>
        /// <returns>文字区域的中心点坐标</returns>
        public static System.Drawing.Point? FindTextCenter(Bitmap screenBmp, string targetText)
        {
            if (screenBmp == null || string.IsNullOrWhiteSpace(targetText)) return null;

            try
            {
                // 为了在同步主循环中调用异步的 WinRT API，这里使用 GetAwaiter().GetResult() 阻塞获取
                return FindTextCenterAsync(screenBmp, targetText).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // 忽略识别过程中的底层报错，防止挂机崩溃
                return null;
            }
        }

        /// <summary>
        /// OCR 核心异步实现
        /// </summary>
        private static async Task<System.Drawing.Point?> FindTextCenterAsync(Bitmap screenBmp, string targetText)
        {
            // 使用临时文件作为中转，避免 .NET 8 下复杂的 WinRT Stream 内存转换
            string tempFilePath = Path.Combine(Path.GetTempPath(), "ocr_temp_screen.png");

            try
            {
                screenBmp.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);

                // 获取 Windows 系统的文件对象
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFilePath);
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // 解码图片
                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                    var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    // 初始化 Windows 自带的 OCR 引擎 (使用系统默认语言)
                    var ocrEngine = Windows.Media.Ocr.OcrEngine.TryCreateFromUserProfileLanguages();
                    if (ocrEngine == null)
                    {
                        return null; // 系统不支持OCR
                    }

                    // 识别文字
                    var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

                    // 遍历识别到的每一行文字
                    foreach (var line in ocrResult.Lines)
                    {
                        // 去掉空格以防识别成 "开 始 试 炼"
                        string recognizedText = line.Text.Replace(" ", "");

                        // 只要识别结果中包含我们的目标词汇
                        if (recognizedText.Contains(targetText))
                        {
                            // 遍历该行所有的单字(Word)，计算包含所有字的总体外框边界
                            double minX = double.MaxValue;
                            double minY = double.MaxValue;
                            double maxX = double.MinValue;
                            double maxY = double.MinValue;

                            foreach (var word in line.Words)
                            {
                                var rect = word.BoundingRect;
                                if (rect.Left < minX) minX = rect.Left;
                                if (rect.Top < minY) minY = rect.Top;
                                if (rect.Right > maxX) maxX = rect.Right;
                                if (rect.Bottom > maxY) maxY = rect.Bottom;
                            }

                            // 计算这行文字的中心点
                            int centerX = (int)(minX + (maxX - minX) / 2);
                            int centerY = (int)(minY + (maxY - minY) / 2);
                            return new System.Drawing.Point(centerX, centerY);
                        }
                    }
                }
            }
            finally
            {
                // 无论是否成功，清理临时文件，防止挤占 C 盘
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }

            return null;
        }
    }
}