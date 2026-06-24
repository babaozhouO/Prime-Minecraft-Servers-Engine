using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：获取指定起始索引之后的日志条目。
    /// 客户端可使用该包向服务器请求从 startIndex 开始的后续日志，最多 count 条。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_GetNewerMCServerLogs(string managerID, ulong startIndex, int count)
    {
        /// <summary>
        /// 管理器ID
        /// </summary>
        [ProtoMember(1)]
        public string ManagerID = managerID;
        /// <summary>
        /// 客户端最新消息的索引
        /// </summary>
        [ProtoMember(2)]
        public ulong StartIndex = startIndex;
        /// <summary>
        /// 获取数量
        /// </summary>
        [ProtoMember(3)]
        public int Count = count;
    }
}
