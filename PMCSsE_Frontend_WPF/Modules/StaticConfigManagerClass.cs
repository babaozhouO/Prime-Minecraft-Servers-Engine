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
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PMCSsE_Frontend_WPF.Modules
{
    internal static class StaticConfigManagerClass
    {
        internal static void SaveAPPConfig()
        {
            APPConfigClass APPConfig = new(StaticAPPConfigClass.NativeServerHistories);


            // 保存配置
            string APPConfigText = JsonSerializer.Serialize(APPConfig, JsonComputeOptions.jsonSerializerOptions);
            System.IO.File.WriteAllText(PathsAndDefaultConfigTextClass.APPConfigPath, APPConfigText);
        }
    }

    internal static class JsonComputeOptions
    {
        internal static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            // 关键设置：禁用命名策略，保持属性名原样
            PropertyNamingPolicy = null,
            IncludeFields = true,
            // 保持缩进格式
            WriteIndented = true,
            // 处理中文等特殊字符
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // 确保所有属性都被序列化
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }
}