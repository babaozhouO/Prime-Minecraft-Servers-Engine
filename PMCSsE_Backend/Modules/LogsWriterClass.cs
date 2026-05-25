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
using PMCSsE_Communicator;
using System.Text;

namespace PMCSsE_Backend.Modules
{
    //维护一个文件流用来读写日志文件
    internal class LogsWriterClass
    {
        private readonly MCServerManagerConfig? MCServerManagerConfig;
        private bool Running = false;
        private FileStream? LogFileStream;
        private readonly string LogFileDir;
        private string? LogFileName;
        private string? LogFilePath;
        private readonly System.Timers.Timer OneSecondTimer = new() { AutoReset = false, Interval = 1000 };
        private DateTime Date = DateTime.Now.Date;
        internal event Action<string, string, string> ReportLog = delegate { };
        internal bool Disposed = false;
        internal List<string> Logs = [];
        internal LogsWriterClass(MCServerManagerConfig mCServerManagerConfig)
        {
            MCServerManagerConfig = mCServerManagerConfig;

            LogFileDir = Path.Combine(Paths.LogDir, $"MCServerManager - {MCServerManagerConfig.ManagerID}");
            Initialize();
        }
        internal LogsWriterClass()
        {
            LogFileDir = Path.Combine(Paths.LogDir, $"主程序日志");
            Initialize();
        }
        internal void Initialize()
        {
            LogFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            LogFilePath = Path.Combine(LogFileDir, LogFileName);
            if (!Directory.Exists(LogFileDir))
            {
                try
                {
                    Directory.CreateDirectory(LogFileDir);
                }
                catch (Exception ex)
                {
                    Running = false;
                    ReportLog("错误", "日志写入器", $"创建日志目录时出错：{ex.Message}");
                    ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                    StaticTools.HandleLog($"创建日志目录时出错：{ex.Message}", false, true);
                    StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                    Dispose();
                    return;
                }

            }
            if (!File.Exists(LogFilePath))
            {
                try
                {
                    using (System.IO.File.Create(LogFilePath)) { }
                }
                catch (Exception ex)
                {
                    Running = false;
                    ReportLog("错误", "日志写入器", $"创建日志文件时出错：{ex.Message}");
                    ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                    StaticTools.HandleLog($"创建日志文件时出错：{ex.Message}", false, true);
                    StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                    Dispose();
                    return;
                }
            }
            try
            {
                LogFileStream = new(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            catch (Exception ex)
            {
                Running = false;
                ReportLog("错误", "日志写入器", $"打开日志文件时出错：{ex.Message}");
                ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                StaticTools.HandleLog($"打开日志文件时出错：{ex.Message}", false, true);
                StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                Dispose();
                return;
            }
            OneSecondTimer.Elapsed += async (sender, equals) =>
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
                            using (System.IO.File.Create(LogFilePath)) { }
                        }
                        LogFileStream = new(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    catch (Exception ex)
                    {
                        Running = false;
                        ReportLog("错误", "日志写入器", $"创建并切换日志文件时出错：{ex.Message}");
                        ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                        StaticTools.HandleLog($"创建并切换日志文件时出错：{ex.Message}", false, true);
                        StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                        Dispose();
                        return;
                    }

                }
                byte[]? buffer = null;
                {
                    StringBuilder stringBuilder = new();
                    lock (Logs)
                    {
                        stringBuilder.AppendJoin(Environment.NewLine, Logs);
                        stringBuilder.Append(Environment.NewLine);
                        Logs.Clear();
                    }
                    try
                    {
                        buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Running = false;
                        ReportLog("错误", "日志写入器", $"使用UTF8编码日志文本时出错：{ex.Message}");
                        ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                        StaticTools.HandleLog($"使用UTF8编码日志文本时出错：{ex.Message}", false, true);
                        StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                        Dispose();
                        return;
                    }

                }
                try
                {
                    if (buffer != null)
                    {
                        await LogFileStream.WriteAsync(buffer.AsMemory());
                        await LogFileStream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Running = false;
                    ReportLog("错误", "日志写入器", $"写入日志时出错：{ex.Message}");
                    ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                    StaticTools.HandleLog($"写入日志时出错：{ex.Message}", false, true);
                    StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                    Dispose();
                    return;
                }
                Date = DateTime.Now.Date;
                OneSecondTimer.Start();
            };
            OneSecondTimer.Start();
            Running = true;
        }

        internal void AppendLog(string message)
        {
            if (!Running) { return; }
            if (message.Contains("日志写入器"))
            {
                return;
            }
            lock (Logs)
            {
                Logs.Add(message);
            }
        }

        internal void Dispose()
        {
            Running = false;
            OneSecondTimer.Stop();
            OneSecondTimer.Dispose();
            if (Logs.Count != 0)
            {
                if (Date != DateTime.Now.Date)
                {
                    LogFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
                    LogFilePath = Path.Combine(LogFileDir, LogFileName);
                    try
                    {
                        LogFileStream?.Flush();
                        LogFileStream?.Dispose();
                        if (!File.Exists(LogFilePath))
                        {
                            using (System.IO.File.Create(LogFilePath)) { }
                        }
                        LogFileStream = new(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    }
                    catch (Exception ex)
                    {
                        Running = false;
                        ReportLog("错误", "日志写入器", $"创建并切换日志文件时出错：{ex.Message}");
                        ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                        StaticTools.HandleLog($"创建并切换日志文件时出错：{ex.Message}", false, true);
                        StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                        LogFileStream?.Close();
                        LogFileStream?.Dispose();
                        ReportLog = delegate { };
                        return;
                    }
                }
                byte[]? buffer = null;
                {
                    StringBuilder stringBuilder = new();
                    lock (Logs)
                    {
                        stringBuilder.AppendJoin(Environment.NewLine, Logs);
                        stringBuilder.Append(Environment.NewLine);
                        Logs.Clear();
                    }
                    try
                    {
                        buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Running = false;
                        ReportLog("错误", "日志写入器", $"使用UTF8编码日志文本时出错：{ex.Message}");
                        ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                        StaticTools.HandleLog($"使用UTF8编码日志文本时出错：{ex.Message}", false, true);
                        StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                        LogFileStream?.Close();
                        LogFileStream?.Dispose();
                        ReportLog = delegate { };
                        return;
                    }

                }
                try
                {
                    if (buffer != null)
                    {
                        LogFileStream?.Write(buffer.AsSpan());
                        LogFileStream?.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Running = false;
                    ReportLog("错误", "日志写入器", $"写入日志时出错：{ex.Message}");
                    ReportLog("错误", "日志写入器", $"堆栈跟踪：{ex.StackTrace}");
                    StaticTools.HandleLog($"写入日志时出错：{ex.Message}", false, true);
                    StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}", false, true);
                    LogFileStream?.Close();
                    LogFileStream?.Dispose();
                    ReportLog = delegate { };
                    return;
                }
                Date = DateTime.Now.Date;
            }

            LogFileStream?.Close();
            LogFileStream?.Dispose();
            ReportLog = delegate { };
        }
    }
}
