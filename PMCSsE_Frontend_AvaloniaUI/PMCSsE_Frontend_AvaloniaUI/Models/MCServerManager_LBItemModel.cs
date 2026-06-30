using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    /// <summary>
    /// MC 服务端管理器列表项的视图模型，包含配置和运行状态信息。
    /// </summary>
    public class MCServerManager_LBItemModel
    {
        internal MCServerManager_LBItemModel(MCServerManagerConfig config, MCServerManagerData state)
        {
            Config = config;
            State = state;
        }
        internal MCServerManagerConfig Config;
        internal MCServerManagerData State;
        /// <summary>
        /// 管理器ID。
        /// </summary>
        public string ManagerID => Config.ManagerID;
        /// <summary>
        /// MC 服务器名称。
        /// </summary>
        public string ServerName => Config.MCServerName;
        /// <summary>
        /// MC 服务端类型。
        /// </summary>
        public string MCServerType => Config.MCServerType;
        /// <summary>
        /// MC 服务端运行状态描述文本（"运行中"/"已停止"）。
        /// </summary>
        public string MCServerRunningState => State.IsMCServerRunning ? "运行中" : "已停止";

    }
}
