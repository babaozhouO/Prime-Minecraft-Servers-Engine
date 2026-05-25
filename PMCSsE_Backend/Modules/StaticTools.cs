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

namespace PMCSsE_Backend.Modules
{
    internal static class StaticTools
    {
        internal readonly static LogsWriterClass LogsWriter = new();
        //private static bool WroteGreaterThanSign = false;
        //private static bool DeletedGreaterThanSign = false;
        internal static void HandleLog(string log, bool isPassword = false, bool isFromLogsWriter = false)
        {
            //if (WroteGreaterThanSign)//实现 >... 效果
            //{
            //    try { Console.Write('\r'); } catch { }
            //    WroteGreaterThanSign = false;
            //    DeletedGreaterThanSign = true;
            //}
            //if (log != ">")
            //{
            log = $"[{DateTime.Now:G}]:{log}";
            try { Console.WriteLine(log); } catch { }//要是这都报错，基本可以放弃抢救了
            if (isPassword)
            {
                log = "你看见密码了吗，在哪里？";//不要把密码输出到日志里
            }
            LogsWriter.AppendLog(log);
            //if (DeletedGreaterThanSign)
            //{
            //    try { Console.Write('>'); } catch { }
            //    DeletedGreaterThanSign = false;
            //    WroteGreaterThanSign = true;
            //}
            // }
            //else
            //{
            //    try { Console.Write(log); } catch { }
            //    WroteGreaterThanSign = true;
            //}
        }
        //internal static void HandleCommand(string command)
        //{
        //    if (WroteGreaterThanSign)
        //    {
        //        try { Console.Write('\r'); } catch { }
        //        WroteGreaterThanSign = false;
        //    }
        //    command = $"[{DateTime.Now:G}]:命令输入>{command}";
        //    try { Console.WriteLine(command); } catch { }
        //    LogsWriter.AppendLog(command);
        //}
    }
}
