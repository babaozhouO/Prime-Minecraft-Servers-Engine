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

namespace PMCSsE_Communicator.SharedCodes
{
    ///<summary>
    /// 用以传输日志的包装类
    ///</summary>
    [ProtoContract(SkipConstructor = true)]
    public partial class LogEntry(ulong id, string log)
    {
        /// <summary>
        /// 编号
        /// </summary>
        [ProtoMember(1)]
        public ulong ID = id;
        /// <summary>
        /// 日志文本
        /// </summary>
        [ProtoMember(2)]
        public string Log = log;
    }
}
