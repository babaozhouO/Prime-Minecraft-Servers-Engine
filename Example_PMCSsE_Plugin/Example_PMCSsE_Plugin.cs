using PMCSsE_Backend.Modules;
using PMCSsE_Backend.PluginsSystem;
using PMCSsE_Communicator.PluginLoader;
using System.Security.Cryptography;

namespace Example_PMCSsE_Plugin
{
    public class Example_PMCSsE_Plugin : IPlugin, ISpecialMCServerFeaturesProvider
    {
        public string Name => "Example_PMCSsE_Plugin";
        public string Version => "1.0.0";
        public int MinSupportedVersion => 1040;

        public int MaxSupportedVersion => 1040;

        public string TargetMCServerType => "Vanilla";

        public IReadOnlyList<IAsyncSpecialMCServerFeature> Features =>
            [
            new VanillaServerFeature_GetServerVersion()
            ];

        public event Action<string> Started = delegate { };
        public event Action<string,string> ReportLog = delegate { };
        public event Action<string> Stopped = delegate { };

        public void Dispose()
        {

        }

        public void Initialize()
        {

        }

        public void Start()
        {
            ReportLog(Name,"示例插件！启动！");
            Started(Name);
        }

        public void Stop()
        {
            ReportLog(Name,"Goodbye!");
            Stopped(Name);
        }
        public class VanillaServerFeature_GetServerVersion : IAsyncSpecialMCServerFeature
        {
            public string FeatureDescription => "服务端开启时发送version获取原版服务端版本号";


            public string FeatureName => "获取版本信息";

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
