using LightProto;

namespace PMCSsE_Communicator.DataPacks
{
    /// <summary>
    /// 包：请求后端保存密文配置文件
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class Pack_SaveCipherConfig(byte[] keyBytes)
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [ProtoMember(1)]
        public byte[] KeyBytes = keyBytes;
    }
}
