using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 数据包：请求获取后端支持的 Minecraft 服务端类型列表。空包体，仅用作信号通知。
    /// </summary>
    [ProtoContract]
    public partial class Pack_GetSupportedMCServerTypes
    {
    }
}
