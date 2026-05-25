using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace PMCSsE_FrontendAndBackendCommunicator
{
    public class WebServer
    {
        private HttpListener HttpListener;
        private CancellationTokenSource CancellationTokenSource;
        private readonly List<WebSocket> WebSocketClients = [];
        public event Action<string, string, string> ReportLog = delegate { };
        private Thread HttpListenerThread;
        public event Action<bool> ReportServiceState = delegate { };
        public bool ServiceState = false;

        public WebServer()
        {
            
        }

        public void Start()
        {

            StartServer();
            ServiceState = true;
            ReportServiceState(true);
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"已进行启动操作，如果启动失败或无法连接请以管理员身份执行以下两条命令，执行过的不用再次执行");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"netsh http add urlacl url=http://+:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}/ user=Everyone");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"netsh advfirewall firewall add rule name=\"PMCSsE的实时服内外通信和远程服务器管理器:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}\" dir=in action=allow protocol=TCP localport={SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"在浏览器中输入[http://{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerIP}:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}/OnlineChatAndManageWebPage.html]可打开网页端\n（若您配置的IP为0.0.0.0，则请输入[http://localhost:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}/OnlineChatAndManageWebPage.html]）");
        }

        public void HandleCommand(string Command)
        {
            if (Command.StartsWith('/'))
            {
                ReportLog("用户操作", "实时服内外通信和远程服务器管理器", $"用户输入命令：[{Command}]");
                string[] commaand = Command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (commaand[0])
                {
                    case "listAccounts" or "列出账户":
                        if (commaand.Length < 2 || commaand.Length > 2)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"命令格式错误，查看所有可用命令和命令用法请输入命令：“help”或“命令提示”或查阅文档");
                            return;
                        }
                        switch (commaand[1])
                        {
                            case "Approved" or "通过审核的":
                                if (SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList.Count == 0)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有任何玩家在网页端注册");
                                    return;
                                }
                                StringBuilder ApprovedAccounts = new("以下为已通过审核的账户");
                                lock (SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                                {
                                    foreach (Account Account in SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                                    {
                                        if (Account.Approved)
                                        {
                                            ApprovedAccounts.AppendLine("玩家角色：");
                                            ApprovedAccounts.Append(Account.PlayerRole);
                                            ApprovedAccounts.AppendLine("玩家名称：");
                                            ApprovedAccounts.Append(Account.PlayerName);
                                            ApprovedAccounts.AppendLine("第三方平台账号：");
                                            ApprovedAccounts.Append(Account.ThirdPartySocialPlatformAccount);
                                            ApprovedAccounts.AppendLine("-------------------------------");
                                        }
                                    }

                                }
                                if (ApprovedAccounts.Length == 11)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有已通过审核的账户");
                                    return;
                                }
                                ReportLog("信息", "实时服内外通信和远程服务器管理器", ApprovedAccounts.ToString());
                                break;
                            case "NotApproved" or "未通过审核的":
                                if (SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList.Count == 0)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有任何玩家在网页端注册");
                                    return;
                                }
                                StringBuilder ApprovedAccounts1 = new("以下为未通过审核的账户");
                                lock (SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                                {
                                    foreach (Account Account in SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                                    {
                                        if (!Account.Approved)
                                        {
                                            ApprovedAccounts1.AppendLine("玩家角色：");
                                            ApprovedAccounts1.Append(Account.PlayerRole);
                                            ApprovedAccounts1.AppendLine("玩家名称：");
                                            ApprovedAccounts1.Append(Account.PlayerName);
                                            ApprovedAccounts1.AppendLine("第三方平台账号：");
                                            ApprovedAccounts1.Append(Account.ThirdPartySocialPlatformAccount);
                                            ApprovedAccounts1.AppendLine("-------------------------------");
                                        }
                                    }

                                }
                                if (ApprovedAccounts1.Length == 11)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有未通过审核的账户");
                                    return;
                                }
                                ReportLog("信息", "实时服内外通信和远程服务器管理器", ApprovedAccounts1.ToString());
                                break;
                            default:
                                ReportLog("错误", "实时服内外通信和远程服务器管理器", $"命令格式错误，查看所有可用命令和命令用法请输入命令：“help”或“命令提示”或查阅文档");
                                break;
                        }
                        break;
                    case "setrole" or "设置身份":
                        if (commaand.Length < 3 || commaand.Length > 3)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"命令格式错误，查看所有可用命令和命令用法请输入命令：“help”或“命令提示”或查阅文档");
                            return;
                        }
                        string playername = commaand[1];
                        Account playerAccount = null;
                        foreach (Account account in SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                        {
                            if (account.PlayerName == playername)
                            {
                                playerAccount = account;
                                break;
                            }
                        }
                        if (playerAccount == null)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"指定的用户未在网页端注册");
                            return;
                        }
                        switch (commaand[2])
                        {
                            case "玩家":
                                playerAccount.PlayerRole = "玩家";
                                break;
                            case "管理员":
                                playerAccount.PlayerRole = "管理员";
                                break;
                            case "服主":
                                playerAccount.PlayerRole = "服主";
                                break;
                            default:
                                ReportLog("错误", "实时服内外通信和远程服务器管理器", $"用户角色只能是 玩家/管理员/服主 中的一个");
                                return;
                        }
                        StaticConfigManagerClass.SaveMCServerManagersConfig();
                        break;
                    case "delaccount" or "删除账户":
                        if (commaand.Length < 2 || commaand.Length > 2)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"命令格式错误，查看所有可用命令和命令用法请输入命令：“help”或“命令提示”或查阅文档");
                            return;
                        }
                        string playername1 = commaand[1];
                        Account playerAccount1 = null;
                        foreach (Account account in SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                        {
                            if (account.PlayerName == playername1)
                            {
                                playerAccount1 = account;
                                break;
                            }
                        }
                        if (playerAccount1 == null)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"指定的用户未在网页端注册");
                            return;
                        }
                        SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList.Remove(playerAccount1);
                        StaticConfigManagerClass.SaveMCServerManagersConfig();
                        ReportLog("成功", "实时服内外通信和远程服务器管理器", $"成功删除账户：[{playerAccount1.PlayerName}]");
                        break;
                    case "help" or "命令提示":
                        ReportLog("信息", "实时服内外通信和远程服务器管理器", "所有可用命令：\n" +
                            "listAccounts Approved/NotApproved （列出已通过审核或未通过审核的账户）\n" +
                            "列出账户 通过审核的/未通过审核的 （列出已通过审核或未通过审核的账户）\n" +
                            "setrole {用户网页端注册用的名字} 玩家/管理员/服主 （设置一个用户的身份）\n" +
                            "设置身份 {用户网页端注册用的名字} 玩家/管理员/服主 （设置一个用户的身份）\n" +
                            "delaccount {用户网页端注册用的名字} （删除一个账户）\n" +
                            "删除账户 {用户网页端注册用的名字} （删除一个账户）\n" +
                            "help （显示本条提示信息）" +
                            "命令提示 （显示本条提示信息）");
                        break;
                    default:
                        ReportLog("错误", "实时服内外通信和远程服务器管理器", $"未知命令，查看所有可用命令和命令用法请输入/help或查阅文档");
                        break;
                }
                return;
            }
        }

        public void Stop()
        {
            SingleMCServerManager.ReportLog -= HandleServerMessage;
            CancellationTokenSource?.Cancel();
            HttpListener?.Close();
            HttpListener = null;
            MessageRecordingsManager?.Dispose();
            MessageRecordingsManager = null;
            ServiceState = false;
            ReportServiceState(false);
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"已停止服务");

        }

        private void StartServer()
        {
            CancellationTokenSource = new CancellationTokenSource();
            ThreadStart threadStart = new(async () =>
            {
                HttpListener = new HttpListener();
                string address = $"http://{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerIP}:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}/";
                if (SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerIP == "0.0.0.0")
                {
                    address = $"http://+:{SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ServerPort}/";
                }
                ReportLog("信息", "实时服内外通信和远程服务器管理器", $"将在{address}上监听HTTP和WebSocket请求");
                try
                {
                    HttpListener.Prefixes.Add(address);
                    HttpListener.Start();
                }
                catch (Exception ex)
                {
                    ReportLog("错误", "实时服内外通信和远程服务器管理器", $"启动Web + WebSocket服务器时发生异常：{ex.Message}");
                    ReportLog("错误", "实时服内外通信和远程服务器管理器", $"正在停止服务");
                    Stop();
                    return;
                }
                ReportLog("成功", "实时服内外通信和远程服务器管理器", $"Web + WebSocket服务器已启动[{address}]");

                while (!CancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var context = await HttpListener.GetContextAsync();
                        if (context.Request.IsWebSocketRequest)
                        {
                            ProcessWebSocketRequest(context);
                        }
                        else
                        {
                            // 静态文件服务
                            string urlPath = context.Request.Url.AbsolutePath.TrimStart('/');
                            if (string.IsNullOrEmpty(urlPath) || urlPath == "index.html")
                                urlPath = "OnlineChatAndManageWebPage.html"; // 默认首页

                            string filePath = Path.Combine("WebPage", urlPath);
                            if (File.Exists(filePath))
                            {
                                context.Response.ContentType = "text/html";
                                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                                await context.Response.OutputStream.WriteAsync(fileBytes.AsMemory(), CancellationTokenSource.Token);
                                context.Response.StatusCode = 200;
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                                byte[] notFound = Encoding.UTF8.GetBytes("404:Page not found");
                                await context.Response.OutputStream.WriteAsync(notFound.AsMemory(), CancellationTokenSource.Token);
                            }
                            context.Response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ReportLog == null || ex.HResult == -2147467259)
                        {
                            return;
                        }
                        ReportLog("错误", "实时服内外通信和远程服务器管理器", $"发生异常：{ex.Message}");
                    }
                }
            });
            HttpListenerThread = new(threadStart);
            HttpListenerThread.Start();
        }

        private async void ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocket webSocket;
            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;
                lock (WebSocketClients)
                {
                    WebSocketClients.Add(webSocket);
                }

                ReportLog("信息", "实时服内外通信和远程服务器管理器", $"新的客户端已连接 (当前客户端数: {WebSocketClients.Count})");

                await HandleClient(webSocket);
            }
            catch (Exception ex)
            {
                ReportLog("错误", "实时服内外通信和远程服务器管理器", $"发生异常：{ex.Message}");
            }
            finally
            {
                if (webSocket != null)
                {
                    lock (WebSocketClients)
                    {
                        WebSocketClients.Remove(webSocket);
                    }
                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"一个客户端断开了连接 (当前客户端数: {WebSocketClients.Count})");

                }
            }
        }

        private async Task HandleClient(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);
                        return;
                    }

                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessage(messageJson, webSocket);
                }
                catch (WebSocketException ex)
                {
                    ReportLog("错误", "实时服内外通信和远程服务器管理器", $"发生WebSocket异常：{ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    ReportLog("错误", "实时服内外通信和远程服务器管理器", $"发生异常：{ex.Message}");
                }
            }
        }

        private async Task ProcessMessage(string messageJson, WebSocket senderSocket)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(messageJson);
                JsonElement root = doc.RootElement;
                string? messageType = root.GetProperty("Type").GetString();
                if (string.IsNullOrEmpty(messageType)) { return; }
                switch (messageType)
                {
                    case "RequestConfig":
                        var Config = new
                        {
                            Type = "Config",
                            SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.ThirdPartySocialPlatformName
                        };
                        await SendStringAsync(senderSocket, JsonSerializer.Serialize(Config));
                        break;

                    case "Login":
                        string playername1 = root.GetProperty("PlayerName").GetString();
                        string passwordhash1 = root.GetProperty("PasswordHash").GetString();

                        bool IsContained1 = false;
                        Account PlayerAccount = null;

                        foreach (Account account in SingleMCServerManager.ThisMCServerManagerConfigInfo.OnlineChattingAndManagerConfigInfo.PlayerAccountList)
                        {
                            if (playername1 == account.PlayerName)
                            {
                                IsContained1 = true;
                                PlayerAccount = account;
                                break;
                            }
                        }
                        if (IsContained1)
                        {
                            if (!PlayerAccount.Approved)
                            {
                                var loginResponse = new
                                {
                                    Type = "LoginResponse",
                                    Message = "NotApproved"
                                };

                                await SendStringAsync(senderSocket, JsonSerializer.Serialize(loginResponse));
                                return;
                            }
                            if (passwordhash1 == PlayerAccount.PasswordHash)
                            {
                                var loginResponse = new
                                {
                                    Type = "LoginResponse",
                                    Message = "Succeed",
                                    PlayerAccount
                                };

                                await SendStringAsync(senderSocket, JsonSerializer.Serialize(loginResponse));

                                int MessagesCount = MessageRecordingsManager.GetMessagesCount();
                                int startIndex = Math.Max(0, MessagesCount - 1 - 20);
                                int takeCount = Math.Min(20, MessagesCount);

                                List<ChatMessageClass> last20Items = MessageRecordingsManager.ReadMessage(startIndex, startIndex + takeCount);
                                var historyMessage = new
                                {
                                    Type = "Latest20Messages",
                                    Messages = last20Items
                                };

                                await SendStringAsync(senderSocket, JsonSerializer.Serialize(historyMessage));
                            }
                            else
                            {
                                var loginResponse = new
                                {
                                    Type = "LoginResponse",
                                    Message = "WrongPassword"
                                };

                                await SendStringAsync(senderSocket, JsonSerializer.Serialize(loginResponse));

                            }
                        }
                        else
                        {
                            var loginResponse = new
                            {
                                type = "LoginResponse",
                                Message = "NotExist"
                            };

                            await SendStringAsync(senderSocket, JsonSerializer.Serialize(loginResponse));
                        }
                        break;

                }
            }
            catch
            {
                ReportLog("错误", "Web服务器", $"处理Web端消息时发生异常");
            }
        }


        private async Task SendStringAsync(WebSocket socket, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationTokenSource.Token);
        }



        public void Dispose()
        {
            CancellationTokenSource?.Cancel();
            HttpListener?.Stop();
            HttpListener?.Close();
            ReportLog = delegate { };
        }
    }
}
