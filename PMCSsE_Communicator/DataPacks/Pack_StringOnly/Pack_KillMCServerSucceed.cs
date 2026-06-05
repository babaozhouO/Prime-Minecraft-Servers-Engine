using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知后端已成功强制终止服务器进程的响应。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID 用于识别目标管理器。
    /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public class Pack_KillMCServerSucceed : Pack_ManagerOperation
    {
        public Pack_KillMCServerSucceed(string managerID) : base(managerID)
        {
        }
    }
}
