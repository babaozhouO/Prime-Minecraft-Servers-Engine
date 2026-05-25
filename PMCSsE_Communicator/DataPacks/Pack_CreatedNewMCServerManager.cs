using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_CreatedNewMCServerManager
    {
        public Pack_CreatedNewMCServerManager(MCServerManagerConfig mCServerManagerConfig)
        {
            MCServerManagerConfig = mCServerManagerConfig;
        }
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig;
    }
}
