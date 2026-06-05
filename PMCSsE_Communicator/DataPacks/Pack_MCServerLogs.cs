using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using PMCSsE_Communicator.SharedCodes;
using ProtoBuf;
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
    public class Pack_MCServerLogs(string managerID, List<LogEntry> logs, bool resetHint)
    {
        [ProtoMember(1)]
        public string ManagerID = managerID;
        [ProtoMember(2)]
        public List<LogEntry> Logs = logs;
        [ProtoMember(3)]
        public bool ResetHint=resetHint;
    }
}
