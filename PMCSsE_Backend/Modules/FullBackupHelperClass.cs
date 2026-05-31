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
    internal class FullBackupHelperClass : IDisposable, IReportLog
    {
        private readonly MCServerManager MCServerManager;
        private SevenZipInvokerClass? SevenZipInvokerClass;
        private readonly CancellationTokenSource CancellationTokenSource;
        private string BackupFileName = "";
        public event Action<string, string, string> ReportLog = delegate { };
        public event Action TaskDone = delegate { };
        internal FullBackupHelperClass(MCServerManager mCServerManagerClass, CancellationTokenSource CancellationTokenSource)
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
                        SFTPClientClass sFTPClient = new(
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
                Action MCServerStatueChanged = delegate { };
                MCServerStatueChanged += () =>
                {
                    MCServerManager.MCServerRunningStateChanged -= MCServerStatueChanged;
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
                                SFTPClientClass sFTPClient = new(
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
                };
                MCServerManager.MCServerRunningStateChanged += MCServerStatueChanged;
                MCServerManager.ShutdownMCServer();

            }
            else
            {
                Action GameSaved = delegate { };
                GameSaved += () =>
                {
                    MCServerManager.MCServerGameSaved -= GameSaved;

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
                            SFTPClientClass sFTPClient = new(
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
                };
                MCServerManager.MCServerGameSaved += GameSaved;
                MCServerManager.SendCommand("save-off");
                MCServerManager.SendCommand("save-all");

            }

        }
        public void Dispose()
        {
            ReportLog = delegate { };
            TaskDone = delegate { };
            GC.SuppressFinalize(this);
        }
    }
}
