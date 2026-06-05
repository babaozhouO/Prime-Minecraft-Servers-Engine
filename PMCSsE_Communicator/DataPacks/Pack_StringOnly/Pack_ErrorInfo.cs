using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通用错误信息载体。
    /// 包含一个 ErrorInfo 字段用于传递错误文本给接收端。
    /// </summary>
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
