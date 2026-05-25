using PMCSsE_Communicator.PluginLoader;
using System.Security.Cryptography;

namespace Example_PMCSsE_Plugin
{
    public class Example_PMCSsE_Plugin : IPlugin
    {
        public string Name => "Example_PMCSsE_Plugin";
        public string Version => "1.0.0";

        public int MinSupportedVersion => 1040;

        public int MaxSupportedVersion => 1040;

        public event Action Started = delegate { };
        public event Action<string> ReportLog = delegate { };
        public event Action Stopped = delegate { };

        public void Dispose()
        {

        }

        public void Initialize()
        {
            Console.WriteLine("示例插件已初始化");
        }

        public void Start()
        {
            Console.WriteLine("示例插件已启动");
            Console.WriteLine("正在计算100次随机int32相加之和的平均数");
            decimal sum = 0;
            for (byte count = 1; count < 100; count++)
            {
                sum += RandomNumberGenerator.GetInt32(int.MaxValue / 2) + RandomNumberGenerator.GetInt32(int.MaxValue / 2);
            }
            Console.Write("结果");
            Console.WriteLine(sum / 100);
        }

        public void Stop()
        {

        }
    }
}
