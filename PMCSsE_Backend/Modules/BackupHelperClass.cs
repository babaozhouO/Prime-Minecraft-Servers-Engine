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
    internal class BackupHelperClass
    {
        private readonly MCServerManagerClass MCServerManager;

        internal CancellationTokenSource CancellationTokenSource;

        internal bool IsWaitingForBackup = false;

        internal bool IsBackupHelperRunning = false;

        internal event Action<bool> ReportServiceRunningStatue = delegate { };
        public event Action<string, string, string> ReportLog = delegate { };
        internal event Action<string, byte, string, byte> ReportProgress = delegate { };

        internal System.Timers.Timer CountDownTimer = new() { AutoReset = false };
        internal System.Timers.Timer OneSecondTimer = new() { AutoReset = true, Interval = 1000d };

        private DateTime NextExecuteTime;
        private TimeSpan NextExecuteTimeSpan;

        internal BackupHelperClass(MCServerManagerClass mCServerManagerClass,
            CancellationTokenSource CancellationTokenSource)
        {
            MCServerManager = mCServerManagerClass;
            this.CancellationTokenSource = CancellationTokenSource;
            CountDownTimer.Elapsed += (sender, e) =>
            {
                StopService();



                StartService();
            };

            OneSecondTimer.Elapsed += (sender, e) =>
            {
                if (IsWaitingForBackup)
                {
                    TimeSpan LeftTimeSpan = NextExecuteTime - DateTime.Now;
                    double LeftTimeSpanMs = LeftTimeSpan.TotalMilliseconds < 0 ? 0 : LeftTimeSpan.TotalMilliseconds;
                    double TotalTimeSpanMs = NextExecuteTimeSpan.TotalMilliseconds;
                    byte CountDownProgress = (byte)((float)(LeftTimeSpanMs / TotalTimeSpanMs) * 100);
                    byte LeftTimeSpanDays = (byte)(LeftTimeSpanMs / 1000 / 60 / 60 / 24);
                    byte LeftTimeSpanHours = (byte)(LeftTimeSpanMs / 1000 / 60 / 60 - LeftTimeSpanDays * 24);
                    byte LeftTimeSpanMin = (byte)(LeftTimeSpanMs / 1000 / 60 - LeftTimeSpanHours * 60 - LeftTimeSpanDays * 24 * 60);
                    byte LeftTimeSpanS = (byte)(LeftTimeSpanMs / 1000 - LeftTimeSpanMin * 60 - LeftTimeSpanHours * 60 * 60 - LeftTimeSpanDays * 24 * 60 * 60);

                    ReportProgress($"等待备份倒计时[{LeftTimeSpanDays}天 {LeftTimeSpanHours}时 {LeftTimeSpanMin}分 {LeftTimeSpanS}秒]", CountDownProgress, "无", 0);

                }
            };

            if (!IsWaitingForBackup && !IsBackupHelperRunning && MCServerManager.MCServerManagerConfig.BackupManagerConfig.AutoBackupEnabled)
            {
                StartService();
            }
        }

        internal void StartService()
        {
            DateTime NowTime = DateTime.Now;
            double NextExecuteTimeSpanMS;
            switch (MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupTimingMode)
            {
                case BackupTimingMode.DayInterval_SpecificTime:
                    string[] SplitedExecuteTime = MCServerManager.MCServerManagerConfig.BackupManagerConfig.SpecificTime.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    double[] SplitedExecuteTime_Double = [
                        Convert.ToDouble(SplitedExecuteTime[0]),
                        Convert.ToDouble(SplitedExecuteTime[1]),
                        Convert.ToDouble(SplitedExecuteTime[2])];
                    DateTime NextExecuteTime = NowTime.Date;
                    NextExecuteTime = NextExecuteTime.AddHours(SplitedExecuteTime_Double[0]);
                    NextExecuteTime = NextExecuteTime.AddMinutes(SplitedExecuteTime_Double[1]);
                    NextExecuteTime = NextExecuteTime.AddSeconds(SplitedExecuteTime_Double[2]);
                    if (NextExecuteTime <= NowTime)
                    {
                        NextExecuteTime = NextExecuteTime.AddDays(Convert.ToDouble(MCServerManager.MCServerManagerConfig.BackupManagerConfig.DayInterval));
                    }
                    TimeSpan timeSpan = NextExecuteTime - NowTime;
                    NextExecuteTimeSpanMS = timeSpan.TotalMilliseconds;

                    this.NextExecuteTime = NextExecuteTime;
                    NextExecuteTimeSpan = timeSpan;

                    break;

                case BackupTimingMode.FixedTimeInterval:
                    string[] SplitedTimeSpan = MCServerManager.MCServerManagerConfig.BackupManagerConfig.TimeInterval.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    int[] SplitedTimeSpan_int = [
                        Convert.ToInt32(SplitedTimeSpan[0]),
                        Convert.ToInt32(SplitedTimeSpan[1]),
                        Convert.ToInt32(SplitedTimeSpan[2])];
                    TimeSpan timeSpan1 = new(SplitedTimeSpan_int[0], SplitedTimeSpan_int[1], SplitedTimeSpan_int[2]);
                    NextExecuteTimeSpanMS = timeSpan1.TotalMilliseconds;

                    this.NextExecuteTime = NowTime + timeSpan1;
                    NextExecuteTimeSpan = timeSpan1;
                    break;

                default:
                    return;
            }

            CountDownTimer.Interval = NextExecuteTimeSpanMS;
            CountDownTimer.Start();
            OneSecondTimer.Start();
            IsWaitingForBackup = true;


            ReportServiceRunningStatue(true);

        }

        internal void StopService()
        {
            IsWaitingForBackup = false;
            CountDownTimer.Stop();
            OneSecondTimer.Stop();


            ReportProgress("无", 0, "无", 0);
            ReportServiceRunningStatue(false);

        }

        internal void StartBackup()
        {
            if (!CheckConfig())
            {
                return;
            }
            switch (MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupMode)
            {
                case BackupMode.Full:
                    FullBackupHelperClass fullBackupHelperClass = new(MCServerManager, CancellationTokenSource);
                    fullBackupHelperClass.ReportLog += ReportLog;
                    fullBackupHelperClass.Run();
                    break;
                case BackupMode.FileLevel_Incremental:
                    ReportLog("信息", "MC服务端备份工具", "功能暂未实现/暂不稳定，无法使用");
                    //FileLevelIncrementalBackupHelperClass fileLevelIncrementalBackupHelperClass = new(MCServerManager.MCServerManagerConfig);
                    break;
                case BackupMode.BlockLevel_Incremental:
                    ReportLog("信息", "MC服务端备份工具", "功能暂未实现/暂不稳定，无法使用");
                    //BlockLevelIncremntalBackupHelperClass blockLevelIncremntalBackupHelperClass = new(MCServerManager.MCServerManagerConfig);
                    break;
                default:
                    break;
            }
        }

        private bool CheckConfig()
        {
            bool IsConfigValid = true;

            if (string.IsNullOrEmpty(MCServerManager.MCServerManagerConfig.MCServerDirectory))
            {
                IsConfigValid = false;
                ReportLog("错误", "MC服务端备份工具", "MC服务端目录未设置");
            }
            if (!Path.Exists(MCServerManager.MCServerManagerConfig.MCServerDirectory))
            {
                IsConfigValid = false;
                ReportLog("错误", "MC服务端备份工具", "MC服务端目录不存在");
            }



            return IsConfigValid;
        }

        public void Dispose()
        {
            CountDownTimer?.Dispose();
            OneSecondTimer?.Dispose();
            ReportServiceRunningStatue = delegate { };
            ReportLog = delegate { };
            ReportProgress = delegate { };
        }
    }
}

