using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using LightProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：请求运行指定管理器下的 Minecraft 服务器进程。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 以定位目标。
    /// </summary>
    /// <remarks>
    /// 使用指定的管理器ID初始化运行服务器请求包。
    /// </remarks>
    /// <param name="managerID">目标管理器的唯一标识符。</param>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_RunMCServer(string managerID) : Pack_ManagerOperation(managerID)
    {
    }
}
