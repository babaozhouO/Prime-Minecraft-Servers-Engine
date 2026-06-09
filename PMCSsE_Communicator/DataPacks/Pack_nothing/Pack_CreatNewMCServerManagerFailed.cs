using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 数据包：通知创建新 MC 服务端管理器失败的响应。空包体，仅用作信号通知。
    /// </summary>
    [ProtoContract]
    public class Pack_CreatNewMCServerManagerFailed
    {
    }
}
