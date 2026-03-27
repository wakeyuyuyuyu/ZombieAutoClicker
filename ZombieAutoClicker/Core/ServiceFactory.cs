using ZombieAutoClicker.Core.Interfaces;
using ZombieAutoClicker.Modules.Vision;
using ZombieAutoClicker.Modules.WindowControl;

namespace ZombieAutoClicker.Core
{
    /// <summary>
    /// 服务工厂
    /// 负责创建和管理服务实例
    /// </summary>
    public static class ServiceFactory
    {
        private static IVisionService _visionService;
        private static IWindowService _windowService;

        /// <summary>
        /// 获取视觉识别服务实例
        /// </summary>
        public static IVisionService GetVisionService()
        {
            if (_visionService == null)
            {
                _visionService = new CompositeVisionService();
            }
            return _visionService;
        }

        /// <summary>
        /// 获取窗口控制服务实例
        /// </summary>
        public static IWindowService GetWindowService()
        {
            if (_windowService == null)
            {
                _windowService = new WindowsWindowService();
            }
            return _windowService;
        }

        /// <summary>
        /// 创建游戏机器人控制器
        /// </summary>
        /// <param name="logger">日志回调</param>
        /// <returns>游戏机器人控制器实例</returns>
        public static IGameBotController CreateGameBotController(Action<string> logger)
        {
            var visionService = GetVisionService();
            var windowService = GetWindowService();
            return new Controllers.GameBotController(logger, visionService, windowService);
        }

        /// <summary>
        /// 重置所有服务实例（用于测试）
        /// </summary>
        public static void Reset()
        {
            _visionService = null;
            _windowService = null;
        }
    }
}