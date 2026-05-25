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
using ProtoBuf;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PMCSsE_Backend.Modules
{
    internal static class RunningStateRecorder
    {
        internal static bool Debug = false;
        internal static readonly bool IsRunningAsAdmin = AdminCheckerClass.IsRunningAsAdmin();
        internal static readonly Encoding SystemCommandLineEncoding = Console.OutputEncoding;
    }
    internal static class Paths
    {
        internal static readonly string APPDir = AppDomain.CurrentDomain.BaseDirectory;

        internal static readonly string? APPExeFile = Environment.ProcessPath;

        internal static readonly string LogDir = Path.Combine(APPDir, "Logs");

        internal static readonly string APPConfigPath = Path.Combine(APPDir, "APPConfig.dat");

        internal static readonly string MCServersConfigPath = Path.Combine(APPDir, "MCServersConfig.dat");

        internal static readonly string MessageRecordingsDir = Path.Combine(APPDir, "MessageRecordings");

        internal static readonly string PluginsDir = Path.Combine(APPDir, "plugins");
    }

    [ProtoContract]
    internal class APPConfigClass
    {
        [ProtoMember(1)]
        internal int ListenPort { get; set; } = 20000;
        [ProtoMember(2)]
        internal int ConfigVersion { get; set; } = 1;
        [ProtoMember(3)]
        internal bool Registered { get; set; } = false;
        [ProtoMember(4)]
        internal byte[] SaltedPasswordHash { get; set; } = [];//加了盐的
        [ProtoMember(5)]
        internal byte[] Salt { get; set; } = [];
    }
    internal static class StaticAPPConfigClass
    {
        internal static int ListenPort { get; set; }
        internal static int ConfigVersion { get; set; }
        internal static bool Registered { get; set; }
        internal static byte[] SaltedPasswordHash { get; set; } = [];
        internal static byte[] Salt { get; set; } = [];
    }
    internal static class JsonComputeOptions
    {
        // 单行
        internal static readonly JsonSerializerOptions jsonSerializerOptions1 = new()
        {
            PropertyNamingPolicy = null,
            IncludeFields = true,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }
}

