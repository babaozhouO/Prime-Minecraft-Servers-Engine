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

namespace PMCSsE_Backend.Modules
{
    internal class FullBackupHelper : IDisposable
    {
        private readonly MCServerManager MCServerManager;
        private SevenZipInvoker? SevenZipInvokerClass;
        private readonly CancellationTokenSource CancellationTokenSource;
        private string BackupFileName = "";
        /// <summary>
        /// 上报日志事件，参数依次为：日志级别、模块名称、日志内容。
        /// </summary>
        public event Action<string, string, string> ReportLog = delegate { };
        /// <summary>
        /// 备份任务完成时触发。
        /// </summary>
        public event Action TaskDone = delegate { };
        internal FullBackupHelper(MCServerManager mCServerManagerClass, CancellationTokenSource CancellationTokenSource)
        {
            MCServerManager = mCServerManagerClass;
            this.CancellationTokenSource = CancellationTokenSource;
        }

        internal void Run()
        {
            BackupFileName = $"{MCServerManager.MCServerManagerConfig.ManagerID}-{MCServerManager.MCServerManagerConfig.MCServerName}的全量备份-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.7z";
            if (!MCServerManager.isMCServerRunning)
            {
                SevenZipInvokerClass = new(CancellationTokenSource);
                SevenZipInvokerClass.ReportLog += (Type, Sender, Log) =>
                {
                    ReportLog(Type, Sender, Log);
                };
                SevenZipInvokerClass.ProcessExited += () =>
                {
                    SevenZipInvokerClass.Dispose();
                    Thread thread = new(() =>
                    {
                        SFTPClient sFTPClient = new(
                            MCServerManager.MCServerManagerConfig.BackupManagerConfig.SFTPClientConfig);

                        sFTPClient.ConnectTaskDone += (Bool) =>
                        {
                            if (Bool)
                            {
                                sFTPClient.UploadLocalFile(Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName), Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.RemoteBackupFileStoreDirectory, BackupFileName));
                            }
                            else
                            {
                                sFTPClient.Dispose();
                                TaskDone();
                            }
                        };
                        sFTPClient.UploadFileTaskDone += (Bool) =>
                        {
                            sFTPClient.Dispose();
                            TaskDone();

                        };
                        sFTPClient.ReportLog += (Type, Sender, Log) =>
                        {
                            ReportLog(Type, Sender, Log);
                        };

                        sFTPClient.ConnectSFTPServer();

                    });
                    thread.Start();
                    return;
                };
                SevenZipInvokerClass.Invoke7Zip(MCServerManager.MCServerManagerConfig.BackupManagerConfig.CompactionLevel,
                    MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFilesList,
                    MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFileExtensionsList,
                    MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFoldersList,
                    Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName),
                    MCServerManager.MCServerManagerConfig.MCServerDirectory);
                return;
            }

            if (MCServerManager.MCServerManagerConfig.BackupManagerConfig.StopServerBeforeBackup)
            {
                void HandleMCServerStatueChanged(string managerID, bool isRunning)
                {
                    MCServerManager.MCServerRunningStateChanged -= HandleMCServerStatueChanged;
                    if (!MCServerManager.isMCServerRunning)
                    {
                        SevenZipInvokerClass = new(CancellationTokenSource);
                        SevenZipInvokerClass.ReportLog += (Type, Sender, Log) =>
                        {
                            ReportLog(Type, Sender, Log);
                        };
                        SevenZipInvokerClass.ProcessExited += () =>
                        {
                            SevenZipInvokerClass.Dispose();
                            MCServerManager.StartMCServer();
                            Thread thread = new(() =>
                            {
                                SFTPClient sFTPClient = new(
                                    MCServerManager.MCServerManagerConfig.BackupManagerConfig.SFTPClientConfig);

                                sFTPClient.ConnectTaskDone += (Bool) =>
                                {
                                    if (Bool)
                                    {
                                        sFTPClient.UploadLocalFile(Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName), MCServerManager.MCServerManagerConfig.BackupManagerConfig.RemoteBackupFileStoreDirectory);
                                    }
                                    else
                                    {
                                        sFTPClient.Dispose();
                                        TaskDone();
                                    }
                                };
                                sFTPClient.UploadFileTaskDone += (Bool) =>
                                {
                                    sFTPClient.Dispose();
                                    TaskDone();

                                };
                                sFTPClient.ReportLog += (Type, Sender, Log) =>
                                {
                                    ReportLog(Type, Sender, Log);
                                };

                                sFTPClient.ConnectSFTPServer();

                            });
                            thread.Start();
                        };
                        SevenZipInvokerClass.Invoke7Zip(MCServerManager.MCServerManagerConfig.BackupManagerConfig.CompactionLevel,
                            MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFilesList,
                            MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFileExtensionsList,
                            MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFoldersList,
                            Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName),
                            MCServerManager.MCServerManagerConfig.MCServerDirectory);

                    }
                }
                ;
                MCServerManager.MCServerRunningStateChanged += HandleMCServerStatueChanged;
                MCServerManager.ShutdownMCServer();

            }
            else
            {
                void HandleGameSaved()
                {
                    SevenZipInvokerClass = new(CancellationTokenSource);
                    SevenZipInvokerClass.ReportLog += (Type, Sender, Log) =>
                    {
                        ReportLog(Type, Sender, Log);
                    };
                    SevenZipInvokerClass.ProcessExited += () =>
                    {
                        SevenZipInvokerClass.Dispose();
                        MCServerManager.SendCommand("save-on");

                        Thread thread = new(() =>
                        {
                            SFTPClient sFTPClient = new(
                                MCServerManager.MCServerManagerConfig.BackupManagerConfig.SFTPClientConfig);

                            sFTPClient.ConnectTaskDone += (Bool) =>
                            {
                                if (Bool)
                                {
                                    sFTPClient.UploadLocalFile(Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName), Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.RemoteBackupFileStoreDirectory, BackupFileName));
                                }
                                else
                                {
                                    sFTPClient.Dispose();
                                    TaskDone();
                                }
                            };
                            sFTPClient.UploadFileTaskDone += (Bool) =>
                            {
                                sFTPClient.Dispose();
                                TaskDone();

                            };
                            sFTPClient.ReportLog += (Type, Sender, Log) =>
                            {
                                ReportLog(Type, Sender, Log);
                            };

                            sFTPClient.ConnectSFTPServer();

                        });
                        thread.Start();
                    };
                    SevenZipInvokerClass.Invoke7Zip(MCServerManager.MCServerManagerConfig.BackupManagerConfig.CompactionLevel,
                        MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFilesList,
                        MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFileExtensionsList,
                        MCServerManager.MCServerManagerConfig.BackupManagerConfig.ExcludedFoldersList,
                        Path.Combine(MCServerManager.MCServerManagerConfig.BackupManagerConfig.BackupFileOutputDirectory, BackupFileName),
                        MCServerManager.MCServerManagerConfig.MCServerDirectory);
                }
                void HandleLog(string managerID,string log)
                {
                    if (log.Contains("Saved the game"))
                    {
                        MCServerManager.ReportServerLog -= HandleLog;
                        HandleGameSaved();
                    }
                }
                MCServerManager.ReportServerLog += HandleLog;
                MCServerManager.SendCommand("save-off");
                MCServerManager.SendCommand("save-all");

            }

        }
        /// <summary>
        /// 释放资源，清空事件订阅。
        /// </summary>
        public void Dispose()
        {
            ReportLog = delegate { };
            TaskDone = delegate { };
            GC.SuppressFinalize(this);
        }
    }
}
