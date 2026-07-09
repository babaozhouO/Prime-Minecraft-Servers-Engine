using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.SharedCodes.ServerMonitor
{
    /// <summary>
    /// 包装类：CPU信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="usage"></param>
    [ProtoContract(SkipConstructor =true)]
    public partial class CPUInfo(string name,ulong usage)
    {
        /// <summary>
        /// CPU名称
        /// </summary>
        [ProtoMember(1)]
        public string Name = name;
        /// <summary>
        /// CPU占用率
        /// </summary>
        [ProtoMember(2)]
        public ulong Usage = usage;
    }
}
