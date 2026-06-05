using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：请求加载（启动）指定的 MCServer 管理器。
    /// 继承自 Pack_ManagerOperation，构造时提供 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_LoadMCServerManager : Pack_ManagerOperation
    {
        public Pack_LoadMCServerManager(string managerID) : base(managerID)
        {
        }
    }
}
