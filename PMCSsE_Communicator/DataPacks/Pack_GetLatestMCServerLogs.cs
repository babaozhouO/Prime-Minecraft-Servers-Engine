using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：请求最新的 MCServer 日志。
    /// 包含目标管理器 ID 与希望获取的日志数量。
    /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public partial class Pack_GetLatestMCServerLogs(string managerID, int count)
    {
        /// <summary>
        /// 目标管理器的唯一标识符。
        /// </summary>
        [ProtoMember(1)]
        public string ManagerID = managerID;
        /// <summary>
        /// 希望获取的日志条目数量。
        /// </summary>
        [ProtoMember(2)]
        public int Count = count;
    }
}
