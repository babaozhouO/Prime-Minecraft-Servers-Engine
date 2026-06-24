using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 数据包：请求创建新的 MC 服务端管理器。空包体，仅用作信号通知。
    /// </summary>
    [ProtoContract]
    public partial class Pack_CreatNewMCServerManager
    {
    }
}
