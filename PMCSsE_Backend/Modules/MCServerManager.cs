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
using PMCSsE_Communicator.SharedCodes;
using System.Diagnostics;
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
        public event Action<string, bool> MCServerRunningStateChanged = delegate { };
        /// <summary>
        /// 上报管理器日志（ID，log）
        /// </summary>
        public event Action<string, string> ReportManagerLog = delegate { };
        /// <summary>
        /// 上报服务端日志（ID，log）
        /// </summary>
        public event Action<string, string> ReportServerLog = delegate { };
        /// <summary>
        /// 服务端日志
        /// </summary>
        internal LogStore ServerLogs = new();
        /// <summary>
        /// 服务端进程
        /// </summary>
        private readonly Process MCServerProcess;
        /// <summary>
        /// 当前管理器的配置
        /// </summary>
        internal MCServerManagerConfig MCServerManagerConfig;
        /// <summary>
        /// 当前管理器的备份工具
        /// </summary>
        internal readonly BackupManager BackupManager;
        ///// <summary>
        ///// 当前管理器的在线聊天与管理工具
        ///// </summary>
        //internal readonly OnlineChattingSystemClass? OnlineChattingSystem;
        /// <summary>
        /// 高危参数黑名单
        /// </summary>
        public static readonly string[] DangerousArgs = ["-agentpath:", "-agentlib:", "-Djava.library.path="];
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

            MCServerProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            MCServerProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            MCServerProcess.StartInfo.StandardInputEncoding = new UTF8Encoding(false);
            MCServerProcess.StartInfo.RedirectStandardInput = true;
            MCServerProcess.StartInfo.RedirectStandardOutput = true;
            MCServerProcess.StartInfo.RedirectStandardError = true;
            MCServerProcess.StartInfo.UseShellExecute = false;
            MCServerProcess.StartInfo.CreateNoWindow = true;

            MCServerProcess.OutputDataReceived += HandleServerLogReceived;


            MCServerProcess.ErrorDataReceived += HandleServerLogReceived;


            MCServerProcess.Exited += HandleServerExited;

            BackupManager = new(this);
            //OnlineChattingSystem = new(this);


            ReportManagerLog(MCServerManagerConfig.ManagerID, "已初始化管理器");
            ServerLogs.Add("管理器已初始化");
        }

        #region 服务端相关
        private void HandleServerLogReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                var log = e.Data;
                ServerLogs.Add(log);
                ReportServerLog(MCServerManagerConfig.ManagerID, log);
            }
        }
        private void HandleServerExited(object? sender, EventArgs e)
        {
            MCServerProcess.CancelOutputRead();
            MCServerProcess.CancelErrorRead();

            isMCServerRunning = false;
            MCServerRunningStateChanged(MCServerManagerConfig.ManagerID, false);
            ReportManagerLog(MCServerManagerConfig.ManagerID, "服务端已停止运行。");
        }
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
                        MCServerRunningStateChanged(MCServerManagerConfig.ManagerID, true);
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
            if (!MCServerManagerConfig.MCServerDirectory.Contains(OperatingSystem.IsWindows() ? '\\' : '/'))
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
        public bool SendCommand(string Command)
        {
            if (isMCServerRunning)
            {
                if (string.IsNullOrWhiteSpace(Command))
                {
                    ReportManagerLog(MCServerManagerConfig.ManagerID, "不可发送空命令");
                    return false;
                }
                MCServerProcess.StandardInput.WriteLine(Command);
                MCServerProcess.StandardInput.Flush();

                ReportManagerLog(MCServerManagerConfig.ManagerID, $"已向服务端发送[{Command}]命令");
                return true;
            }
            else
            {
                ReportManagerLog(MCServerManagerConfig.ManagerID, "发送命令失败，服务端未启动");
                return false;
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
        private readonly LogsWriter LogsWriter;
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
            MCServerProcess.OutputDataReceived -= HandleServerLogReceived;
            MCServerProcess.ErrorDataReceived -= HandleServerLogReceived;
            MCServerProcess.Exited -= HandleServerExited;
            MCServerRunningStateChanged = delegate { };
            ReportManagerLog = delegate { };
            MCServerProcess.Dispose();

        }
    }
    /// <summary>
    /// 来自DeepSeek，实现了日志存储
    /// </summary>
    internal class LogStore
    {
        private readonly Dictionary<ulong, string> _logs = [];
        private readonly LinkedList<ulong> _order = new();
        private ulong _nextId = 1;
        /// <summary>
        /// 最大持有量
        /// </summary>
        private readonly int _capacity;
        private readonly Lock _lock = new();

        internal LogStore(int capacity = 10_000)
        {
            _capacity = capacity;
        }

        /// <summary>添加一条日志，返回其全局唯一编号。</summary>
        internal ulong Add(string log)
        {
            using (_lock.EnterScope())
            {
                ulong id = _nextId++;//先赋值再+1
                _logs[id] = log;//添加日志
                _order.AddLast(id);//追加编号到有链表末尾

                while (_order.Count > _capacity)//
                {
                    ulong oldest = _order.First!.Value;//获取最旧日志的编号
                    _order.RemoveFirst();
                    _logs.Remove(oldest);
                }
                return id;
            }
        }

        /// <summary>（获取新日志）获取从某个编号之后（不含）的最多 count 条日志，按时间升序返回。</summary>
        /// <param name="after">起始编号，为 null 表示从最早可用日志开始</param>
        /// <param name="count">最大返回条数</param>
        /// <param name="resetHint">若因游标失效而强制返回最新日志，则设为 true</param>
        internal LogEntry[] GetAfter(ulong? after, int count, out bool resetHint)
        {
            using (_lock.EnterScope())
            {
                resetHint = false;

                // 确定起始节点
                LinkedListNode<ulong>? node;
                if (after == null)
                {
                    node = _order.First;
                }
                else
                {
                    // 如果请求的 after 大于当前最大编号，说明游标失效（服务重启或日志全部淘汰）
                    if (after.Value >= _nextId)
                    {
                        resetHint = true;
                        return GetLatest(count); // 返回最新 count 条
                    }

                    node = FindFirstAfter(after.Value);
                    // 如果 after 对应的日志已被淘汰（after < 当前最小编号），重置游标
                    if (node == null && _order.Count > 0 && after.Value < _order.First!.Value)
                    {
                        resetHint = true;
                        return GetLatest(count);
                    }
                }

                LogEntry[] result = new LogEntry[count];
                int written = 0;
                while (node != null && written < count)
                {
                    result[written++] = new LogEntry(node.Value, _logs[node.Value]);
                    node = node.Next;
                }
                return written == count ? result : result[..written];
            }
        }

        /// <summary>获取最新 count 条日志。</summary>
        internal LogEntry[] GetLatest(int count)
        {
            using (_lock.EnterScope())
            {
                count = Math.Min(count, _order.Count);
                LogEntry[] result = new LogEntry[count];
                var node = _order.Last;
                for (int i = count - 1; i >= 0 && node != null; i--)
                {
                    result[i] = new LogEntry(node.Value, _logs[node.Value]);
                    node = node.Previous;
                }
                return result;
            }
        }

        internal ulong CurrentMaxId
        {
            get { using (_lock.EnterScope()) { return _nextId; } }
        }

        internal LinkedListNode<ulong>? FindFirstAfter(ulong id)
        {
            var node = _order.First;
            while (node != null && node.Value <= id)//向后遍历找到id+1的节点
                node = node.Next;
            return node;
        }

        /// <summary>
        /// （获取旧日志）获取某个编号之前（不含）的最多 count 条日志，按时间升序返回。
        /// <para>参数 before 为起始编号（不含），为 null 表示返回最新的 count 条日志。</para>
        /// <para>当传入的 before 超出当前日志范围（例如大于等于当前最大编号或小于最小可用编号）时，
        /// 会将 resetHint 置为 true 并返回最新的 count 条日志，以提示调用方游标已失效需要重置。</para>
        /// </summary>
        internal LogEntry[] GetBefore(ulong? before, int count, out bool resetHint)
        {
            using (_lock.EnterScope())
            {
                resetHint = false;
                // null 表示请求最新的几条日志
                if (before == null)
                {
                    return GetLatest(count);
                }

                // 若请求的 before 超过当前最大编号，说明游标失效，返回最新日志并提示重置
                if (before.Value > _nextId)
                {
                    resetHint = true;
                    return GetLatest(count);
                }

                // 找到第一个小于 before 的节点（即最后一个在 before 之前的节点）
                var node = FindLastBefore(before.Value);

                // 如果没有找到，可能是因为请求的 before 在可用日志之前
                if (node == null)
                {
                    if (_order.Count > 0 && before.Value <= _order.First!.Value)
                    {
                        resetHint = true;
                        return GetLatest(count);
                    }
                    return [];
                }

                // 从该节点向前收集最多 count 条（向前即时间更早），从数组末尾往前填以保持时间升序
                LogEntry[] result = new LogEntry[count];
                int written = 0;
                var cur = node;
                for (int i = count - 1; i >= 0 && cur != null; i--)
                {
                    result[i] = new LogEntry(cur.Value, _logs[cur.Value]);
                    written++;
                    cur = cur.Previous;
                }

                return written == count ? result : result[(count - written)..];
            }
        }

        internal LinkedListNode<ulong>? FindLastBefore(ulong id)
        {
            var node = _order.Last;
            while (node != null && node.Value >= id) // 向前遍历找到第一个小于 id 的节点
                node = node.Previous;
            return node;
        }
    }
}
