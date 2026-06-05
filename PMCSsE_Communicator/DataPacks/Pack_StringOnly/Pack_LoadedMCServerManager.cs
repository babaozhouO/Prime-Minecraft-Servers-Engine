using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知客户端某管理器已加载（运行或准备就绪）。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID 标识目标。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_LoadedMCServerManager : Pack_ManagerOperation
    {
        public Pack_LoadedMCServerManager(string managerID) : base(managerID)
        {
        }
    }
}
