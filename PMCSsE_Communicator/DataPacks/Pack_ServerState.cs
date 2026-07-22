using LightProto;
using PMCSsE_Communicator.SharedCodes.ServerMonitor;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 返回的服务器信息（例如CPU/内存占用等）
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_ServerState(DateTime checkTime, CPUInfo[] cPUs, ulong totalMemory, ulong usingMemory)
    {
        /// <summary>
        /// 查询CPU占用率的时间点
        /// </summary>
        [ProtoMember(1)]
        public DateTime CheckTime = checkTime;
        /// <summary>
        /// CPU列表
        /// </summary>
        [ProtoMember(2)]
        public CPUInfo[] CPUs = cPUs;
        /// <summary>
        /// 总内存大小（字节）
        /// </summary>
        [ProtoMember(3)]
        public ulong TotalMemory = totalMemory;
        /// <summary>
        /// 已用内存大小（字节）
        /// </summary>
        [ProtoMember(4)]
        public ulong UsingMemory = usingMemory;
    }
}
