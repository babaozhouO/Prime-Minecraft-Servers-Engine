using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：请求优雅关闭指定管理器下的 Minecraft 服务器（发送 stop 命令）。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_ShutdownMCServer(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
