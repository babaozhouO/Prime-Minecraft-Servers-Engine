using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_LoadedMCServerManager : Pack_ManagerOperation
    {
        public Pack_LoadedMCServerManager(string managerID) : base(managerID)
        {
        }
    }
}
