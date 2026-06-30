using LightProto;
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
    public partial class Pack_MCServerManagers(MCServerManagersData mCServerManagersData)
    {
        /// <summary>
        /// 包含多个管理器状态信息的聚合数据对象。
        /// </summary>
        [ProtoMember(1)]
        public MCServerManagersData MCServerManagersData = mCServerManagersData;
    }
    /// <summary>
    /// 多个 MC 服务端管理器数据的聚合容器，用于一次性传输所有管理器的状态信息。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class MCServerManagersData
    {
        /// <summary>
        /// MC 服务端管理器数据列表。
        /// </summary>
        [ProtoMember(1)]
        public List<MCServerManagerData> MCServerManagerDataList = [];
    }
    /// <summary>
    /// 单个 MC 服务端管理器的运行时状态数据，包含管理器的唯一标识和运行状态。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class MCServerManagerData
    {
        /// <summary>
        /// 管理器的唯一标识符。
        /// </summary>
        [ProtoMember(1)]
        public required string ManagerID;
        /// <summary>
        /// 指示该管理器下的 MC 服务端是否正在运行。
        /// </summary>
        [ProtoMember(2)]
        public required bool IsMCServerRunning;
    }
}
