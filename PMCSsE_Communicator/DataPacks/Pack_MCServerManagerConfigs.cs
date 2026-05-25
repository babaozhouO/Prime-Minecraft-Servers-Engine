using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{

    [ProtoContract(SkipConstructor =true)]
    public class Pack_MCServerManagerConfigs
    {
        public Pack_MCServerManagerConfigs(MCServerManagerConfigs MCServerManagerConfigs)
        {
            this.MCServerManagerConfigs = MCServerManagerConfigs;
        }

        [ProtoMember(1)]
        public MCServerManagerConfigs MCServerManagerConfigs;
    }
}
