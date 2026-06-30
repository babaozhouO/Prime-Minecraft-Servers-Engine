using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using PMCSsE_Communicator.SharedCodes;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：传输指定管理器的日志集合。
    /// 通常用于同步或推送管理器的历史或实时日志。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_MCServerLogs(string managerID, LogEntry[] logs, bool resetHint)
    {
        /// <summary>
        /// 日志所属管理器的唯一标识符。
        /// </summary>
        [ProtoMember(1)]
        public string ManagerID = managerID;
        /// <summary>
        /// 日志条目列表。
        /// </summary>
        [ProtoMember(2)]
        public LogEntry[] Logs = logs;
        /// <summary>
        /// 是否提示前端重置日志视图（如清空现有日志重新加载）。
        /// </summary>
        [ProtoMember(3)]
        public bool ResetHint=resetHint;
    }
}
