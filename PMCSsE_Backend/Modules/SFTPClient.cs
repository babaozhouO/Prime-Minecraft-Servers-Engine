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
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Text;

namespace PMCSsE_Backend.Modules;

internal class SFTPClient
{
    private readonly SftpClient SftpClient;
    private string? TransferringRemoteFilePath;
    private long LocalFileByteCount;
    private long LastTimeByteCount = 0L;
    private long LatestByteCount = 0L;
    private readonly System.Timers.Timer Timer = new() { Interval = 1000d, AutoReset = true };

    private readonly long BufferSize = 1024L * 1024L * 10L - 1L;

    public event Action<string, string, string> ReportLog = delegate { };

    internal event Action Initialized = delegate { };

    internal event Action<bool> ConnectTaskDone = delegate { };

    internal event Action<bool> UploadFileTaskDone = delegate { };

    internal event Action<bool> DeleteFileTaskDone = delegate { };

    internal event Action<bool> TestTaskDone = delegate { };

    internal event Action<string, string, string> ReportTransmissionSpeedAndProcess = delegate { };
    internal SFTPClient(string host, int port, string username, string password, int bufferSize = 10)
    {
        BufferSize = bufferSize * 1024 * 1024 - 1;
        PasswordConnectionInfo passwordConnectionInfo = new(host, port, username, password) { Encoding = Encoding.UTF8 };
        SftpClient = new(passwordConnectionInfo);
        Timer.Elapsed += (sender, e) => CalculateTransmissionSpeedAndProcess();


        ReportLog("信息", "SFTP客户端", "已初始化客户端");

        Initialized();

    }

    internal SFTPClient(SFTPClientConfig SFTPHelperConfig)
    {
        BufferSize = SFTPHelperConfig.BufferSize * 1024 * 1024 - 1;
        PasswordConnectionInfo passwordConnectionInfo = new(
            SFTPHelperConfig.Host,
            SFTPHelperConfig.Port,
            SFTPHelperConfig.UserName,
            SFTPHelperConfig.Password)
        { Encoding = Encoding.UTF8 };
        SftpClient = new(passwordConnectionInfo);
        Timer.Elapsed += (sender, e) => CalculateTransmissionSpeedAndProcess();

        ReportLog("信息", "SFTP客户端", "已初始化客户端");

        Initialized();

    }

    internal async Task ConnectSFTPServer(CancellationToken token)
    {
        if (!SftpClient.IsConnected)
        {
            try
            {
                await SftpClient.ConnectAsync(token);
            }
            catch (Exception ex)
            {

                ReportLog("错误", "SFTP客户端", $"错误:({ex.Message})");
                ReportLog("失败", "SFTP客户端", "连接失败");

                ConnectTaskDone(false);
                return;
            }

            if (SftpClient.IsConnected)
            {

                ReportLog("成功", "SFTP客户端", "已连接至服务器");

                ConnectTaskDone(true);
            }
            else
            {

                ReportLog("失败", "SFTP客户端", "连接失败");

                ConnectTaskDone(false);
            }
        }
        else
        {
            ReportLog("成功", "SFTP客户端", "已连接至服务器");
        }
    }

    private void CalculateTransmissionSpeedAndProcess()
    {
        try
        {
            if (TransferringRemoteFilePath == null) { return; }
            LatestByteCount = SftpClient.Get(TransferringRemoteFilePath).Length;
            string Speed = $"{ConversionUnits(LatestByteCount - LastTimeByteCount)}/s";
            string Process = $"{(byte)((float)LatestByteCount / LocalFileByteCount * 100)}%";
            string SpeedAndProcessText = $"[SFTP客户端]:当前进度:{Process} | 传输速度:{Speed}/s";


            ReportTransmissionSpeedAndProcess(SpeedAndProcessText, Process, Speed);
            ReportLog("信息", "SFTP客户端", SpeedAndProcessText);


            LastTimeByteCount = LatestByteCount;
        }
        catch
        {
            return;
        }
    }

