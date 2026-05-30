using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor =true)]
    public class Pack_ModifyMCServerManagerConfig
    {
        public Pack_ModifyMCServerManagerConfig(MCServerManagerConfig config)
        {
            MCServerManagerConfig = config;
        }
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig;
    }
}
