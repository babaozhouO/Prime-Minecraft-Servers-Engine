using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using LightProto;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using PMCSsE_Communicator.SharedCodes;

namespace PMCSsE_Communicator
{
    /// <summary>
    /// 原生服务器类
    /// </summary>
    public class NativeServer
    {
        /// <summary>
        /// 最大数据包长度(当前：16MB)
        /// </summary>
        private const int MAXDATAPACKLENGHT = 1024 * 1024 * 16;
        /// <summary>
        /// 握手超时时长(当前：120s)
        /// </summary>
        private const long HANDSHAKETIMEOUT = 120 * 1000;
        /// <summary>
        /// 调试模式,此模式下要尽可能详细地输出日志
        /// </summary>
        public bool DebugMode;
        private string ListenAddress;
        private int ListenPort;
        private TcpListener? TcpListener;
        private readonly ThreadStart ServerThreadStart;
        private Thread ServerThread;
        private CancellationTokenSource StopServerTokenSource;
        /// <summary>
        /// 密码错误记录,long:系统启动时间（ms），不受系统时间影响
        /// </summary>
        private readonly Dictionary<string, (int, long)> TempBannedIP = [];
        private string CurrentClientIP = "";
        //Server
        /// <summary>
        /// 上报日志
        /// </summary>
        public event Action<string> ReportLog = delegate { };
        /// <summary>
        /// 启动失败
        /// </summary>
        public event Action<string> StartServerFailed = delegate { };
        private event Action TcpListenerThreadStoped = delegate { };
        private string RSAPublicKey;
        private string RSAPrivateKey;
        //Client
        private ClientInfo? ClientInfo;
        /// <summary>
        /// 包传递
        /// </summary>
        public DataPackBus DataPackBus = new();
        /// <summary>
        /// 客户端登录，传出Key
        /// </summary>
        public event Action<byte[]> ReceivedKey = delegate { };
        /// <summary>
        /// 客户端连接并握手成功后发生
        /// </summary>
        public event Action ClientConnected = delegate { };
        /// <summary>
        /// 收到了来自客户端插件的数据
        /// </summary>
        public event Action<byte, byte[]> ReceivedDataFromClient_Plugin = delegate { };
        /// <summary>
        /// 客户端断开连接后发生
        /// </summary>
        public event Action ClientDisconnected = delegate { };

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听的端口</param>
        /// <param name="listenAddress">监听的地址</param>
        /// <param name="debugMode">调试模式开关</param>
        public NativeServer(string listenAddress, int port, bool debugMode = false)
        {
            DebugMode = debugMode;
            ListenPort = port;
            ListenAddress = listenAddress;
            StopServerTokenSource = new();
            ServerThreadStart = new(TcpListenerThreadWork);
            var (puk, prk) = SimpleHybridEncryption.GenerateRSAKey();
            RSAPublicKey = puk; RSAPrivateKey = prk;
            ServerThread = new(ServerThreadStart);
            DataPackBus.PulishedDataType += ReportLog;
        }
        /// <summary>
        /// 启动
        /// </summary>
        public void StartService()
        {
            TcpListenerThreadStoped -= StartService;
            if (ServerThread.IsAlive == true)
            {
                StartServerFailed("原生服务器已经在运行了");
                return;
            }
            StopServerTokenSource = new();
            try
            {
                if (ServerThread.ThreadState != ThreadState.Unstarted)
                    ServerThread = new(ServerThreadStart);
                ServerThread.Start();
            }
            catch (ThreadStartException ex)
            {
                TcpListener?.Dispose();
                StartServerFailed($"线程状态错误：{ex.Message}");
            }
            catch (OutOfMemoryException ex)
            {
                TcpListener?.Dispose();
                StartServerFailed("内存不足！");
                Environment.FailFast("内存不足！", ex);
            }
        }
        /// <summary>
        /// 更改端口并重启
        /// </summary>
        /// <param name="listenAddress">监听的地址</param>
        /// <param name="port">监听的端口</param>
        public void ChangePortAndRestart(string listenAddress, int port)
        {
            if (listenAddress != ListenAddress || port != ListenPort)
            {
                var (puk, prk) = SimpleHybridEncryption.GenerateRSAKey();
                RSAPublicKey = puk; RSAPrivateKey = prk;
                ListenAddress = listenAddress;//仅在启动时访问，无需锁
                ListenPort = port;

                TcpListenerThreadStoped += StartService;
                try
                {
                    StopServerTokenSource.Cancel();
                }
                catch { }
            }
        }
        /// <summary>
        /// 清理释放资源
        /// </summary>
        public void Dispose()
        {
            try { StopServerTokenSource.Cancel(); } catch { }
            try { TcpListener?.Stop(); } catch { }
            try { TcpListener?.Dispose(); } catch { }
            try { ServerThread?.Join(); } catch { }
            try { StopServerTokenSource.Dispose(); } catch { }
            TcpListenerThreadStoped = delegate { };
            StartServerFailed = delegate { };
            ReportLog = delegate { };
        }

