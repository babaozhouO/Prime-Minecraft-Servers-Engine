using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：传输一组管理器配置（MCServerManagerConfigs）。
    /// 用于客户端获取或更新所有管理器的配置集合。
    /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public class Pack_MCServerManagerConfigs(MCServerManagerConfigs MCServerManagerConfigs)
    {
        [ProtoMember(1)]
        public MCServerManagerConfigs MCServerManagerConfigs = MCServerManagerConfigs;
    }
}
