using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：表示某个管理器已被删除的通知。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 字段用于标识目标管理器。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_DeletedMCServerManager(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
