using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using PMCSsE_Frontend_AvaloniaUI.Controls;
using PMCSsE_Frontend_AvaloniaUI.Models;
using PMCSsE_Frontend_AvaloniaUI.Modules;
using PMCSsE_Frontend_AvaloniaUI.Views;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;

namespace PMCSsE_Frontend_AvaloniaUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IImage ConnectedImage = new Bitmap(AssetLoader.Open(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/Link.png")));
        private readonly IImage DisconnectedImage = new Bitmap(AssetLoader.Open(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/Unlink.png")));
        #region prop
        public int FirstTabControlPage
        {
            get => _firstTabControlPage;
            set => this.RaiseAndSetIfChanged(ref _firstTabControlPage, value);
        }
        private int _firstTabControlPage = 2;//

        public ObservableCollection<MessageModel> Messages_IS
        {
            get => _messages_IS;
            set => this.RaiseAndSetIfChanged(ref _messages_IS, value);
        }
        private ObservableCollection<MessageModel> _messages_IS = [];
        public IImage? ConnectionStateImage
        {
            get => _connectionStateImage;
            set => this.RaiseAndSetIfChanged(ref _connectionStateImage, value);
        }
        private IImage? _connectionStateImage;
        public int ConnectPageIndex
        {
            get => _connectPageIndex;
            set => this.RaiseAndSetIfChanged(ref _connectPageIndex, value);
        }
        private int _connectPageIndex = 0;
        public string? DisplayingIP
        {
            get => _displayingIP;
            set => this.RaiseAndSetIfChanged(ref _displayingIP, value);
        }
        private string? _displayingIP = string.Empty;
        public string? DisplayingPort
        {
            get => _displayingPort;
            set => this.RaiseAndSetIfChanged(ref _displayingPort, value);
        }
        private string? _displayingPort = string.Empty;
        public string? DisplayingPassword
        {
            get => _displayingPassword;
            set => this.RaiseAndSetIfChanged(ref _displayingPassword, value);
        }
        private string? _displayingPassword = string.Empty;
        public ObservableCollection<ConnectHistory_LBItemModel> Histories_IS
        {
            get => _histories_IS;
            set => this.RaiseAndSetIfChanged(ref _histories_IS, value);
        }
        private ObservableCollection<ConnectHistory_LBItemModel> _histories_IS = [];
        public string? ConnectLogs
        {
            get => _connectLogs;
            set => this.RaiseAndSetIfChanged(ref _connectLogs, value);
        }
        private string? _connectLogs = string.Empty;
        public bool? PasswordEnterControlVisibility
        {
            get => _passwordEnterControlVisibility;
            set => this.RaiseAndSetIfChanged(ref _passwordEnterControlVisibility, value);
        }
        private bool? _passwordEnterControlVisibility = false;
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        private string? _password = string.Empty;
        public ObservableCollection<MCServerManagerConfig_LBItemModel> AllMCServerManagers_IS//item source
        {
            get => _allMCServerManagers_IS;
            set => this.RaiseAndSetIfChanged(ref _allMCServerManagers_IS, value);
        }
        private ObservableCollection<MCServerManagerConfig_LBItemModel> _allMCServerManagers_IS = [];

        public object? SelectedMCServerManagerConfig_LBItem
        {
            get => _selectedMCServerManagerConfig_LBItem;
            set => this.RaiseAndSetIfChanged(ref _selectedMCServerManagerConfig_LBItem, value);
        }
        private object? _selectedMCServerManagerConfig_LBItem;
        public ObservableCollection<MCServerManager_LBItemModel> LoadedMCServerManagers_IS//item source
        {
            get => _loadedMCServerManagers_IS;
            set => this.RaiseAndSetIfChanged(ref _loadedMCServerManagers_IS, value);
        }
        private ObservableCollection<MCServerManager_LBItemModel> _loadedMCServerManagers_IS = [];

        public object? SelectedMCServerManager_LBItem
        {
            get => _selectedMCServerManager_LBItem;
            set => this.RaiseAndSetIfChanged(ref _selectedMCServerManager_LBItem, value);
        }
        private object? _selectedMCServerManager_LBItem;

        #endregion
        #region 命令绑定
        private readonly BehaviorSubject<bool> _canCleanMessage = new(false);
        public ReactiveCommand<Unit, Unit> CleanMessageCommand { get; }

        private readonly BehaviorSubject<bool> _canAddNewBackend = new(true);
        public ReactiveCommand<Unit, Unit> AddNewBackendCommand { get; }


        private readonly BehaviorSubject<bool> _canConnect = new(true);
        public ReactiveCommand<NativeServerHistory, Unit> ConnectCommand { get; }


        private readonly BehaviorSubject<bool> _canVerify = new(false);
        public ReactiveCommand<Unit, Unit> VerifyCommand { get; }


        private readonly BehaviorSubject<bool> _canSendPassword = new(false);
        public ReactiveCommand<Unit, Unit> SendPasswordCommand { get; }


        private readonly BehaviorSubject<bool> _canDisconnect = new(false);
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; }


        private readonly BehaviorSubject<bool> _canRefreshAllMCServerManagerList = new(true);
        public ReactiveCommand<Unit, Unit> RefreshAllMCServerManagerListCommand { get; }


        private readonly BehaviorSubject<bool> _canCreateNewMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> CreateNewMCServerManagerCommand { get; }


        private readonly BehaviorSubject<bool> _canLoadNewMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> LoadNewMCServerManagerCommand { get; }


        private readonly BehaviorSubject<bool> _canStopNewMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> StopNewMCServerManagerCommand { get; }


        private readonly BehaviorSubject<bool> _canDeleteMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> DeleteMCServerManagerCommand { get; }


        private readonly BehaviorSubject<bool> _canRefreshLoadedMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> RefreshLoadedMCServerManagerCommand { get; }


        private readonly BehaviorSubject<bool> _canOpenSelectedMCServerManager = new(true);
        public ReactiveCommand<Unit, Unit> OpenSelectedMCServerManagerCommand { get; }
        #endregion
        public MainViewModel()
        {
            string error = StaticConfigManagerClass.LoadConfig();
            if (error != string.Empty)
            {
                SendMessage(error, 2);
            }
            foreach (var item in StaticAPPConfigClass.NativeServerHistories)
            {
                ConnectHistory_LBItemModel model = new(item);
                Histories_IS.Add(model);
            }
            var uiScheduler = AvaloniaScheduler.Instance;
            CleanMessageCommand = ReactiveCommand.Create(CleanMessageAction, _canCleanMessage, uiScheduler);
            AddNewBackendCommand = ReactiveCommand.Create(AddNewBackendAction, _canAddNewBackend, uiScheduler);
            ConnectCommand = ReactiveCommand.Create<NativeServerHistory>(ConnectAction, _canConnect, uiScheduler);
            VerifyCommand = ReactiveCommand.Create(VerifyAction, _canVerify, uiScheduler);
            SendPasswordCommand = ReactiveCommand.Create(SendPasswordAction, _canSendPassword, uiScheduler);
            DisconnectCommand = ReactiveCommand.Create(DisconnectAction, _canDisconnect, uiScheduler);
            RefreshAllMCServerManagerListCommand = ReactiveCommand.Create(RefreshAllMCServerManagerListAction, _canRefreshAllMCServerManagerList, uiScheduler);
            CreateNewMCServerManagerCommand = ReactiveCommand.Create(CreateNewMCServerManagerAction, _canCreateNewMCServerManager, uiScheduler);
            LoadNewMCServerManagerCommand = ReactiveCommand.Create(LoadMCServerManagerAction, _canLoadNewMCServerManager, uiScheduler);
            StopNewMCServerManagerCommand = ReactiveCommand.Create(StopMCServerManagerAction, _canStopNewMCServerManager, uiScheduler);
            DeleteMCServerManagerCommand = ReactiveCommand.Create(DeleteMCServerManagerAction, _canDeleteMCServerManager, uiScheduler);
            RefreshLoadedMCServerManagerCommand = ReactiveCommand.Create(RefreshLoadedMCServerManagerAction, _canRefreshLoadedMCServerManager, uiScheduler);
            OpenSelectedMCServerManagerCommand = ReactiveCommand.Create(OpenSelectedMCServerManagerAction, _canOpenSelectedMCServerManager, uiScheduler);
            ExitManagerPanelCommand = ReactiveCommand.Create(ExitManagerPanelAction, _canExitManagerPanel, uiScheduler);
            ConnectionStateImage = DisconnectedImage;
        }

        #region 命令
        public void CleanMessageAction()
        {
            Messages_IS.Clear();
            _canCleanMessage.OnNext(false);
        }
        public void AddNewBackendAction()
        {
            if (DisplayingIP == null || DisplayingPassword == null || DisplayingPort == null) return;
            if (DisplayingIP == string.Empty)
            {
                SendMessage($"主机名/IP为空", 2);
                return;
            }
            string iP = DisplayingIP;
            int iPort;
            try
            {
                iPort = Convert.ToInt32(DisplayingPort);
            }
            catch (FormatException)
            {
                SendMessage($"请输入有效的端口号（1~65535）", 2);
                return;
            }
            catch (OverflowException)
            {
                SendMessage($"请输入有效的端口号（1~65535）", 2);
                return;
            }
            catch (Exception ex)
            {
                SendMessage($"文字->int32转换失败{Environment.NewLine}原因:{ex.Message}{Environment.NewLine}堆栈:{ex.StackTrace}", 2);
                return;
            }
            if (iPort < 1 || iPort > 65535)
            {
                SendMessage($"请输入有效的端口号（1~65535）", 2);
                return;
            }
            foreach (var item in StaticAPPConfigClass.NativeServerHistories)
            {
                if (item.IP == iP && item.Port == iPort)
                {
                    SendMessage($"已经存在了", 2);
                    return;
                }
            }
            NativeServerHistory nativeServerHistory = new() { IP = iP, Port = iPort, RSAPublicKeyHash = string.Empty, Password = DisplayingPassword };
            StaticAPPConfigClass.NativeServerHistories.Add(nativeServerHistory);
            StaticConfigManagerClass.SaveAPPConfig();
            ConnectHistory_LBItemModel model = new(nativeServerHistory);
            Histories_IS.Add(model);
            SendMessage($"添加成功", 3);
            DisplayingIP = string.Empty;
            DisplayingPort = string.Empty;
            DisplayingPassword = string.Empty;
            return;
        }

        private NativeClient? nativeClient;
        private NativeServerHistory? ConnectingNativeServer;
        public void ConnectAction(NativeServerHistory nsh)
        {
            ConnectingNativeServer = nsh;
            nativeClient = new(ConnectingNativeServer.IP, ConnectingNativeServer.Port);
            nativeClient.ReportLog += HandleLog;
            nativeClient.NeedToVerifyRSAPublicKey += HandleVerifyRSAPublicKey;
            nativeClient.NeedPassword += HandleNeedPasswordEvent;
            nativeClient.Connected += HandleConnected;
            nativeClient.Disconnected += HandleDisconnected;
            _canConnect.OnNext(false);
            _canVerify.OnNext(false);
            _canSendPassword.OnNext(false);
            _canDisconnect.OnNext(true);

            nativeClient.DataPackBus.Subscribe<Pack_MCServerManagerConfigs>(HandlePack_MCServerManagerConfigs);

            nativeClient.DataPackBus.Subscribe<Pack_CreatedNewMCServerManager>(HandlePack_CreatedNewMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_CreatNewMCServerManagerFailed>(HandlePack_CreatNewMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_LoadedMCServerManager>(HandlePack_LoadedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_LoadMCServerManagerFailed>(HandlePack_LoadMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_StoppedMCServerManager>(HandlePack_StoppedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_StopMCServerManagerFailed>(HandlePack_StopMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_DeletedMCServerManager>(HandlePack_DeletedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_DeleteMCServerManagerFailed>(HandlePack_DeleteMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_MCServerManagers>(HandlePack_MCServerManagers);
            nativeClient.DataPackBus.Subscribe<Pack_ErrorInfo>(HandlePack_ErrorInfo);
            nativeClient.Connect();
            ConnectPageIndex = 2;
        }
        public void VerifyAction()
        {
            if (nativeClient == null) return;
            nativeClient.IsRSAPublicKeyRight = true;
            StaticConfigManagerClass.SaveAPPConfig();
            _canVerify.OnNext(false);
        }
        public void SendPasswordAction()
        {
            if (Password == null)
            {
                ConnectLogs += $"null!!!{Environment.NewLine}";
                return;
            }
            _canSendPassword.OnNext(false);
            PasswordEnterControlVisibility = false;
            nativeClient!.TypePassword(Password);
            Password = string.Empty;
        }
        public void DisconnectAction()
        {
            if (ConnectingNativeServer == null) return;
            if (ConnectionStateImage == DisconnectedImage)//Connecting
            {
                ConnectingNativeServer.RSAPublicKeyHash = string.Empty;
                StaticConfigManagerClass.SaveAPPConfig();
            }
            nativeClient?.Disconnect();
            _canConnect.OnNext(true);
            _canVerify.OnNext(false);
            _canSendPassword.OnNext(false);
            _canDisconnect.OnNext(false);
            PasswordEnterControlVisibility = false;
        }
        public void RefreshAllMCServerManagerListAction()
        {
            if (nativeClient == null)
            {
                FirstTabControlPage = 0;
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.GetMCServerManagersList, new Pack_GetMCServerManagerConfigsList());
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage("已发送获取服务端管理器列表请求", 0);
            }, null);
        }
        public void CreateNewMCServerManagerAction()
        {
            if (nativeClient == null)
            {
                FirstTabControlPage = 0;
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.CreatNewMCServerManager, new Pack_CreatNewMCServerManager());
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage("已发送新建服务端管理器列表请求", 0);
            }, null);

        }
        public void LoadMCServerManagerAction()
        {
            if (SelectedMCServerManagerConfig_LBItem == null)
            {
                SendMessage("未选择要启动的MC服务端管理器", 2);
                return;
            }
            if (nativeClient == null)
            {
                SendMessage("未连接到后端", 2);
                return;
            }
            if (SelectedMCServerManagerConfig_LBItem is MCServerManagerConfig_LBItemModel m)
            {
                nativeClient.RequestBackend(RequestTypeEnum.LoadMCServerManager, new Pack_LoadMCServerManager(m.ManagerID));
                SendMessage("已发送启动MC服务端管理器请求", 0);
            }
        }
        public void StopMCServerManagerAction()
        {
            if (SelectedMCServerManagerConfig_LBItem == null)
            {
                SendMessage("未选择要停止的MC服务端管理器", 2);
                return;
            }
            if (nativeClient == null)
            {
                SendMessage("未连接到后端", 2);
                return;
            }
            if (SelectedMCServerManagerConfig_LBItem is MCServerManagerConfig_LBItemModel m)
            {
                nativeClient.RequestBackend(RequestTypeEnum.StopMCServerManager, new Pack_StopMCServerManager(m.ManagerID));
                SendMessage("已发送停止MC服务端管理器请求", 0);
            }
        }
        public void DeleteMCServerManagerAction()
        {
            if (SelectedMCServerManagerConfig_LBItem == null)
            {
                SendMessage("未选择要删除的MC服务端管理器", 2);
                return;
            }
            if (nativeClient == null)
            {
                SendMessage("未连接到后端", 2);
                return;
            }
            if (SelectedMCServerManagerConfig_LBItem is MCServerManagerConfig_LBItemModel m)
            {
                AllMCServerManagers_IS.Remove(m);
                nativeClient.RequestBackend(RequestTypeEnum.DeleteMCServerManager, new Pack_DeleteMCServerManager(m.ManagerID));
                SendMessage("已发送删除MC服务端管理器请求", 0);

            }
        }
        public void RefreshLoadedMCServerManagerAction()
        {
            if (nativeClient == null)
            {
                SendMessage("未连接到后端", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.GetLoadedMCServerManagers, new Pack_GetMCServerManager());

            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage("已发送获取已加载的MC服务端管理器请求", 0);
            }, null);
        }
        #endregion
        #region nativeclient事件处理
        private void HandlePack_MCServerManagerConfigs(Pack_MCServerManagerConfigs pack)
        {
            StaticMCServerManagerConfigs.ConfigVersion = pack.MCServerManagerConfigs.ConfigVersion;
            StaticMCServerManagerConfigs.MCServerManagerConfigsList = pack.MCServerManagerConfigs.MCServerManagerConfigsList;

            Dispatcher.UIThread.Post((state) =>
            {
                AllMCServerManagers_IS.Clear();
                foreach (MCServerManagerConfig mCServerManagerConfig in StaticMCServerManagerConfigs.MCServerManagerConfigsList)
                {
                    AllMCServerManagers_IS.Add(new MCServerManagerConfig_LBItemModel(mCServerManagerConfig));
                }
                SendMessage("加载服务端管理器列表成功", 3);
            }, null);
        }
        private void HandlePack_CreatedNewMCServerManager(Pack_CreatedNewMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                AllMCServerManagers_IS.Add(new MCServerManagerConfig_LBItemModel(pack.MCServerManagerConfig));
                SendMessage("新建服务端管理器成功", 3);
            }, null);
        }
        private void HandlePack_CreatNewMCServerManagerFailed(Pack_CreatNewMCServerManagerFailed _)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"新建服务端管理器失败", 2);
            }, null);
        }
        private void HandlePack_LoadedMCServerManager(Pack_LoadedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_LoadMCServerManagerFailed(Pack_LoadMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_StoppedMCServerManager(Pack_StoppedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_StopMCServerManagerFailed(Pack_StopMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_DeletedMCServerManager(Pack_DeletedMCServerManager pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"删除服务端管理器[{pack.ManagerID}]成功", 3);
            }, null);
        }
        private void HandlePack_DeleteMCServerManagerFailed(Pack_DeleteMCServerManagerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"删除服务端管理器[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_MCServerManagers(Pack_MCServerManagers pack)
        {
            LoadedMCServerManagers_IS.Clear();
            pack.MCServerManagersData.MCServerManagerDataList?.ForEach((item) =>//效率低，以后再改
                {
                    foreach (var item1 in AllMCServerManagers_IS)
                    {
                        if (item.ManagerID == item1.ManagerID)
                        {
                            LoadedMCServerManagers_IS.Add(new MCServerManager_LBItemModel(item1.Config, item));
                            return;
                        }
                    }
                });

            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"刷新已加载的服务端管理器成功", 3);
            }, null);
        }
        private void HandlePack_ErrorInfo(Pack_ErrorInfo pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"错误信息：{pack.ErrorInfo}", 2);
            }, null);
        }
        private void HandleVerifyRSAPublicKey(string rsa)
        {
            if (ConnectingNativeServer == null || nativeClient == null) { return; }
            if (rsa == ConnectingNativeServer.RSAPublicKeyHash)
            {
                nativeClient.IsRSAPublicKeyRight = true;
            }
            else
            {
                if (ConnectingNativeServer.RSAPublicKeyHash != string.Empty)
                {
                    Dispatcher.UIThread.Post((state) =>
                    {
                        ConnectLogs += $"密钥指纹与记忆的不一致，原因可能为：重启了后端、更换了后端端口、中间人攻击{Environment.NewLine}";
                    }, null);
                }
                Dispatcher.UIThread.Post((state) =>
                {
                    ConnectLogs += $"请比对与RSA公钥指纹是否与原生服务器显示的一致(限时60s){Environment.NewLine}{rsa}{Environment.NewLine}";
                    _canVerify.OnNext(true);
                }, null);
            }
            ConnectingNativeServer.RSAPublicKeyHash = rsa;
            nativeClient.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
        }
        private void HandleDisconnected(NativeClient.DisconnectedReasonEnum nativeClientDisconnectedReasons)
        {
            nativeClient!.ReportLog -= HandleLog;
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
            nativeClient!.Connected -= HandleConnected;
            nativeClient.Disconnected -= HandleDisconnected;
            string reason;
            switch (nativeClientDisconnectedReasons)
            {
                case NativeClient.DisconnectedReasonEnum.DisconnectingCalledByToken:
                    reason = "令牌被取消";
                    break;
                case NativeClient.DisconnectedReasonEnum.SocketException:
                    reason = "发生了Socket错误，请根据代码上网查阅有关资料";
                    break;
                case NativeClient.DisconnectedReasonEnum.InvalidConnectionParameter:
                    reason = "填写了不规范的IP地址或端口";
                    break;
                case NativeClient.DisconnectedReasonEnum.OutOfMemory:
                    reason = "内存严重不足";
                    break;
                case NativeClient.DisconnectedReasonEnum.UnknownPackFormat:
                    reason = "不匹配的客户端版本";
                    break;
                case NativeClient.DisconnectedReasonEnum.VerifyRSAPublicKeyTimeOut:
                    ConnectingNativeServer?.RSAPublicKeyHash = "";
                    reason = "验证超时";
                    break;
                case NativeClient.DisconnectedReasonEnum.RSAPublicKeyMismatch:
                    reason = "遭遇中间人攻击，RSA公钥指纹不匹配";
                    break;
                case NativeClient.DisconnectedReasonEnum.PasswordMismatch:
                    reason = "密码错误";
                    break;
                case NativeClient.DisconnectedReasonEnum.ConnectionUnexpectlyDisconnected:
                    reason = "意外断开了网络连接";
                    break;
                case NativeClient.DisconnectedReasonEnum.GenerateDataPackFailed:
                    reason = "构造数据包失败";
                    break;
                case NativeClient.DisconnectedReasonEnum.EncoderFallBack:
                    reason = "UTF8编/解码失败";
                    break;
                default:
                    reason = "未知原因，通常因为客户端版本不匹配导致";
                    break;
            }
            Dispatcher.UIThread.Post((state) =>
            {
                ConnectLogs += "连接断开:" + reason + Environment.NewLine;
                ConnectionStateImage = DisconnectedImage;
                _canConnect.OnNext(true);
                _canVerify.OnNext(false);
                _canSendPassword.OnNext(false);
                _canDisconnect.OnNext(false);
            }, null);
            ConnectingNativeServer = null;

            nativeClient.DataPackBus.Unsubscribe<Pack_MCServerManagerConfigs>(HandlePack_MCServerManagerConfigs);

            nativeClient.DataPackBus.Unsubscribe<Pack_CreatedNewMCServerManager>(HandlePack_CreatedNewMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_CreatNewMCServerManagerFailed>(HandlePack_CreatNewMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_LoadedMCServerManager>(HandlePack_LoadedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_LoadMCServerManagerFailed>(HandlePack_LoadMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_StoppedMCServerManager>(HandlePack_StoppedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_StopMCServerManagerFailed>(HandlePack_StopMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_DeletedMCServerManager>(HandlePack_DeletedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_DeleteMCServerManagerFailed>(HandlePack_DeleteMCServerManagerFailed);
            nativeClient.DataPackBus.Unsubscribe<Pack_ErrorInfo>(HandlePack_ErrorInfo);
            nativeClient.Dispose();
            nativeClient = null;
        }

        private void HandleConnected()
        {
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
            nativeClient!.Connected -= HandleConnected;
            RefreshAllMCServerManagerListAction();
            RefreshLoadedMCServerManagerAction();
            Dispatcher.UIThread.Post((state) =>
            {
                ConnectLogs += "连接成功" + Environment.NewLine;
                ConnectionStateImage = ConnectedImage;
                _canConnect.OnNext(false);
                _canVerify.OnNext(false);
                _canSendPassword.OnNext(false);
                _canDisconnect.OnNext(true);
                SendMessage("连接后端成功", 3);
                SendMessage("已发送获取服务端管理器列表请求", 0);
            }, null);
        }

        private void HandleNeedPasswordEvent()
        {
            if (ConnectingNativeServer == null) return;
            if (ConnectingNativeServer.Password != string.Empty)
            {
                nativeClient!.TypePassword(ConnectingNativeServer.Password);
            }
            else
            {
                Dispatcher.UIThread.Post((state) =>
                {
                    ConnectLogs += $"未填写访问密钥，请输入{Environment.NewLine}";
                    PasswordEnterControlVisibility = true;
                    _canSendPassword.OnNext(true);
                }, null);
            }
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
        }

        private void HandleLog(string log)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                ConnectLogs += (log + Environment.NewLine);
            }, null);
        }
        #endregion
        #region 供CHLBI调用
        internal void DeleteConnectHistory_LBItemModel(ConnectHistory_LBItemModel m)
        {
            StaticAPPConfigClass.NativeServerHistories.Remove(m.MyNativeServerHistory);
            StaticConfigManagerClass.SaveAPPConfig();
            Histories_IS.Remove(m);
        }
        #endregion
        #region 消息
        public void SendMessage(string m, byte level)
        {
            Messages_IS.Add(new MessageModel(m, level));
            _canCleanMessage.OnNext(true);
        }
        public void DeleteMessage(MessageModel m)
        {
            Messages_IS.Remove(m);
        }
        #endregion
        #region MCServerManager叠加层

        private readonly BehaviorSubject<bool> _canExitManagerPanel = new(true);
        public ReactiveCommand<Unit, Unit> ExitManagerPanelCommand { get; }
        public bool MCServerManagerPanelVisibility
        {
            get => _mCServerManagerPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _mCServerManagerPanelVisibility, value);
        }
        private bool _mCServerManagerPanelVisibility = true;
        public string? MCServerName
        {
            get => _mCServerName;
            set => this.RaiseAndSetIfChanged(ref _mCServerName, value);
        }
        private string? _mCServerName = string.Empty;
        private MCServerManager_LBItemModel UsingManager;

        public void OpenSelectedMCServerManagerAction()
        {
            if (SelectedMCServerManager_LBItem is MCServerManager_LBItemModel m)
            {
                UsingManager = m;
                MCServerManagerPanelVisibility = true;

            }
        }
        private void ExitManagerPanelAction()
        {
            MCServerManagerPanelVisibility = false;
        }
        #endregion
    }
}
