using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：告知客户端某个管理器配置已被修改并包含新的配置对象。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_ModifiedMCServerManagerConfig(MCServerManagerConfig config)
    {
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig = config;
    }
}
