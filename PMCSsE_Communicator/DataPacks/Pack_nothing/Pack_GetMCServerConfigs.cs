using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 数据包：请求获取所有 MC 服务端管理器的配置列表。空包体，仅用作信号通知。
    /// </summary>
    [ProtoContract]
    public class Pack_GetMCServerManagerConfigsList
    {
    }
}
