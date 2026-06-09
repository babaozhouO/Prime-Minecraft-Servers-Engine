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
    public class Pack_ErrorInfo(string errorInfo)
    {
        /// <summary>
        /// 错误信息文本，用于向接收端传递详细的错误描述。
        /// </summary>
        [ProtoMember(1)]
        public string ErrorInfo = errorInfo;
    }
}
