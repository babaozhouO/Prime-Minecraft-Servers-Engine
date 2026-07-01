using LightProto;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 数据包：通知新建的 MCServer 管理器及其配置。
    /// 用于在创建管理器成功后将 MCServerManagerConfig 发送给接收端。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_CreatedNewMCServerManager(MCServerManagerConfig mCServerManagerConfig)
    {
        /// <summary>
        /// 新创建的管理器配置对象。
        /// </summary>
        [ProtoMember(1)]
        public MCServerManagerConfig MCServerManagerConfig = mCServerManagerConfig;
    }
}
