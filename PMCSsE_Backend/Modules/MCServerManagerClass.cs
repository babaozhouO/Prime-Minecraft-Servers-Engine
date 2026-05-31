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
using System.Reflection;
using System.Text;

namespace PMCSsE_Backend.Modules
{
    /// <summary>
    /// 单个MC服务端管理类
    /// </summary>
    public class MCServerManager
    {
        /// <summary>
        /// 服务端运行状态
        /// </summary>
        public bool isMCServerRunning = false;
        /// <summary>
        /// 服务端运行状态改变
        /// </summary>
        public event Action<string,bool> MCServerRunningStateChanged = delegate { };
        /// <summary>
        /// 上报管理器日志（ID，log）
        /// </summary>
        public event Action<string, string> ReportManagerLog = delegate { };
        /// <summary>
        /// 上报服务端日志（ID，log）
        /// </summary>
        public event Action<string, string> ReportServerLog = delegate { };
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
        private BackupManager BackupManager;
        /// <summary>
        /// 当前管理器的在线聊天与管理工具
        /// </summary>
        internal OnlineChattingSystemClass OnlineChattingSystem;
        /// <summary>
        /// 构造函数
        /// </summary>
        internal MCServerManager(MCServerManagerConfig mCServerManagerConfig)
        {
            MCServerManagerConfig = mCServerManagerConfig;
            this.ReportManagerLog += WriteManagerLog;
            LogsWriter = new(MCServerManagerConfig);
            LogsWriter.ReportLog += HandleLogsWriterLog;
            MCServerProcess = new()
            {
                EnableRaisingEvents = true
            };

            MCServerProcess.OutputDataReceived += (sender, OutputDataReceived) =>
            {
                if (!string.IsNullOrEmpty(OutputDataReceived.Data))
                {
                    ReportServerLog(MCServerManagerConfig.ManagerID, OutputDataReceived.Data);
                }
            };


            MCServerProcess.ErrorDataReceived += (sender, ErrorDataReceived) =>
            {
                if (!string.IsNullOrEmpty(ErrorDataReceived.Data))
                {
                    ReportServerLog(MCServerManagerConfig.ManagerID, ErrorDataReceived.Data);
                }
            };


            MCServerProcess.Exited += (sender, e) =>
            {
                MCServerProcess.CancelOutputRead();
                MCServerProcess.CancelErrorRead();

                isMCServerRunning = false;
                MCServerRunningStateChanged(MCServerManagerConfig.ManagerID,false);
                ReportManagerLog(MCServerManagerConfig.ManagerID, "服务端已停止运行。");
            };

            BackupManager = new(this);
            OnlineChattingSystem = new(this);


            ReportManagerLog(MCServerManagerConfig.ManagerID, "已初始化管理器");

        }

