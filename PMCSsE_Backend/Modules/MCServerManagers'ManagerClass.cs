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

using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using System.Net;

namespace PMCSsE_Backend.Modules
{
    internal static class MCServerManagers_ManagerClass
    {
        internal static NativeServer? NativeServer;
        private static readonly List<MCServerManagerClass> LoadedMCServerManagersList = [];
        internal static event Action ExitCalled = delegate { };
        public static DataPackBus? DataPackBus => NativeServer?.DataPackBus;
        internal static void Initialize()
        {
            NativeServer = new(IPAddress.Any, StaticAPPConfigClass.ListenPort, StaticAPPConfigClass.SaltedPasswordHash, StaticAPPConfigClass.Salt, RunningStateRecorder.Debug);

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
            NativeServer.DataPackBus.Subscribe<Pack_GetMCServerManagerConfigsList>(HandlePack_GetMCServerManagersList);
            NativeServer.DataPackBus.Subscribe<Pack_CreatNewMCServerManager>(HandlePack_CreatNewMCServerManager);
            NativeServer.DataPackBus.Subscribe<Pack_LoadMCServerManager>(HandlePack_LoadMCServerManager);
            NativeServer.DataPackBus.Subscribe<Pack_StopMCServerManager>(HandlePack_StopMCServerManager);
            NativeServer.DataPackBus.Subscribe<Pack_DeleteMCServerManager>(HandlePack_DeleteMCServerManager);
            NativeServer.DataPackBus.Subscribe<Pack_GetMCServerManager>(HandlePack_GetMCServerManager);
            NativeServer.StartService();
            PluginsManager.LoadAllPlugins();
        }
        private static void HandlePack_GetMCServerManagersList(Pack_GetMCServerManagerConfigsList _)
        {
            if (NativeServer == null) { return; }
            StaticTools.HandleLog($"客户端请求获取所有管理器");
            MCServerManagerConfigs mCServerManagerConfigs = new();
            {
                mCServerManagerConfigs.ConfigVersion = StaticMCServerManagerConfigs.ConfigVersion;
                mCServerManagerConfigs.MCServerManagerConfigsList = StaticMCServerManagerConfigs.MCServerManagerConfigsList;
            }
            NativeServer.RespondClient(RespondTypeEnum.MCServerManagerConfigs, new Pack_MCServerManagerConfigs(mCServerManagerConfigs));
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
        private static void HandlePack_GetMCServerManager(Pack_GetMCServerManager _)
        {
            if (NativeServer == null) { return; }
            StaticTools.HandleLog($"客户端请求获取已加载的管理器");
            NativeServer.RespondClient(RespondTypeEnum.LoadedMCServerManagers, new Pack_MCServerManagers(GetLoadedMCServerManagers()));
        }
        private static MCServerManagerConfig? CreatNewMCServerManager()
        {
            string NewID = GenerateNewID();
            if (string.IsNullOrEmpty(NewID))
            {
                return null;
            }
            MCServerManagerConfig mCServerManagerConfig = new() { ManagerID = NewID };
            StaticMCServerManagerConfigs.MCServerManagerConfigsList.Add(mCServerManagerConfig);
            StaticMCServerManagerConfigs.MCServerManagerConfigsList.Sort((a, b) =>
                int.Parse(a.ManagerID).CompareTo(int.Parse(b.ManagerID)));
            if (!StaticConfigManagerClass.SaveMCServerManagersConfig())//保存失败
            {
                StaticMCServerManagerConfigs.MCServerManagerConfigsList.Remove(mCServerManagerConfig);
                StaticTools.HandleLog("保存新创建的MC服务端管理器失败");
                return null;
            }

            return mCServerManagerConfig;
        }
        private static string GenerateNewID()
        {
            List<int> IDs = [];
            foreach (MCServerManagerConfig singleMCServerManagerConfigInfo1 in StaticMCServerManagerConfigs.MCServerManagerConfigsList)
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
            foreach (var item in StaticMCServerManagerConfigs.MCServerManagerConfigsList)
            {
                if (item.ManagerID == ID)
                {
                    mCServerManagerConfig = item;
                    break;
                }
            }
            if (mCServerManagerConfig != null)
            {
                MCServerManagerClass mCServerManager = new(mCServerManagerConfig);
                LoadedMCServerManagersList.Add(mCServerManager);
                return 0;
            }
            return 2;
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
            var config = StaticMCServerManagerConfigs.MCServerManagerConfigsList
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

            // 6. 删除配置并保存
            StaticMCServerManagerConfigs.MCServerManagerConfigsList.Remove(config);
            if (!StaticConfigManagerClass.SaveMCServerManagersConfig())
            {
                // 保存失败，复原配置
                StaticMCServerManagerConfigs.MCServerManagerConfigsList.Add(config);
                return 2;
            }

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
