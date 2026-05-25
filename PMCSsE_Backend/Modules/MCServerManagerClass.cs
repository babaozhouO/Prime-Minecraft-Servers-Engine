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

namespace PMCSsE_Backend.Modules
{
    /// <summary>
    /// 单个MC服务端管理类
    /// </summary>
    internal class MCServerManagerClass
    {
        /// <summary>
        /// 服务端运行状态
        /// </summary>
        internal bool isMCServerRunning = false;
        /// <summary>
        /// 服务端运行状态改变
        /// </summary>
        internal event Action MCServerRunningStatusChanged = delegate { };

        /// <summary>
        /// 上报服务端游戏已保存（备份功能使用
        /// </summary>
        internal event Action MCServerGameSaved = delegate { };
        internal event Action<bool> ReportBackupServiceRunningStatus = delegate { };
        internal event Action<string, byte, string, byte> ReportBackupProgress = delegate { };

        internal event Action<string, string, string> ReportLog = delegate { };
        /// <summary>
        /// 服务端进程
        /// </summary>
        private readonly System.Diagnostics.Process MCServerProcess;
        /// <summary>
        /// 当前管理器的配置
        /// </summary>
        internal MCServerManagerConfig MCServerManagerConfig;
        /// <summary>
        /// 当前管理器的备份工具
        /// </summary>
        private BackupHelperClass? BackupHelper = null;
        /// <summary>
        /// 当前管理器的在线聊天与管理工具
        /// </summary>
        internal OnlineChattingSystemClass? OnlineChattingSystem = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        internal MCServerManagerClass(MCServerManagerConfig mCServerManagerConfig)
        {
            MCServerManagerConfig = mCServerManagerConfig;
            MCServerProcess = new()
            {
                EnableRaisingEvents = true,
            };

            MCServerProcess.OutputDataReceived += (sender, OutputDataReceived) =>
            {
                if (!string.IsNullOrEmpty(OutputDataReceived.Data))
                {
                    if (OutputDataReceived.Data.Contains("Saved the game") || OutputDataReceived.Data.Contains("游戏已保存"))
                    {
                        MCServerGameSaved();
                    }
                    if (OutputDataReceived.Data.Contains("WARN"))
                    {
                        ReportLog("警告", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{OutputDataReceived.Data}");
                        return;
                    }
                    if (OutputDataReceived.Data.Contains("ERROR"))
                    {
                        ReportLog("错误", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{OutputDataReceived.Data}");
                        return;
                    }
                    ReportLog("信息", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{OutputDataReceived.Data}");

                }
            };


            MCServerProcess.ErrorDataReceived += (sender, ErrorDataReceived) =>
            {
                if (!string.IsNullOrEmpty(ErrorDataReceived.Data))
                {
                        if (ErrorDataReceived.Data.Contains("Saved the game") || ErrorDataReceived.Data.Contains("游戏已保存"))
                        {
                            MCServerGameSaved();
                        }
                        if (ErrorDataReceived.Data.Contains("WARN"))
                        {
                            ReportLog("警告", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{ErrorDataReceived.Data}");
                            return;
                        }
                        if (ErrorDataReceived.Data.Contains("ERROR"))
                        {
                            ReportLog("错误", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{ErrorDataReceived.Data}");
                            return;
                        }
                        ReportLog("信息", $"{MCServerManagerConfig.MCServerName}(服务端)", $"{ErrorDataReceived.Data}");
                 
                }
            };


            MCServerProcess.Exited += (sender, e) =>
            {
                MCServerProcess.CancelOutputRead();
                MCServerProcess.CancelErrorRead();

                isMCServerRunning = false;
                MCServerRunningStatusChanged();
                ReportLog("信息", "MC服务端管理器", "服务端已停止运行。");
            };

            InitializeLogsWriter();
            ReportLog("成功", "MC服务端管理器", "已初始化");

        }

        #region 服务端相关
        /// <summary>        
        /// 启动服务端
        /// </summary>
        internal void StartMCServer()
        {
            if ((MCServerProcess != null) && !isMCServerRunning && CheckConfig())
            {
                if (RunningStateRecorder.SystemCommandLineEncoding != System.Text.Encoding.UTF8)
                {
                    ReportLog("警告", "MC服务端管理器", $"系统控制台编码不是UTF8编码,若您修改了默认Java参数头部，服务端输出非英文字符可能乱码");
                }

                MCServerProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                MCServerProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                MCServerProcess.StartInfo.StandardInputEncoding = System.Text.Encoding.UTF8;
                MCServerProcess.StartInfo.RedirectStandardInput = true;
                MCServerProcess.StartInfo.RedirectStandardOutput = true;
                MCServerProcess.StartInfo.RedirectStandardError = true;
                MCServerProcess.StartInfo.UseShellExecute = false;
                MCServerProcess.StartInfo.CreateNoWindow = true;
                MCServerProcess.StartInfo.FileName = MCServerManagerConfig.JavaPath;
                MCServerProcess.StartInfo.Arguments = MCServerManagerConfig.StartUpArgument;
                MCServerProcess.StartInfo.WorkingDirectory = MCServerManagerConfig.MCServerDirectory;
                try
                {
                    if (MCServerProcess.Start())
                    {
                        MCServerProcess.BeginOutputReadLine();
                        MCServerProcess.BeginErrorReadLine();
                        isMCServerRunning = true;
                        MCServerRunningStatusChanged();
                        ReportLog("成功", "MC服务端管理器", "MC服务端已成功启动");
                        MCServerProcess.StandardInput.WriteLine("");
                    }
                }
                catch (Exception ex)
                {
                    ReportLog("错误", "MC服务端管理器", $"错误:({ex.Message})");
                    ReportLog("堆栈跟踪", "MC服务端管理器", $"堆栈跟踪:({ex.StackTrace})");
                    if (!isMCServerRunning)
                    {
                        ReportLog("失败", "MC服务端管理器", "MC服务端启动失败");
                    }
                }
            }
        }
        /// <summary>
        /// 检查配置
        /// </summary>
        /// <returns>返回正确与否</returns>
        private bool CheckConfig()
        {
            bool isOK = true;
            if (MCServerManagerConfig.MCServerDirectory == string.Empty)
            {
                isOK = false;
                ReportLog("错误", "管理面板", "MC服务端目录为空，请设置正确的目录");
            }
            if (!MCServerManagerConfig.MCServerDirectory.Contains('\\'))
            {
                isOK = false;
                ReportLog("错误", "管理面板", "MC服务端目录格式错误，请设置正确的目录");
            }
            if (MCServerManagerConfig.JavaPath == string.Empty)
            {
                isOK = false;
                ReportLog("错误", "管理面板", "Java路径为空，请设置正确的Java路径");
            }
            if (MCServerManagerConfig.StartUpArgument == string.Empty)
            {
                isOK = false;
                ReportLog("错误", "管理面板", "启动参数为空，请设置正确的启动参数");
            }
            return isOK;
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        internal void SendCommand(string Command)
        {
            if (!(MCServerProcess == null) && isMCServerRunning)
            {
                if (string.IsNullOrWhiteSpace(Command))
                {
                    ReportLog("错误", "MC服务端管理器", "命令为空");
                    return;
                }
                MCServerProcess.StandardInput.WriteLine(Command);
                MCServerProcess.StandardInput.Flush();

                ReportLog("成功", "MC服务端管理器", $"已发送[{Command}]命令");
            }
            else
            {
                ReportLog("错误", "MC服务端管理器", "服务端未启动");
            }
        }
        /// <summary>
        /// 发送命令(互联专用)
        /// </summary>
        internal void SendCommandHL(string Command)
        {
            if (!(MCServerProcess == null) && isMCServerRunning)
            {
                MCServerProcess.StandardInput.WriteLine(Command);
                MCServerProcess.StandardInput.Flush();
            }
        }
        /// <summary>
        /// stop服务端
        /// </summary>
        internal void ShutdownMCServer()
        {
            if ((MCServerProcess != null) && isMCServerRunning)
            {
                MCServerProcess.StandardInput.WriteLine("stop");

                ReportLog("成功", "MC服务端管理器", "已发送[stop]命令。");

            }
        }
        /// <summary>
        /// 终结服务端
        /// </summary>
        internal void KillMCServer()
        {
            if ((MCServerProcess != null) && isMCServerRunning)
            {
                MCServerProcess.Kill();

                ReportLog("成功", "MC服务端管理器", "已进行Kill操作。");

            }
        }
        #endregion
        #region 备份工具相关
        internal void InitializeBackupHelper(CancellationTokenSource cancellationTokenSource)
        {
            if (BackupHelper == null)
            {
                ReportLog("信息", "MC服务端管理器", "正在加载备份工具");
                BackupHelper = new(this, cancellationTokenSource);

                BackupHelper.ReportLog += ReportLog;

                BackupHelper.ReportProgress += ReportBackupProgress;
                BackupHelper.ReportServiceRunningStatue += ReportBackupServiceRunningStatus;
            }

        }
        #endregion
        #region 互联工具相关
        internal void InitializeOnlineChattingAndManager()
        {
            if (OnlineChattingSystem == null)
            {
                ReportLog("信息", "实时服内外通信和远程服务器管理器", "正在加载实时服内外通信与管理工具");
                OnlineChattingSystem = new(this);
                OnlineChattingSystem.ReportLog += (type, sender, log) =>
                {
                    this.ReportLog?.Invoke(type, sender, log);
                };
                ReportLog("信息", "实时服内外通信和远程服务器管理器", "加载完成");
            }
        }
        #endregion
        #region 日志处理
        private LogsWriterClass? LogsWriter = null;

        private void InitializeLogsWriter()
        {
            LogsWriter = new(MCServerManagerConfig);
            // 订阅 ReportLog 事件
            this.ReportLog += (type, sender, log) =>
            {
                string logLine = $"[{DateTime.Now:G}] | [{type}] | [{sender}] {log}";
                LogsWriter.AppendLog(logLine);
            };
            LogsWriter.ReportLog += this.ReportLog;
        }
        #endregion

        internal void Dispose()
        {
            MCServerRunningStatusChanged = delegate { };
            MCServerGameSaved = delegate { };
            ReportBackupServiceRunningStatus = delegate { };
            ReportBackupProgress = delegate { };
            ReportLog = delegate { };
            MCServerProcess.Dispose();

        }
    }
}
