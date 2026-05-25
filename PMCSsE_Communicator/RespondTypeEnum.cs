namespace PMCSsE_Communicator
{
    /// <summary>
    /// 响应类型枚举
    /// </summary>
    public enum RespondTypeEnum
    {
        /// <summary>
        /// 所有MC服务端管理器的配置信息
        /// </summary>
        MCServerManagerConfigs,
        /// <summary>
        /// 已创建新MC服务端管理器
        /// </summary>
        CreatedNewMCServerManager,
        /// <summary>
        /// 创建新MC服务端管理器失败
        /// </summary>
        CreatNewMCServerManagerFailed,
        LoadedMCServerManager,
        LoadMCServerManagerFailed,
        StoppedMCServerManager,
        StopMCServerManagerFailed,
        DeletedMCServerManager,
        DeleteMCServerManagerFailed,
        ErrorInfo

    }
    internal enum RespondTypeEnum_Private
    {
        MCServerManagerConfigs,
        CreatedNewMCServerManager,
        CreatNewMCServerManagerFailed,
        LoadedMCServerManager,
        LoadMCServerManagerFailed,
        StoppedMCServerManager,
        StopMCServerManagerFailed,
        DeletedMCServerManager,
        DeleteMCServerManagerFailed,
        ErrorInfo,
        ConnectionAlive,
        RSAPublicKey,
        NeedAES,
        GotAES,
        Succeed,
        Unkonwn
    }
}
