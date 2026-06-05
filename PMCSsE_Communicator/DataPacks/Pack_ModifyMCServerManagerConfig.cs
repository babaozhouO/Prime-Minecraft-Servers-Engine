using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{    /// <summary>
     /// 数据包：传输单个管理器的配置修改请求或响应。
     /// 包含需要修改或已修改的 MCServerManagerConfig 对象。
     /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public class Pack_ModifyMCServerManagerConfig(MCServerManagerConfig config)
    {
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig = config;
    }
}
