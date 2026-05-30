using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_ModifiedMCServerManagerConfig
    {
        public Pack_ModifiedMCServerManagerConfig(MCServerManagerConfig config)
        {
            MCServerManagerConfig = config;
        }
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig;
    }
}
