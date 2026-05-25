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
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PMCSsE_Frontend_AvaloniaUI.Modules
{
    internal static class PathsClass
    {
        internal static string APPPath = (OperatingSystem.IsAndroid()||OperatingSystem.IsIOS()) ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) : AppDomain.CurrentDomain.BaseDirectory;

        internal static string LogDir = System.IO.Path.Combine(APPPath, "Logs");

        internal static string APPConfigPath = System.IO.Path.Combine(APPPath, "APPConfig.dat");

        internal static string MessageRecordingsPath = System.IO.Path.Combine(APPPath, "MessageRecordings");
    }

    [ProtoContract]
    internal class APPConfigClass
    {
        [ProtoMember(1)]
        internal List<NativeServerHistory> NativeServerHistories { get; set; } = [];
    }
    internal static class StaticAPPConfigClass
    {
        internal static List<NativeServerHistory> NativeServerHistories { get; set; } = [];
    }

    [ProtoContract]
    public class NativeServerHistory
    {
        [ProtoMember(1)]
        internal string IP { get; set; } = "";
        [ProtoMember(2)]
        internal int Port { get; set; } = 20000;
        [ProtoMember(3)]
        internal string RSAPublicKeyHash { get; set; } = "";
        [ProtoMember(4)]
        internal string Password { get; set; } = "";
    }
}