        private async void TcpListenerThreadWork()
        {
            try
            {
                if (ListenAddress == "All")
                {
                    TcpListener = new(IPAddress.IPv6Any, ListenPort);
                    TcpListener.Server.DualMode = true;
                }
                else if (IPAddress.TryParse(ListenAddress, out IPAddress? address))
                {
                    TcpListener = new(address, ListenPort);
                }
                else
                {
                    ReportLog($"创建TCP服务端对象失败,原因：监听地址配置异常");
                    ReportLog($"可能的解决办法：重新指定监听地址");
                    return;
                }
            }
            catch (Exception ex)
            {
                ReportLog($"创建TCP服务端对象失败,原因：监听地址、端口配置异常");
                ReportLog($"可能的解决办法：以管理员身份运行、更换端口");
                ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                TcpListener?.Dispose();
                StopServerTokenSource.Cancel();
                StartServerFailed("监听地址、端口配置异常");
                return;
            }
            ReportLog("若出现防火墙提示，请同意PMCSsE通过防火墙，不然只有本地的UI面板可以连接");
            try
            {
                TcpListener.Start();
            }
            catch (SocketException ex)
            {
                ReportLog($"启动原生服务器失败,原因：Socket异常");
                ReportLog($"Socket异常代码：{ex.SocketErrorCode},帮助链接：https://learn.microsoft.com/zh-cn/windows/win32/winsock/windows-sockets-error-codes-2");
                ReportLog($"可能的解决办法：以管理员身份运行、更换端口、检查系统资源");
                TcpListener?.Dispose();
                StopServerTokenSource.Cancel();
                StartServerFailed("Socket异常");
                return;
            }
            ReportLog("启动原生服务器成功，若无法连接请检查防火墙");
            ReportLog("快速放行指令（Windows）：");
            ReportLog($"netsh advfirewall firewall add rule name=\"PMCSsE的远程原生客户端连接:{ListenPort}\" dir=in action=allow protocol=TCP localport={ListenPort}");
            ReportLog("快速放行指令（Linux）：");
            ReportLog($"（CentOS/RHEL 7+，Fedora）{Environment.NewLine}firewall-cmd --zone=public --add-port={ListenPort}/tcp --permanent{Environment.NewLine}firewall-cmd --reload");
            ReportLog($"（Ubuntu/Debian）{Environment.NewLine}ufw allow {ListenPort}/tcp");
            ReportLog("由于Linux不同发行版的防火墙指令存在差异，其它发行版请查阅官方文档、上网搜索");

            while (!StopServerTokenSource.IsCancellationRequested)
            {
                TcpClient tcpClient;
                try { tcpClient = await TcpListener.AcceptTcpClientAsync(StopServerTokenSource.Token); } //操作取消异常
                catch { break; }
                tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                try
                {
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
                }
                catch (SocketException ex)
                {
                    ReportLog($"无法设置TcpClient的TcpKeepAlive设置：{ex.Message}，异常代码：{ex.SocketErrorCode}，堆栈：{ex.StackTrace}");
                    ReportLog("关闭当前连接并继续等待下一个");
                    tcpClient.Close();
                    tcpClient.Dispose();
                    continue;
                }
                catch (Exception ex)
                {
                    ReportLog($"无法设置TcpClient的TcpKeepAlive设置：{ex.Message}，堆栈：{ex.StackTrace}");
                    ReportLog("关闭当前连接并继续等待下一个");
                    tcpClient.Close();
                    tcpClient.Dispose();
                    continue;
                }
                IPEndPoint? iPEndPoint;
                try
                {
                    iPEndPoint = (IPEndPoint?)tcpClient.Client.RemoteEndPoint;
                    if (iPEndPoint == null)
                    {
                        ReportLog($"有原生客户端请求连接，但无法获取其IP和端口，终止连接");
                        ReportLog($"RemoteEndPoint 为 null");
                        tcpClient.Close();
                        tcpClient.Dispose();
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    ReportLog($"有原生客户端请求连接，但无法获取其IP和端口，终止连接");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    tcpClient.Close();
                    tcpClient.Dispose();
                    continue;
                }
                CurrentClientIP = iPEndPoint.Address.ToString();
                ReportLog($"有客户端请求连接,IP: {iPEndPoint.Address},端口:{iPEndPoint.Port}");

                if (TempBannedIP.TryGetValue(iPEndPoint.Address.ToString(), out (int, long) info))
                {
                    long banTimeLenght = (long)(30 * 1000 * Math.Pow(2, info.Item1));
                    long unbannedTime = info.Item2 + banTimeLenght;
                    long now = Environment.TickCount64;
                    if (now < unbannedTime)
                    {
                        ReportLog($"有原生客户端请求连接，但其因（多次）不及时完成握手或密码错误而被暂时封禁");
                        ReportLog($"剩余时长：{(unbannedTime - now) / 1000}s");
                        tcpClient.Close();
                        tcpClient.Dispose();
                        continue;
                    }
                }


                NetworkStream networkStream;
                try
                {
                    networkStream = tcpClient.GetStream();
                }
                catch (Exception ex)
                {
                    ReportLog($"获取网络流时发生异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    tcpClient.Close();
                    tcpClient.Dispose();
                    continue;
                }
                ClientInfo = new(tcpClient, networkStream)
                {
                    HandShakeProcess = HandShakeProcess_Server.WaitingNeedRSAPublicKey
                };
                ReportLog($"与原生客户端（IP: {iPEndPoint.Address},端口:{iPEndPoint.Port}）连接成功，将进行握手以建立加密连接");

                bool didwork1 = false;
                bool didwork2 = false;
                byte didntworkTimes = 0;
                long handshakeEndTime = Environment.TickCount64+HANDSHAKETIMEOUT;
                //握手循环
                while (ClientInfo.TcpClient.Connected && !StopServerTokenSource.IsCancellationRequested && !ClientInfo.CloseConnectionTokenSource.IsCancellationRequested && ClientInfo.HandShakeProcess != HandShakeProcess_Server.Finished)
                {
                    try
                    {
                        if (ClientInfo.NetworkStream.DataAvailable)
                        {
                            byte[] dataPackContentReceived = await ReceiveDataPack();
                            if (!dataPackContentReceived.SequenceEqual([]))
                            {
                                didwork1 = true;
                                ProcessDataPack_HandShake(dataPackContentReceived);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        ReportLog("尝试读取网络流时发生异常，断开连接");
                        ReportLog($"Socket异常代码：{ex.SocketErrorCode}{Environment.NewLine}帮助链接：https://learn.microsoft.com/zh-cn/windows/win32/winsock/windows-sockets-error-codes-2{Environment.NewLine}异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        ReportLog("尝试读取网络流时发生异常，断开连接");
                        ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        break;
                    }
                    if (!ClientInfo.TcpClient.Connected || StopServerTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    if (ClientInfo.TasksQueue.Count != 0)
                    {
                        didwork2 = true;
                        Func<Task>[] tasksQueueCopy;
                        using (ClientInfo.TasksQueueLock.EnterScope())//仅复制指针引用，开销极小
                        {
                            tasksQueueCopy = [.. ClientInfo.TasksQueue];
                            ClientInfo.TasksQueue.Clear();
                        }
                        foreach (var task in tasksQueueCopy)
                        {
                            await task();//统一顺序执行
                        }
                    }

                    if (didwork1 || didwork2)
                    {
                        didntworkTimes = 0;
                        didwork1 = false;
                        didwork2 = false;
                        continue;
                    }
                    await Task.Delay(50);
                    didwork1 = false;
                    didwork2 = false;
                    didntworkTimes++;
                    if (didntworkTimes == 100)//约5s，发送心跳包
                    {
                        await SendConnectionAlive();
                    }
                    if (didntworkTimes == 200)//约5+5s
                    {
                        ReportLog($"原生客户端5内未回应心跳包，判定为已断开连接/网络极差");
                        break;
                    }
                    if (Environment.TickCount64 >= handshakeEndTime)
                    {
                        ReportLog($"客户端在 {HANDSHAKETIMEOUT / 1000}s 内未完成握手，断开连接");
                        break;
                    }
                }

                if (ClientInfo.HandShakeProcess != HandShakeProcess_Server.Finished)
                {
                    ClientDisconnected();
                    ClientInfo.NetworkStream.Close();
                    ClientInfo.NetworkStream.Dispose();
                    ClientInfo.TcpClient.Close();
                    ClientInfo.TcpClient.Dispose();
                    ClientInfo.TasksQueue.Clear();
                    if (ClientInfo.Aes != null)
                    {
                        CryptographicOperations.ZeroMemory(ClientInfo.Aes.Key.AsSpan());
                        CryptographicOperations.ZeroMemory(ClientInfo.Aes.IV.AsSpan());
                    }
                    ClientInfo.Aes?.Dispose();
                    ClientInfo.CloseConnectionTokenSource.Dispose();
                    ReportLog($"握手流程在进行到{ClientInfo.HandShakeProcess}时终止");
                    continue;
                }

                didwork1 = false;
                didwork2 = false;
                didntworkTimes = 0;

                while (ClientInfo.TcpClient.Connected && !StopServerTokenSource.IsCancellationRequested && !ClientInfo.CloseConnectionTokenSource.IsCancellationRequested && ClientInfo.HandShakeProcess == HandShakeProcess_Server.Finished)
                {
                    try
                    {
                        if (ClientInfo.NetworkStream.DataAvailable)
                        {
                            byte[] dataPackContentReceived = await ReceiveDataPack();
                            if (!dataPackContentReceived.SequenceEqual([]))
                            {
                                didwork1 = true;
                                ProcessDataPack(dataPackContentReceived);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        ReportLog("尝试读取网络流时发生异常，断开连接");
                        ReportLog($"Socket异常代码：{ex.SocketErrorCode}{Environment.NewLine}帮助链接：https://learn.microsoft.com/zh-cn/windows/win32/winsock/windows-sockets-error-codes-2{Environment.NewLine}异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        ReportLog("尝试读取网络流时发生异常，断开连接");
                        ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        break;
                    }
                    if (!ClientInfo.TcpClient.Connected || StopServerTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    if (ClientInfo.TasksQueue.Count != 0)
                    {
                        didwork2 = true;
                        Func<Task>[] tasksQueueCopy;
                        using (ClientInfo.TasksQueueLock.EnterScope())//仅复制指针引用，开销极小
                        {
                            tasksQueueCopy = [.. ClientInfo.TasksQueue];
                            ClientInfo.TasksQueue.Clear();
                        }
                        foreach (var task in tasksQueueCopy)
                        {
                            await task();//统一顺序执行
                        }
                    }

                    if (didwork1 || didwork2)
                    {
                        didntworkTimes = 0;
                        didwork1 = false;
                        didwork2 = false;
                        continue;
                    }
                    await Task.Delay(50);
                    didwork1 = false;
                    didwork2 = false;
                    didntworkTimes++;
                    if (didntworkTimes == 100)//约5s，发送心跳包
                    {
                        await SendConnectionAlive();
                    }
                    if (didntworkTimes == 200)//约5+5s
                    {
                        ReportLog($"原生客户端5内未回应心跳包，判定为已断开连接/网络极差");
                        break;
                    }
                }
                ClientDisconnected();
                ClientInfo.NetworkStream.Close();
                ClientInfo.NetworkStream.Dispose();
                ClientInfo.TcpClient.Close();
                ClientInfo.TcpClient.Dispose();
                ClientInfo.TasksQueue.Clear();
                if (ClientInfo.Aes != null)
                {
                    CryptographicOperations.ZeroMemory(ClientInfo.Aes.Key.AsSpan());
                    CryptographicOperations.ZeroMemory(ClientInfo.Aes.IV.AsSpan());
                }
                ClientInfo.Aes?.Dispose();
                ClientInfo.CloseConnectionTokenSource.Dispose();
                ReportLog($"与原生客户端断开了连接");

            }
            try { TcpListener.Stop(); } catch { }
            try { TcpListener.Dispose(); } catch { }
            ReportLog("已关闭原生服务器");
            TcpListenerThreadStoped();
        }
        #region 握手task
        private async Task SendRSAPublicKeyAsync()
        {
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.SendingRSAPublicKey;
            byte[] keyBytes;
            try
            {
                keyBytes = Encoding.UTF8.GetBytes(RSAPublicKey);
            }
            catch (Exception ex)
            {
                ReportLog("编码RSA公钥时发生异常");
                ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }
            var (dataPack, succeed) = GenerateDataPack(RespondTypeEnum_Private.RSAPublicKey, keyBytes, false);
            if (!succeed)
            {
                ReportLog($"在握手流程的“客户端请求RSA公钥”阶段生成RSA公钥数据包失败");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            if (!await SendDataPack(dataPack))
            {
                ReportLog($"在握手流程的“客户端请求RSA公钥”阶段发送RSA公钥数据包失败");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.SentRSAPublicKey;

            string rsaPublicKeyHash;
            try
            {
                rsaPublicKeyHash = GetSHA256FromTextClass.GetSHA256FromText(RSAPublicKey);
            }
            catch (Exception ex)
            {
                ReportLog($"与原生客户端握手失败：计算RSA公钥指纹失败");
                ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ReportLog($"RSA公钥指纹：{rsaPublicKeyHash}");
            ReportLog($"已向原生客户端发送RSA公钥，请在前端上检查RSA公钥指纹是否一致，如果一致，请确认连接");
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.WaitingGotRSAPublicKey;
        }
        private async Task SendNeedAESAsync()
        {
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.SendingNeedAES;
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RespondTypeEnum_Private.NeedAES];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("在握手流程的“服务端请求AES密钥”阶段发送数据包失败");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ClientInfo.HandShakeProcess = HandShakeProcess_Server.SentNeedAES;
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.WaitingAESKey;
        }
        private async Task SendGotAESAsync()
        {
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.SendingGotAES;
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RespondTypeEnum_Private.GotAES];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("在握手流程的“服务端确认AES密钥”阶段发送数据包失败");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ClientInfo.HandShakeProcess = HandShakeProcess_Server.SentGotAES;
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.WaitingLogin;
        }
        private async Task SendSucceedAsync()
        {
            ClientInfo!.HandShakeProcess = HandShakeProcess_Server.SendingSucceed;
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RespondTypeEnum_Private.Succeed];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("在握手流程的“服务端报告登录成功”阶段发送数据包失败");
                try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ClientInfo.HandShakeProcess = HandShakeProcess_Server.Finished;
            ClientConnected();
        }
        #endregion
        private async Task SendConnectionAlive()
        {
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RespondTypeEnum_Private.ConnectionAlive];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("发送心跳包失败");
            }
        }
        /// <summary>
        /// 响应客户端
        /// </summary>
        /// <param name="type">响应类型</param>
        /// <param name="content">（可选）附带的数据（必须有ProtoContract标记）</param>
        public void RespondClient<T>(RespondTypeEnum type, T content) where T : IProtoParser<T>
        {
            if (ClientInfo == null)
            {
                return;
            }

            using (ClientInfo.TasksQueueLock.EnterScope())
            {
                ClientInfo.TasksQueue.Enqueue(async () =>
                {
                    (byte[] payload, bool succeed) = SerializePayloadObject<T>(content);
                    if (succeed)
                    {
                        (byte[] dataPack, bool succeed1) = GenerateDataPack((RespondTypeEnum_Private)type, payload);
                        if (succeed1)
                        {
                            await SendDataPack(dataPack);
                        }
                        else
                        {
                            ReportLog($"生成[{type}]数据包失败");
                        }
                    }
                    else
                    {
                        ReportLog($"序列化[{typeof(T)}]对象失败");
                    }
                });
            }

        }
        /// <summary>
        /// 在DataAvailable时才会调用，接收不到完整数据包/意外断开连接/无字节可读都会返回空数组
        /// </summary>
        /// <returns></returns>
        private async Task<byte[]> ReceiveDataPack()
        {
            if (ClientInfo == null)
            {
                return [];
            }
            if (!ClientInfo.NetworkStream.DataAvailable)
            {
                return [];
            }
            byte[] dataPackHeader = new byte[4];
            int readLength = 0;
            while (ClientInfo.TcpClient.Connected && readLength < 4 && !ClientInfo.CloseConnectionTokenSource.IsCancellationRequested && !StopServerTokenSource.IsCancellationRequested)
            {
                int dataRead;
                try
                {
                    dataRead = await ClientInfo.NetworkStream.ReadAsync(dataPackHeader.AsMemory(readLength, 4 - readLength), ClientInfo.CloseConnectionTokenSource.Token);
                }
                catch (ObjectDisposedException) { return []; }
                catch (OperationCanceledException) { return []; }
                catch (IOException ex)
                {
                    ReportLog($"接收来自原生客户端的数据包时发生网络IO异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    if (ex.InnerException != null && ex.InnerException is SocketException se)
                    {
                        ReportLog($"Socket异常：{se.SocketErrorCode}");
                        ReportLog($"异常信息：{se.Message}{Environment.NewLine}{ex.StackTrace}");
                    }
                    return [];
                }
                catch (Exception ex)
                {
                    ReportLog($"接收来自原生客户端的数据包时发生异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    return [];
                }
                if (dataRead == 0)//连接关闭
                {
                    ReportLog($"原生客户端已关闭连接");
                    try
                    {
                        ClientInfo.CloseConnectionTokenSource.Cancel();
                    }
                    catch { }
                    return [];
                }
                readLength += dataRead;
            }
            if (!ClientInfo.TcpClient.Connected || readLength != 4) { return []; }
            readLength = 0;
            int dataPackContentLenght = BinaryPrimitives.ReadInt32BigEndian(dataPackHeader);
            if (dataPackContentLenght <= 0) { return []; }
            if (dataPackContentLenght > MAXDATAPACKLENGHT)
            {
                try
                {
                    ClientInfo.CloseConnectionTokenSource.Cancel();
                }
                catch { }
                ReportLog($"前端发送过大的数据包，可能为攻击者恶意发送，长度：{dataPackContentLenght}");
                return [];
            }//修复大数据包攻击
            byte[] dataPackContent = new byte[dataPackContentLenght];
            while (ClientInfo.TcpClient.Connected && readLength < dataPackContentLenght && !ClientInfo.CloseConnectionTokenSource.IsCancellationRequested && !StopServerTokenSource.IsCancellationRequested)
            {
                int dataRead;
                try
                {
                    dataRead = await ClientInfo.NetworkStream.ReadAsync(dataPackContent.AsMemory(readLength, dataPackContentLenght - readLength), ClientInfo.CloseConnectionTokenSource.Token);
                }
                catch (ObjectDisposedException) { return []; }
                catch (OperationCanceledException) { return []; }
                catch (IOException ex)
                {
                    ReportLog($"接收来自原生客户端的数据包时发生网络IO异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    return [];
                }
                catch (Exception ex)
                {
                    ReportLog($"接收来自原生客户端的数据包时发生异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    return [];
                }
                if (dataRead == 0)//连接关闭
                {
                    ReportLog($"原生客户端已关闭连接");
                    try
                    {
                        ClientInfo.CloseConnectionTokenSource.Cancel();
                    }
                    catch { }
                    return [];
                }
                readLength += dataRead;
            }
            if (readLength != dataPackContentLenght)
            {
                return [];
            }
            return dataPackContent;
        }
        private (byte[] payload, bool succeed) SerializePayloadObject<T>(T payloadObject) where T : IProtoParser<T>
        {
            try
            {
                return (payloadObject.ToByteArray(), true);
            }
            catch (Exception ex)
            {
                ReportLog($"序列化对象[{typeof(T)}]失败:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return ([], false);
        }
        private (byte[] dataPack, bool succeed) GenerateDataPack(RespondTypeEnum_Private type, byte[] payload, bool needAes = true)
        {
            byte[] dataPack;
            if (needAes)//最少1次分配，但复杂
            {
                dataPack = [0, (byte)type, .. payload];
                if (ClientInfo != null && ClientInfo.Aes != null)
                {
                    try
                    {
                        dataPack = SimpleHybridEncryption.EncryptWithAES(dataPack, ClientInfo.Aes, 4);//已预留长度头
                    }
                    catch (Exception ex)
                    {
                        ReportLog($"进行Aes加密时发生异常");
                        ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        return ([], false);
                    }
                }
                else
                {
                    ReportLog($"AES为null，无法加密，生成数据包失败");
                    return ([], false);
                }
            }
            else//单次分配，已最优
            {             //长度头x4    类型           载荷
                dataPack = [0, 0, 0, 0, 0, (byte)type, .. payload];
            }
            BinaryPrimitives.WriteInt32BigEndian(dataPack.AsSpan(0, 4), dataPack.Length - 4);
            return (dataPack, true);
        }
        private async Task<bool> SendDataPack(byte[] dataPack)
        {
            if (ClientInfo == null)
            {
                return false;
            }
            try
            {
                await ClientInfo.NetworkStream.WriteAsync(dataPack.AsMemory(), StopServerTokenSource.Token);
                await ClientInfo.NetworkStream.FlushAsync(StopServerTokenSource.Token);
                return true;
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            catch (IOException ex)
            {
                ReportLog($"向原生客户端发送数据包时发生网络IO异常");
                ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                ReportLog($"向原生客户端发送数据包时发生异常");
                ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return false;
        }
        private void ProcessDataPack(byte[] dataPack)
        {
            if (ClientInfo == null || ClientInfo.Aes == null) { return; }
            if (ClientInfo.HandShakeProcess == HandShakeProcess_Server.Finished)
            {
                if (dataPack.Length == 2)//心跳包不加密
                {
                    return;
                }
                try
                {
                    dataPack = SimpleHybridEncryption.DecryptWithAES(dataPack, ClientInfo.Aes!);
                }
                catch (Exception ex)
                {
                    ReportLog($"使用AES解密来自原生客户端的数据包时发生异常");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    return;
                }

                byte type1 = dataPack[0];
                int type2 = dataPack[1];
                if (type1 == 0)
                {
                    RequestTypeEnum_Private requestTypeEnum_Private;
                    if (Enum.IsDefined(typeof(RequestTypeEnum_Private), type2))
                    {
                        requestTypeEnum_Private = (RequestTypeEnum_Private)type2;
                    }
                    else
                    {
                        return;
                    }
                    try
                    {
                        switch (requestTypeEnum_Private)
                        {
                            case RequestTypeEnum_Private.SaveCipthertextConfig:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_SaveCipherConfig>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.GetMCServerManagersList:
                                DataPackBus.Publish(new Pack_GetMCServerManagerConfigsList());
                                break;
                            case RequestTypeEnum_Private.GetLoadedMCServerManagers:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_GetMCServerManager>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.GetSupportedMCServerTypes:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_GetSupportedMCServerTypes>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.CreatNewMCServerManager:
                                DataPackBus.Publish(new Pack_CreatNewMCServerManager());
                                break;
                            case RequestTypeEnum_Private.LoadMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_LoadMCServerManager>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.StopMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_StopMCServerManager>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.DeleteMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_DeleteMCServerManager>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.ModifyMCServerManagerConfig:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ModifyMCServerManagerConfig>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.RunMCServer:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_RunMCServer>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.SendCommand:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_SendCommand>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.ShutdownMCServer:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ShutdownMCServer>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.KillMCServer:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_KillMCServer>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.GetLatestLogs:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_GetLatestMCServerLogs>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.GetNewerLogs:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_GetNewerMCServerLogs>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.GetOlderLogs:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_GetOlderMCServerLogs>(dataPack.AsSpan(2)));
                                break;
                            case RequestTypeEnum_Private.ConnectionAlive:
                                break;
                            case RequestTypeEnum_Private.Close:
                                break;
                            case RequestTypeEnum_Private.Unknown:
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        ReportLog($"处理接收到的数据包时发生异常：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    }
                }
                else
                {
                    ReceivedDataFromClient_Plugin(type1, dataPack);
                }
            }
        }

        private void ProcessDataPack_HandShake(byte[] dataPack)
        {
            if (ClientInfo == null) { return; }
            if (dataPack.Length == 2)
            {
                ReadOnlySpan<byte> shortDataPack = dataPack.AsSpan();
                switch (shortDataPack)
                {
                    case [0, (byte)RequestTypeEnum_Private.ConnectionAlive]:
                        return;
                    case [0, (byte)RequestTypeEnum_Private.NeedRSAPublicKey]:
                        ReportLog("收到客户端请求RSA公钥的数据包");
                        if (ClientInfo!.HandShakeProcess != HandShakeProcess_Server.WaitingNeedRSAPublicKey)
                        {
                            ReportLog("客户端不遵循PMCSsE的握手协议，断开连接");
                            try
                            {
                                ClientInfo.CloseConnectionTokenSource.Cancel();
                            }
                            catch { }
                            return;
                        }
                        ClientInfo.HandShakeProcess = HandShakeProcess_Server.ReceivedNeedRSAPublicKey;
                        using (ClientInfo.TasksQueueLock.EnterScope())
                        {
                            ClientInfo.TasksQueue.Enqueue(SendRSAPublicKeyAsync);
                        }
                        return;
                    case [0, (byte)RequestTypeEnum_Private.GotRSAPublicKey]:
                        ReportLog("收到客户端确认RSA公钥的数据包");
                        if (ClientInfo!.HandShakeProcess != HandShakeProcess_Server.WaitingGotRSAPublicKey)
                        {
                            ReportLog("客户端不遵循PMCSsE的握手协议，断开连接");
                            try
                            {
                                ClientInfo.CloseConnectionTokenSource.Cancel();
                            }
                            catch { }
                            return;
                        }
                        ClientInfo.HandShakeProcess = HandShakeProcess_Server.ReceivedGotRSAPublicKey;
                        using (ClientInfo.TasksQueueLock.EnterScope())
                        {
                            ClientInfo.TasksQueue.Enqueue(SendNeedAESAsync);
                        }
                        return;
                    case [0, (byte)RequestTypeEnum_Private.RSAPublicKeyMismatch]:
                        ReportLog("收到客户端反馈RSA公钥不一致的数据包,断开连接");
                        if (ClientInfo!.HandShakeProcess != HandShakeProcess_Server.WaitingGotRSAPublicKey)
                        {
                            ReportLog("客户端不遵循PMCSsE的握手协议，断开连接");
                            try
                            {
                                ClientInfo.CloseConnectionTokenSource.Cancel();
                            }
                            catch { }
                            return;
                        }
                        try
                        {
                            ClientInfo.CloseConnectionTokenSource.Cancel();
                        }
                        catch { }
                        return;
                }
                return;
            }
            else
            {
                switch (ClientInfo.HandShakeProcess)
                {
                    case HandShakeProcess_Server.WaitingAESKey:
                        ReportLog("收到客户端发送AES密钥的数据包");
                        try
                        {
                            dataPack = SimpleHybridEncryption.DecryptWithRSA(dataPack, RSAPrivateKey);
                        }
                        catch (Exception ex)
                        {
                            ReportLog("在握手流程的“客户端发送AES密钥”阶段用RSA密钥解密数据包失败");
                            ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                            try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                            return;
                        }
                        if (dataPack.Length != 50)//2+32+16
                        {
                            ReportLog("处理AES数据包时发现数据长度不正确");
                            return;
                        }
                        byte[] aesKey = new byte[32];
                        byte[] aesIv = new byte[16];
                        Buffer.BlockCopy(dataPack, 2, aesKey, 0, 32);
                        Buffer.BlockCopy(dataPack, 34, aesIv, 0, 16);
                        Aes aes = Aes.Create(); aes.Key = aesKey; aes.IV = aesIv;
                        ClientInfo.Aes = aes;
                        ClientInfo.HandShakeProcess = HandShakeProcess_Server.ReceivedAESKey;
                        using (ClientInfo.TasksQueueLock.EnterScope())
                        {
                            ClientInfo.TasksQueue.Enqueue(SendGotAESAsync);
                        }
                        return;
                    case HandShakeProcess_Server.WaitingLogin:
                        ReportLog("收到客户端请求登录的数据包");
                        if (ClientInfo!.Aes == null)
                        {
                            ReportLog("在握手流程的“客户端登录”阶段发现AES为null");
                            try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                            return;
                        }
                        try
                        {
                            dataPack = SimpleHybridEncryption.DecryptWithAES(dataPack, ClientInfo.Aes);
                        }
                        catch (Exception ex)
                        {
                            ReportLog("在握手流程的“客户端登录”阶段解密数据包失败");
                            ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                            try { ClientInfo.CloseConnectionTokenSource.Cancel(); } catch { }
                            return;
                        }
                        byte[] keyBytes = new byte[dataPack.Length - 2];
                        Buffer.BlockCopy(dataPack, 2, keyBytes, 0, dataPack.Length - 2);
                        ClientInfo.HandShakeProcess = HandShakeProcess_Server.ReceivedLogin;

                        ReceivedKey(keyBytes);

                        return;
                    default:
                        ReportLog("客户端不遵循PMCSsE的握手协议，断开连接");
                        try
                        {
                            ClientInfo.CloseConnectionTokenSource.Cancel();
                        }
                        catch { }
                        return;
                }
            }
        }
        /// <summary>
        /// 验证密钥是否正确后调用以继续握手流程
        /// </summary>
        /// <param name="isKeyRight"></param>
        public void VerifyKey(bool isKeyRight)
        {
            if (!isKeyRight)
            {
                ReportLog("客户端的访问密钥错误");
                if (TempBannedIP.TryGetValue(CurrentClientIP, out (int, long) banInfo))
                {

                    banInfo.Item1++;
                    banInfo.Item2 = Environment.TickCount64;
                    TempBannedIP[CurrentClientIP] = banInfo;  //写回字典
                }
                else
                {
                    TempBannedIP.Add(CurrentClientIP, (1, Environment.TickCount64));
                }
                try { ClientInfo?.CloseConnectionTokenSource.Cancel(); } catch { }
                return;
            }
            ReportLog("客户端的访问密钥正确");
            if (ClientInfo != null)
                using (ClientInfo.TasksQueueLock.EnterScope())
                {
                    ClientInfo.TasksQueue.Enqueue(SendSucceedAsync);
                }
        }
    }
}

