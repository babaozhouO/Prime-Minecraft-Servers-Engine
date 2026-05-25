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
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace PMCSsE_Frontend_WPF.Modules
{
    //维护一个文件流用来读写日志文件
    internal class LogsWriterClass
    {
        private FileStream LogFileStream;
        private readonly string LogFileDir;
        private string LogFileName;
        private string LogFilePath;
        private readonly DispatcherTimer OneSecondTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        private DateTime Date = DateTime.Now.Date;

        internal event Action<string, string, string> ReportLog = delegate { };
        internal List<string> Logs = [];
        internal LogsWriterClass()
        {
             

            LogFileDir = Path.Combine(PathsAndDefaultConfigTextClass.LogDir, $"APPLogs");
            LogFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            LogFilePath = Path.Combine(LogFileDir, LogFileName);
            if (!Directory.Exists(LogFileDir))
            {
                Directory.CreateDirectory(LogFileDir);
            }
            if (!File.Exists(LogFilePath))
            {
                using (File.Create(LogFilePath)) { }
            }
            LogFileStream = new(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            OneSecondTimer.Tick += async (sender, equals) =>
            {
                OneSecondTimer.Stop();
                if (Logs.Count == 0)
                {
                    OneSecondTimer.Start();
                    return;
                }
                if (Date != DateTime.Now.Date)
                {
                    LogFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
                    LogFilePath = Path.Combine(LogFileDir, LogFileName);
                    try
                    {
                        await LogFileStream.FlushAsync();
                        await LogFileStream.DisposeAsync();
                        if (!File.Exists(LogFilePath))
                        {
                            using (File.Create(LogFilePath)) { }
                        }
                        LogFileStream = new(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    catch (Exception ex)
                    {
                        ReportLog("错误", "日志写入器", $"切换文件时出错：{ex.Message}");
                    }

                }
                byte[] buffer;
                {
                    StringBuilder stringBuilder = new();
                    lock (Logs)
                    {
                        stringBuilder.Append(string.Join(Environment.NewLine, Logs));
                        stringBuilder.Append(Environment.NewLine);
                        Logs.Clear();
                    }
                    buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                }
                try
                {
                    await LogFileStream.WriteAsync(buffer.AsMemory());
                    await LogFileStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    ReportLog("错误", "日志写入器", $"写入日志时出错：{ex.Message}");
                }
                finally
                {
                    Date = DateTime.Now.Date;
                    OneSecondTimer.Start();

                }
            };
            OneSecondTimer.Start();
        }



        internal void AppendLog(string message)
        {
            if (message.Contains("日志写入器"))
            {
                return;
            }
            lock (Logs)
            {
                Logs.Add(message);
            }
        }
    }
}
