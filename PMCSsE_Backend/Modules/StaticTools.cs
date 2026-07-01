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

using System.Security.Principal;

namespace PMCSsE_Backend.Modules
{
    internal static class StaticTools
    {
        internal readonly static LogsWriter LogsWriter = new();
        internal static void HandleLog(string log, bool isPassword = false, bool isFromLogsWriter = false)
        {
            log = $"[{DateTime.Now:G}]:{log}";
            try { Console.WriteLine(log); } catch { }//要是这都报错，基本可以放弃抢救了
            if (isPassword)
            {
                log = "你看见密码了吗，在哪里？";//不要把密码输出到日志里
            }
            if (!isFromLogsWriter)
                LogsWriter.AppendLog(log);
        }
        internal static void InitialiseLogsWriter()
        {
            LogsWriter.ReportLog += HandleLogsWriterLog;
        }
        private static void HandleLogsWriterLog(string log)
        {
            HandleLog(log, false, true);
        }
        internal static bool AskUserForYesOrNo(string question)
        {
            while (true)
            {
                StaticTools.HandleLog($"{question}（Y/N）");
                string? choice = Console.ReadLine();
                if (!string.IsNullOrEmpty(choice))
                {
                    choice = choice.ToUpper();
                    if (choice == "Y")
                    {
                        return true;
                    }
                    else if (choice == "N")
                    {
                        return false;
                    }
                }
            }
        }
        internal static bool CheckProgramPermission()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (Exception ex)
                {
                    HandleLog($"检查进程权限时发生异常：{ex.Message}，堆栈：{ex.StackTrace}");
                    return false;
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return Environment.UserName == "root";
            }
            return false;
        }
    }
}
