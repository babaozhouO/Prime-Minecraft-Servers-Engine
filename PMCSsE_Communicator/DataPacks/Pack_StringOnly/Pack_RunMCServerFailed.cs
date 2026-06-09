using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知尝试运行 MCServer 失败的响应。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID 标识目标。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_RunMCServerFailed(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
