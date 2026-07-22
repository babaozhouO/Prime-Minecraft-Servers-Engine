using Hardware.Info;
using PMCSsE_Communicator.SharedCodes.ServerMonitor;

namespace PMCSsE_Backend.Modules
{
    internal static class ServerMonitor
    {
        internal static bool GotInfo = false;//windows平台上初次获取会有21s延迟
        internal static CPUInfo[] CPUs
        {
            get
            {
                CPUInfo[] infos = new CPUInfo[HardwareInfo.CpuList.Count];
                for (byte i = 0; i < infos.Length; i++)
                {
                    infos[i] = new(HardwareInfo.CpuList[i].ProcessorId, HardwareInfo.CpuList[i].Name, HardwareInfo.CpuList[i].PercentProcessorTime);
                }
                return infos;
            }
        }
        internal static ulong TotalMemory
        {
            get
            {
                return HardwareInfo.MemoryStatus.TotalPhysical;
            }
        }
        internal static ulong UsingMemory
        {
            get
            {
                return TotalMemory - HardwareInfo.MemoryStatus.AvailablePhysical;
            }
        }
        internal static HardwareInfo HardwareInfo = new();
        internal static System.Timers.Timer RefreshCaller = new() { AutoReset = false, Interval = 1000 };
        internal static void Initialize()
        {
            RefreshCaller.Elapsed += RefreshWork;
            RefreshCaller.Start();
        }
        private static void RefreshWork(object? sender, EventArgs e)
        {
            RefreshCaller.Stop();
            HardwareInfo.RefreshAll();
            if (!GotInfo)
            {
                GotInfo = true;
                StaticTools.HandleLog($"已完成首次硬件信息刷新");
            }
            RefreshCaller.Start();
        }
        internal static void Dispose()
        {
            RefreshCaller.Stop();
            RefreshCaller.Elapsed -= RefreshWork;
            RefreshCaller.Dispose();
        }
    }
}
