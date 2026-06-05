using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：请求停止（关闭）指定的 MCServer 管理器进程。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_StopMCServerManager : Pack_ManagerOperation
    {
        public Pack_StopMCServerManager(string managerID) : base(managerID)
        {
        }
    }
}
