using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 数据包：请求获取已加载的 MC 服务端管理器列表。空包体，仅用作信号通知。
    /// </summary>
    [ProtoContract]
    public class Pack_GetMCServerManager
    {
    }
}
