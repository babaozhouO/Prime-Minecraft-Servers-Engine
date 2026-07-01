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
using PMCSsE_Communicator;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PMCSsE_Backend.Modules
{
    internal static class RunningStateRecorder
    {
        internal static bool Debug = false;
        internal static readonly bool IsRunningAsAdmin = StaticTools.CheckProgramPermission();
        internal static readonly Encoding SystemCommandLineEncoding = Console.OutputEncoding;
    }
    internal static class Paths
    {
        internal static readonly string APPDir = AppDomain.CurrentDomain.BaseDirectory;

        internal static readonly string? APPExeFile = Environment.ProcessPath;

        internal static readonly string ConfigsDir = Path.Combine(APPDir, "Configs");

        internal static readonly string LogDir = Path.Combine(APPDir, "Logs");

        internal static readonly string PluginsDir = Path.Combine(APPDir, "plugins");

        internal static readonly string Config_PlaintextPath = Path.Combine(ConfigsDir, "Config_Plaintext.dat");

        internal static readonly string Config_CiphertextPath = Path.Combine(ConfigsDir, "Config_Ciphertext.dat");

        internal static readonly string MessageRecordsDir = Path.Combine(APPDir, "MessageRecords");
    }
    [ProtoContract(SkipConstructor = true)]
    internal partial class Config_Plaintext
    {
        [ProtoMember(1)]
        internal string ListenAddress { get; set; } = "0.0.0.0";
        [ProtoMember(2)]
        internal int ListenPort { get; set; } = 20000;
        [ProtoMember(3)]
        internal byte[] SaltOfCipherConfigKey { get; set; } = [];
    }

    internal static class StaticConfig_Plaintext
    {
        internal static string ListenAddress { get; set; } = "0.0.0.0";
        internal static int ListenPort { get; set; } = 20000;
        internal static byte[] SaltOfCipherConfigKey { get; set; } = [];
    }

    [ProtoContract(SkipConstructor = true)]
    internal partial class Config_Ciphertext
    {
        [ProtoMember(1)]
        internal int ConfigVersion { get; set; } = 1;
        [ProtoMember(2)]
        internal byte[] SaltedLoginKeyHash { get; set; } = [];
        [ProtoMember(3)]
        internal byte[] SaltOfLoginKey { get; set; } = [];
        [ProtoMember(4)]
        internal List<MCServerManagerConfig> MCServerManagerConfigsList { get; set; } = [];
    }
    internal static class StaticConfig_Ciphertext
    {
        internal static int ConfigVersion { get; set; } = 1;
        internal static byte[] SaltedLoginKeyHash { get; set; } = [];
        internal static byte[] SaltOfLoginKey { get; set; } = [];
        internal static List<MCServerManagerConfig> MCServerManagerConfigsList { get; set; } = [];
    }
    internal static class JsonComputeOptions
    {
        // 单行
        internal static readonly JsonSerializerOptions jsonSerializerOptions1 = new()
        {
            PropertyNamingPolicy = null,
            IncludeFields = true,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }
}

