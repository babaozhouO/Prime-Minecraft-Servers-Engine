using ProtoBuf;

namespace PMCSsE_Communicator.DataPacks.Pack_StringOnly.Base
{
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
    public class Pack_ManagerOperation
    {
        public Pack_ManagerOperation(string managerID)
        {
            ManagerID = managerID;
        }
        [ProtoMember(10)]
        public string ManagerID;
    }
}
