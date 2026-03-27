using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using ZombieAutoClicker.Core.Interfaces;

namespace ZombieAutoClicker.Controllers
{
    /// <summary>
    /// 机器人的主控大脑 (严格阶段控制：支持文字与图片混合识别)
    /// 重构版本：使用接口依赖，实现模块分离
    /// </summary>
    public class GameBotController : IGameBotController
    {
        private bool _isRunning;
        private readonly Action<string> _logCallback;
        private readonly IVisionService _visionService;
        private readonly IWindowService _windowService;
        private readonly string _windowTitle = "计算器";

        // ==========================================
        // 数组 1：阶段1 (进图准备阶段)
        // 强烈建议使用相对路径！即直接填 "assets/xxx.png"
        // 这要求你的图片必须放在 exe 同级目录的 assets 文件夹内
        // ==========================================
        private readonly string[] _step1Texts =
        {
            "assets/btn_start_element.png",
            "assets/GameStart.png"
        };

        // ==========================================
        // 数组 2：阶段2 (战斗循环阶段)
        // 同样支持图文混合，如果是图片请务必以 .png 结尾
        // ==========================================
        private readonly string[] _step2Texts = { "温压弹", "干冰弹", "确定", "下一层", "跳过","返回主界面" };

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 构造函数，通过依赖注入提供服务
        /// </summary>
        /// <param name="logger">日志回调</param>
        /// <param name="visionService">视觉识别服务</param>
        /// <param name="windowService">窗口控制服务</param>
        public GameBotController(Action<string> logger, IVisionService visionService, IWindowService windowService)
        {
            _logCallback = logger;
            _visionService = visionService;
            _windowService = windowService;
            _isRunning = false;
        }

        /// <summary>
        /// 启动机器人
        /// </summary>
        public void Start(string mode)
        {
            if (_isRunning) return;
            _isRunning = true;
            _logCallback($"[系统] 已启动模式：{mode} (启用严格两阶段状态机 + 图文混合引擎)");

            Task.Run(() =>
            {
                try
                {
                    if (mode == "元素试炼")
                    {
                        LoopElementalTrial();
                    }
                    else
                    {
                        _logCallback($"[系统] {mode} 功能开发中...");
                        _isRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    _logCallback($"[致命错误] {ex.Message}\n{ex.StackTrace}");
                    _isRunning = false;
                }
            });
        }

        /// <summary>
        /// 停止机器人
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _logCallback("[系统] 挂机已停止。");
        }

        private void LoopElementalTrial()
        {
            int loopCount = 0;
            int currentStep1Index = 0; // 用于记录数组1点到了第几个
            bool isPhase1 = true;      // 阶段锁，默认为阶段1

            _logCallback("[调试] 正在进入 元素试炼 循环线程...");

            while (_isRunning)
            {
                loopCount++;

                // 1. 获取窗口句柄
                IntPtr hWnd = _windowService.FindWindow(_windowTitle);
                if (hWnd == IntPtr.Zero)
                {
                    if (loopCount % 5 == 1) _logCallback($"[警告] 找不到游戏窗口 '{_windowTitle}'...");
                    Thread.Sleep(2000);
                    continue;
                }

                // 2. 截取当前屏幕画面
                Bitmap? screen = _windowService.CaptureWindow(_windowTitle, out Point winTopLeft);
                if (screen == null)
                {
                    Thread.Sleep(2000);
                    continue;
                }

                // 为了实时显示OCR结果，无论在哪个阶段，都先对整个画面进行一次文字识别
                _visionService.RecognizeScreen(screen);

                if (isPhase1)
                {
                    // ==========================================
                    // 阶段 1：严格顺序执行数组 1
                    // ==========================================
                    string target = _step1Texts[currentStep1Index];
                    var pos = _visionService.FindTargetCenter(screen, target);

                    if (pos.HasValue)
                    {
                        _logCallback($"[阶段1-按顺序进图] 发现目标【{target}】，正在点击... ({currentStep1Index + 1}/{_step1Texts.Length})");
                        _windowService.ClickRelativePoint(pos.Value, winTopLeft);

                        // 步数+1
                        currentStep1Index++;

                        // 判断数组1是否已经全部点完了
                        if (currentStep1Index >= _step1Texts.Length)
                        {
                            isPhase1 = false; // 关闭阶段1，解锁阶段2
                            _logCallback("[阶段转换] 数组1已全部点击完毕！正式进入阶段2：技能优先循环。");
                        }

                        Thread.Sleep(2000); // 等待画面跳转
                    }
                    else
                    {
                        // 如果没找到当前该点的按钮，就一直等，直到它出现为止
                        if (loopCount % 3 == 1)
                        {
                            _logCallback($"[阶段1-等待] 正在等待画面出现目标【{target}】...");
                        }
                        Thread.Sleep(1500);
                    }
                }
                else
                {
                    // ==========================================
                    // 阶段 2：优先级循环执行数组 2
                    // ==========================================
                    bool actionTakenThisLoop = false;
                    foreach (string target in _step2Texts)
                    {
                        var pos = _visionService.FindTargetCenter(screen, target);
                        if (pos.HasValue)
                        {
                            _logCallback($"[阶段2-战斗循环] 发现目标【{target}】，正在点击...");
                            _windowService.ClickRelativePoint(pos.Value, winTopLeft);
                            actionTakenThisLoop = true;

                            Thread.Sleep(1500);
                            break; // 根据优先级，点到一个就跳出本次检测
                        }
                    }

                    // 智能重置机制：如果在阶段2的循环中，游戏又回到了主界面(看到了数组1的第一个元素)
                    // 自动重置回阶段1，实现无限挂机死循环。
                    var resetPos = _visionService.FindTargetCenter(screen, _step1Texts[0]);
                    if (resetPos.HasValue && !actionTakenThisLoop)
                    {
                        _logCallback("[系统] 检测到回到初始界面，自动重置回阶段1！");
                        isPhase1 = true;
                        currentStep1Index = 0;
                    }

                    if (!actionTakenThisLoop)
                    {
                        Thread.Sleep(1500);
                    }
                }

                // 心跳日志
                if (loopCount % 5 == 1 && !isPhase1)
                {
                    _logCallback($"[调试] 战斗阶段画面检测正常，正在挂机中...");
                }

                // 释放图片内存
                screen.Dispose();
            }
        }
    }
}