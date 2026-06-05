using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：向后端请求强制终止指定管理器下的 Minecraft 服务器进程。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 以定位目标。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_KillMCServer : Pack_ManagerOperation
    {
        public Pack_KillMCServer(string managerID) : base(managerID)
        {

        }
    }
}
