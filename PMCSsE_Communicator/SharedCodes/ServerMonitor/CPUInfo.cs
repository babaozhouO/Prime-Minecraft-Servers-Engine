using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.SharedCodes.ServerMonitor
{
    /// <summary>
    /// 包装类：CPU信息
    /// </summary>
    /// <param name="iD"></param>
    /// <param name="name"></param>
    /// <param name="usage"></param>
    /// <param name="checkTime"></param>
    [ProtoContract(SkipConstructor =true)]
    public partial class CPUInfo(string iD,string name,ulong usage)
    {
        /// <summary>
        /// CPUID
        /// </summary>
        [ProtoMember(1)]
        public string ID = iD;
        /// <summary>
        /// CPU名称
        /// </summary>
        [ProtoMember(2)]
        public string Name = name;
        /// <summary>
        /// CPU占用率
        /// </summary>
        [ProtoMember(3)]
        public ulong Usage = usage;
    }
}
