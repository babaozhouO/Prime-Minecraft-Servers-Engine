using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知后端尝试强制终止服务器失败的响应。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 以标识目标。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_KillMCServerFailed(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