        #region 服务端相关
        /// <summary>        
        /// 启动服务端
        /// </summary>
        ///  <returns>返回值0：正确，1：空目录2：不合法的目录，3：空Java路径，4：空启动参数,5:启动失败，6：仍在运行</returns>
        public int StartMCServer()
        {
            int flag = CheckConfig();
            if (flag != 0) return flag;
            if (!isMCServerRunning)
            {
                MCServerProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                MCServerProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                MCServerProcess.StartInfo.StandardInputEncoding = new UTF8Encoding(false);
                MCServerProcess.StartInfo.RedirectStandardInput = true;
                MCServerProcess.StartInfo.RedirectStandardOutput = true;
                MCServerProcess.StartInfo.RedirectStandardError = true;
                MCServerProcess.StartInfo.UseShellExecute = false;
                MCServerProcess.StartInfo.CreateNoWindow = true;
                MCServerProcess.StartInfo.FileName = MCServerManagerConfig.JavaPath;
                MCServerProcess.StartInfo.Arguments = MCServerManagerConfig.StartUpArguments;
                MCServerProcess.StartInfo.WorkingDirectory = MCServerManagerConfig.MCServerDirectory;
                try
                {
                    if (MCServerProcess.Start())
                    {
                        MCServerProcess.BeginOutputReadLine();
                        MCServerProcess.BeginErrorReadLine();
                        isMCServerRunning = true;
                        MCServerRunningStateChanged(MCServerManagerConfig.ManagerID,true);
                        ReportManagerLog(MCServerManagerConfig.ManagerID, "MC服务端已成功启动");
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    ReportManagerLog(MCServerManagerConfig.ManagerID, $"错误:({ex.Message})");
                    ReportManagerLog(MCServerManagerConfig.ManagerID, $"堆栈跟踪:({ex.StackTrace})");
                    if (!isMCServerRunning)
                    {
                        ReportManagerLog(MCServerManagerConfig.ManagerID, "MC服务端启动失败");
                    }
                    return 5;
                }
            }
            return 6;
        }
        /// <summary>
        /// 检查配置
        /// </summary>
        /// <returns>返回值0：正确，1：空目录2：不合法的目录，3：空Java路径，4：空启动参数</returns>
        public int CheckConfig()
        {
            if (MCServerManagerConfig.MCServerDirectory == string.Empty)
            {
                return 1;
            }
            if (!MCServerManagerConfig.MCServerDirectory.Contains('\\'))
            {
                return 2;
            }
            if (MCServerManagerConfig.JavaPath == string.Empty)
            {
                return 3;
            }
            if (MCServerManagerConfig.StartUpArguments == string.Empty)
            {
                return 4;
            }
            return 0;
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        public void SendCommand(string Command)
        {
            if (isMCServerRunning)
            {
                if (string.IsNullOrWhiteSpace(Command))
                {
                    ReportManagerLog(MCServerManagerConfig.ManagerID, "不可发送空命令");
                    return;
                }
                MCServerProcess.StandardInput.WriteLine(Command);
                MCServerProcess.StandardInput.Flush();

                ReportManagerLog(MCServerManagerConfig.ManagerID, $"已向服务端发送[{Command}]命令");
            }
            else
            {
                ReportManagerLog(MCServerManagerConfig.ManagerID, "发送命令失败，服务端未启动");
            }
        }
        /// <summary>
        /// 发送命令(互联专用)
        /// </summary>
        public void SendCommandHL(string Command)
        {
            if (isMCServerRunning)
            {
                MCServerProcess.StandardInput.WriteLine(Command);
                MCServerProcess.StandardInput.Flush();
            }
        }
        /// <summary>
        /// stop服务端
        /// </summary>
        public bool ShutdownMCServer()
        {
            if (isMCServerRunning)
            {
                MCServerProcess.StandardInput.WriteLine("stop");
                ReportManagerLog(MCServerManagerConfig.ManagerID, "已发送[stop]命令。");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 终结服务端
        /// </summary>
        public bool KillMCServer()
        {
            if (isMCServerRunning)
            {
                try
                {
                    MCServerProcess.Kill();
                    ReportManagerLog(MCServerManagerConfig.ManagerID, "已进行强制终结MC服务端操作。");
                    return true;
                }
                catch (Exception ex)
                {
                    ReportManagerLog(MCServerManagerConfig.ManagerID, $"进行强制终结MC服务端操作时发生异常：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
                return false;
            }
            ReportManagerLog(MCServerManagerConfig.ManagerID, "进行强制终结MC服务端操作时发现服务端未运行");
            return false;
        }
        #endregion
        #region 备份工具相关
        #endregion
        #region 互联工具相关
        #endregion
        #region 日志处理
        private readonly LogsWriterClass LogsWriter;
        private void WriteManagerLog(string managerID, string log)
        {
            string logLine = $"[{DateTime.Now:G}] | {log}";
            LogsWriter.AppendLog(logLine);
        }
        private void HandleLogsWriterLog(string log)
        {
            ReportManagerLog(MCServerManagerConfig.ManagerID, log);
        }
        #endregion
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (isMCServerRunning)
            {
                ShutdownMCServer();
            }
            //BackupHelper
            LogsWriter.Dispose();
            LogsWriter.ReportLog -= HandleLogsWriterLog;
            ReportManagerLog -= WriteManagerLog;
            MCServerRunningStateChanged = delegate { };
            ReportManagerLog = delegate { };
            MCServerProcess.Dispose();

        }
    }
}
