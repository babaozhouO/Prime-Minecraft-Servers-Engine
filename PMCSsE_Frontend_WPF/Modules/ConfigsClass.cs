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
using System.Text.Json.Serialization;

namespace PMCSsE_Frontend_WPF.Modules
{
    internal static class UIContextClass
    {
        internal static SynchronizationContext? UIContext { get; set; }
    }
    internal static class PathsAndDefaultConfigTextClass
    {
        internal static string APPPath = AppDomain.CurrentDomain.BaseDirectory;

        internal static string LogDir = System.IO.Path.Combine(APPPath, "Logs");

        internal static string APPConfigPath = System.IO.Path.Combine(APPPath, "APPConfig.json");

        internal static string MessageRecordingsPath = System.IO.Path.Combine(APPPath, "MessageRecordings");

        internal const string DefaultAPPConfigText = @"{
""NativeServerHistories"":[]
}";
    }
    internal class APPConfigClass
    {
        [JsonConstructor]
        internal APPConfigClass(List<NativeServerHistory> NativeServerHistories)
        {
            this.NativeServerHistories = NativeServerHistories;
        }
        [JsonInclude]
        internal List<NativeServerHistory> NativeServerHistories { get; set; } = [];
    }
    internal static class StaticAPPConfigClass
    {
        internal static List<NativeServerHistory> NativeServerHistories { get; set; } = [];
    }
    internal class NativeServerHistory
    {
        [JsonConstructor]
        internal NativeServerHistory(string IP,int Port, string RSAPublicKeyHash, string Password)
        {
            this.IP = IP;
            this.Port = Port;
            this.RSAPublicKeyHash = RSAPublicKeyHash;
            this.Password = Password;
        }
        [JsonInclude]
        internal string IP { get; set; } = "";
        [JsonInclude]
        internal int Port { get; set; } = 20000;
        [JsonInclude]
        internal string RSAPublicKeyHash { get; set; } = "";
        [JsonInclude]
        internal string Password { get; set; } = "";
    }
}

