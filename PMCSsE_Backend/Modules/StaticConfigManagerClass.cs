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
using LightProto;
using PMCSsE_Communicator.SharedCodes;
using System.Security;

namespace PMCSsE_Backend.Modules
{
    internal static class StaticConfigManager
    {
        internal static ConfigFilesStateEnum ConfigFilesState
        {
            get
            {
                if (Directory.Exists(Paths.ConfigsDir))
                {
                    switch (File.Exists(Paths.Config_PlaintextPath), File.Exists(Paths.Config_CiphertextPath))//无异常
                    {
                        case (true, true):
                            return ConfigFilesStateEnum.Good;
                        case (false, true):
                            return ConfigFilesStateEnum.WhereIsThePlaintextConfig;
                        case (true, false):
                            return ConfigFilesStateEnum.WhereIsTheCiphertextConfig;
                        case (false, false):
                            return ConfigFilesStateEnum.Welcome;
                    }
                }
                else
                {
                    return ConfigFilesStateEnum.Welcome;
                }
            }
        }
        internal static bool LoadConfig_Plaintext()
        {
            StaticTools.HandleLog("正在读取配置文件（明文部分）");
            Config_Plaintext config;
            if (!Directory.Exists(Paths.ConfigsDir))
            {
                try
                {
                    Directory.CreateDirectory(Paths.ConfigsDir);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"创建配置目录[{Paths.ConfigsDir}]失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整目录权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            if (File.Exists(Paths.Config_PlaintextPath))//存在
            {
                try
                {
                    using FileStream configFile = File.OpenRead(Paths.Config_PlaintextPath);
                    config = Serializer.Deserialize<Config_Plaintext>(configFile);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"配置文件[{Paths.Config_PlaintextPath}]读取或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            else//不存在
            {
                StaticTools.HandleLog($"配置文件[{Paths.Config_PlaintextPath}]不存在");
                StaticTools.HandleLog("程序无法继续运行");
                return false;
            }
            StaticConfig_Plaintext.ListenAddress = config.ListenAddress;
            StaticConfig_Plaintext.ListenPort = config.ListenPort;
            StaticConfig_Plaintext.SaltOfCipherConfigKey = config.SaltOfCipherConfigKey;
            StaticTools.HandleLog($"配置文件（明文部分）读取成功");
            return true;
        }
        internal static bool LoadConfig_Ciphertext(byte[] configKey)
        {
            StaticTools.HandleLog("正在读取配置文件（密文部分）");
            Config_Ciphertext config;
            if (!Directory.Exists(Paths.ConfigsDir))
            {
                try
                {
                    Directory.CreateDirectory(Paths.ConfigsDir);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"创建配置目录[{Paths.ConfigsDir}]失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整目录权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            if (File.Exists(Paths.Config_CiphertextPath))//存在
            {
                try
                {
                    byte[] configData = File.ReadAllBytes(Paths.Config_CiphertextPath);
                    configData = ConfigCrypto.Decrypt(configData, configKey);
                    config = Serializer.Deserialize<Config_Ciphertext>(configData.AsSpan());
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"配置文件[{Paths.Config_CiphertextPath}]读取或解密或反序列化失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            else//不存在
            {
                StaticTools.HandleLog($"配置文件[{Paths.Config_CiphertextPath}]不存在");
                StaticTools.HandleLog("程序无法继续运行");
                return false;
            }
            StaticConfig_Ciphertext.ConfigVersion = config.ConfigVersion;
            StaticConfig_Ciphertext.SaltedLoginKeyHash = config.SaltedLoginKeyHash;
            StaticConfig_Ciphertext.SaltOfLoginKey = config.SaltOfLoginKey;
            StaticConfig_Ciphertext.MCServerManagerConfigsList = config.MCServerManagerConfigsList;
            StaticTools.HandleLog($"配置文件（密文部分）读取成功");
            return true;
        }
        internal static bool SaveConfig_Plaintext()
        {
            if (!Directory.Exists(Paths.ConfigsDir))
            {
                try
                {
                    Directory.CreateDirectory(Paths.ConfigsDir);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"创建配置目录[{Paths.ConfigsDir}]失败");
                    StaticTools.HandleLog($"异常:{ex.Message}");
                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                    StaticTools.HandleLog("可能的解决办法：调整目录权限、以管理员身份运行、删除配置文件");
                    StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                    return false;
                }
            }
            Config_Plaintext config = new()
            {
                ListenAddress = StaticConfig_Plaintext.ListenAddress,
                ListenPort = StaticConfig_Plaintext.ListenPort,
                SaltOfCipherConfigKey = StaticConfig_Plaintext.SaltOfCipherConfigKey
            };
            try
            {
                using FileStream appConfigFile = File.Create(Paths.Config_PlaintextPath);
                Serializer.Serialize(appConfigFile, config);
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"配置文件[{Paths.Config_PlaintextPath}]写入或序列化失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                return false;
            }
            return true;
        }
        internal static bool SaveConfig_Ciphertext(byte[] configKey)
        {
            Config_Ciphertext config = new()
            {
                ConfigVersion = StaticConfig_Ciphertext.ConfigVersion,
                SaltedLoginKeyHash = StaticConfig_Ciphertext.SaltedLoginKeyHash,
                SaltOfLoginKey = StaticConfig_Ciphertext.SaltOfLoginKey,
                MCServerManagerConfigsList = StaticConfig_Ciphertext.MCServerManagerConfigsList
            };
            try
            {
                byte[] configData = config.ToByteArray();
                configData = ConfigCrypto.Encrypt(configData, configKey);
                using FileStream configFile = File.Create(Paths.Config_CiphertextPath);
                configFile.Write(configData.AsSpan());
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"配置文件[{Paths.Config_CiphertextPath}]写入或序列化或加密失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                StaticTools.HandleLog("程序无法继续运行，请关闭程序");
                return false;
            }
            return true;
        }
        internal static bool DeleteAllConfigFile()
        {
            try
            {
                File.Delete(Paths.Config_PlaintextPath);
                File.Delete(Paths.Config_CiphertextPath);
                return true;
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"配置文件[{Paths.Config_PlaintextPath}]和[{Paths.Config_CiphertextPath}]删除失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                StaticTools.HandleLog("可能的解决办法：调整文件权限、以管理员身份运行");
                StaticTools.HandleLog("程序无法删除未完成的配置文件，请手动删除“Configs”文件夹");
                return false;
            }
        }
        internal enum ConfigFilesStateEnum
        {
            Good,
            WhereIsThePlaintextConfig,
            WhereIsTheCiphertextConfig,
            Welcome
        }
    }
}
