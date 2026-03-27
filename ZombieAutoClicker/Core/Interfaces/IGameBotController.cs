namespace ZombieAutoClicker.Core.Interfaces
{
    /// <summary>
    /// 游戏机器人控制器接口
    /// </summary>
    public interface IGameBotController
    {
        /// <summary>
        /// 启动机器人
        /// </summary>
        /// <param name="mode">游戏模式</param>
        void Start(string mode);

        /// <summary>
        /// 停止机器人
        /// </summary>
        void Stop();

        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }
    }
}