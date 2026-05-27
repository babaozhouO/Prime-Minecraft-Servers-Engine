using PMCSsE_Communicator;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    public class MCServerManager_LBItemModel
    {
        internal MCServerManager_LBItemModel(MCServerManagerConfig config, MCServerManagerData state)
        {
            Config = config;
            State = state;
        }
        internal MCServerManagerConfig Config;
        internal MCServerManagerData State;
        public string ManagerID => Config.ManagerID;
        public string ServerName => Config.MCServerName;
        public string MCServerType => Config.MCServerType;
        public string MCServerRunningState => State.IsMCServerRunning ? "运行中" : "已停止";

    }
}
