using PMCSsE_Communicator;


namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    public class MCServerManagerConfig_LBItemModel
    {
        internal MCServerManagerConfig_LBItemModel(MCServerManagerConfig config)
        {
            Config = config;
        }
        internal MCServerManagerConfig Config;
        public string ManagerID => Config.ManagerID;
        public string ServerName => Config.MCServerName;
        public string MCServerType => Config.MCServerType;
    }
}
