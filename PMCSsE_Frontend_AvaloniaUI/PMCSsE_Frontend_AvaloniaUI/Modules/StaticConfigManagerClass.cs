/*Copyright 2025 【Babao Zhou (Legal Name: RenJie Zhou) <Contact: 1749861851@qq.com>】

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using Avalonia.Threading;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using ProtoBuf;
using System;
using System.IO;

namespace PMCSsE_Frontend_AvaloniaUI.Modules
{
    internal static class StaticConfigManagerClass
    {
        internal static string SaveAPPConfig()
        {
            try
            {
                APPConfigClass APPConfig = new() { NativeServerHistories = StaticAPPConfigClass.NativeServerHistories };
                using FileStream appConfigFile = File.Create(PathsClass.APPConfigPath);
                Serializer.Serialize<APPConfigClass>(appConfigFile, APPConfig);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"保存主程序配置文件失败{Environment.NewLine}原因:{ex.Message}{Environment.NewLine}堆栈:{ex.StackTrace}";
            }

        }
        internal static string LoadConfig()
        {
            APPConfigClass? APPConfig;
            if (File.Exists(PathsClass.APPConfigPath))
            {
                try
                {
                    using FileStream APPConfigFile = File.OpenRead(PathsClass.APPConfigPath);
                    APPConfig = Serializer.Deserialize<APPConfigClass>(APPConfigFile);
                }
                catch (Exception ex)
                {
                    return $"已找到主程序配置文件但加载失败{Environment.NewLine}原因:{ex.Message}{Environment.NewLine}堆栈:{ex.StackTrace}";
                }
            }
            else
            {
                try
                {
                    using FileStream APPConfigFile = File.Create(PathsClass.APPConfigPath);
                    APPConfig = new();
                    Serializer.Serialize<APPConfigClass>(APPConfigFile, APPConfig);
                }
                catch (Exception ex)
                {
                    return $"初始化主程序配置文件失败{Environment.NewLine}原因:{ex.Message}{Environment.NewLine}堆栈:{ex.StackTrace}";
                }
            }

            if (APPConfig == null)
            {
                return $"加载主程序配置文件失败{Environment.NewLine}原因:null{Environment.NewLine}可能由版本更新造成";
            }

            StaticAPPConfigClass.NativeServerHistories = APPConfig.NativeServerHistories;
            return string.Empty;
        }
    }
}