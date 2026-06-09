using PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly
{
    /// <summary>
    /// 数据包：向指定管理器发送控制台命令。
    /// 继承自 Pack_ManagerOperation，包含 ManagerID 与 Command 字段。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class Pack_SendCommand : Pack_ManagerOperation
    {
        public Pack_SendCommand(string managerID,string command) : base(managerID)
        {
            Command = command;
        }
        /// <summary>
        /// 要发送到 Minecraft 服务器的控制台命令文本。
        /// </summary>
        [ProtoMember(2)]
        public string Command;
    }
}
