/*Copyright 2025 八宝粥(1749861851@qq.com)

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using PMCSsE_Backend.PluginsSystem;
using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using PMCSsE_Communicator.SharedCodes;
using Renci.SshNet.Security;
using System.Security.Cryptography;

namespace PMCSsE_Backend.Modules
{
    internal static class MCServerManagers_Manager
    {
        internal static NativeServer? NativeServer;
        private static readonly List<MCServerManager> LoadedMCServerManagersList = [];
        internal static readonly List<string> SupportedMCServerTypes = ["Vanilla"];
        internal static event Action ExitCalled = delegate { };
        /// <summary>
        /// 获取原生服务器的数据包总线，用于订阅和发布数据包。
        /// 若原生服务器未初始化则返回 null。
        /// </summary>
        public static DataPackBus? DataPackBus => NativeServer?.DataPackBus;
        internal static void Initialize()
        {
            NativeServer = new(StaticConfig_Plaintext.ListenAddress, StaticConfig_Plaintext.ListenPort, RunningStateRecorder.Debug);

            NativeServer.ReportLog += (log) =>
            {
                StaticTools.HandleLog(log);
            };
            NativeServer.StartServerFailed += (reason) =>
            {
                StaticTools.HandleLog($"原生服务器启动时发生致命性错误:{reason}");
                StaticTools.HandleLog("正在停止程序");
                ExitCalled();
            };
            NativeServer.ReceivedKey += (key) =>
            {
                if (StaticConfigManager.ReadCiphertextConfig)//已解密并读取
                {
                    byte[] saltedLoginKeyHash = ConfigCrypto.DeriveKey(key, StaticConfig_Ciphertext.SaltOfLoginKey);
                    if (saltedLoginKeyHash.SequenceEqual(StaticConfig_Ciphertext.SaltedLoginKeyHash))
                    {
                        NativeServer.VerifyKey(true);
                    }
                    else
                    {
                        NativeServer.VerifyKey(false);
                    }
                    CryptographicOperations.ZeroMemory(saltedLoginKeyHash.AsSpan());
                }
                else//未解密
                {
                    byte[] saltedCipherConfigKeyHash = ConfigCrypto.DeriveKey(key, StaticConfig_Plaintext.SaltOfCipherConfigKey);
                    if (!StaticConfigManager.LoadConfig_Ciphertext(saltedCipherConfigKeyHash))
                    {
                        NativeServer.VerifyKey(false);
                        CryptographicOperations.ZeroMemory(saltedCipherConfigKeyHash.AsSpan());
                        return;
                    }
                    //解密、加载成功
                    CryptographicOperations.ZeroMemory(saltedCipherConfigKeyHash.AsSpan());
                    StaticConfigManager.ReadCiphertextConfig = true;

                    byte[] saltedLoginKeyHash = ConfigCrypto.DeriveKey(key, StaticConfig_Ciphertext.SaltOfLoginKey);
                    if (saltedLoginKeyHash.SequenceEqual(StaticConfig_Ciphertext.SaltedLoginKeyHash))
                    {
                        NativeServer.VerifyKey(true);
                    }
                    else
                    {
                        NativeServer.VerifyKey(false);
                    }
                    CryptographicOperations.ZeroMemory(saltedLoginKeyHash.AsSpan());
                }
            };

            NativeServer.DataPackBus.Subscribe<Pack_GetServerState>(HandlePack_GetServerState);
            NativeServer.DataPackBus.Subscribe<Pack_SaveCipherConfig>(HandlePack_SaveCipherConfig);
            NativeServer.DataPackBus.Subscribe<Pack_GetMCServerManagerConfigsList>(HandlePack_GetMCServerManagersList);
            NativeServer.DataPackBus.Subscribe<Pack_GetMCServerManager>(HandlePack_GetMCServerManager);
            NativeServer.DataPackBus.Subscribe<Pack_GetSupportedMCServerTypes>(HandlePack_GetSupportedMCServerTypes);

            NativeServer.DataPackBus.Subscribe<Pack_CreatNewMCServerManager>(HandlePack_CreatNewMCServerManager);

            NativeServer.DataPackBus.Subscribe<Pack_LoadMCServerManager>(HandlePack_LoadMCServerManager);

            NativeServer.DataPackBus.Subscribe<Pack_StopMCServerManager>(HandlePack_StopMCServerManager);

            NativeServer.DataPackBus.Subscribe<Pack_DeleteMCServerManager>(HandlePack_DeleteMCServerManager);

            NativeServer.DataPackBus.Subscribe<Pack_ModifyMCServerManagerConfig>(HandlePack_ModifyMCServerConfig);

            NativeServer.DataPackBus.Subscribe<Pack_RunMCServer>(HandlePack_RunMCServer);
            NativeServer.DataPackBus.Subscribe<Pack_SendCommand>(HandlePack_SendCommand);
            NativeServer.DataPackBus.Subscribe<Pack_ShutdownMCServer>(HandlePack_ShutdownMCServer);
            NativeServer.DataPackBus.Subscribe<Pack_KillMCServer>(HandlePack_KillMCServer);

            NativeServer.DataPackBus.Subscribe<Pack_GetLatestMCServerLogs>(HandlePack_GetLatestMCServerLogs);
            NativeServer.DataPackBus.Subscribe<Pack_GetNewerMCServerLogs>(HandlePack_GetNewerMCServerLogs);
            NativeServer.DataPackBus.Subscribe<Pack_GetOlderMCServerLogs>(HandlePack_GetOlderMCServerLogs);
            PluginsManager.LoadAllPlugins();
            PluginsManager.SpecialMCServerFeaturesProviders.ForEach((provider) =>
            {
                if (!SupportedMCServerTypes.Contains(provider.TargetMCServerType))
                    SupportedMCServerTypes.Add(provider.TargetMCServerType);
            });
            StaticTools.HandleLog("正在初始化服务器状态监视器");
            ServerMonitor.Initialize();
            NativeServer.StartService();
        }
        private static void HandlePack_GetServerState(Pack_GetServerState _)
        {
            StaticTools.HandleLog("客户端请求获取服务器状态");
            if (!ServerMonitor.GotInfo)
            {
                StaticTools.HandleLog("尚未获取到硬件信息");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("尚未获取到硬件信息"));
                return;
            }
            NativeServer?.RespondClient(RespondTypeEnum.ServerState, new Pack_ServerState(DateTime.Now, ServerMonitor.CPUs, ServerMonitor.TotalMemory, ServerMonitor.UsingMemory));
        }
        private static void HandlePack_SaveCipherConfig(Pack_SaveCipherConfig pack)
        {
            StaticTools.HandleLog($"客户端请求保存密文配置文件");
            byte[] saltedLoginKeyHash = ConfigCrypto.DeriveKey(pack.KeyBytes, StaticConfig_Ciphertext.SaltOfLoginKey);
            if (saltedLoginKeyHash.SequenceEqual(StaticConfig_Ciphertext.SaltedLoginKeyHash))
            {
                StaticTools.HandleLog($"客户端发送的访问密钥正确");
            }
            else
            {
                StaticTools.HandleLog($"客户端发送的访问密钥错误");
                StaticTools.HandleLog($"保存密文配置文件失败");
                NativeServer?.RespondClient(RespondTypeEnum.SaveCiptherConfigFailed, new Pack_SaveCipthertextConfigFailed());
                return;
            }
            CryptographicOperations.ZeroMemory(saltedLoginKeyHash.AsSpan());

            byte[] saltedCipherConfigKeyHash = ConfigCrypto.DeriveKey(pack.KeyBytes, StaticConfig_Plaintext.SaltOfCipherConfigKey);
            CryptographicOperations.ZeroMemory(pack.KeyBytes.AsSpan());
            bool isSucceed = StaticConfigManager.SaveConfig_Ciphertext(saltedCipherConfigKeyHash);
            CryptographicOperations.ZeroMemory(saltedCipherConfigKeyHash.AsSpan());
            if (isSucceed)
            {
                StaticTools.HandleLog($"保存密文配置文件成功");
                NativeServer?.RespondClient(RespondTypeEnum.SaveCiptherConfigSucceed, new Pack_SaveCipthertextConfigSucceed());
            }
            else
            {
                StaticTools.HandleLog($"保存密文配置文件失败");
                NativeServer?.RespondClient(RespondTypeEnum.SaveCiptherConfigFailed, new Pack_SaveCipthertextConfigFailed());
            }
        }
        private static void HandlePack_GetMCServerManagersList(Pack_GetMCServerManagerConfigsList _)
        {
            if (NativeServer == null) { return; }
            StaticTools.HandleLog($"客户端请求获取所有管理器");
            MCServerManagerConfigs mCServerManagerConfigs = new();
            {
                mCServerManagerConfigs.ConfigVersion = StaticConfig_Ciphertext.ConfigVersion;
                mCServerManagerConfigs.MCServerManagerConfigsList = StaticConfig_Ciphertext.MCServerManagerConfigsList;
            }
            NativeServer.RespondClient(RespondTypeEnum.MCServerManagerConfigs, new Pack_MCServerManagerConfigs(mCServerManagerConfigs));
        }
        private static void HandlePack_GetMCServerManager(Pack_GetMCServerManager _)
        {
            if (NativeServer == null) { return; }
            StaticTools.HandleLog($"客户端请求获取已加载的管理器");
            NativeServer.RespondClient(RespondTypeEnum.LoadedMCServerManagers, new Pack_MCServerManagers(GetLoadedMCServerManagers()));
        }
        private static void HandlePack_GetSupportedMCServerTypes(Pack_GetSupportedMCServerTypes _)
        {
            if (NativeServer == null) { return; }
            StaticTools.HandleLog($"客户端请求获取支持的服务端类型");
            NativeServer.RespondClient(RespondTypeEnum.SupportedMCServerTypes, new Pack_SupportedMCServerTypes(SupportedMCServerTypes));
        }
        private static void HandlePack_CreatNewMCServerManager(Pack_CreatNewMCServerManager _)
        {
            if (NativeServer == null) { return; }
            MCServerManagerConfig? mCServerManagerConfig = CreatNewMCServerManager();
            if (mCServerManagerConfig == null)
            {
                StaticTools.HandleLog($"客户端请求添加新的MC服务端管理器失败");
                NativeServer.RespondClient(RespondTypeEnum.CreatNewMCServerManagerFailed, new Pack_CreatNewMCServerManagerFailed());
            }
            else
            {
                StaticTools.HandleLog($"客户端添加了新的MC服务端管理器");
                NativeServer.RespondClient(RespondTypeEnum.CreatedNewMCServerManager, new Pack_CreatedNewMCServerManager(mCServerManagerConfig));
            }
        }
        private static void HandlePack_LoadMCServerManager(Pack_LoadMCServerManager pack)
        {
            if (NativeServer == null) { return; }
            switch (LoadMCServerManager(pack.ManagerID))
            {
                case 0:
                    StaticTools.HandleLog($"MC服务端管理器[{pack.ManagerID}]已被成功加载");
                    NativeServer.RespondClient(RespondTypeEnum.LoadedMCServerManager, new Pack_LoadedMCServerManager(pack.ManagerID));
                    break;
                case 1:
                    {
                        string einfo = $"加载MC服务端管理器[{pack.ManagerID}]失败：已经加载了";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.LoadMCServerManagerFailed, new Pack_LoadMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
                case 2:
                    {
                        string einfo = $"加载MC服务端管理器[{pack.ManagerID}]失败：未找到指定的MC服务端管理器";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.LoadMCServerManagerFailed, new Pack_LoadMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
            }
        }
        private static void HandlePack_StopMCServerManager(Pack_StopMCServerManager pack)
        {
            if (NativeServer == null) { return; }
            switch (StopMCServerManager(pack.ManagerID))
            {
                case 0:
                    StaticTools.HandleLog($"MC服务端管理器[{pack.ManagerID}]已被成功停止");
                    NativeServer.RespondClient(RespondTypeEnum.StoppedMCServerManager, new Pack_StoppedMCServerManager(pack.ManagerID));
                    break;
                case 1:
                    {
                        string einfo = $"停止MC服务端管理器[{pack.ManagerID}]失败：未启动";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.StopMCServerManagerFailed, new Pack_StopMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
                case 2:
                    {
                        string einfo = $"停止MC服务端管理器[{pack.ManagerID}]失败：MC服务端仍在运行";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.StopMCServerManagerFailed, new Pack_StopMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
            }
        }
        private static void HandlePack_DeleteMCServerManager(Pack_DeleteMCServerManager pack)
        {
            if (NativeServer == null) { return; }
            switch (DeleteMCServerManager(pack.ManagerID))
            {
                case 0:
                    StaticTools.HandleLog($"MC服务端管理器[{pack.ManagerID}]已被成功删除");
                    NativeServer.RespondClient(RespondTypeEnum.DeletedMCServerManager, new Pack_DeletedMCServerManager(pack.ManagerID));
                    break;
                case 1:
                    {
                        string einfo = $"MC服务端管理器[{pack.ManagerID}]删除失败：不存在";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.DeleteMCServerManagerFailed, new Pack_DeleteMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
                case 2:
                    {
                        string einfo = $"MC服务端管理器[{pack.ManagerID}]删除失败：保存数据失败";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.DeleteMCServerManagerFailed, new Pack_DeleteMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
                case 3:
                    {
                        string einfo = $"MC服务端管理器[{pack.ManagerID}]删除失败：MC服务端仍在运行";
                        StaticTools.HandleLog(einfo);
                        NativeServer.RespondClient(RespondTypeEnum.DeleteMCServerManagerFailed, new Pack_DeleteMCServerManagerFailed(pack.ManagerID));
                        NativeServer.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(einfo));
                        break;
                    }
            }
        }
        private static void HandlePack_ModifyMCServerConfig(Pack_ModifyMCServerManagerConfig pack)
        {
            StaticTools.HandleLog($"前端请求修改ID为[{pack.MCServerManagerConfig.ManagerID}]配置文件");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.MCServerManagerConfig.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog("未找到要修改的MC服务端管理器配置");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("未找到要修改的MC服务端管理器配置"));
                return;
            }
            if (m.isMCServerRunning)
            {
                StaticTools.HandleLog("前端在服务端仍在运行的情况下编辑配置文件");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("在服务端仍在运行的情况下编辑配置文件"));
                return;
            }
            m.MCServerManagerConfig.MCServerName = pack.MCServerManagerConfig.MCServerName;//引用，可直接修改到静态配置
            m.MCServerManagerConfig.MCServerType = pack.MCServerManagerConfig.MCServerType;
            m.MCServerManagerConfig.MCServerDirectory = pack.MCServerManagerConfig.MCServerDirectory;
            string javaFileName = Path.GetFileName(pack.MCServerManagerConfig.JavaPath);
            if (javaFileName == "javaw.exe"
                || javaFileName == "java.exe"
                || javaFileName == "java"
                || javaFileName == "javaw")
            {
                m.MCServerManagerConfig.JavaPath = pack.MCServerManagerConfig.JavaPath;
            }
            else
            {
                StaticTools.HandleLog("前端指定的Java路径不包含有效的java可执行文件，跳过此设置项");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("指定的Java路径不包含有效的java可执行文件，跳过此设置项"));
            }
            bool isSafeArg = true;
            foreach (var dangerous in MCServerManager.DangerousArgs)
            {
                if (pack.MCServerManagerConfig.StartUpArguments.Contains(dangerous, StringComparison.OrdinalIgnoreCase))
                {
                    isSafeArg = false;
                    StaticTools.HandleLog($"启动参数包含高危项: {dangerous}，跳过此设置项");
                    NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"启动参数包含高危项: {dangerous}，跳过此设置项"));
                    break;
                }
            }
            if (isSafeArg)
            {
                m.MCServerManagerConfig.StartUpArguments = pack.MCServerManagerConfig.StartUpArguments;
            }
            m.MCServerManagerConfig.BackupManagerConfig = pack.MCServerManagerConfig.BackupManagerConfig;
            m.MCServerManagerConfig.OnlineChattingSystemConfig = pack.MCServerManagerConfig.OnlineChattingSystemConfig;
            StaticTools.HandleLog($"修改ID为[{pack.MCServerManagerConfig.ManagerID}]配置文件成功");
            NativeServer?.RespondClient(RespondTypeEnum.ModifiedMCServerManagerConfig, new Pack_ModifiedMCServerManagerConfig(pack.MCServerManagerConfig));
        }
        private static void HandlePack_RunMCServer(Pack_RunMCServer pack)
        {
            StaticTools.HandleLog($"前端请求运行ID为[{pack.ManagerID}]的服务端");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.RunMCServerFailed, new Pack_RunMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            if (m.isMCServerRunning)
            {
                StaticTools.HandleLog("前端在服务端仍在运行的情况下启动服务器");
                NativeServer?.RespondClient(RespondTypeEnum.RunMCServerFailed, new Pack_RunMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("在服务端仍在运行的情况下启动服务器"));
                return;
            }
            string error;
            switch (m.StartMCServer())
            {
                case 0:
                    StaticTools.HandleLog($"服务端[{m.MCServerManagerConfig.ManagerID}]启动成功");

                    void HandleMCServerExited(string managerID, bool state)
                    {
                        m.MCServerRunningStateChanged -= HandleMCServerExited;
                        if (!state)
                        {
                            StaticTools.HandleLog($"服务端[{pack.ManagerID}]进程已退出");
                            NativeServer?.RespondClient(RespondTypeEnum.MCServerExited, new Pack_MCServerExited(managerID));
                        }
                    }
                    m.MCServerRunningStateChanged += HandleMCServerExited;

                    NativeServer?.RespondClient(RespondTypeEnum.RunMCServerSucceed, new Pack_RunMCServerSucceed(pack.ManagerID));
                    return;
                case 1:
                    error = "目录为空";
                    break;
                case 2:
                    error = "目录格式不正确";
                    break;
                case 3:
                    error = "Java路径为空";
                    break;
                case 4:
                    error = "启动参数为空";
                    break;
                case 5:
                    error = "服务端启动失败";
                    break;
                case 6:
                    error = "服务端仍在运行";
                    break;
                default:
                    error = "未知错误";
                    break;
            }
            StaticTools.HandleLog($"启动ID为[{pack.ManagerID}]的服务端失败");
            NativeServer?.RespondClient(RespondTypeEnum.RunMCServerFailed, new Pack_RunMCServerFailed(pack.ManagerID));
            NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo(error));
        }
        private static void HandlePack_SendCommand(Pack_SendCommand pack)
        {
            StaticTools.HandleLog($"前端请求向ID为[{pack.ManagerID}]的服务端发送命令[{pack.Command}]");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.SendCommandFailed, new Pack_SendCommandFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            if (!m.isMCServerRunning)
            {
                StaticTools.HandleLog("前端在服务端未在运行的情况下发送命令");
                NativeServer?.RespondClient(RespondTypeEnum.SendCommandFailed, new Pack_SendCommandFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"前端在服务端未在运行的情况下发送命令"));
                return;
            }
            if (m.SendCommand(pack.Command))
            {
                StaticTools.HandleLog($"成功向ID为[{pack.ManagerID}]的服务端发送命令[{pack.Command}]");
                NativeServer?.RespondClient(RespondTypeEnum.SendCommandSucceed, new Pack_SendCommandSucceed(pack.ManagerID));
                return;
            }
            else
            {
                StaticTools.HandleLog($"向ID为[{pack.ManagerID}]的服务端发送命令[{pack.Command}]失败");
                NativeServer?.RespondClient(RespondTypeEnum.SendCommandFailed, new Pack_SendCommandFailed(pack.ManagerID));
            }
        }
        private static void HandlePack_ShutdownMCServer(Pack_ShutdownMCServer pack)
        {
            StaticTools.HandleLog($"前端请求停止ID为[{pack.ManagerID}]的服务端");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.ShutdownMCServerFailed, new Pack_ShutdownMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            if (!m.isMCServerRunning)
            {
                StaticTools.HandleLog("前端在服务端未在运行的情况下停止服务器");
                NativeServer?.RespondClient(RespondTypeEnum.ShutdownMCServerFailed, new Pack_ShutdownMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("在服务端未在运行的情况下停止服务器"));
                return;
            }
            if (m.ShutdownMCServer())
            {
                StaticTools.HandleLog($"对ID为[{m.MCServerManagerConfig.ManagerID}]的服务端进行停止操作成功");
                NativeServer?.RespondClient(RespondTypeEnum.ShutdownMCServerSucceed, new Pack_ShutdownMCServerSucceed(pack.ManagerID));
                return;
            }
            else
            {
                StaticTools.HandleLog($"对ID为[{m.MCServerManagerConfig.ManagerID}]的服务端进行停止操作失败");
                NativeServer?.RespondClient(RespondTypeEnum.ShutdownMCServerFailed, new Pack_ShutdownMCServerFailed(pack.ManagerID));
            }
        }
        private static void HandlePack_KillMCServer(Pack_KillMCServer pack)
        {
            StaticTools.HandleLog($"前端请求强制终止ID为[{pack.ManagerID}]的服务端");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.KillMCServerFailed, new Pack_KillMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            if (!m.isMCServerRunning)
            {
                StaticTools.HandleLog("前端在服务端未在运行的情况下强制终止服务器");
                NativeServer?.RespondClient(RespondTypeEnum.KillMCServerFailed, new Pack_KillMCServerFailed(pack.ManagerID));
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo("在服务端未在运行的情况下强制终止服务器"));
                return;
            }
            if (m.KillMCServer())
            {
                StaticTools.HandleLog($"对ID为[{m.MCServerManagerConfig.ManagerID}]的服务端进行强制终止操作成功");
                NativeServer?.RespondClient(RespondTypeEnum.KillMCServerSucceed, new Pack_KillMCServerSucceed(pack.ManagerID));
                return;
            }
            else
            {
                StaticTools.HandleLog($"对ID为[{m.MCServerManagerConfig.ManagerID}]的服务端进行强制终止操作失败");
                NativeServer?.RespondClient(RespondTypeEnum.KillMCServerFailed, new Pack_KillMCServerFailed(pack.ManagerID));
            }
        }
        private static void HandlePack_GetLatestMCServerLogs(Pack_GetLatestMCServerLogs pack)
        {
            StaticTools.HandleLog($"前端请求获取ID为[{pack.ManagerID}]的服务端的{pack.Count}条最新日志");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            NativeServer?.RespondClient(RespondTypeEnum.MCServerLogs, new Pack_MCServerLogs(pack.ManagerID, m.ServerLogs.GetLatest(pack.Count), false));
        }
        private static void HandlePack_GetNewerMCServerLogs(Pack_GetNewerMCServerLogs pack)
        {
            StaticTools.HandleLog($"前端请求获取ID为[{pack.ManagerID}]的服务端的第{pack.StartIndex + 1}~{pack.StartIndex + 1 + (ulong)pack.Count}条日志");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            NativeServer?.RespondClient(RespondTypeEnum.MCServerLogs, new Pack_MCServerLogs(pack.ManagerID, m.ServerLogs.GetAfter(pack.StartIndex, pack.Count, out bool resetHint), resetHint));
        }
        private static void HandlePack_GetOlderMCServerLogs(Pack_GetOlderMCServerLogs pack)
        {
            if (pack.Count > 2000)
            {
                StaticTools.HandleLog("请求数量过多");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"请求数量过多"));
                return;
            }
            if (pack.EndIndex == 1)
            {
                StaticTools.HandleLog("没有更旧的日志了");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"没有更旧的日志了"));
                return;
            }
            if (pack.Count > pack.EndIndex - 1)
            {
                pack.Count = (uint)pack.EndIndex - 1;
            }
            StaticTools.HandleLog($"前端请求获取ID为[{pack.ManagerID}]的服务端的第{pack.EndIndex - 1 - pack.Count}~{pack.EndIndex - 1}条日志");
            var m = LoadedMCServerManagersList.FirstOrDefault(mc => mc.MCServerManagerConfig.ManagerID == pack.ManagerID);
            if (m == null)
            {
                StaticTools.HandleLog($"未找到指定的MC服务端管理器[{pack.ManagerID}]");
                NativeServer?.RespondClient(RespondTypeEnum.ErrorInfo, new Pack_ErrorInfo($"未找到指定的MC服务端管理器[{pack.ManagerID}]"));
                return;
            }
            NativeServer?.RespondClient(RespondTypeEnum.MCServerLogs, new Pack_MCServerLogs(pack.ManagerID, m.ServerLogs.GetBefore(pack.EndIndex, (int)pack.Count, out bool resetHint), resetHint));


        }
        private static MCServerManagerConfig? CreatNewMCServerManager()
        {
            string NewID = GenerateNewID();
            if (string.IsNullOrEmpty(NewID))
            {
                return null;
            }
            MCServerManagerConfig mCServerManagerConfig = new() { ManagerID = NewID };

            StaticConfig_Ciphertext.MCServerManagerConfigsList.Add(mCServerManagerConfig);
            StaticConfig_Ciphertext.MCServerManagerConfigsList.Sort((a, b) =>
                int.Parse(a.ManagerID).CompareTo(int.Parse(b.ManagerID)));

            return mCServerManagerConfig;
        }
        private static string GenerateNewID()
        {
            List<int> IDs = [];
            foreach (MCServerManagerConfig singleMCServerManagerConfigInfo1 in StaticConfig_Ciphertext.MCServerManagerConfigsList)
            {
                try
                {
                    int ID = Convert.ToInt32(singleMCServerManagerConfigInfo1.ManagerID);
                    IDs.Add(ID);
                }
                catch
                {
                    StaticTools.HandleLog("生成新ID时发生字符串->int32转换异常");
                    StaticTools.HandleLog("可能的原因：手动修改了配置文件，数据损坏");
                    return string.Empty;
                }
            }
            int NewID = -1;
            for (int i = 1; i < 999; i++)
            {
                if (!(IDs.Contains(i)))
                {
                    NewID = i;
                    break;
                }
            }

            string newID;
            if (NewID == -1)
            {
                return "";
            }
            else
            {
                newID = NewID.ToString();
                if (newID.Length == 1)
                {
                    newID = $"00{newID}";
                }
                else if (newID.Length == 2)
                {
                    newID = $"0{newID}";
                }
            }
            return newID;
        }
        /// <summary>
        /// 返回值：0加载成功，1已加载，2未找到
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private static int LoadMCServerManager(string ID)
        {
            foreach (var item in LoadedMCServerManagersList)
            {
                if (item.MCServerManagerConfig.ManagerID == ID)
                {
                    return 1;
                }
            }
            MCServerManagerConfig? mCServerManagerConfig = null;
            foreach (var item in StaticConfig_Ciphertext.MCServerManagerConfigsList)
            {
                if (item.ManagerID == ID)
                {
                    mCServerManagerConfig = item;
                    break;
                }
            }
            if (mCServerManagerConfig != null)
            {
                MCServerManager mCServerManager = new(mCServerManagerConfig);
                LoadedMCServerManagersList.Add(mCServerManager);
                mCServerManager.ReportManagerLog += HandleManagerReportLog;
                return 0;
            }
            return 2;
        }
        private static void HandleManagerReportLog(string managerID, string log)
        {
            StaticTools.HandleLog($"MC服务端管理器[{managerID}]:{log}");
        }
        /// <summary>
        /// 0成功，1未启动，2服务端仍在运行
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private static int StopMCServerManager(string ID)
        {
            foreach (var item in LoadedMCServerManagersList)
            {
                if (item.MCServerManagerConfig.ManagerID == ID)
                {
                    if (item.isMCServerRunning)
                    {
                        return 2;
                    }
                    LoadedMCServerManagersList.Remove(item);
                    item.ReportManagerLog -= HandleManagerReportLog;
                    item.Dispose();
                    return 0;
                }
            }
            return 1;
        }
        /// <summary>
        /// 返回值：0成功，1未找到，2已删除但保存配置文件失败，3服务端仍在运行
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private static int DeleteMCServerManager(string ID)
        {
            // 1. 找到目标配置
            var config = StaticConfig_Ciphertext.MCServerManagerConfigsList
                .FirstOrDefault(c => c.ManagerID == ID);
            if (config == null) return 1;

            // 2. 获取所有已加载且关联到该配置的项
            var loadedItems = LoadedMCServerManagersList
                .Where(item => item.MCServerManagerConfig == config)
                .ToList();

            // 3. 检查是否有正在运行的实例
            if (loadedItems.Any(item => item.isMCServerRunning))
                return 3;

            // 4. 从 LoadedMCServerManagersList 中安全删除（使用 RemoveAll）
            LoadedMCServerManagersList.RemoveAll(item => item.MCServerManagerConfig == config);

            // 5. 释放资源（在删除之后进行，避免影响列表操作）
            foreach (var item in loadedItems)
                item.Dispose();

            // 6. 删除配置
            StaticConfig_Ciphertext.MCServerManagerConfigsList.Remove(config);

            return 0;
        }
        private static MCServerManagersData GetLoadedMCServerManagers()
        {
            MCServerManagersData result = new();
            foreach (var item in LoadedMCServerManagersList)
            {
                result.MCServerManagerDataList.Add(new()
                {
                    ManagerID = item.MCServerManagerConfig.ManagerID,
                    IsMCServerRunning = item.isMCServerRunning
                });
            }
            return result;
        }
        internal static void ShutDown()
        {
            StaticTools.HandleLog("正在关闭原生服务器");
            NativeServer?.Dispose();
            StaticTools.HandleLog("已关闭原生服务器");
            StaticTools.HandleLog("正在关闭所有MC服务器");
            foreach (var item in LoadedMCServerManagersList)
            {
                if (item.isMCServerRunning)
                {
                    item.ShutdownMCServer();
                    while (item.isMCServerRunning)
                    {
                        Thread.Sleep(200);
                    }
                }
                item.Dispose();
                StaticTools.HandleLog($"已关闭MC服务器：{item.MCServerManagerConfig.MCServerName}");//配置文件不会被dispose
            }
            StaticTools.HandleLog("已关闭所有MC服务器");
        }
    }
}
