using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：传输管理器聚合数据（MCServerManagersData）。
    /// 用于一次性将多个管理器的信息发送给客户端或接收端。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_MCServerManagers(MCServerManagersData mCServerManagersData)
    {
        /// <summary>
        /// 包含多个管理器状态信息的聚合数据对象。
        /// </summary>
        [ProtoMember(1)]
        public MCServerManagersData MCServerManagersData = mCServerManagersData;
    }
}
