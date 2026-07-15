using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using LightProto;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace PMCSsE_Communicator
{
    /// <summary>
    /// 原生客户端类
    /// </summary>
    public partial class NativeClient
    {
        /// <summary>
        /// 最大数据包长度(当前：16MB)
        /// </summary>
        private const int MAXDATAPACKLENGHT = 1024 * 1024 * 16;
        /// <summary>
        /// 调试模式,此模式下要尽可能详细地输出日志
        /// </summary>
        public bool DebugMode;

        private readonly string IP;
        private readonly int Port;
        private ClientInfo? ClientInfo;
        private CancellationTokenSource StopToken;
        private bool HandShakeFinished = false;
        private HandShakeProcess_Client HandShakeStep = HandShakeProcess_Client.Beginning;
        /// <summary>
        /// 报告握手进度
        /// </summary>
        public event Action<HandShakeProcess_Client> HandskakeProcessChanged = delegate { };
        private string RSAPublicKey = string.Empty;

        /// <summary>
        /// 上报日志（前端记录日志时注意线程安全，不要在此事件内操作UI，请将操作推送到UI线程）
        /// </summary>
        public event Action<string> ReportLog = delegate { };
        /// <summary>
        /// 已建立安全连接（前端记录日志时注意线程安全，不要在此事件内操作UI，请将操作推送到UI线程）
        /// </summary>
        public event Action Connected = delegate { };
        /// <summary>
        /// 需要验证RSA公钥指纹时发生（需要以非阻塞方式让用户确认），并附带RSA公钥指纹，若正确请设置IsRSAPublicKeyRight为true否则为false（前端记录日志时注意线程安全，不要在此事件内操作UI，请将操作推送到UI线程）
        /// </summary>
        public event Action<string> NeedToVerifyRSAPublicKey = delegate { };
        /// <summary>
        /// 需要输入访问密钥时发生此事件，请将用户输入的密钥通过方法"TypePassword(string password)"给予NativeClient
        /// </summary>
        public event Action NeedPassword = delegate { };
        /// <summary>
        /// 数据出餐口
        /// </summary>
        public DataPackBus DataPackBus = new();
        /// <summary>
        /// 收到插件发送的数据时发生此事件，插件作者自己处理
        /// </summary>
        public event Action<byte, byte[]> CommunicatorDataReceivedForPlugins = delegate { };
        /// <summary>
        /// 连接已断开（前端记录日志时注意线程安全，不要在此事件内操作UI，请将操作推送到UI线程）
        /// </summary>
        public event Action<DisconnectedReasonEnum> Disconnected = delegate { };

        /// <summary>
        /// 初始化新的NativeClient，应在初始化后，调用任何方法之前订阅好事件处理逻辑
        /// </summary>
        /// <param name="ip">后端所在机器的IP</param>
        /// <param name="port">后端监听的端口</param>
        public NativeClient(string ip, int port)
        {
            IP = ip;
            Port = port;
            StopToken = new();
        }

        /// <summary>
        /// 发起连接请求
        /// </summary>
        public void Connect()
        {
            if (ClientInfo?.TcpClient?.Connected == true)
            {
                ReportLog("前端连接器运行中，不能重复发起请求");
                return;
            }
            ReportLog($"正在连接原生服务器 {IP}:{Port}");

            try
            {
                Thread tcpThread = new(TcpClientThreadWork)
                {
                    IsBackground = true
                };
                tcpThread.Start();
            }
            catch (Exception ex)
            {
                ReportLog($"启动连接线程时发生异常：{ex.Message}");
                Disconnected(DisconnectedReasonEnum.OutOfMemory);
            }
        }
        /// <summary>
        /// 发送断联请求
        /// </summary>
        private async Task RequireDiconnect()
        {
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RequestTypeEnum_Private.Disconnect];
            if (await SendDataPack(dataPack))
            {
                ReportLog("已向服务端发送断联请求");
            }
            else { ReportLog("发送 DisconnectDataPack 失败"); }
            Disconnected(DisconnectedReasonEnum.UserDisconnect);

            try { ClientInfo?.CloseConnectionTokenSource.Cancel(); }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { ReportLog($"取消令牌时发生异常：{ex.Message}"); }
            try { StopToken.Cancel(); }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { ReportLog($"取消令牌时发生异常：{ex.Message}"); }

        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (ClientInfo == null)
            {
                ReportLog("断联失败，ClientInfo为null");
                return;
            }
            using (ClientInfo.TasksQueueLock.EnterScope())
            {
                ClientInfo.TasksQueue.Enqueue(RequireDiconnect);
            }
        }

        /// <summary>
        /// 释放资源，调用此方法后需重新创建新的NativeClient实例,重新订阅事件
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            try { StopToken.Dispose(); } catch { }
            Connected = delegate { };
            ReportLog = delegate { };
            DataPackBus.Dispose();
            Disconnected = delegate { };
            CommunicatorDataReceivedForPlugins = delegate { };
            NeedToVerifyRSAPublicKey = delegate { };
            NeedPassword = delegate { };
            HandskakeProcessChanged = delegate { };
        }

        private async void TcpClientThreadWork()
        {
            StopToken = new();
            if (StopToken.IsCancellationRequested)
            {
                Disconnected(DisconnectedReasonEnum.DisconnectingCalledByToken);
                return;
            }

            TcpClient tcpClient = new();
            try { await tcpClient.ConnectAsync(IP, Port, StopToken.Token); }
            catch (ObjectDisposedException)
            {
                Disconnected(DisconnectedReasonEnum.DisconnectingCalledByToken);
                return;
            }
            catch (SocketException ex)
            {
                ReportLog($"Socket异常代码:{ex.NativeErrorCode}，{ex.Message}");
                Disconnected(DisconnectedReasonEnum.SocketException);
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                Disconnected(DisconnectedReasonEnum.InvalidConnectionParameter);
                return;
            }
            catch (ArgumentNullException)
            {
                Disconnected(DisconnectedReasonEnum.InvalidConnectionParameter);
                return;
            }
            catch (Exception ex)
            {
                ReportLog($"连接时发生未预料的异常：{ex.Message}");
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
                return;
            }

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
                ReportLog("关闭当前连接");
                tcpClient.Close();
                tcpClient.Dispose();
                return;
            }
            catch (Exception ex)
            {
                ReportLog($"无法设置TcpClient的TcpKeepAlive设置：{ex.Message}，堆栈：{ex.StackTrace}");
                ReportLog("关闭当前连接");
                tcpClient.Close();
                tcpClient.Dispose();
                return;
            }

            NetworkStream networkStream;
            try { networkStream = tcpClient.GetStream(); }
            catch (ObjectDisposedException)
            {
                Disconnected(DisconnectedReasonEnum.DisconnectingCalledByToken);
                return;
            }
            catch (InvalidOperationException)
            {
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
                return;
            }
            catch (Exception ex)
            {
                ReportLog($"获取网络流失败：{ex.Message}");
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
                return;
            }

            ClientInfo = new(tcpClient, networkStream);
            ReportLog($"已连接到原生服务器 {IP}:{Port}，开始握手");

            // 将第一个握手任务入队
            using (ClientInfo.TasksQueueLock.EnterScope())
            {
                ClientInfo.TasksQueue.Enqueue(SendNeedRSAPublicKeyAsync);
            }

            bool didWork1 = false;
            bool didWork2 = false;
            byte idleCount = 0;
            //握手循环
            while (ClientInfo.TcpClient.Connected && !StopToken.IsCancellationRequested && HandShakeStep != HandShakeProcess_Client.Finished)
            {
                try
                {
                    if (ClientInfo.NetworkStream.DataAvailable)
                    {
                        byte[]? dataPackContentReceived = await ReceiveDataPack();
                        if (dataPackContentReceived != null && dataPackContentReceived.Length > 0)
                        {
                            didWork1 = true;
                            ProcessHandshakeData(dataPackContentReceived);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    ReportLog("尝试读取网络流时发生异常，断开连接");
                    ReportLog($"Socket异常代码：{ex.SocketErrorCode}{Environment.NewLine}异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    break;
                }
                catch (Exception ex)
                {
                    ReportLog("尝试读取网络流时发生异常，断开连接");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    break;
                }
                if (!ClientInfo.TcpClient.Connected || StopToken.IsCancellationRequested)
                    break;

                if (ClientInfo.TasksQueue.Count != 0)
                {
                    didWork2 = true;
                    Func<Task>[] tasksCopy;
                    using (ClientInfo.TasksQueueLock.EnterScope())
                    {
                        tasksCopy = [.. ClientInfo.TasksQueue];
                        ClientInfo.TasksQueue.Clear();
                    }
                    foreach (var task in tasksCopy)
                        await task();
                }

                if (didWork1 || didWork2)
                {
                    idleCount = 0;
                    didWork1 = false;
                    didWork2 = false;
                    continue;
                }
                await Task.Delay(50);
                didWork1 = false;
                didWork2 = false;
                idleCount++;
                if (idleCount == 200)//约10s
                {
                    ReportLog("服务器长时间无响应，判定连接已断开");
                    break;
                }
            }

            if (HandShakeStep != HandShakeProcess_Client.Finished)
            {
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
                try { ClientInfo.NetworkStream.Close(); } catch { }
                try { ClientInfo.TcpClient.Close(); } catch { }
                try { ClientInfo.Aes?.Dispose(); } catch { }
                ClientInfo.TasksQueue.Clear();
                ReportLog($"握手流程在进行到{HandShakeStep}时终止");
                return;
            }

            didWork1 = false;
            didWork2 = false;
            idleCount = 0;

            while (ClientInfo.TcpClient.Connected && !StopToken.IsCancellationRequested && HandShakeStep == HandShakeProcess_Client.Finished)
            {
                try
                {
                    if (ClientInfo.NetworkStream.DataAvailable)
                    {
                        byte[]? dataPackContentReceived = await ReceiveDataPack();
                        if (dataPackContentReceived != null && dataPackContentReceived.Length > 0)
                        {
                            didWork1 = true;
                            ProcessDataPack(dataPackContentReceived);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    ReportLog("尝试读取网络流时发生异常，断开连接");
                    ReportLog($"Socket异常代码：{ex.SocketErrorCode}{Environment.NewLine}异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    break;
                }
                catch (Exception ex)
                {
                    ReportLog("尝试读取网络流时发生异常，断开连接");
                    ReportLog($"异常信息：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    break;
                }
                if (!ClientInfo.TcpClient.Connected || StopToken.IsCancellationRequested)
                    break;

                if (ClientInfo.TasksQueue.Count != 0)
                {
                    didWork2 = true;
                    Func<Task>[] tasksCopy;
                    using (ClientInfo.TasksQueueLock.EnterScope())
                    {
                        tasksCopy = [.. ClientInfo.TasksQueue];
                        ClientInfo.TasksQueue.Clear();
                    }
                    foreach (var task in tasksCopy)
                        await task();
                }

                if (didWork1 || didWork2)
                {
                    idleCount = 0;
                    didWork1 = false;
                    didWork2 = false;
                    continue;
                }
                await Task.Delay(50);
                didWork1 = false;
                didWork2 = false;
                idleCount++;
                if (idleCount == 200)//约10s
                {
                    ReportLog("服务器长时间无响应，判定连接已断开");
                    break;
                }
            }

            // 清理资源
            if (StopToken.IsCancellationRequested)
                Disconnected(DisconnectedReasonEnum.DisconnectingCalledByToken);
            else if (!ClientInfo.TcpClient.Connected)
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
            else if (idleCount >= 200)
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
            else
                Disconnected(DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected);
            try { ClientInfo.NetworkStream.Close(); } catch { }
            try { ClientInfo.TcpClient.Close(); } catch { }
            try { ClientInfo.Aes?.Dispose(); } catch { }
            ClientInfo.TasksQueue.Clear();
            ReportLog("连接已关闭");
        }

        #region 握手task

        private async Task SendNeedRSAPublicKeyAsync()
        {
            HandShakeStep = HandShakeProcess_Client.SendingNeedRSAPublicKey;
            HandskakeProcessChanged(HandShakeStep);
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RequestTypeEnum_Private.NeedRSAPublicKey];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("发送 NeedRSAPublicKey 失败，断开连接");
                Disconnect();
                return;
            }
            HandShakeStep = HandShakeProcess_Client.SentNeedRSAPublicKey;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("已发送 NeedRSAPublicKey，等待服务器RSA公钥");
            HandShakeStep = HandShakeProcess_Client.WaitingRSAPublicKey;
            HandskakeProcessChanged(HandShakeStep);
        }

        private async Task SendGotRSAPublicKeyAsync()
        {
            HandShakeStep = HandShakeProcess_Client.SendingGotRSAPublicKey;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("正在发送 GotRSAPublicKey");
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RequestTypeEnum_Private.GotRSAPublicKey];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("发送 GotRSAPublicKey 失败，断开连接");
                Disconnect();
                return;
            }
            HandShakeStep = HandShakeProcess_Client.SentGotRSAPublicKey;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("已确认RSA公钥，等待服务器请求AES密钥");
            HandShakeStep = HandShakeProcess_Client.WaitingNeedAES;
            HandskakeProcessChanged(HandShakeStep);
        }
        private async Task SendRSAPublicKeyMismatch()
        {
            ReportLog("用户拒绝RSA公钥指纹，发送 RSAPublicKeyMismatch 并断开");
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RequestTypeEnum_Private.RSAPublicKeyMismatch];
            await SendDataPack(dataPack);
            Disconnect();
            Disconnected(DisconnectedReasonEnum.RSAPublicKeyMismatch);
        }
        private async Task SendAESKeyAsync()
        {
            HandShakeStep = HandShakeProcess_Client.SendingAESKey;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("正在发送 AESKey（RSA加密）");
            if (ClientInfo!.Aes == null)
                return;
            byte[] dataPack = [0, (byte)RequestTypeEnum_Private.AESKey, .. ClientInfo.Aes.Key, .. ClientInfo.Aes.IV];
            try
            {
                dataPack = SimpleHybridEncryption.EncryptWithRSA(dataPack, RSAPublicKey);
            }
            catch (Exception ex)
            {
                ReportLog($"使用RSA加密AES密钥失败：{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }
            dataPack = [0, 0, 0, 0, .. dataPack];
            BinaryPrimitives.WriteInt32BigEndian(dataPack.AsSpan(0, 4), dataPack.Length - 4);
            if (dataPack.Length == 0)
            {
                ReportLog("生成 AESKey 数据包失败，断开连接");
                Disconnect();
                return;
            }
            if (!await SendDataPack(dataPack))
            {
                ReportLog("发送 AESKey 失败，断开连接");
                Disconnect();
                return;
            }
            HandShakeStep = HandShakeProcess_Client.SentAESKey;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("已发送AES密钥，等待服务器确认");
            HandShakeStep = HandShakeProcess_Client.WaitingGotAES;
            HandskakeProcessChanged(HandShakeStep);
        }

        private async Task SendLoginAsync(string password)
        {
            HandShakeStep = HandShakeProcess_Client.SendingLogin;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("正在发送登录凭据");

            byte[] passwordBytes;
            try
            {
                passwordBytes = Encoding.UTF8.GetBytes(password);
            }
            catch (Exception ex)
            {
                ReportLog($"编码访问密钥时发生异常：{ex.Message}");
                return;
            }
            var (dataPack, succeed) = GenerateDataPack(RequestTypeEnum_Private.Login, passwordBytes);
            CryptographicOperations.ZeroMemory(passwordBytes.AsSpan());
            if (!succeed)
            {
                ReportLog("生成 Login 数据包失败，断开连接");
                Disconnect();
                return;
            }
            if (!await SendDataPack(dataPack))
            {
                ReportLog("发送 Login 失败，断开连接");
                Disconnect();
                return;
            }
            CryptographicOperations.ZeroMemory(dataPack.AsSpan());
            HandShakeStep = HandShakeProcess_Client.SentLogin;
            HandskakeProcessChanged(HandShakeStep);
            ReportLog("已发送登录凭据，等待服务器确认登录成功");
            HandShakeStep = HandShakeProcess_Client.WaitingSucceed;
            HandskakeProcessChanged(HandShakeStep);
        }

        #endregion

        /// <summary>
        /// 向后端发送请求，详细用法请看文档或翻官方前端代码
        /// </summary>
        /// <param name="requestTypeEnum">请求的枚举类型</param>
        /// <param name="payloadObject"></param>
        public void RequestBackend<T>(RequestTypeEnum requestTypeEnum, T payloadObject) where T : IProtoParser<T>
        {
            if (!HandShakeFinished || ClientInfo == null)
                return;

            using (ClientInfo.TasksQueueLock.EnterScope())
            {
                ClientInfo.TasksQueue.Enqueue(async () =>
                {
                    var (payload, succeed) = SerializePayloadObject<T>(payloadObject);
                    if (succeed)
                    {
                        var (dataPack, succeed1) = GenerateDataPack((RequestTypeEnum_Private)requestTypeEnum, payload);
                        if (succeed1)
                            await SendDataPack(dataPack);
                    }
                });
            }
        }

        /// <summary>
        /// 心跳包回复任务（客户端只回复心跳，不主动发送）
        /// </summary>
        private async Task ReplyHeartbeatAsync()
        {
            byte[] dataPack = [0, 0, 0, 2, 0, (byte)RequestTypeEnum_Private.ConnectionAlive];
            if (!await SendDataPack(dataPack))
            {
                ReportLog("回复心跳包失败");
            }
        }
        private (byte[] payload, bool succeed) SerializePayloadObject<T>(T payloadObject) where T : IProtoParser<T>
        {
            try
            {
#if DEBUG//方便添加断点查看序列化数据
                byte[] result = payloadObject.ToByteArray();
                return (result, true);

#else
                return (payloadObject.ToByteArray(), true);
#endif
            }
            catch (Exception ex)
            {
                ReportLog($"序列化对象[{typeof(T)}]失败:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return ([], false);
        }
        /// <summary>
        /// 生成数据包（HTTP风格的包体包含类型标识和LightProto序列化内容）
        /// </summary>
        private (byte[] dataPack, bool succeed) GenerateDataPack(RequestTypeEnum_Private type, byte[] payload, bool needAes = true)
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

        /// <summary>
        /// 发送数据包（先发送4字节头，再发送包体）
        /// </summary>
        private async Task<bool> SendDataPack(byte[] dataPack)
        {
            if (ClientInfo == null)
                return false;
            try
            {
                await ClientInfo.NetworkStream.WriteAsync(dataPack, StopToken.Token);
                await ClientInfo.NetworkStream.FlushAsync(StopToken.Token);
                return true;
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            catch (IOException ex)
            {
                ReportLog($"发送数据时发生IO异常：{ex.Message}");
            }
            catch (Exception ex)
            {
                ReportLog($"发送数据时发生未知异常：{ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 接收一个完整的数据包（4字节头+包体）。若检测到对端关闭，会取消StopToken并返回null。
        /// </summary>
        private async Task<byte[]?> ReceiveDataPack()
        {
            if (ClientInfo == null)
                return null;

            byte[] header = new byte[4];
            int read = 0;
            while (ClientInfo.TcpClient.Connected && read < 4 && !StopToken.IsCancellationRequested)
            {
                int received;
                try
                {
                    received = await ClientInfo.NetworkStream.ReadAsync(header.AsMemory(read, 4 - read), StopToken.Token);
                }
                catch (ObjectDisposedException) { return null; }
                catch (OperationCanceledException) { return null; }
                catch (IOException ex)
                {
                    ReportLog($"接收数据头时发生IO异常：{ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    ReportLog($"接收数据头时发生异常：{ex.Message}");
                    return null;
                }

                if (received == 0)
                {
                    ReportLog("远端主机关闭了连接（接收数据头时返回0）");
                    try { StopToken.Cancel(); } catch { }
                    return null;
                }
                read += received;
            }
            if (read != 4)
                return null;

            int contentLength = BinaryPrimitives.ReadInt32BigEndian(header);
            if (contentLength <= 0)
                return null;
            if (contentLength > MAXDATAPACKLENGHT)
            {
                try
                {
                    StopToken.Cancel();
                }
                catch { }
                ReportLog($"前端发送过大的数据包，可能为攻击者恶意发送，长度：{contentLength}");
                return [];
            }//修复大数据包攻击
            byte[] content = new byte[contentLength];
            read = 0;
            while (ClientInfo.TcpClient.Connected && read < contentLength && !StopToken.IsCancellationRequested)
            {
                int received;
                try
                {
                    received = await ClientInfo.NetworkStream.ReadAsync(content.AsMemory(read, contentLength - read), StopToken.Token);
                }
                catch (ObjectDisposedException) { return null; }
                catch (OperationCanceledException) { return null; }
                catch (IOException ex)
                {
                    ReportLog($"接收数据体时发生IO异常：{ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    ReportLog($"接收数据体时发生异常：{ex.Message}");
                    return null;
                }

                if (received == 0)
                {
                    ReportLog("远端主机关闭了连接（接收数据体时返回0）");
                    try { StopToken.Cancel(); } catch { }
                    return null;
                }
                read += received;
            }
            if (read != contentLength)
                return null;

            return content;
        }

        /// <summary>
        /// 处理握手阶段的数据包（含状态校验、状态转换、任务入队）
        /// </summary>
        private void ProcessHandshakeData(byte[] data)
        {
            if (ClientInfo == null) return;
            if (data.Length == 6)
            {
                ReadOnlySpan<byte> shortData = data.AsSpan();
                if (shortData is [0, (byte)RespondTypeEnum_Private.ConnectionAlive])
                {
                    using (ClientInfo.TasksQueueLock.EnterScope())
                    {
                        ClientInfo.TasksQueue.Enqueue(ReplyHeartbeatAsync);
                    }
                    return;
                }
            }
            if (data.Length < 2) return;
            byte type1 = data[0];
            byte type2 = data[1];
            if (type1 != 0 || !Enum.IsDefined(typeof(RespondTypeEnum_Private), (int)type2))
                return;

            RespondTypeEnum_Private type = (RespondTypeEnum_Private)type2;
            switch (type)
            {
                case RespondTypeEnum_Private.ConnectionAlive:
                    using (ClientInfo.TasksQueueLock.EnterScope())
                    {
                        ClientInfo.TasksQueue.Enqueue(ReplyHeartbeatAsync);
                    }
                    break;
                case RespondTypeEnum_Private.RSAPublicKey:
                    if (HandShakeStep != HandShakeProcess_Client.WaitingRSAPublicKey)
                    {
                        ReportLog("服务器不遵循PMCSsE的握手协议，断开连接");
                        Disconnect();
                        return;
                    }
                    try
                    {
                        RSAPublicKey = Encoding.UTF8.GetString(data, 2, data.Length - 2);
                    }
                    catch (Exception ex)
                    {
                        ReportLog($"解析RSA公钥失败：{ex.Message}");
                        Disconnect();
                        return;
                    }
                    HandShakeStep = HandShakeProcess_Client.ReceivedRSAPublicKey;
                    HandskakeProcessChanged(HandShakeStep);
                    ReportLog("已收到服务器RSA公钥");

                    string rsaFingerprint;
                    try
                    {
                        rsaFingerprint = GetSHA256FromTextClass.GetSHA256FromText(RSAPublicKey);
                    }
                    catch (Exception ex)
                    {
                        ReportLog($"计算RSA公钥指纹失败：{ex.Message}，断开连接");
                        Disconnect();
                        return;
                    }
                    ReportLog($"RSA公钥指纹：{rsaFingerprint}");
                    ReportLog("请检查RSA公钥指纹与后端是否一致，如果一致，请确认连接");
                    NeedToVerifyRSAPublicKey(rsaFingerprint);
                    return;
                case RespondTypeEnum_Private.NeedAES:
                    if (HandShakeStep != HandShakeProcess_Client.WaitingNeedAES)
                    {
                        ReportLog("服务器不遵循PMCSsE的握手协议，断开连接");
                        Disconnect();
                        return;
                    }
                    HandShakeStep = HandShakeProcess_Client.ReceivedNeedAES;
                    HandskakeProcessChanged(HandShakeStep);
                    ReportLog("收到 NeedAES 请求，准备生成AES密钥并发送");
                    try
                    {
                        ClientInfo!.Aes = SimpleHybridEncryption.GenerateAes();
                    }
                    catch (Exception ex)
                    {
                        ReportLog($"生成AES密钥失败：{ex.Message}，断开连接");
                        Disconnect();
                        return;
                    }
                    using (ClientInfo.TasksQueueLock.EnterScope())
                    {
                        ClientInfo.TasksQueue.Enqueue(SendAESKeyAsync);
                    }
                    return;
                case RespondTypeEnum_Private.GotAES:
                    if (HandShakeStep != HandShakeProcess_Client.WaitingGotAES)
                    {
                        ReportLog("服务器不遵循PMCSsE的握手协议，断开连接");
                        Disconnect();
                        return;
                    }
                    HandShakeStep = HandShakeProcess_Client.ReceivedGotAES;
                    HandskakeProcessChanged(HandShakeStep);
                    ReportLog("服务器已确认AES密钥，等待输入访问密钥");
                    NeedPassword();
                    return;
                case RespondTypeEnum_Private.Succeed:
                    if (HandShakeStep != HandShakeProcess_Client.WaitingSucceed)
                    {
                        ReportLog("服务器不遵循PMCSsE的握手协议，断开连接");
                        Disconnect();
                        return;
                    }
                    HandShakeFinished = true;
                    HandShakeStep = HandShakeProcess_Client.Finished;
                    HandskakeProcessChanged(HandShakeStep);
                    ReportLog("握手完成，连接已安全建立");
                    Connected();
                    return;
                default:
                    ReportLog($"握手阶段收到未知类型: {type}");
                    return;
            }
        }

        /// <summary>
        /// 处理握手完成后收到的数据包（解密后分派至事件，并处理心跳回复）
        /// </summary>
        private void ProcessDataPack(byte[] dataPack)
        {
            if (ClientInfo == null)
                return;

            if (HandShakeFinished)
            {
                // 简短心跳包长度可能为2（不加密的 ConnectionAlive）
                if (dataPack.Length == 2)
                {
                    byte t1 = dataPack[0];
                    byte t2 = dataPack[1];
                    if (t1 == 0 && t2 == (byte)RespondTypeEnum_Private.ConnectionAlive)
                    {
                        // 收到服务器心跳，立即回复（不加密）
                        using (ClientInfo.TasksQueueLock.EnterScope())
                        {
                            ClientInfo.TasksQueue.Enqueue(ReplyHeartbeatAsync);
                        }
                        return;
                    }
                }

                if (ClientInfo.Aes == null)
                    return;
                try
                {
                    dataPack = SimpleHybridEncryption.DecryptWithAES(dataPack, ClientInfo.Aes);
                }
                catch (Exception ex)
                {
                    ReportLog($"AES解密失败：{ex.Message}");
                    return;
                }

                if (dataPack.Length < 2)
                    return;
                byte type1 = dataPack[0];
                byte type2 = dataPack[1];

                if (type1 == 0) // 系统消息
                {
                    if (!Enum.IsDefined(typeof(RespondTypeEnum_Private), (int)type2))
                    {
                        ReportLog($"收到未知的响应类型:{type2}");
                        return;
                    }
                    RespondTypeEnum_Private respondType = (RespondTypeEnum_Private)type2;
                    ReadOnlySpan<byte> payload = dataPack.AsSpan(2);
                    try
                    {
                        switch (respondType)
                        {
                            case RespondTypeEnum_Private.ServerState:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ServerState>(payload));
                                return;
                            case RespondTypeEnum_Private.SaveCipthertextConfigSucceed:
                                DataPackBus.Publish(new Pack_SaveCipthertextConfigSucceed());
                                return;
                            case RespondTypeEnum_Private.SaveCipthertextConfigFailed:
                                DataPackBus.Publish(new Pack_SaveCipthertextConfigFailed());
                                return;
                            case RespondTypeEnum_Private.MCServerManagerConfigs:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_MCServerManagerConfigs>(payload));
                                return;
                            case RespondTypeEnum_Private.LoadedMCServerManagers:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_MCServerManagers>(payload));
                                return;
                            case RespondTypeEnum_Private.SupportedMCServerTypes:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_SupportedMCServerTypes>(payload));
                                return;
                            case RespondTypeEnum_Private.CreatedNewMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_CreatedNewMCServerManager>(payload));
                                return;
                            case RespondTypeEnum_Private.CreatNewMCServerManagerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_CreatNewMCServerManagerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.LoadedMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_LoadedMCServerManager>(payload));
                                return;
                            case RespondTypeEnum_Private.LoadMCServerManagerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_LoadMCServerManagerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.StoppedMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_StoppedMCServerManager>(payload));
                                return;
                            case RespondTypeEnum_Private.StopMCServerManagerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_StopMCServerManagerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.DeletedMCServerManager:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_DeletedMCServerManager>(payload));
                                return;
                            case RespondTypeEnum_Private.DeleteMCServerManagerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_DeleteMCServerManagerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.ModifiedMCServerManagerConfig:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ModifiedMCServerManagerConfig>(payload));
                                return;
                            case RespondTypeEnum_Private.RunMCServerSucceed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_RunMCServerSucceed>(payload));
                                return;
                            case RespondTypeEnum_Private.RunMCServerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_RunMCServerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.SendCommandSucceed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_SendCommandSucceed>(payload));
                                return;
                            case RespondTypeEnum_Private.SendCommandFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_SendCommandFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.ShutdownMCServerSucceed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ShutdownMCServerSucceed>(payload));
                                return;
                            case RespondTypeEnum_Private.ShutdownMCServerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ShutdownMCServerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.KillMCServerSucceed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_KillMCServerSucceed>(payload));
                                return;
                            case RespondTypeEnum_Private.KillMCServerFailed:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_KillMCServerFailed>(payload));
                                return;
                            case RespondTypeEnum_Private.MCServerExited:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_MCServerExited>(payload));
                                return;
                            case RespondTypeEnum_Private.MCServerLogs:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_MCServerLogs>(payload));
                                return;
                            case RespondTypeEnum_Private.ErrorInfo:
                                DataPackBus.Publish(Serializer.Deserialize<Pack_ErrorInfo>(payload));
                                return;
                            default:
                                ReportLog($"响应类型: {respondType}，未定义处理方法");
                                return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportLog($"LightProto 反序列化响应内容失败 (类型: {respondType})：{ex.Message}");
                        return;
                    }
                }
                else // 插件数据
                {
                    CommunicatorDataReceivedForPlugins(type1, dataPack);
                }
            }
            else
            {
                ReportLog("在非预期状态下收到数据，可能握手已完成但状态未更新");
            }
        }

        /// <summary>
        /// 把用户输入的访问密钥给予NativeClient发给后端验证
        /// </summary>
        /// <param name="password">访问密钥</param>
        public void TypePassword(string password)
        {
            using (ClientInfo!.TasksQueueLock.EnterScope())
            {
                ClientInfo.TasksQueue.Enqueue(async () => await SendLoginAsync(password));
            }
        }
        /// <summary>
        /// 验证RSA公钥是否匹配
        /// </summary>
        /// <param name="isRSAPublicKeyMatch"></param>
        public void VerifyRSAPublicKey(bool isRSAPublicKeyMatch)
        {
            if (isRSAPublicKeyMatch == true)
            {
                // 用户确认，入队 SendGotRSAPublicKeyAsync
                using (ClientInfo!.TasksQueueLock.EnterScope())
                {
                    ClientInfo.TasksQueue.Enqueue(SendGotRSAPublicKeyAsync);
                }
            }
            else
            {
                using (ClientInfo!.TasksQueueLock.EnterScope())
                {
                    ClientInfo.TasksQueue.Enqueue(SendRSAPublicKeyMismatch);
                }
            }
        }

        /// <summary>
        /// NativeClient连接断开原因的枚举
        /// </summary>
        public enum DisconnectedReasonEnum
        {
            /// <summary>
            /// 用户手动取消连接/断开连接
            /// </summary>
            DisconnectingCalledByToken,
            /// <summary>
            /// 发生Socket异常(会同时在reportlog上报Socket异常代码)
            /// </summary>
            SocketException,
            /// <summary>
            /// 连接参数错误(如IP，端口)
            /// </summary>
            InvalidConnectionParameter,
            /// <summary>
            /// 内存严重不足
            /// </summary>
            OutOfMemory,
            /// <summary>
            /// 未知的数据包格式，这一般在版本不匹配时发生
            /// </summary>
            UnknownPackFormat,
            /// <summary>
            /// 验证RSA公钥指纹超时
            /// </summary>
            VerifyRSAPublicKeyTimeOut,
            /// <summary>
            /// RSA公钥指纹不匹配
            /// </summary>
            RSAPublicKeyMismatch,
            /// <summary>
            /// 密码错误
            /// </summary>
            PasswordMismatch,
            /// <summary>
            /// 连接意外断开
            /// </summary>
            ConnectionUnexpectlyDisconnected,
            /// <summary>
            /// 生成数据包失败
            /// </summary>
            GenerateDataPackFailed,
            /// <summary>
            /// UTF8解码失败
            /// </summary>
            EncoderFallBack,
            /// <summary>
            /// 用户取消连接
            /// </summary>
            UserDisconnect
        }
    }
}
