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
using ProtoBuf;
using System.Text.Json;

namespace PMCSsE_Backend.Modules
{
    internal static class StaticConfigManagerClass
    {
        internal static bool LoadConfig()
        {
            StaticTools.HandleLog("正在读取配置文件");
            APPConfigClass? APPConfig = null;
            if (File.Exists(Paths.APPConfigPath))//存在
            {
                try
                {
                    using FileStream appConfigFile = File.OpenRead(Paths.APPConfigPath);
                    APPConfig = Serializer.Deserialize<APPConfigClass>(appConfigFile);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"配置文件[{Paths.APPConfigPath}]读取或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            else//不存在
            {
                try
                {
                    using FileStream appConfigFile = File.Create(Paths.APPConfigPath);
                    APPConfig = new APPConfigClass();
                    Serializer.Serialize<APPConfigClass>(appConfigFile, APPConfig);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"配置文件[{Paths.APPConfigPath}]写入或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                    StaticTools.HandleLog("程序无法继续运行");
                    return false;
                }
            }
            if (APPConfig == null)
            {
                StaticTools.HandleLog($"无法获得有效的主程序配置(null)，程序无法继续运行");
                return false;
            }
            StaticAPPConfigClass.ListenPort = APPConfig.ListenPort;
            StaticAPPConfigClass.ConfigVersion = APPConfig.ConfigVersion;
            StaticAPPConfigClass.Registered = APPConfig.Registered;
            StaticAPPConfigClass.SaltedPasswordHash = APPConfig.SaltedPasswordHash;
            StaticAPPConfigClass.Salt = APPConfig.Salt;
            StaticTools.HandleLog($"主程序配置读取成功");

            MCServerManagerConfigs? mCServerManagerConfigs = null;
            if (File.Exists(Paths.MCServersConfigPath))//存在
            {
                try
                {
                    using FileStream mCServersConfigFile = File.OpenRead(Paths.MCServersConfigPath);
                    mCServerManagerConfigs = Serializer.Deserialize<MCServerManagerConfigs>(mCServersConfigFile);
                }
                catch (Exception ex)
                {

                    StaticTools.HandleLog($"配置文件[{Paths.MCServersConfigPath}]读取或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行");
                    return false;
                }
            }
            else//不存在
            {
                try
                {
                    using FileStream mCServersConfigFile = File.Create(Paths.MCServersConfigPath);
                    mCServerManagerConfigs = new MCServerManagerConfigs();
                    Serializer.Serialize<MCServerManagerConfigs>(mCServersConfigFile, mCServerManagerConfigs);
                }
                catch (Exception ex)
                {

                    StaticTools.HandleLog($"配置文件[{Paths.MCServersConfigPath}]写入或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行");
                    return false;
                }
            }
            if (mCServerManagerConfigs == null)
            {
                StaticTools.HandleLog($"无法获得有效的服务端管理器配置，程序无法继续运行");
                return false;
            }
            StaticMCServerManagerConfigs.ConfigVersion = mCServerManagerConfigs.ConfigVersion;
            StaticMCServerManagerConfigs.MCServerManagerConfigsList = mCServerManagerConfigs.MCServerManagerConfigsList;
            StaticTools.HandleLog($"MC服务端管理器配置读取成功");
            StaticTools.HandleLog($"全部配置读取成功，太棒了！");
            return true;
        }
        internal static bool SaveAPPConfig()
        {
            APPConfigClass APPConfig = new()
            {
                ListenPort = StaticAPPConfigClass.ListenPort,
                ConfigVersion = StaticAPPConfigClass.ConfigVersion,
                Registered = StaticAPPConfigClass.Registered,
                SaltedPasswordHash = StaticAPPConfigClass.SaltedPasswordHash,
                Salt = StaticAPPConfigClass.Salt
            };
            try
            {
                using FileStream appConfigFile = File.Create(Paths.APPConfigPath);
                Serializer.Serialize(appConfigFile, APPConfig);
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"配置文件[{Paths.APPConfigPath}]写入或序列化失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                return false;
            }
            return true;
        }
        internal static bool SaveMCServerManagersConfig()
        {
            MCServerManagerConfigs mCServerManagerConfigs = new()
            {
                ConfigVersion = StaticMCServerManagerConfigs.ConfigVersion,
                MCServerManagerConfigsList = StaticMCServerManagerConfigs.MCServerManagerConfigsList
            };
            try
            {
                using FileStream mCServerManagerConfigsFile = File.Create(Paths.MCServersConfigPath);
                Serializer.Serialize(mCServerManagerConfigsFile, mCServerManagerConfigs);
                return true;
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"配置文件[{Paths.MCServersConfigPath}]写入或序列化失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                return false;
            }
        }
    }
}
