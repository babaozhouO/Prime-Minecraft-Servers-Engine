using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_SendCommand : Pack_ManagerOperation
    {
        public Pack_SendCommand(string managerID,string command) : base(managerID)
        {
            Command = command;
        }
        [ProtoMember(2)]
        public string Command;
    }
}
