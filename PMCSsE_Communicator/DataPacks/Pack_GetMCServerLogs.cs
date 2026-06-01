using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor =true)]
    public class Pack_GetMCServerLogs
    {
        public Pack_GetMCServerLogs(string managerID,ulong startIndex,ulong count)
        {
            ManagerID = managerID;
            StartIndex = startIndex;
            Count = count;
        }
        [ProtoMember(1)]
        public string ManagerID;
        [ProtoMember(2)]
        public ulong StartIndex;
        [ProtoMember(3)]
        public ulong Count;
    }
}
