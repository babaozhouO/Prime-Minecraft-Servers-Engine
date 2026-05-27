using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract(SkipConstructor =true)]
    public class Pack_MCServerManagers
    {
        public Pack_MCServerManagers(MCServerManagersData mCServerManagersData)
        {
            MCServerManagersData = mCServerManagersData;
        }
        [ProtoMember(1)]
        public MCServerManagersData MCServerManagersData;
    }
}
