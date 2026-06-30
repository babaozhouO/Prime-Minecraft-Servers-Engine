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
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PMCSsE_Backend.Modules
{
    internal partial class OnlineChattingSystemClass
    {
        private HttpListener? HttpListener;
        private CancellationTokenSource? CancellationTokenSource;
        private readonly List<WebSocket> WebSocketClients = [];
        internal event Action<string, string, string> ReportLog = delegate { };
        private readonly MCServerManager MCServerManager;
        private Thread? HttpListenerThread;
        private MessageRecordingsManagerClass? MessageRecordingsManager;
        internal event Action<bool> ReportServiceState = delegate { };
        internal bool ServiceState = false;

        internal OnlineChattingSystemClass(MCServerManager mCServerManager)
        {
            MCServerManager = mCServerManager;
            throw new NotSupportedException("包含多个高危漏洞，请勿使用");//阻止初始化
        }

        internal void Start()
        {
            MessageRecordingsManager = new(MCServerManager.MCServerManagerConfig);
            MessageRecordingsManager.ReportLog += ReportLog;
            MessageRecordingsManager.Initialize();
            if (!MessageRecordingsManager.ManagerStarted)
            {
                MessageRecordingsManager.Dispose();
                ServiceState = false;
                ReportServiceState(false);
                return;
            }

            StartServer();
            MCServerManager.ReportManagerLog += HandleServerMessage;
            ServiceState = true;
            ReportServiceState(true);
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"已进行启动操作，如果启动失败或无法连接请以管理员身份执行以下两条命令，执行过的不用再次执行");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"netsh http add urlacl url=http://+:{MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ServerPort}/ user=Everyone");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"netsh advfirewall firewall add rule name=\"PMCSsE的实时服内外通信和远程服务器管理器:{MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ServerPort}\" dir=in action=allow protocol=TCP localport={MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ServerPort}");
            ReportLog("信息", "实时服内外通信和远程服务器管理器", $"在浏览器中输入[http://127.0.0.1:{MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ServerPort}/OnlineChatAndManageWebPage.html]可打开聊天网页");
        }

        internal void HandleCommand(string Command)
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
                                if (MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList.Count == 0)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有任何玩家在网页端注册");
                                    return;
                                }
                                StringBuilder ApprovedAccounts = new("以下为已通过审核的账户");
                                lock (MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
                                {
                                    foreach (PlayerAccount Account in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
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
                                if (MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList.Count == 0)
                                {
                                    ReportLog("信息", "实时服内外通信和远程服务器管理器", $"还没有任何玩家在网页端注册");
                                    return;
                                }
                                StringBuilder ApprovedAccounts1 = new("以下为未通过审核的账户");
                                lock (MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
                                {
                                    foreach (PlayerAccount Account in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
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
                        PlayerAccount? playerAccount = null;
                        foreach (PlayerAccount account in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
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
                        StaticConfigManagerClass.SaveConfig_Ciphertext();
                        break;
                    case "delaccount" or "删除账户":
                        if (commaand.Length < 2 || commaand.Length > 2)
                        {
                            ReportLog("错误", "实时服内外通信和远程服务器管理器", $"命令格式错误，查看所有可用命令和命令用法请输入命令：“help”或“命令提示”或查阅文档");
                            return;
                        }
                        string playername1 = commaand[1];
                        PlayerAccount? playerAccount1 = null;
                        foreach (PlayerAccount account in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
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
                        MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList.Remove(playerAccount1);
                        StaticConfigManagerClass.SaveConfig_Ciphertext();
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

        internal void Stop()
        {
            MCServerManager.ReportManagerLog -= HandleServerMessage;
            CancellationTokenSource?.Cancel();
            HttpListener?.Close();
            MessageRecordingsManager?.Dispose();
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
                string address = $"http://+:{MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ServerPort}/";

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
                            if (context.Request.Url is null) { continue; }
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
            WebSocket? webSocket = null;
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

                switch (messageType)
                {
                    case "RequestConfig":
                        var Config = new
                        {
                            Type = "Config",
                            MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.ThirdPartySocialPlatformName
                        };
                        await SendStringAsync(senderSocket, JsonSerializer.Serialize(Config));
                        break;

                    case "Register":
                        string? playername = root.GetProperty("Playername").GetString();
                        if (playername == null) { break; }
                        string? passwordhash = root.GetProperty("PasswordHash").GetString();
                        if (passwordhash == null) { break; }
                        string? ThirdPartySocialPlatformAccount = root.GetProperty("ThirdPartySocialPlatformAccount").GetString();
                        if (ThirdPartySocialPlatformAccount == null) { break; }

                        bool IsContained = false;

                        foreach (PlayerAccount playerAccount in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
                        {
                            if (playername == playerAccount.PlayerName)
                            {
                                IsContained = true;
                                break;
                            }
                        }
                        if (IsContained)
                        {
                            var registerRespone = new
                            {
                                Type = "RegisterRespone",
                                Message = "Existed"
                            };

                            await SendStringAsync(senderSocket, JsonSerializer.Serialize(registerRespone));
                        }
                        else
                        {
                            MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList.Add(new() { Approved = false, PlayerName = playername, PasswordHash = passwordhash, ThirdPartySocialPlatformAccount = ThirdPartySocialPlatformAccount });
                            StaticConfigManagerClass.SaveConfig_Ciphertext();
                            var RegisterRespone = new
                            {
                                Type = "RegisterRespone",
                                Message = "Succeed"
                            };

                            await SendStringAsync(senderSocket, JsonSerializer.Serialize(RegisterRespone));
                        }
                        break;

                    case "Login":
                        string? playername1 = root.GetProperty("PlayerName").GetString();
                        string? passwordhash1 = root.GetProperty("PasswordHash").GetString();

                        bool IsContained1 = false;
                        PlayerAccount? PlayerAccount = null;

                        foreach (PlayerAccount account in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
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
                            if (PlayerAccount == null) { break; }
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
                                if (MessageRecordingsManager == null) { break; }
                                int MessagesCount = MessageRecordingsManager.GetMessagesCount();
                                int startIndex = Math.Max(0, MessagesCount - 1 - 20);
                                int takeCount = Math.Min(20, MessagesCount);

                                List<ChatMessageClass>? last20Items = MessageRecordingsManager.ReadMessage(startIndex, startIndex + takeCount);
                                if (last20Items == null) { break; }
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

                    case "RequestHistory":
                        int startIndex1 = root.GetProperty("StartIndex").GetInt32();
                        int count = root.GetProperty("Count").GetInt32();
                        if (MessageRecordingsManager == null) { break; }
                        int MessagesCount1 = MessageRecordingsManager.GetMessagesCount();
                        startIndex1 = Math.Max(0, startIndex1);
                        count = Math.Min(MessagesCount1, count);
                        if (count == 0)
                        {
                            break;
                        }
                        List<ChatMessageClass>? HistoryMessages = MessageRecordingsManager.ReadMessage(startIndex1, startIndex1 + count);
                        if (HistoryMessages == null) { break; }
                        var historyResponse = new
                        {
                            Type = "HistoryMessages",
                            HistoryMessages
                        };

                        await SendStringAsync(senderSocket, JsonSerializer.Serialize(historyResponse));
                        break;

                    case "Chat":
                        ChatMessageClass? chatMessage = root.GetProperty("ChatMessage").Deserialize<ChatMessageClass>();
                        if (chatMessage != null)
                        {
                            if (MessageRecordingsManager == null) { break; }
                            chatMessage.Index = MessageRecordingsManager.GetMessagesCount();

                            // 添加到历史记录
                            MessageRecordingsManager.AppendMessageRecording(chatMessage);

                            // 广播给所有客户端
                            await BroadcastMessage(chatMessage);
                            if (MCServerManager.isMCServerRunning)
                            {
                                MCServerManager.SendCommandHL($"tellraw @a \"[消息互通][{chatMessage.PlayerName}({chatMessage.PlayerRole})]:{chatMessage.Message}\"");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ReportLog("错误", "实时服内外通信和远程服务器管理器", $"发生异常：{ex.Message}");
            }
        }

        private async Task BroadcastMessage(ChatMessageClass message)
        {
            var messageObj = new
            {
                Type = "Chat",
                ChatMessage = message
            };

            string json = JsonSerializer.Serialize(messageObj);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            List<WebSocket> clientsCopy;
            lock (WebSocketClients)
            {
                clientsCopy = [.. WebSocketClients];
            }

            foreach (var client in clientsCopy)
            {
                if (client.State == WebSocketState.Open)
                {
                    try
                    {
                        if (CancellationTokenSource == null) { break; }
                        await client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationTokenSource.Token);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private async Task SendStringAsync(WebSocket socket, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            if (CancellationTokenSource == null) { return; }
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationTokenSource.Token);
        }


        [GeneratedRegex(@"<([^>]+)> (.+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
        private static partial Regex MessageRegex();
        private void HandleServerMessage(string _, string Log)
        {
            var match = MessageRegex().Match(Log);
            if (match.Success)
            {
                string playerName = match.Groups[1].Value;
                string message = match.Groups[2].Value;
                string playerRole = "玩家";
                foreach (var item in MCServerManager.MCServerManagerConfig.OnlineChattingSystemConfig.PlayerAccountList)
                {
                    if (item.PlayerName == playerName)
                    {
                        playerRole = item.PlayerRole;
                    }
                }
                if (MessageRecordingsManager == null) { return; }
                ChatMessageClass chatMessage = new(MessageRecordingsManager.GetMessagesCount(), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), playerName, playerRole.ToString(), message);
                MessageRecordingsManager.AppendMessageRecording(chatMessage);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                BroadcastMessage(chatMessage);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
        }


        internal void Dispose()
        {
            MessageRecordingsManager?.Dispose();
            CancellationTokenSource?.Cancel();
            HttpListener?.Stop();
            HttpListener?.Close();
            ReportLog = delegate { };
        }
    }
    internal class ChatMessageClass
    {
        [JsonConstructor]
        internal ChatMessageClass(int Index,
                                    string SendTime,
                                    string PlayerName,
                                    string PlayerRole,
                                    string Message)
        {
            this.Index = Index;
            this.SendTime = SendTime;
            this.PlayerName = PlayerName;
            this.PlayerRole = PlayerRole;
            this.Message = Message;
        }
        [JsonInclude]
        internal int Index { get; set; }
        [JsonInclude]
        internal string SendTime { get; set; }
        [JsonInclude]
        internal string PlayerName { get; set; }
        [JsonInclude]
        internal string PlayerRole { get; set; }
        [JsonInclude]
        internal string Message { get; set; }
    }

}




