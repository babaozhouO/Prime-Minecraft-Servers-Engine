using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_StoppedMCServerManager : Pack_ManagerOperation
    {
        public Pack_StoppedMCServerManager(string managerID) : base(managerID)
        {
        }
    }
}
