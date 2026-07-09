using LightProto;

namespace PMCSsE_Communicator.DataPacks.Pack_nothing
{
    /// <summary>
    /// 获取服务器信息（例如CPU/内存占用等）
    /// </summary>
    [ProtoContract(SkipConstructor =true)]
    public partial class Pack_GetServerState
    {
    }
}
