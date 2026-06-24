using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks
{    /// <summary>
     /// 数据包：传输单个管理器的配置修改请求或响应。
     /// 包含需要修改或已修改的 MCServerManagerConfig 对象。
     /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public partial class Pack_ModifyMCServerManagerConfig(MCServerManagerConfig config)
    {
        /// <summary>
        /// 需要修改或已修改的管理器配置对象。
        /// </summary>
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig = config;
    }
}
