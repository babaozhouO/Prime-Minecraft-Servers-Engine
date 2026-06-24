using LightProto;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base
{
    /// <summary>
    /// 管理器操作数据包的基类，包含 ManagerID 字段并注册所有子类型的 ProtoInclude 映射。
    /// 所有针对特定管理器的请求/响应数据包均继承自此基类。
    /// </summary>
    /// <remarks>
    /// 使用指定的管理器ID初始化操作包。
    /// </remarks>
    /// <param name="managerID">目标管理器的唯一标识符。</param>
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(1, typeof(Pack_LoadMCServerManager))]
    [ProtoInclude(2, typeof(Pack_LoadedMCServerManager))]
    [ProtoInclude(3, typeof(Pack_LoadMCServerManagerFailed))]
    [ProtoInclude(4, typeof(Pack_StopMCServerManager))]
    [ProtoInclude(5, typeof(Pack_StoppedMCServerManager))]
    [ProtoInclude(6, typeof(Pack_StopMCServerManagerFailed))]
    [ProtoInclude(7, typeof(Pack_DeleteMCServerManager))]
    [ProtoInclude(8, typeof(Pack_DeletedMCServerManager))]
    [ProtoInclude(9, typeof(Pack_DeleteMCServerManagerFailed))]
    [ProtoInclude(11,typeof(Pack_RunMCServer))]
    [ProtoInclude(12, typeof(Pack_RunMCServerSucceed))]
    [ProtoInclude(13, typeof(Pack_RunMCServerFailed))]
    [ProtoInclude(14, typeof(Pack_SendCommand))]
    [ProtoInclude(15, typeof(Pack_SendCommandSucceed))]
    [ProtoInclude(16, typeof(Pack_SendCommandFailed))]
    [ProtoInclude(17, typeof(Pack_ShutdownMCServer))]
    [ProtoInclude(18, typeof(Pack_ShutdownMCServerSucceed))]
    [ProtoInclude(19, typeof(Pack_ShutdownMCServerFailed))]
    [ProtoInclude(20, typeof(Pack_KillMCServer))]
    [ProtoInclude(21, typeof(Pack_KillMCServerSucceed))]
    [ProtoInclude(22, typeof(Pack_KillMCServerFailed))]
    [ProtoInclude(23, typeof(Pack_MCServerExited))]
    public partial class Pack_ManagerOperation(string managerID)
    {
        /// <summary>
        /// 目标管理器的唯一标识符。
        /// </summary>
        [ProtoMember(10)]
        public string ManagerID = managerID;
    }
}
