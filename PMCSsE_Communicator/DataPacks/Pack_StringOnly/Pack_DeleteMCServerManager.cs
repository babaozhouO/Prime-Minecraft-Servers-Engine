using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：请求删除指定 Manager 的操作包。
    /// 继承自 Pack_ManagerOperation，构造时需提供 ManagerID。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_DeleteMCServerManager(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
