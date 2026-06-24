using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知 MC 服务端退出的推送。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 字段用于标识目标管理器。
    /// </summary>
    /// <remarks>
    /// 使用指定的管理器ID初始化包。
    /// </remarks>
    /// <param name="managerID">目标管理器ID。</param>
    [ProtoContract(SkipConstructor =true)]
    public partial class Pack_MCServerExited(string managerID):Pack_ManagerOperation(managerID)
    {

    }
}
