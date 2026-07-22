namespace PMCSsE_Communicator
{
    /// <summary>
    /// 响应类型枚举
    /// </summary>
    public enum RespondTypeEnum
    {
        /// <summary>
        /// 返回服务器状态
        /// </summary>
        ServerState,
        /// <summary>
        /// 保存密文配置成功
        /// </summary>
        SaveCiptherConfigSucceed,
        /// <summary>
        /// 保存密文配置失败
        /// </summary>
        SaveCiptherConfigFailed,
        /// <summary>
        /// 所有MC服务端管理器的配置信息
        /// </summary>
        MCServerManagerConfigs,
        /// <summary>
        /// 所有已加载的MC服务端管理器
        /// </summary>
        LoadedMCServerManagers,
        /// <summary>
        /// 支持的服务端类型
        /// </summary>
        SupportedMCServerTypes,
        /// <summary>
        /// 已创建新MC服务端管理器
        /// </summary>
        CreatedNewMCServerManager,
        /// <summary>
        /// 创建新MC服务端管理器失败
        /// </summary>
        CreatNewMCServerManagerFailed,
        /// <summary>
        /// 加载服务端管理器成功
        /// </summary>
        LoadedMCServerManager,
        /// <summary>
        /// 加载服务端管理器失败
        /// </summary>
        LoadMCServerManagerFailed,
        /// <summary>
        /// 停止服务端管理器成功
        /// </summary>
        StoppedMCServerManager,
        /// <summary>
        /// 停止服务端管理器失败
        /// </summary>
        StopMCServerManagerFailed,
        /// <summary>
        /// 删除服务端管理器成功
        /// </summary>
        DeletedMCServerManager,
        /// <summary>
        /// 删除服务端管理器失败
        /// </summary>
        DeleteMCServerManagerFailed, 
        /// <summary>
        /// 修改服务端管理器配置成功
        /// </summary>
        ModifiedMCServerManagerConfig,
        /// <summary>
        /// 启动服务端成功
        /// </summary>
        RunMCServerSucceed,
        /// <summary>
        /// 启动服务端失败
        /// </summary>
        RunMCServerFailed,
        /// <summary>
        /// 向服务端发送命令成功
        /// </summary>
        SendCommandSucceed,
        /// <summary>
        /// 向服务端发送命令失败
        /// </summary>
        SendCommandFailed,
        /// <summary>
        /// 关闭服务端成功
        /// </summary>
        ShutdownMCServerSucceed,
        /// <summary>
        /// 关闭服务端失败
        /// </summary>
        ShutdownMCServerFailed,
        /// <summary>
        /// 杀死服务端成功
        /// </summary>
        KillMCServerSucceed,
        /// <summary>
        /// 杀死服务端失败
        /// </summary>
        KillMCServerFailed,
        /// <summary>
        /// 服务端进程已退出
        /// </summary>
        MCServerExited,
        /// <summary>
        /// 服务端日志
        /// </summary>
        MCServerLogs,
        /// <summary>
        /// 错误信息
        /// </summary>
        ErrorInfo

    }
    internal enum RespondTypeEnum_Private
    {
        ServerState,
        SaveCipthertextConfigSucceed,
        SaveCipthertextConfigFailed,
        MCServerManagerConfigs,
        LoadedMCServerManagers,
        SupportedMCServerTypes,
        CreatedNewMCServerManager,
        CreatNewMCServerManagerFailed,
        LoadedMCServerManager,
        LoadMCServerManagerFailed,
        StoppedMCServerManager,
        StopMCServerManagerFailed,
        DeletedMCServerManager,
        DeleteMCServerManagerFailed,
        ModifiedMCServerManagerConfig,
        RunMCServerSucceed,
        RunMCServerFailed,
        SendCommandSucceed,
        SendCommandFailed,
        ShutdownMCServerSucceed,
        ShutdownMCServerFailed,
        KillMCServerSucceed,
        KillMCServerFailed,
        MCServerExited,
        MCServerLogs,
        ErrorInfo,
        //------------------------
        ConnectionAlive,
        RSAPublicKey,
        NeedAES,
        GotAES,
        Succeed,
        KeyWrong,
        Disconnect,
        Unkonwn
    }
}
