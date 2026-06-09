using PMCSsE_Communicator;


namespace PMCSsE_Frontend_AvaloniaUI.Models
{
    /// <summary>
    /// MC 服务端管理器配置列表项的视图模型，封装 MCServerManagerConfig 用于前端展示。
    /// </summary>
    public class MCServerManagerConfig_LBItemModel
    {
        internal MCServerManagerConfig_LBItemModel(MCServerManagerConfig config)
        {
            Config = config;
        }
        internal MCServerManagerConfig Config;
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
    }
}
