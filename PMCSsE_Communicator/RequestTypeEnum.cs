namespace PMCSsE_Communicator
{
    /// <summary>
    /// 请求类型的枚举
    /// </summary>
    public enum RequestTypeEnum
    {
        /// <summary>
        /// 获取MC服务端管理器列表
        /// </summary>
        GetMCServerManagersList,
        /// <summary>
        /// 创建新的MC服务端管理器
        /// </summary>
        CreatNewMCServerManager,
        /// <summary>
        /// 加载MC服务端管理器
        /// </summary>
        LoadMCServerManager,
        /// <summary>
        /// 停止MC服务端管理器
        /// </summary>
        StopMCServerManager,
        /// <summary>
        /// 删除MC服务端管理器
        /// </summary>
        DeleteMCServerManager,
        /// <summary>
        /// 获取已加载的MC服务端管理器
        /// </summary>
        GetLoadedMCServers
    }
    internal enum RequestTypeEnum_Private
    {
        GetMCServerManagersList,
        CreatNewMCServerManager,
        LoadMCServerManager,
        StopMCServerManager,
        DeleteMCServerManager,
        GetLoadedMCServers,
        ConnectionAlive,
        NeedRSAPublicKey,
        VerifyRSAPublicKeyTimeOut,
        RSAPublicKeyMismatch,
        GotRSAPublicKey,
        AESKey,
        Login,
        Close,
        Unknown,
    }
}

