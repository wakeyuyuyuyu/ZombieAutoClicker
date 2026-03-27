using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ZombieAutoClicker.Core.Interfaces;

namespace ZombieAutoClicker.Modules.Test
{
    /// <summary>
    /// 计算器测试服务
    /// 用于验证OCR识别和鼠标点击功能
    /// 通过功能宏开关 CALCULATOR_TEST 控制是否编译
    /// </summary>
#if CALCULATOR_TEST
    public class CalculatorTestService
    {
        private readonly IVisionService _visionService;
        private readonly IWindowService _windowService;
        
        /// <summary>
        /// 测试结果事件
        /// </summary>
        public event Action<string, bool>? OnTestResult;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CalculatorTestService(IVisionService visionService, IWindowService windowService)
        {
            _visionService = visionService;
            _windowService = windowService;
        }
        
        /// <summary>
        /// 运行计算器测试
        /// </summary>
        public void RunCalculatorTest()
        {
            try
            {
                OnTestResult?.Invoke("开始计算器测试...", false);
                
                // 1. 查找计算器窗口
                var calculatorHandle = _windowService.FindWindow("计算器");
                if (calculatorHandle == IntPtr.Zero)
                {
                    OnTestResult?.Invoke("未找到计算器窗口，请确保计算器已打开", false);
                    return;
                }
                
                // 2. 激活计算器窗口
                _windowService.SetForegroundWindow(calculatorHandle);
                Thread.Sleep(500); // 等待窗口激活
                
                // 3. 清除计算器
                ClearCalculator();
                
                // 4. 执行1+2+3...+10的测试
                ExecuteSumTest();
                
                // 5. 验证结果
                VerifyResult();
                
                OnTestResult?.Invoke("计算器测试完成！", true);
            }
            catch (Exception ex)
            {
                OnTestResult?.Invoke($"测试过程中发生错误: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// 清除计算器
        /// </summary>
        private void ClearCalculator()
        {
            OnTestResult?.Invoke("清除计算器...", false);
            
            // 尝试点击C按钮
            ClickCalculatorButton("C");
            Thread.Sleep(300);
        }
        
        /// <summary>
        /// 执行1+2+3...+10的求和测试
        /// </summary>
        private void ExecuteSumTest()
        {
            OnTestResult?.Invoke("开始执行1+2+3...+10的求和测试...", false);
            
            // 点击1
            ClickCalculatorButton("1");
            Thread.Sleep(200);
            
            // 点击+
            ClickCalculatorButton("+");
            Thread.Sleep(200);
            
            // 循环点击2到10
            for (int i = 2; i <= 10; i++)
            {
                ClickCalculatorButton(i.ToString());
                Thread.Sleep(200);
                
                if (i < 10)
                {
                    ClickCalculatorButton("+");
                    Thread.Sleep(200);
                }
            }
            
            // 点击=计算结果
            ClickCalculatorButton("=");
            Thread.Sleep(500); // 等待计算完成
        }
        
        /// <summary>
        /// 验证计算结果
        /// </summary>
        private void VerifyResult()
        {
            OnTestResult?.Invoke("验证计算结果...", false);
            
            // 截取计算器窗口
            var screenshot = _windowService.CaptureWindow("计算器", out var windowTopLeft);
            if (screenshot == null)
            {
                OnTestResult?.Invoke("无法截取计算器窗口", false);
                return;
            }
            
            // 创建OCR结果收集器
            string? recognizedResult = null;
            VisionOCRResult? lastOcrResult = null;
            
            // 订阅OCR结果事件
            Action<VisionOCRResult>? ocrHandler = null;
            ocrHandler = (result) =>
            {
                lastOcrResult = result;
                if (result?.TextBlocks != null)
                {
                    // 查找显示区域的结果（通常是最上方的文本）
                    foreach (var block in result.TextBlocks)
                    {
                        string? text = block.Text?.Trim();
                        if (!string.IsNullOrEmpty(text) && IsNumericResult(text))
                        {
                            recognizedResult = text;
                            OnTestResult?.Invoke($"识别到结果: {recognizedResult}", false);
                            break;
                        }
                    }
                }
            };
            
            // 临时订阅事件
            _visionService.OnOcrResult += ocrHandler;
            
            try
            {
                // 识别屏幕
                _visionService.RecognizeScreen(screenshot);
                
                // 等待识别完成（简化处理，实际应该使用异步等待）
                Thread.Sleep(1000);
                
                // 验证结果
                if (!string.IsNullOrEmpty(recognizedResult))
                {
                    if (recognizedResult.Contains("55"))
                    {
                        OnTestResult?.Invoke($"✅ 测试成功！计算结果正确: {recognizedResult}", true);
                    }
                    else
                    {
                        OnTestResult?.Invoke($"❌ 测试失败！预期55，实际: {recognizedResult}", false);
                    }
                }
                else if (lastOcrResult != null && lastOcrResult.TextBlocks != null)
                {
                    // 显示所有识别到的文本用于调试
                    OnTestResult?.Invoke("调试信息 - 所有识别到的文本:", false);
                    foreach (var block in lastOcrResult.TextBlocks)
                    {
                        OnTestResult?.Invoke($"  - '{block.Text}'", false);
                    }
                    OnTestResult?.Invoke("未找到明确的数字结果，请手动验证", false);
                }
                else
                {
                    OnTestResult?.Invoke("未识别到任何文本，请检查OCR功能", false);
                }
            }
            finally
            {
                // 取消订阅事件
                if (ocrHandler != null)
                {
                    _visionService.OnOcrResult -= ocrHandler;
                }
                screenshot.Dispose();
            }
        }
        
        /// <summary>
        /// 检查文本是否为数字结果
        /// </summary>
        private bool IsNumericResult(string text)
        {
            // 移除空格和常见符号
            string cleanText = text.Replace(" ", "").Replace(",", "").Replace(".", "");
            
            // 检查是否包含数字
            foreach (char c in cleanText)
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 点击计算器按钮（改进版，解决精确匹配问题）
        /// </summary>
        private void ClickCalculatorButton(string buttonText)
        {
            try
            {
                OnTestResult?.Invoke($"点击按钮: {buttonText}", false);
                
                // 截取计算器窗口
                var screenshot = _windowService.CaptureWindow("计算器", out var windowTopLeft);
                if (screenshot == null)
                {
                    OnTestResult?.Invoke($"无法截取计算器窗口来查找按钮: {buttonText}", false);
                    return;
                }
                
                // 使用改进的按钮查找方法
                var buttonCenter = FindCalculatorButtonExact(screenshot, buttonText, windowTopLeft);
                if (buttonCenter.HasValue)
                {
                    // 转换为屏幕坐标并点击
                    _windowService.ClickRelativePoint(buttonCenter.Value, windowTopLeft);
                    OnTestResult?.Invoke($"成功点击按钮: {buttonText}", false);
                }
                else
                {
                    OnTestResult?.Invoke($"未找到按钮: {buttonText}，使用备用位置", false);
                    // 使用备用位置（根据按钮文本估算位置）
                    ClickButtonByEstimatedPosition(buttonText, windowTopLeft);
                }
                
                screenshot.Dispose();
                Thread.Sleep(100); // 点击后短暂等待
            }
            catch (Exception ex)
            {
                OnTestResult?.Invoke($"点击按钮 {buttonText} 时出错: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// 精确查找计算器按钮（解决C/MC、M+/+、显示区域误点击等问题）
        /// </summary>
        private Point? FindCalculatorButtonExact(Bitmap screenshot, string targetButton, Point windowTopLeft)
        {
            try
            {
                // 定义计算器按钮区域（排除显示区域）
                // 显示区域通常在上部1/3，按钮区域在下部2/3
                int displayAreaHeight = screenshot.Height / 3;
                Rectangle buttonArea = new Rectangle(0, displayAreaHeight, screenshot.Width, screenshot.Height - displayAreaHeight);
                
                // 识别屏幕
                VisionOCRResult? ocrResult = null;
                bool resultReceived = false;
                
                Action<VisionOCRResult> ocrHandler = (result) =>
                {
                    ocrResult = result;
                    resultReceived = true;
                };
                
                _visionService.OnOcrResult += ocrHandler;
                _visionService.RecognizeScreen(screenshot);
                
                // 等待识别完成
                int maxWait = 10; // 最多等待1秒
                for (int i = 0; i < maxWait && !resultReceived; i++)
                {
                    Thread.Sleep(100);
                }
                
                _visionService.OnOcrResult -= ocrHandler;
                
                if (ocrResult == null || ocrResult.TextBlocks == null)
                {
                    return null;
                }
                
                // 准备目标按钮的多种可能文本
                var targetVariants = GetButtonTextVariants(targetButton);
                
                // 在按钮区域中查找最佳匹配
                Point? bestMatch = null;
                double bestScore = 0;
                
                foreach (var block in ocrResult.TextBlocks)
                {
                    if (block.BoxPoints == null || block.BoxPoints.Length < 4)
                        continue;
                    
                    // 计算文本块中心点
                    int centerX = (block.BoxPoints[0].X + block.BoxPoints[2].X) / 2;
                    int centerY = (block.BoxPoints[0].Y + block.BoxPoints[2].Y) / 2;
                    
                    // 检查是否在按钮区域内
                    if (!buttonArea.Contains(centerX, centerY))
                    {
                        continue; // 跳过显示区域的文本
                    }
                    
                    string? recognizedText = block.Text?.Trim();
                    if (string.IsNullOrEmpty(recognizedText))
                        continue;
                    
                    // 计算匹配分数
                    double score = CalculateButtonMatchScore(recognizedText, targetVariants, targetButton);
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = new Point(centerX, centerY);
                    }
                }
                
                // 如果找到匹配分数足够高的按钮，返回其位置
                if (bestMatch.HasValue && bestScore >= 0.7) // 阈值设为0.7
                {
                    return bestMatch.Value;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                OnTestResult?.Invoke($"精确查找按钮时出错: {ex.Message}", false);
                return null;
            }
        }
        
        /// <summary>
        /// 计算按钮匹配分数
        /// </summary>
        private double CalculateButtonMatchScore(string recognizedText, string[] targetVariants, string originalTarget)
        {
            string cleanRecognized = recognizedText.Replace(" ", "").ToLower();
            
            // 检查精确匹配
            foreach (var variant in targetVariants)
            {
                string cleanVariant = variant.Replace(" ", "").ToLower();
                
                if (cleanRecognized == cleanVariant)
                {
                    return 1.0; // 精确匹配
                }
                
                // 检查是否为子字符串（但要避免C匹配到MC的问题）
                if (cleanRecognized.Contains(cleanVariant))
                {
                    // 对于单字符目标（如C、+），需要更严格的检查
                    if (cleanVariant.Length == 1)
                    {
                        // 检查是否是独立的字符（前后不是字母数字）
                        int index = cleanRecognized.IndexOf(cleanVariant);
                        if (index >= 0)
                        {
                            // 检查前后字符
                            bool isStart = index == 0;
                            bool isEnd = index == cleanRecognized.Length - 1;
                            char? prevChar = index > 0 ? cleanRecognized[index - 1] : null;
                            char? nextChar = index < cleanRecognized.Length - 1 ? cleanRecognized[index + 1] : null;
                            
                            // 如果前后是字母数字，可能是复合词的一部分（如MC中的C）
                            bool isIsolated = (!prevChar.HasValue || !char.IsLetterOrDigit(prevChar.Value)) &&
                                            (!nextChar.HasValue || !char.IsLetterOrDigit(nextChar.Value));
                            
                            if (isIsolated)
                            {
                                return 0.8; // 独立字符匹配
                            }
                            else
                            {
                                return 0.3; // 可能是复合词的一部分
                            }
                        }
                    }
                    else
                    {
                        return 0.6; // 多字符子字符串匹配
                    }
                }
            }
            
            return 0; // 无匹配
        }
        
        /// <summary>
        /// 获取按钮文本的多种变体（考虑OCR可能识别为不同格式）
        /// </summary>
        private string[] GetButtonTextVariants(string buttonText)
        {
            var variants = new List<string> { buttonText };
            
            // 根据按钮类型添加变体
            switch (buttonText)
            {
                case "C":
                    variants.AddRange(new[] { "C", "c", "清除", "Clear", "CE" });
                    break;
                case "=":
                    variants.AddRange(new[] { "=", "等号", "等于", "Enter" });
                    break;
                case "+":
                    variants.AddRange(new[] { "+", "加", "加号", "add" });
                    // 特别注意：排除"M+"中的"+"
                    break;
                case "-":
                    variants.AddRange(new[] { "-", "减", "减号", "minus" });
                    break;
                case "*":
                    variants.AddRange(new[] { "*", "×", "乘", "乘号", "multiply" });
                    break;
                case "/":
                    variants.AddRange(new[] { "/", "÷", "除", "除号", "divide" });
                    break;
                case ".":
                    variants.AddRange(new[] { ".", "点", "小数点", "decimal" });
                    break;
                default:
                    // 数字按钮
                    if (int.TryParse(buttonText, out _))
                    {
                        variants.Add(buttonText);
                    }
                    break;
            }
            
            return variants.ToArray();
        }
        
        /// <summary>
        /// 根据按钮文本估算位置并点击（备用方案）
        /// </summary>
        private void ClickButtonByEstimatedPosition(string buttonText, Point windowTopLeft)
        {
            // 计算器按钮布局估算（标准Windows计算器 - 更精确的布局）
            var buttonPositions = new Dictionary<string, Point>
            {
                // 显示区域（不需要点击）
                
                // 第一行：清除按钮
                {"C", new Point(70, 180)}, {"CE", new Point(140, 180)}, 
                
                // 第二行：数字7-9和除号
                {"7", new Point(70, 230)}, {"8", new Point(140, 230)}, {"9", new Point(210, 230)}, {"/", new Point(280, 230)},
                
                // 第三行：数字4-6和乘号
                {"4", new Point(70, 280)}, {"5", new Point(140, 280)}, {"6", new Point(210, 280)}, {"*", new Point(280, 280)},
                
                // 第四行：数字1-3和减号
                {"1", new Point(70, 330)}, {"2", new Point(140, 330)}, {"3", new Point(210, 330)}, {"-", new Point(280, 330)},
                
                // 第五行：数字0、小数点、等号、加号
                {"0", new Point(70, 380)}, {".", new Point(140, 380)}, {"=", new Point(210, 380)}, {"+", new Point(280, 380)}
            };
            
            // 尝试查找按钮（支持多种可能的文本）
            string[] possibleTexts = GetPossibleButtonTexts(buttonText);
            
            foreach (var text in possibleTexts)
            {
                if (buttonPositions.TryGetValue(text, out var estimatedPosition))
                {
                    _windowService.ClickRelativePoint(estimatedPosition, windowTopLeft);
                    OnTestResult?.Invoke($"使用估算位置点击按钮 '{buttonText}' (映射为 '{text}')", false);
                    return;
                }
            }
            
            OnTestResult?.Invoke($"没有找到按钮 '{buttonText}' 的估算位置", false);
        }
        
        /// <summary>
        /// 获取按钮的可能文本变体（OCR可能识别为不同格式）
        /// </summary>
        private string[] GetPossibleButtonTexts(string buttonText)
        {
            var possibleTexts = new List<string> { buttonText };
            
            switch (buttonText)
            {
                case "C":
                    possibleTexts.AddRange(new[] { "C", "c", "清除", "Clear" });
                    break;
                case "=":
                    possibleTexts.AddRange(new[] { "=", "等号", "等于", "Enter" });
                    break;
                case "+":
                    possibleTexts.AddRange(new[] { "+", "加", "加号", "add" });
                    break;
                case "-":
                    possibleTexts.AddRange(new[] { "-", "减", "减号", "minus" });
                    break;
                case "*":
                    possibleTexts.AddRange(new[] { "*", "×", "乘", "乘号", "multiply" });
                    break;
                case "/":
                    possibleTexts.AddRange(new[] { "/", "÷", "除", "除号", "divide" });
                    break;
                case ".":
                    possibleTexts.AddRange(new[] { ".", "点", "小数点", "decimal" });
                    break;
                default:
                    // 数字按钮
                    possibleTexts.Add(buttonText);
                    break;
            }
            
            return possibleTexts.ToArray();
        }
    }
#endif
}