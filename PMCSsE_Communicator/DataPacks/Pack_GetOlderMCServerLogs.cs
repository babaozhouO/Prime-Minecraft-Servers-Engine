using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：获取指定起始索引之前的日志条目。
    /// 客户端可使用该包向服务器请求以 endIndex-1 结束的历史日志，最多 count 条。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_GetOlderMCServerLogs(string managerID, ulong endIndex, uint count)
    {
        /// <summary>
        /// 管理器ID
        /// </summary>
        [ProtoMember(1)]
        public string ManagerID = managerID;
        /// <summary>
        /// 客户端最旧消息的索引
        /// </summary>
        [ProtoMember(2)]
        public ulong EndIndex = endIndex;
        /// <summary>
        /// 获取数量
        /// </summary>
        [ProtoMember(3)]
        public uint Count = count;
    }
}
