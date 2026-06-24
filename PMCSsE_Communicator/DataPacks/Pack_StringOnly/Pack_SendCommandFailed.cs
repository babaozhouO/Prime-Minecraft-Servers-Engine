using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知发送命令到服务器失败的响应。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_SendCommandFailed(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
