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
        /// 获取已加载的MC服务端管理器
        /// </summary>
        GetLoadedMCServerManagers,
        /// <summary>
        /// 获取支持的服务端列表
        /// </summary>
        GetSupportedMCServerTypes,
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
        /// 修改管理器配置
        /// </summary>
        ModifyMCServerManagerConfig,
        /// <summary>
        /// 运行服务端
        /// </summary>
        RunMCServer,
        /// <summary>
        /// 发送命令
        /// </summary>
        SendCommand,
        /// <summary>
        /// 关闭服务端
        /// </summary>
        ShutdownMCServer,
        /// <summary>
        /// 杀死服务端
        /// </summary>
        KillMCServer,
    }
    internal enum RequestTypeEnum_Private
    {
        GetMCServerManagersList,
        GetLoadedMCServerManagers,
        GetSupportedMCServerTypes,
        CreatNewMCServerManager,
        LoadMCServerManager,
        StopMCServerManager,
        DeleteMCServerManager,
        ModifyMCServerManagerConfig,
        RunMCServer,
        SendCommand,
        ShutdownMCServer,
        KillMCServer,
        //---------------------------
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

