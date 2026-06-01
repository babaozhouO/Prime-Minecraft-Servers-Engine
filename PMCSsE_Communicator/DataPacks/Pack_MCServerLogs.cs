using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_MCServerLogs
    {
        public Pack_MCServerLogs(string managerID, Dictionary<ulong,string> logs)
        {
            ManagerID = managerID;
            Logs = logs;
        }
        [ProtoMember(1)]
        public string ManagerID;
        [ProtoMember(2)]
        public Dictionary<ulong, string> Logs;
    }
}
