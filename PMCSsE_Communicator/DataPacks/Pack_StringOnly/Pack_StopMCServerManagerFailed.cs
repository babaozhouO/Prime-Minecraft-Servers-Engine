using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知停止管理器失败的响应。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_StopMCServerManagerFailed(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
