using System;
using System.Drawing;
using System.Windows.Forms;
using ZombieAutoClicker.Core;
using ZombieAutoClicker.Core.Interfaces;
#if CALCULATOR_TEST
using ZombieAutoClicker.Modules.Test;
#endif

namespace ZombieAutoClicker
{
    public partial class MainForm : Form
    {
        private ComboBox? comboMode;
        private Button? btnStart;
        private Button? btnStop;
        private RichTextBox? rtbLog;
        private IGameBotController? bot;
        private OverlayForm? _overlayForm;
        private IVisionService? _visionService;
#if CALCULATOR_TEST
        private Button? btnTestCalculator;
        private CalculatorTestService? _calculatorTestService;
#endif

        public MainForm()
        {
            // 1. 初始化界面所有控件
            InitializeUI();
            this.FormClosing += MainForm_FormClosing;

            // 2. 获取视觉识别服务实例
            _visionService = ServiceFactory.GetVisionService();

            // 3. 初始化挂机大脑（使用服务工厂创建）
            bot = ServiceFactory.CreateGameBotController(msg =>
            {
                if (rtbLog?.InvokeRequired == true)
                    rtbLog.Invoke(new Action(() => UpdateLog(msg)));
                else
                    UpdateLog(msg);
            });

#if CALCULATOR_TEST
            // 初始化计算器测试服务
            var windowService = ServiceFactory.GetWindowService();
            _calculatorTestService = new CalculatorTestService(_visionService, windowService);
            _calculatorTestService.OnTestResult += (msg, isSuccess) =>
            {
                string logMsg = isSuccess ? $"✅ {msg}" : $"⚠️ {msg}";
                if (rtbLog?.InvokeRequired == true)
                    rtbLog.Invoke(new Action(() => UpdateLog(logMsg)));
                else
                    UpdateLog(logMsg);
            };
#endif

            // 4. 初始化并显示悬浮窗
            _overlayForm = new OverlayForm();
            _overlayForm.Show();

            // 5. 订阅OCR识别事件，用于在悬浮窗上显示结果
            if (_visionService != null && _overlayForm != null)
            {
                _visionService.OnOcrResult += (result) =>
                {
                    if (!_overlayForm.IsDisposed)
                    {
                        _overlayForm.BeginInvoke(new Action(() => _overlayForm.UpdateOcrResult(result)));
                    }
                };
            }
        }

        private void UpdateLog(string msg)
        {
            if (rtbLog == null) return;
            
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            rtbLog.ScrollToCaret();
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (comboMode?.SelectedItem == null || bot == null || btnStart == null || btnStop == null)
                return;
                
            string selectedMode = comboMode.SelectedItem.ToString() ?? string.Empty;
            bot.Start(selectedMode);
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            if (bot == null || btnStart == null || btnStop == null)
                return;
                
            bot.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

#if CALCULATOR_TEST
        private void BtnTestCalculator_Click(object? sender, EventArgs e)
        {
            try
            {
                UpdateLog("开始计算器测试...");
                if (btnTestCalculator == null || _calculatorTestService == null)
                    return;
                    
                btnTestCalculator.Enabled = false;
                
                // 在新线程中运行测试，避免阻塞UI
                System.Threading.Tasks.Task.Run(() =>
                {
                    _calculatorTestService.RunCalculatorTest();
                    
                    // 测试完成后重新启用按钮
                    if (btnTestCalculator.InvokeRequired)
                        btnTestCalculator.Invoke(new Action(() =>
                        {
                            if (btnTestCalculator != null)
                                btnTestCalculator.Enabled = true;
                        }));
                    else
                    {
                        if (btnTestCalculator != null)
                            btnTestCalculator.Enabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                UpdateLog($"计算器测试出错: {ex.Message}");
                if (btnTestCalculator != null)
                    btnTestCalculator.Enabled = true;
            }
        }
#endif

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Stop the bot to ensure all background tasks are terminated
            bot?.Stop();

            // Close the overlay form
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.Close();
            }
        }

        // --- 核心：纯代码生成所有 UI 控件 ---
        // --- 核心：纯代码生成所有 UI 控件 ---
        // --- 核心：纯代码生成所有 UI 控件 ---
        private void InitializeUI()
        {
            this.Text = "自动化助手 v1.1";

            // 【修改点1】大幅增加了窗口的初始大小为 800x600
            this.Size = new Size(800, 600);
            // 【新增】设置了窗口的最小限制，防止缩得太小导致控件重叠
            this.MinimumSize = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblMode = new Label() { Text = "请选择功能模式：", Location = new Point(15, 20), AutoSize = true };

            comboMode = new ComboBox() { Location = new Point(120, 15), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            comboMode.Items.AddRange(new string[] { "元素试炼", "雾境迷宫", "寰球试炼", "主线推图" });
            comboMode.SelectedIndex = 0;

            btnStart = new Button() { Text = "▶ 开始挂机", Location = new Point(15, 60), Size = new Size(150, 45), BackColor = Color.LightGreen };
            btnStart.Click += BtnStart_Click;

            btnStop = new Button() { Text = "⏹ 停止", Location = new Point(180, 60), Size = new Size(150, 45), BackColor = Color.LightPink, Enabled = false };
            btnStop.Click += BtnStop_Click;

#if CALCULATOR_TEST
            // 计算器测试按钮
            btnTestCalculator = new Button() { Text = "🧮 测试计算器", Location = new Point(345, 60), Size = new Size(150, 45), BackColor = Color.LightSkyBlue };
            btnTestCalculator.Click += BtnTestCalculator_Click;
#endif

            // 【修改点2】大幅扩大了日志框的初始尺寸 (宽750, 高420)
            // 【修改点3】新增了 Anchor 属性！现在你可以用鼠标自由拉拽放大软件窗口，黑框会自动跟着变大！
            rtbLog = new RichTextBox()
            {
                Location = new Point(15, 120),
                Size = new Size(750, 420),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            this.Controls.Add(lblMode);
            this.Controls.Add(comboMode);
            this.Controls.Add(btnStart);
            this.Controls.Add(btnStop);
#if CALCULATOR_TEST
            this.Controls.Add(btnTestCalculator);
#endif
            this.Controls.Add(rtbLog);
        }
    }
}