    private static string ConversionUnits(long ByteCount)
    {
        string[] UnitsArray = ["B", "KB", "MB", "GB"];

        int index = 0;
        while (ByteCount >= 1024L && index < UnitsArray.Length - 1)
        {
            ByteCount /= 1024L;
            ++index;
        }
        return $"{ByteCount} {UnitsArray[index]}";
    }

    internal void Test()
    {
        long StartPosition = 0L;
        long TestFileByteCount = 1024L * 1024L * 10L;//10mib文件
        LocalFileByteCount = TestFileByteCount;
        TransferringRemoteFilePath = "/测试文件.测试文件";
        int RetryCount = 0;
        int MaxRetryCount = 5;

        using SftpFileStream RemoteFileStream = SftpClient.OpenWrite(TransferringRemoteFilePath);
        {
            byte[] Buffer = new byte[1024L * 1024L * 1L - 1L];//缓冲区
            Timer.Start();

            for (; RetryCount <= MaxRetryCount; RetryCount++)
            {
                try
                {
                    if (!SftpClient.IsConnected)
                    {
                        SftpClient.Connect();

                        ReportLog("成功", "SFTP客户端", "重连成功");

                    }
                    while (StartPosition < TestFileByteCount)
                    {
                        RemoteFileStream.Position = StartPosition;

                        ReportLog("调试", "SFTP客户端", $"已设置上传流起始点[{StartPosition}]");

                        RemoteFileStream.Write(Buffer, 0, (int)(TestFileByteCount - StartPosition > Buffer.Length ? Buffer.Length : TestFileByteCount - StartPosition));

                        ReportLog("调试", "SFTP客户端", $"已将[{(TestFileByteCount - StartPosition > 1048576 ? 1048576 : TestFileByteCount - StartPosition)}B]的数据发送至上传流");


                        StartPosition = SftpClient.Get(TransferringRemoteFilePath).Length;

                        ReportLog("调试", "SFTP客户端", $"已获取到远程文件的字节数[{StartPosition}]");


                    }
                    if (StartPosition == TestFileByteCount)
                    {

                        ReportLog("成功", "SFTP客户端", "文件已完整上传");

                        break;
                    }

                }
                catch (Exception ex)
                {
                    RetryCount++;
                    if (SftpClient.IsConnected)
                    {


                        ReportLog("错误", "SFTP客户端", $"上传失败（{ex.Message}）");
                        ReportLog("程序操作", "SFTP客户端", $"5S后重试（第{RetryCount}次）");

                    }
                    else
                    {

                        ReportLog("错误", "SFTP客户端", $"上传失败（{ex.Message}）");
                        ReportLog("程序操作", "SFTP客户端", $"连接中断，5s后尝试重新连接（第{RetryCount}次重试）");

                    }
                    Thread.Sleep(5000);
                }
            }

            if (SftpClient.IsConnected)
            {
                RemoteFileStream.Dispose();
            }
            Buffer = [];
            Timer.Stop();

        }
        if (RetryCount <= MaxRetryCount)
        {

            ReportLog("成功", "SFTP客户端", "已成功上传大小为10MiB的空文件");



            if (!SftpClient.IsConnected)
            {

                ReportLog("错误", "SFTP客户端", "未连接至服务器");
                ReportLog("失败", "SFTP客户端", "删除远程文件失败");
                TestTaskDone(false);


            }
            else
            {
                try
                {
                    SftpClient.DeleteFile("/测试文件.测试文件");

                    ReportLog("成功", "SFTP客户端", "成功删除远程文件");
                    TestTaskDone(true);

                }
                catch (Exception ex)
                {

                    ReportLog("错误", "SFTP客户端", $"错误:({ex.Message})");
                    ReportLog("失败", "SFTP客户端", "删除远程文件失败");
                    TestTaskDone(false);

                }
            }
        }
        else
        {

            ReportLog("错误", "SFTP客户端", $"上传失败已达最大重试次数{MaxRetryCount}");
            ReportLog("失败", "SFTP客户端", "文件上传失败");
            TestTaskDone(false);

        }
    }
    internal void UploadLocalFile(string LocalFilePath, string RemoteFileDirectory)
    {
        if (!SftpClient.IsConnected)
        {

            ReportLog("错误", "SFTP客户端", "未连接至SFTP服务器");
            ReportLog("失败", "SFTP客户端", "已终止上传操作");
            UploadFileTaskDone(false);

            return;
        }
        else if (string.IsNullOrEmpty(LocalFilePath))
        {

            ReportLog("错误", "SFTP客户端", "未指定要上传的本地文件");
            ReportLog("失败", "SFTP客户端", "已终止上传操作");
            UploadFileTaskDone(false);

            return;
        }
        else if (!File.Exists(LocalFilePath))
        {

            ReportLog("错误", "SFTP客户端", "指定的本地文件不存在");
            ReportLog("失败", "SFTP客户端", "已终止上传操作");
            UploadFileTaskDone(false);

            return;
        }
        else
        {
            string FixedRemoteFileDirectory;
            if (!RemoteFileDirectory.StartsWith('/') && !RemoteFileDirectory.StartsWith('\\'))
            {
                FixedRemoteFileDirectory = $"/{RemoteFileDirectory}";
            }

            else if (!RemoteFileDirectory.StartsWith('/') && RemoteFileDirectory.StartsWith('\\'))
            {
                FixedRemoteFileDirectory = RemoteFileDirectory.Replace('\\', '/');
            }
            else
            {
                FixedRemoteFileDirectory = RemoteFileDirectory;
            }
            FixedRemoteFileDirectory = FixedRemoteFileDirectory.Replace('\\', '/').TrimEnd('/');

            if (string.IsNullOrEmpty(FixedRemoteFileDirectory))
            {
                FixedRemoteFileDirectory = "/";
            }
            if (SftpClient.Exists(FixedRemoteFileDirectory))
            {

                ReportLog("信息", "SFTP客户端", "远程目录已存在");

            }
            else
            {

                ReportLog("信息", "SFTP客户端", "远程目录不存在");
                ReportLog("程序操作", "SFTP客户端", "将创建远程目录");


                string[] SplitedFixedRemoteFileDirectory = FixedRemoteFileDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries);
                string CurrentDirectory = "/";
                int index = 0;
                while (index < SplitedFixedRemoteFileDirectory.Length)
                {
                    string NextDirectoryName = SplitedFixedRemoteFileDirectory[index];
                    string NextDirectory = $"{CurrentDirectory.TrimEnd('/')}/{NextDirectoryName}";
                    try
                    {
                        if (!SftpClient.Exists(NextDirectory))
                        {
                            SftpClient.ChangeDirectory(CurrentDirectory);
                            SftpClient.CreateDirectory(NextDirectoryName);
                            if (!SftpClient.Exists(NextDirectory))
                            {

                                ReportLog("错误", "SFTP客户端", "远程目录创建失败");
                                ReportLog("失败", "SFTP客户端", "已终止上传操作");
                                UploadFileTaskDone(false);

                                return;
                            }
                            CurrentDirectory = NextDirectory;
                            ++index;
                        }
                    }
                    catch (Exception ex)
                    {

                        ReportLog("错误", "SFTP客户端", $"错误:({ex.Message})");
                        ReportLog("失败", "SFTP客户端", "远程目录创建失败");
                        ReportLog("失败", "SFTP客户端", "已终止上传操作");

                        return;
                    }
                }

                ReportLog("成功", "SFTP客户端", "远程目录创建成功");

            }

            FileInfo LocalFileInfo = new(LocalFilePath);
            string LocalFileName = LocalFileInfo.Name;
            string RemoteFilePath = FixedRemoteFileDirectory.EndsWith('/') ? $"{FixedRemoteFileDirectory}{LocalFileName}" : $"{FixedRemoteFileDirectory}/{LocalFileName}";
            string TransferringRemoteFilePath = $"{RemoteFilePath}.文件块";
            long StartPosition;
            if (SftpClient.Exists(RemoteFilePath))
            {

                ReportLog("信息", "SFTP客户端", "远程文件已存在");
                ReportLog("等待用户操作", "SFTP客户端", "等待用户确认是否覆盖");

                bool Result = true; //AskUserForYesOrNo.Ask($"[SFTP客户端]远程文件\n[{RemoteFilePath}]\n已存在，是否覆盖？");
                if (Result)
                {

                    ReportLog("用户操作", "SFTP客户端", "用户确认覆盖");

                    SftpClient.DeleteFile(RemoteFilePath);
                    if (SftpClient.Exists(RemoteFilePath))
                    {

                        ReportLog("错误", "SFTP客户端", "远程文件删除失败");
                        ReportLog("失败", "SFTP客户端", "已取消上传文件操作");
                        UploadFileTaskDone(false);

                        return;
                    }
                    else
                    {

                        ReportLog("成功", "SFTP客户端", "远程文件删除成功");
                        ReportLog("程序操作", "SFTP客户端", "开始上传文件");

                        StartPosition = 0L;
                    }
                }
                else
                {

                    ReportLog("用户操作", "SFTP客户端", "用户选择不覆盖");
                    ReportLog("失败", "SFTP客户端", "已取消上传文件操作");
                    UploadFileTaskDone(false);

                    return;
                }
            }
            else if (SftpClient.Exists(TransferringRemoteFilePath))
            {

                ReportLog("信息", "SFTP客户端", "存在未传输完成的文件");
                ReportLog("等待用户操作", "SFTP客户端", "等待用户确认是否续传");

                bool result = true;// AskUserForYesOrNo.Ask($"[SFTP客户端]存在未传输完成的文件\n[{TransferringRemoteFilePath}]\n是否续传？");
                if (result)
                {

                    ReportLog("用户操作", "SFTP客户端", "用户选择续传远程文件");
                    ReportLog("程序操作", "SFTP客户端", "正在读取远程文件信息");

                    try
                    {
                        StartPosition = SftpClient.Get(TransferringRemoteFilePath).Length;
                        ReportLog("调试", "SFTP客户端", $"已获取到远程文件的字节数[{StartPosition}]");
                    }
                    catch (Exception)
                    {

                        ReportLog("错误", "SFTP客户端", "读取远程文件信息失败");
                        ReportLog("失败", "SFTP客户端", "已终止上传操作");

                        return;
                    }

                    ReportLog("成功", "SFTP客户端", "成功获取到上次传输进度");
                    ReportLog("程序操作", "SFTP客户端", "开始续传");

                }
                else
                {

                    ReportLog("用户操作", "SFTP客户端", "用户选择不续传");
                    ReportLog("等待用户操作", "SFTP客户端", "等待用户确认是否删除并重新上传");

                    bool result1 = true; //AskUserForYesOrNo.Ask($"[SFTP客户端]存在未传输完成的文件\n[{TransferringRemoteFilePath}]\n是否删除并重新上传？");

                    if (result1)
                    {

                        ReportLog("用户操作", "SFTP客户端", "用户选择删除并重新上传");

                        SftpClient.DeleteFile(TransferringRemoteFilePath);
                        if (SftpClient.Exists(TransferringRemoteFilePath))
                        {

                            ReportLog("错误", "SFTP客户端", "远程文件删除失败");
                            ReportLog("失败", "SFTP客户端", "已取消上传文件操作");
                            UploadFileTaskDone(false);

                            return;
                        }

                        ReportLog("成功", "SFTP客户端", "远程文件删除成功");
                        ReportLog("程序操作", "SFTP客户端", "开始上传文件");

                        StartPosition = 0L;
                    }
                    else
                    {

                        ReportLog("用户操作", "SFTP客户端", "用户选择不删除并重新上传");
                        ReportLog("失败", "SFTP客户端", "已取消上传文件操作");
                        UploadFileTaskDone(false);

                        return;
                    }
                }
            }
            else
            {

                ReportLog("信息", "SFTP客户端", "无已存在的同名文件或同名文件的片段(.文件块)");
                ReportLog("程序操作", "SFTP客户端", "开始上传文件");

                StartPosition = 0L;
            }



            long LocalFileByteCount = LocalFileInfo.Length;
            this.LocalFileByteCount = LocalFileByteCount;
            this.TransferringRemoteFilePath = TransferringRemoteFilePath;
            int RetryCount = 0;
            int MaxRetryCount = 5;


            FileStream LocalFileStream = new(LocalFilePath, FileMode.Open, FileAccess.Read);
            {
                SftpFileStream RemoteFileStream = SftpClient.OpenWrite(TransferringRemoteFilePath);
                {
                    byte[] Buffer = new byte[BufferSize];//缓冲区
                    Timer.Start();

                    for (; RetryCount <= MaxRetryCount;)
                    {
                        try
                        {
                            if (!SftpClient.IsConnected)
                            {
                                SftpClient.Connect();
                                RemoteFileStream = SftpClient.OpenWrite(TransferringRemoteFilePath);

                                ReportLog("成功", "SFTP客户端", "重连成功");

                            }
                            while (StartPosition < LocalFileByteCount)
                            {
                                LocalFileStream.Position = StartPosition;

                                ReportLog("调试", "SFTP客户端", $"已设置读取流起始点[{StartPosition}]");


                                RemoteFileStream.Position = StartPosition;

                                ReportLog("调试", "SFTP客户端", $"已设置上传流起始点[{StartPosition}]");


                                int ReadByteCount = LocalFileStream.Read(Buffer, 0, Buffer.Length);

                                ReportLog("调试", "SFTP客户端", $"已从读取流中读取到[{ReadByteCount}]字节");


                                RemoteFileStream.Write(Buffer, 0, ReadByteCount);

                                ReportLog("调试", "SFTP客户端", "已将读取到的数据发送至上传流");


                                StartPosition = SftpClient.Get(TransferringRemoteFilePath).Length;

                                ReportLog("调试", "SFTP客户端", $"已获取到远程文件的字节数[{StartPosition}]");


                            }
                            if (StartPosition == LocalFileByteCount)
                            {

                                ReportLog("成功", "SFTP客户端", "文件已完整上传");

                                break;
                            }

                        }
                        catch (Exception ex)
                        {
                            if (RetryCount == MaxRetryCount)
                            {
                                RetryCount++;
                                break;
                            }
                            RetryCount++;
                            if (SftpClient.IsConnected)
                            {

                                ReportLog("错误", "SFTP客户端", $"上传失败（{ex.Message}）");
                                ReportLog("程序操作", "SFTP客户端", $"5s后重试（第{RetryCount}次）");

                            }
                            else
                            {

                                ReportLog("错误", "SFTP客户端", $"上传失败（{ex.Message}）");
                                ReportLog("程序操作", "SFTP客户端", $"连接中断，5s后尝试重新连接（第{RetryCount}次重试）");

                            }
                            Thread.Sleep(5000);
                        }
                    }

                    LocalFileStream.Dispose();
                    if (SftpClient.IsConnected)
                    {
                        RemoteFileStream.Dispose();
                    }
                    Timer.Stop();
                }
            }
            if (RetryCount <= MaxRetryCount)
            {
                SftpClient.RenameFile(TransferringRemoteFilePath, RemoteFilePath);
                if (SftpClient.Exists(RemoteFilePath))
                {

                    ReportLog("成功", "SFTP客户端", "文件重命名成功");
                    ReportLog("成功", "SFTP客户端", "文件上传成功");
                    UploadFileTaskDone(true);

                }
                else
                {

                    ReportLog("错误", "SFTP客户端", "文件重命名失败");
                    ReportLog("失败", "SFTP客户端", "文件上传失败");
                    UploadFileTaskDone(false);

                }
            }
            else
            {

                ReportLog("错误", "SFTP客户端", $"上传失败已达最大重试次数{MaxRetryCount}");
                ReportLog("失败", "SFTP客户端", "文件上传失败");
                UploadFileTaskDone(false);

            }
        }
    }

    internal bool CheckIsRemoteFileExists(string RemoteFilePath)
    {

        if (!SftpClient.IsConnected)
        {

            ReportLog("错误", "SFTP客户端", "未连接至服务器");
            ReportLog("失败", "SFTP客户端", "检测远程文件是否存在失败");

            return false;
        }
        else
        {
            try
            {
                if (SftpClient.Exists(RemoteFilePath))
                {

                    ReportLog("信息", "SFTP客户端", "远程文件存在");
                    ReportLog("成功", "SFTP客户端", "检测远程文件是否存在成功");

                    return true;
                }
                else
                {

                    ReportLog("信息", "SFTP客户端", "远程文件不存在");
                    ReportLog("成功", "SFTP客户端", "检测远程文件是否存在成功");

                    return false;
                }
            }
            catch (Exception ex)
            {

                ReportLog("错误", "SFTP客户端", $"错误:({ex.Message})");
                ReportLog("失败", "SFTP客户端", "检测远程文件是否存在失败");

                return false;
            }
        }
    }

    internal bool DeleteRemoteFile(string RemoteFilePath)
    {
        if (!SftpClient.IsConnected)
        {

            ReportLog("错误", "SFTP客户端", "未连接至服务器");
            ReportLog("失败", "SFTP客户端", "删除远程文件失败");
            DeleteFileTaskDone(false);

            return false;
        }
        else
        {
            try
            {
                SftpClient.DeleteFile(RemoteFilePath);

                ReportLog("成功", "SFTP客户端", "成功删除远程文件");
                DeleteFileTaskDone(true);

                return true;
            }
            catch (Exception ex)
            {

                ReportLog("错误", "SFTP客户端", $"错误:({ex.Message})");
                ReportLog("失败", "SFTP客户端", "删除远程文件失败");
                DeleteFileTaskDone(false);

                return false;
            }
        }
    }

    internal void Disconnect()
    {
        if (SftpClient.IsConnected)
        {
            SftpClient.Disconnect();

            ReportLog("成功", "SFTP客户端", "已断开连接");

        }
    }

    internal void Dispose()
    {
        if (SftpClient != null && SftpClient.IsConnected)
        {
            Disconnect();
        }
        SftpClient?.Dispose();
        Timer?.Dispose();
        Initialized = delegate { };
        ConnectTaskDone = delegate { };
        UploadFileTaskDone = delegate { };
        DeleteFileTaskDone = delegate { };
        ReportTransmissionSpeedAndProcess = delegate { };

        ReportLog("信息", "SFTP客户端", "已释放资源");

        ReportLog = delegate { };

    }
#if DEBUG
    internal static void Example()
    {
        Thread thread = new(async () =>
        {
            SFTPClient sFTPClient = new("192.168.101.2", 22, "admin", "admin");

            sFTPClient.ConnectTaskDone += (Bool) =>
            {
                if (Bool)
                {
                    sFTPClient.UploadLocalFile($"D:\\Desktop\\Server.zip", "/");
                }
                else
                {
                    sFTPClient.Dispose();
                }
            };
            sFTPClient.UploadFileTaskDone += (Bool) =>
            {
                sFTPClient.Dispose();

            };
            sFTPClient.ReportLog += (Type, Sender, Log) =>
            {
                Console.WriteLine(Log);
            };
            sFTPClient.ReportTransmissionSpeedAndProcess += (SpeedAndProcessText, Process, Speed) =>
            {
                Console.WriteLine(SpeedAndProcessText);
            };

            await sFTPClient.ConnectSFTPServer(CancellationToken.None);

        });
        thread.Start();
    }
#endif
}
