using PMCSsE_Backend.Modules;
using PMCSsE_Backend.PluginsSystem;
using PMCSsE_Communicator.PluginLoader;
using System.Security.Cryptography;

namespace Example_PMCSsE_Plugin
{
    /// <summary>
    /// 示例 PMCSsE 插件，同时实现 IPlugin 和 ISpecialMCServerFeaturesProvider 接口。
    /// 演示了插件的基本结构：生命周期管理、日志上报、以及为目标服务端类型提供特定功能。
    /// </summary>
    public class Example_PMCSsE_Plugin : IPlugin, ISpecialMCServerFeaturesProvider
    {
        /// <summary>
        /// 插件名称。
        /// </summary>
        public string Name => "Example_PMCSsE_Plugin";
        /// <summary>
        /// 插件版本号。
        /// </summary>
        public string Version => "1.0.0";
        /// <summary>
        /// 最低支持的 PMCSsE 版本号（1040 表示 1.0.4.0）。
        /// </summary>
        public int MinSupportedVersion => 1040;

        /// <summary>
        /// 最高支持的 PMCSsE 版本号。
        /// </summary>
        public int MaxSupportedVersion => 1040;

        /// <summary>
        /// 目标 MC 服务端类型。
        /// </summary>
        public string TargetMCServerType => "Vanilla";

        /// <summary>
        /// 为此服务端类型提供的特殊功能列表。
        /// </summary>
        public IReadOnlyList<IAsyncSpecialMCServerFeature> Features =>
            [
            new VanillaServerFeature_GetServerVersion()
            ];

        /// <summary>
        /// 插件启动事件。
        /// </summary>
        public event Action<string> Started = delegate { };
        /// <summary>
        /// 插件上报日志事件，参数为插件名称和日志内容。
        /// </summary>
        public event Action<string,string> ReportLog = delegate { };
        /// <summary>
        /// 插件停止事件。
        /// </summary>
        public event Action<string> Stopped = delegate { };

        /// <summary>
        /// 释放插件占用的资源。
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// 插件初始化，在加载时由插件系统调用。
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// 启动插件。
        /// </summary>
        public void Start()
        {
            ReportLog(Name,"示例插件！启动！");
            Started(Name);
        }

        /// <summary>
        /// 停止插件。
        /// </summary>
        public void Stop()
        {
            ReportLog(Name,"Goodbye!");
            Stopped(Name);
        }
        /// <summary>
        /// Vanilla 服务端的特殊功能：获取服务端版本号。
        /// </summary>
        public class VanillaServerFeature_GetServerVersion : IAsyncSpecialMCServerFeature
        {
            /// <summary>
            /// 功能描述。
            /// </summary>
            public string FeatureDescription => "服务端开启时发送version获取原版服务端版本号";


            /// <summary>
            /// 功能名称。
            /// </summary>
            public string FeatureName => "获取版本信息";

            /// <summary>
            /// 异步执行获取版本信息的功能，向服务端发送 version 命令。
            /// </summary>
            /// <param name="mCServerManager">目标 MC 服务端管理器。</param>
            public async Task AsyncFeature(MCServerManager mCServerManager)
            {
                if (mCServerManager.isMCServerRunning)
                {
                    mCServerManager.SendCommand("version");
                }
            }
        }
    }
}
