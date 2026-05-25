using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    [ProtoContract(SkipConstructor = true)]
    public class Pack_ErrorInfo
    {
        public Pack_ErrorInfo(string errorInfo)
        {
            ErrorInfo = errorInfo;
        }
        [ProtoMember(1)]
        public string ErrorInfo;
    }
}
