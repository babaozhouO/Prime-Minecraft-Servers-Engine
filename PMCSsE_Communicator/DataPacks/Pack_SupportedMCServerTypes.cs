using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：后端支持的 Minecraft 服务端类型列表。
    /// 用于将可选的服务端类型（字符串列表）发送给前端以供选择。
    /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public class Pack_SupportedMCServerTypes(List<string> supportedMCServerTypes)
    {
        [ProtoMember(1)]
        public List<string> SupportedMCServerTypes = supportedMCServerTypes;
    }
}
