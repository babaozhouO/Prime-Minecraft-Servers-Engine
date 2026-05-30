using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    [ProtoContract]
    public class Pack_SupportedMCServerTypes
    {
        [ProtoMember(1)]
        public List<string> SupportedMCServerTypes = [];
    }
}
