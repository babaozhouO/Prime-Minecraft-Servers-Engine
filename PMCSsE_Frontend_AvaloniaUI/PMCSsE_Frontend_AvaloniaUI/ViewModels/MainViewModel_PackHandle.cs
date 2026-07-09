using Avalonia.Threading;
using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using PMCSsE_Frontend_AvaloniaUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMCSsE_Frontend_AvaloniaUI.ViewModels
{
    public partial class MainViewModel
    {
        private void HandlePack_ServerState(Pack_ServerState pack)
        {

        }
        private void HandlePack_SaveCiphertextConfigSucceed(Pack_SaveCipthertextConfigSucceed _)
        {
            SendMessage("保存密文配置成功", 3);
        }
        private void HandlePack_SaveCiphertextConfigFailed(Pack_SaveCipthertextConfigFailed _)
        {
            SendMessage("保存密文配置失败", 2);
        }
        private void HandlePack_MCServerManagerConfigs(Pack_MCServerManagerConfigs pack)
        {
            StaticMCServerManagerConfigs.ConfigVersion = pack.MCServerManagerConfigs.ConfigVersion;
            StaticMCServerManagerConfigs.MCServerManagerConfigsList = pack.MCServerManagerConfigs.MCServerManagerConfigsList ?? [];

            Dispatcher.UIThread.Post((state) =>
            {
                AllMCServerManagers_IS.Clear();
                foreach (MCServerManagerConfig mCServerManagerConfig in StaticMCServerManagerConfigs.MCServerManagerConfigsList)
                {
                    AllMCServerManagers_IS.Add(new MCServerManagerConfig_LBItemModel(mCServerManagerConfig));
                }
                SendMessage("加载服务端管理器列表成功", 3);
            }, null);
        }
        private void HandlePack_MCServerManagers(Pack_MCServerManagers pack)
        {

            Dispatcher.UIThread.Post((state) =>
            {
                LoadedMCServerManagers_IS.Clear();
                pack.MCServerManagersData.MCServerManagerDataList?.ForEach((md) =>
                {
                    var mc = AllMCServerManagers_IS.FirstOrDefault(mc => mc.ManagerID == md.ManagerID);
                    if (mc == null) return;
                    LoadedMCServerManagers_IS.Add(new MCServerManager_LBItemModel(mc.Config, md));
                    return;
                });
                SendMessage($"刷新已加载的服务端管理器成功", 3);
            }, null);
        }
        private void HandlePack_SupportedMCServerTypes(Pack_SupportedMCServerTypes pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SupportedMCServerTypes_IS.Clear();
                foreach (var type in pack.SupportedMCServerTypes)
                {
                    SupportedMCServerTypes_IS.Add(type);
                }
                SendMessage("获取支持的服务端类型成功", 3);
            }, null);
        }
        private void HandlePack_CreatedNewMCServerManager(Pack_CreatedNewMCServerManager pack)
        {
            AllMCServerManagers_IS.Add(new MCServerManagerConfig_LBItemModel(pack.MCServerManagerConfig));//此集合无Sort()
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage("新建服务端管理器成功", 3);
            }, null);
        }
        private void HandlePack_CreatNewMCServerManagerFailed(Pack_CreatNewMCServerManagerFailed _)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"新建服务端管理器失败", 2);
            }, null);
        }
        private void HandlePack_LoadedMCServerManager(Pack_LoadedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_LoadMCServerManagerFailed(Pack_LoadMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_StoppedMCServerManager(Pack_StoppedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_StopMCServerManagerFailed(Pack_StopMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_DeletedMCServerManager(Pack_DeletedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"删除服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_DeleteMCServerManagerFailed(Pack_DeleteMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"删除服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_ModifiedMCServerManagerConfig(Pack_ModifiedMCServerManagerConfig pack)
        {
            var mc = LoadedMCServerManagers_IS.FirstOrDefault(mc => mc.ManagerID == pack.MCServerManagerConfig.ManagerID);
            if (mc == null)
            {
                Dispatcher.UIThread.Post((state) =>
                {
                    SendMessage($"修改服务端管理器[{pack.MCServerManagerConfig.ManagerID}]的配置成功但同步失败", 2);
                }, null);
                return;
            }
            mc.Config.MCServerName = pack.MCServerManagerConfig.MCServerName;
            mc.Config.MCServerType = pack.MCServerManagerConfig.MCServerType;
            mc.Config.MCServerDirectory = pack.MCServerManagerConfig.MCServerDirectory;
            mc.Config.JavaPath = pack.MCServerManagerConfig.JavaPath;
            mc.Config.StartUpArguments = pack.MCServerManagerConfig.StartUpArguments;
            mc.Config.BackupManagerConfig = pack.MCServerManagerConfig.BackupManagerConfig;
            mc.Config.OnlineChattingSystemConfig = pack.MCServerManagerConfig.OnlineChattingSystemConfig;
            MCServerName = UsingManager?.Config.MCServerName;
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"修改服务端管理器[{pack.MCServerManagerConfig.ManagerID}]的配置成功", 3);
            }, null);
        }
        private void HandlePack_RunMCServerSucceed(Pack_RunMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端[{pack.ManagerID}]成功", 3);
                if (UsingManager?.ManagerID == pack.ManagerID)
                {
                    UsingManager.State.IsMCServerRunning = true;
                    _canRunMCServer.OnNext(false);
                    _canSendCommand.OnNext(true);
                    _canStopMCServer.OnNext(true);
                    _canKillMCServer.OnNext(true);
                }
            }, null);
        }
        private void HandlePack_RunMCServerFailed(Pack_RunMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_SendCommandSucceed(Pack_SendCommandSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"向服务端[{pack.ManagerID}]发送命令成功", 3);
            }, null);
        }
        private void HandlePack_SendCommandFailed(Pack_SendCommandFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"向服务端[{pack.ManagerID}]发送命令失败", 2);
            }, null);
        }
        private void HandlePack_ShutdownMCServerSucceed(Pack_ShutdownMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"对服务端[{pack.ManagerID}]进行停止操作成功", 3);
            }, null);
        }
        private void HandlePack_ShutdownMCServerFailed(Pack_ShutdownMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"对服务端[{pack.ManagerID}]进行停止操作失败", 2);
            }, null);
        }
        private void HandlePack_KillMCServerSucceed(Pack_KillMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"对服务端[{pack.ManagerID}]进行强制终止操作成功", 3);
            }, null);
        }
        private void HandlePack_KillMCServerFailed(Pack_KillMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"对服务端[{pack.ManagerID}]进行强制终止操作失败", 2);
            }, null);
        }

        private void HandlePack_MCServerExited(Pack_MCServerExited pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"服务端[{pack.ManagerID}]进程已退出", 3);
                if (UsingManager?.ManagerID == pack.ManagerID)
                {
                    UsingManager.State.IsMCServerRunning = false;
                    _canRunMCServer.OnNext(true);
                    _canSendCommand.OnNext(false);
                    _canStopMCServer.OnNext(false);
                    _canKillMCServer.OnNext(false);
                }
            }, null);

        }

        private ulong OldestLogID = ulong.MaxValue;
        private ulong LatestLogID = ulong.MinValue;//0代表没有
        private void HandlePack_MCServerLogs(Pack_MCServerLogs pack)
        {
            if (UsingManager == null)
            {
                return;
            }
            if (pack.ManagerID != UsingManager.ManagerID)
            {
                return;
            }
            if (pack.ResetHint)
            {
                Dispatcher.UIThread.Post((state) =>
                {
                    ServerLogsDocument = new()
                    {
                        FileName = "source.log"
                    };
                    OldestLogID = ulong.MaxValue;
                    LatestLogID = ulong.MinValue;
                }, null);
            }
            if (pack.Logs == null)
            {
                GetNewerLogsTimer.Start();
                return;
            }
            if (pack.Logs.Length == 0)
            {
                GetNewerLogsTimer.Start();
                return;
            }

            StringBuilder sb = new();
            foreach (var log in pack.Logs)
            {
                sb.AppendLine(log.Log);
            }
            string logs = sb.ToString();

            if (LatestLogID == ulong.MinValue)//初次
            {
                OldestLogID = pack.Logs[0].ID;
                LatestLogID = pack.Logs[^1].ID;
                Dispatcher.UIThread.Post((state) =>
                {
                    ServerLogsDocument.BeginUpdate();

                    ServerLogsDocument.Insert(ServerLogsDocument.TextLength, logs);

                    ServerLogsDocument.EndUpdate();
                    _canGetOlderLogs.OnNext(true);
                    GetNewerLogsTimer.Start();
                }, null);
                return;
            }
            if (pack.Logs[0].ID > LatestLogID)//包里最旧新于当前最新
            {
                LatestLogID = pack.Logs[^1].ID;
                Dispatcher.UIThread.Post((state) =>
                {
                    ServerLogsDocument.BeginUpdate();

                    ServerLogsDocument.Insert(ServerLogsDocument.TextLength, logs);

                    if (ServerLogsDocument.LineCount > 30000)
                    {
                        int linesToRemove = ServerLogsDocument.LineCount - 30000;
                        var firstLineToKeep = ServerLogsDocument.GetLineByNumber(linesToRemove + 1);
                        int removeEndOffset = firstLineToKeep.Offset;
                        ServerLogsDocument.Remove(0, removeEndOffset);
                    }

                    ServerLogsDocument.EndUpdate();
                    GetNewerLogsTimer.Start();
                }, null);
                return;
            }
            if (pack.Logs[^1].ID < OldestLogID)//包里最新旧于当前最旧
            {
                OldestLogID = pack.Logs[0].ID;
                Dispatcher.UIThread.Post((state) =>
                {
                    ServerLogsDocument.BeginUpdate();

                    ServerLogsDocument.Insert(0, logs);

                    ServerLogsDocument.EndUpdate();
                }, null);
                return;
            }
        }
        private void HandlePack_ErrorInfo(Pack_ErrorInfo pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"错误信息：{pack.ErrorInfo}", 2);
            }, null);
        }
    }
}
