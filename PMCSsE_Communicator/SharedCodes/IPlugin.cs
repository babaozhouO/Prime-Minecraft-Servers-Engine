

namespace PMCSsE_Communicator.PluginLoader
{
    /// <summary>
    /// 插件接口,插件需实现此接口（项目类型需为类库）
    /// 选项一：纯后端插件：引用PMCSsE_Backend和PMCSsE_Communicator项目
    /// 选项二：纯前端插件：引用PMCSsE_Frontend_AvaloniaUI和PMCSsE_Communicator项目
    /// 选项三：通用自适应插件：引用全部项目
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件名
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 插件自身版本
        /// </summary>
        public string Version { get; }
        /// <summary>
        /// 最低适用版本
        /// 1.0.4.0=>1040
        /// </summary>
        public int MinSupportedVersion { get; }
        /// <summary>
        /// 最高适用版本
        /// 1.0.4.0=>1040
        /// </summary>
        public int MaxSupportedVersion { get; }
        /// <summary>
        /// 启动事件
        /// </summary>
        public event Action<string> Started;
        /// <summary>
        /// 上报日志事件
        /// </summary>
        public event Action<string,string> ReportLog;
        /// <summary>
        /// 停止事件
        /// </summary>
        public event Action<string> Stopped;
        /// <summary>
        /// 初始化函数
        /// </summary>
        public void Initialize();
        /// <summary>
        /// 启动函数
        /// </summary>
        public void Start();
        /// <summary>
        /// 停止函数
        /// </summary>
        public void Stop();
        /// <summary>
        /// 彻底释放
        /// </summary>
        public void Dispose();

    }
}
