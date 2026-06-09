using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：通知删除 MC 服务端管理器失败的响应。
    /// 继承自 Pack_ManagerOperation，携带 ManagerID 字段用于标识目标管理器。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_DeleteMCServerManagerFailed : Pack_ManagerOperation
    {
        /// <summary>
        /// 使用指定的管理器ID初始化删除失败响应包。
        /// </summary>
        /// <param name="managerID">删除失败的目标管理器ID。</param>
        public Pack_DeleteMCServerManagerFailed(string managerID) : base(managerID)
        {
        }
    }
}
