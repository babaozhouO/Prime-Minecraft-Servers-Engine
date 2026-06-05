using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using PMCSsE_Communicator;
using PMCSsE_Communicator.DataPacks;
using PMCSsE_Communicator.DataPacks.Pack_nothing;
using PMCSsE_Communicator.DataPacks.Pack_StringOnly;
using PMCSsE_Frontend_AvaloniaUI.Models;
using PMCSsE_Frontend_AvaloniaUI.Modules;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection.Metadata;
using System.Text;

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
        private int _firstTabControlPage = 0;//

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
        private int _connectPageIndex = 1;
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
            ChangePageCommand = ReactiveCommand.Create<string>(ChangePageAction, _canChangePage, uiScheduler);
            RunMCServerCommand = ReactiveCommand.Create(RunMCServerAction, _canRunMCServer, uiScheduler);
            SendCommandCommand = ReactiveCommand.Create(SendCommandAction, _canSendCommand, uiScheduler);
            StopMCServerCommand = ReactiveCommand.Create(StopMCServerAction, _canStopMCServer, uiScheduler);
            KillMCServerCommand = ReactiveCommand.Create(KillMCServerAction, _canKillMCServer, uiScheduler);
            EditConfigCommand = ReactiveCommand.Create(EditConfigAction, _canEditConfig, uiScheduler);
            CommitConfigCommand = ReactiveCommand.Create(CommitConfigAction, _canCommitConfig, uiScheduler);
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
            nativeClient.DataPackBus.Subscribe<Pack_MCServerManagers>(HandlePack_MCServerManagers);
            nativeClient.DataPackBus.Subscribe<Pack_SupportedMCServerTypes>(HandlePack_SupportedMCServerTypes);

            nativeClient.DataPackBus.Subscribe<Pack_CreatedNewMCServerManager>(HandlePack_CreatedNewMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_CreatNewMCServerManagerFailed>(HandlePack_CreatNewMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_LoadedMCServerManager>(HandlePack_LoadedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_LoadMCServerManagerFailed>(HandlePack_LoadMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_StoppedMCServerManager>(HandlePack_StoppedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_StopMCServerManagerFailed>(HandlePack_StopMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_DeletedMCServerManager>(HandlePack_DeletedMCServerManager);
            nativeClient.DataPackBus.Subscribe<Pack_DeleteMCServerManagerFailed>(HandlePack_DeleteMCServerManagerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_ModifiedMCServerManagerConfig>(HandlePack_ModifiedMCServerManagerConfig);

            nativeClient.DataPackBus.Subscribe<Pack_RunMCServerSucceed>(HandlePack_RunMCServerSucceed);
            nativeClient.DataPackBus.Subscribe<Pack_RunMCServerFailed>(HandlePack_RunMCServerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_SendCommandSucceed>(HandlePack_SendCommandSucceed);
            nativeClient.DataPackBus.Subscribe<Pack_SendCommandFailed>(HandlePack_SendCommandFailed);

            nativeClient.DataPackBus.Subscribe<Pack_ShutdownMCServerSucceed>(HandlePack_ShutdownMCServerSucceed);
            nativeClient.DataPackBus.Subscribe<Pack_ShutdownMCServerFailed>(HandlePack_ShutdownMCServerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_KillMCServerSucceed>(HandlePack_KillMCServerSucceed);
            nativeClient.DataPackBus.Subscribe<Pack_KillMCServerFailed>(HandlePack_KillMCServerFailed);

            nativeClient.DataPackBus.Subscribe<Pack_MCServerLogs>(HandlePack_MCServerLogs);

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
        private void HandlePack_MCServerManagers(Pack_MCServerManagers pack)
        {
            LoadedMCServerManagers_IS.Clear();
            pack.MCServerManagersData.MCServerManagerDataList?.ForEach((md) =>
                {
                    var mc = AllMCServerManagers_IS.FirstOrDefault(mc => mc.ManagerID == md.ManagerID);
                    if (mc == null) return;
                    LoadedMCServerManagers_IS.Add(new MCServerManager_LBItemModel(mc.Config, md));
                    return;
                });

            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"刷新已加载的服务端管理器成功", 3);
            }, null);
        }
        private void HandlePack_SupportedMCServerTypes(Pack_SupportedMCServerTypes pack)
        {
            SupportedMCServerTypes_IS.Clear();
            foreach (var type in pack.SupportedMCServerTypes)
            {
                SupportedMCServerTypes_IS.Add(type);
            }
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage("获取支持的服务端类型成功", 3);
            }, null);
        }
        private void HandlePack_CreatedNewMCServerManager(Pack_CreatedNewMCServerManager pack)
        {
            AllMCServerManagers_IS.Add(new MCServerManagerConfig_LBItemModel(pack.MCServerManagerConfig));//此集合无Sort()
            Dispatcher.UIThread.Post((state) =>
            {
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
        private void HandlePack_ModifiedMCServerManagerConfig(Pack_ModifiedMCServerManagerConfig pack)
        {
            var mc = LoadedMCServerManagers_IS.FirstOrDefault(mc => mc.ManagerID == pack.MCServerManagerConfig.ManagerID);
            if (mc == null)
            {
                Dispatcher.UIThread.Post((state) =>
                {
                    SendMessage($"修改服务端管理器[{pack.MCServerManagerConfig.ManagerID}]的配置成功但同步失败", 2);
                }, null);
                return;
            }
            mc.Config.MCServerName = pack.MCServerManagerConfig.MCServerName;
            mc.Config.MCServerType = pack.MCServerManagerConfig.MCServerType;
            mc.Config.MCServerDirectory = pack.MCServerManagerConfig.MCServerDirectory;
            mc.Config.JavaPath = pack.MCServerManagerConfig.JavaPath;
            mc.Config.StartUpArguments = pack.MCServerManagerConfig.StartUpArguments;
            mc.Config.BackupManagerConfig = pack.MCServerManagerConfig.BackupManagerConfig;
            mc.Config.OnlineChattingSystemConfig = pack.MCServerManagerConfig.OnlineChattingSystemConfig;
            MCServerName = UsingManager?.Config.MCServerName;
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"修改服务端管理器[{pack.MCServerManagerConfig.ManagerID}]的配置成功", 3);
            }, null);
        }
        private void HandlePack_RunMCServerSucceed(Pack_RunMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端[{pack.ManagerID}]成功", 3);
                if (UsingManager?.ManagerID == pack.ManagerID)
                {
                    UsingManager.State.IsMCServerRunning = true;
                    _canRunMCServer.OnNext(false);
                    _canSendCommand.OnNext(true);
                    _canStopMCServer.OnNext(true);
                    _canKillMCServer.OnNext(true);
                }
            }, null);
        }
        private void HandlePack_RunMCServerFailed(Pack_RunMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"启动服务端[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_SendCommandSucceed(Pack_SendCommandSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"向服务端[{pack.ManagerID}]发送命令成功", 3);
            }, null);
        }
        private void HandlePack_SendCommandFailed(Pack_SendCommandFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"向服务端[{pack.ManagerID}]发送命令失败", 2);
            }, null);
        }
        private void HandlePack_ShutdownMCServerSucceed(Pack_ShutdownMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端[{pack.ManagerID}]成功", 3);
                if (UsingManager?.ManagerID == pack.ManagerID)
                {
                    UsingManager.State.IsMCServerRunning = false;
                    _canRunMCServer.OnNext(true);
                    _canSendCommand.OnNext(false);
                    _canStopMCServer.OnNext(false);
                    _canKillMCServer.OnNext(false);
                }
            }, null);
        }
        private void HandlePack_ShutdownMCServerFailed(Pack_ShutdownMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"停止服务端[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private void HandlePack_KillMCServerSucceed(Pack_KillMCServerSucceed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"强制终止服务端[{pack.ManagerID}]成功", 3);
                if (UsingManager?.ManagerID == pack.ManagerID)
                {
                    UsingManager.State.IsMCServerRunning = false;
                    _canRunMCServer.OnNext(true);
                    _canSendCommand.OnNext(false);
                    _canStopMCServer.OnNext(false);
                    _canKillMCServer.OnNext(false);
                }
            }, null);
        }
        private void HandlePack_KillMCServerFailed(Pack_KillMCServerFailed pack)
        {
            Dispatcher.UIThread.Post((state) =>
            {
                SendMessage($"强制终止服务端[{pack.ManagerID}]失败", 2);
            }, null);
        }
        private ulong OldestLogID = 0;
        private ulong LatestLogID = 0;
        private void HandlePack_MCServerLogs(Pack_MCServerLogs pack)
        {
            if (UsingManager == null)
            {
                return;
            }
            if (pack.ManagerID != UsingManager.ManagerID)
            {
                return;
            }
            if (pack.ResetHint)
            {
                Dispatcher.UIThread.Post((state) =>
                {
                    ServerLogsDocument = new()
                    {
                        FileName = "source.log"
                    };
                    OldestLogID = 0;
                    LatestLogID = 0;
                }, null);
            }
            if (pack.Logs == null)
            {
                GetNewerLogsTimer.Start();
                return;
            }
            if (pack.Logs.Count == 0)
            {
                GetNewerLogsTimer.Start();
                return;
            }
            if (pack.Logs[^1].ID > LatestLogID)
            {
                LatestLogID = pack.Logs[^1].ID;
            }
            if (pack.Logs[0].ID < OldestLogID)
            {
                OldestLogID = pack.Logs[0].ID;
            }
            StringBuilder sb = new();
            foreach (var log in pack.Logs)
            {
                sb.AppendLine(log.Log);
            }
            string logs = sb.ToString();
            Dispatcher.UIThread.Post((state) =>
            {
                ServerLogsDocument.BeginUpdate();

                ServerLogsDocument.Insert(ServerLogsDocument.TextLength, logs);

                if (ServerLogsDocument.LineCount > 30000)
                {
                    int linesToRemove = ServerLogsDocument.LineCount - 30000;
                    var firstLineToKeep = ServerLogsDocument.GetLineByNumber(linesToRemove + 1);
                    int removeEndOffset = firstLineToKeep.Offset;
                    ServerLogsDocument.Remove(0, removeEndOffset);
                }

                ServerLogsDocument.EndUpdate();
                GetNewerLogsTimer.Start();
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
            nativeClient.DataPackBus.Unsubscribe<Pack_MCServerManagers>(HandlePack_MCServerManagers);
            nativeClient.DataPackBus.Unsubscribe<Pack_SupportedMCServerTypes>(HandlePack_SupportedMCServerTypes);

            nativeClient.DataPackBus.Unsubscribe<Pack_CreatedNewMCServerManager>(HandlePack_CreatedNewMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_CreatNewMCServerManagerFailed>(HandlePack_CreatNewMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_LoadedMCServerManager>(HandlePack_LoadedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_LoadMCServerManagerFailed>(HandlePack_LoadMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_StoppedMCServerManager>(HandlePack_StoppedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_StopMCServerManagerFailed>(HandlePack_StopMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_DeletedMCServerManager>(HandlePack_DeletedMCServerManager);
            nativeClient.DataPackBus.Unsubscribe<Pack_DeleteMCServerManagerFailed>(HandlePack_DeleteMCServerManagerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_ModifiedMCServerManagerConfig>(HandlePack_ModifiedMCServerManagerConfig);

            nativeClient.DataPackBus.Unsubscribe<Pack_RunMCServerSucceed>(HandlePack_RunMCServerSucceed);
            nativeClient.DataPackBus.Unsubscribe<Pack_RunMCServerFailed>(HandlePack_RunMCServerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_SendCommandSucceed>(HandlePack_SendCommandSucceed);
            nativeClient.DataPackBus.Unsubscribe<Pack_SendCommandFailed>(HandlePack_SendCommandFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_ShutdownMCServerSucceed>(HandlePack_ShutdownMCServerSucceed);
            nativeClient.DataPackBus.Unsubscribe<Pack_ShutdownMCServerFailed>(HandlePack_ShutdownMCServerFailed);

            nativeClient.DataPackBus.Unsubscribe<Pack_KillMCServerSucceed>(HandlePack_KillMCServerSucceed);
            nativeClient.DataPackBus.Unsubscribe<Pack_KillMCServerFailed>(HandlePack_KillMCServerFailed);


            nativeClient.DataPackBus.Unsubscribe<Pack_ErrorInfo>(HandlePack_ErrorInfo);
            nativeClient.Dispose();
            nativeClient = null;
            AllMCServerManagers_IS.Clear();
            LoadedMCServerManagers_IS.Clear();
            SupportedMCServerTypes_IS.Clear();
        }

        private void HandleConnected()
        {
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
            nativeClient!.Connected -= HandleConnected;
            RefreshAllMCServerManagerListAction();
            RefreshLoadedMCServerManagerAction();
            nativeClient.RequestBackend(RequestTypeEnum.GetSupportedMCServerTypes, new Pack_GetSupportedMCServerTypes());
            Dispatcher.UIThread.Post((state) =>
            {
                ConnectLogs += "连接成功" + Environment.NewLine;
                ConnectionStateImage = ConnectedImage;
                _canConnect.OnNext(false);
                _canVerify.OnNext(false);
                _canSendPassword.OnNext(false);
                _canDisconnect.OnNext(true);
                SendMessage("连接后端成功", 3);
                SendMessage("已发送获取所有服务端管理器列表请求", 0);
                SendMessage("已发送获取已加载服务端管理器列表请求", 0);
                SendMessage("已发送获取受支持的MC服务端类型请求", 0);
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

        private readonly BehaviorSubject<bool> _canChangePage = new(true);
        public ReactiveCommand<string, Unit> ChangePageCommand { get; }

        private readonly BehaviorSubject<bool> _canEditConfig = new(true);
        public ReactiveCommand<Unit, Unit> EditConfigCommand { get; }

        private readonly BehaviorSubject<bool> _canCommitConfig = new(false);
        public ReactiveCommand<Unit, Unit> CommitConfigCommand { get; }

        private readonly BehaviorSubject<bool> _canRunMCServer = new(true);
        public ReactiveCommand<Unit, Unit> RunMCServerCommand { get; }

        private readonly BehaviorSubject<bool> _canSendCommand = new(false);
        public ReactiveCommand<Unit, Unit> SendCommandCommand { get; }

        private readonly BehaviorSubject<bool> _canStopMCServer = new(false);
        public ReactiveCommand<Unit, Unit> StopMCServerCommand { get; }

        private readonly BehaviorSubject<bool> _canKillMCServer = new(false);
        public ReactiveCommand<Unit, Unit> KillMCServerCommand { get; }
        public bool MCServerManagerPanelVisibility
        {
            get => _mCServerManagerPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _mCServerManagerPanelVisibility, value);
        }
        private bool _mCServerManagerPanelVisibility = true;
        public bool ConsolePanelVisibility
        {
            get => _consolePanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _consolePanelVisibility, value);
        }
        private bool _consolePanelVisibility = true;
        public bool BackupPanelVisibility
        {
            get => _backupPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _backupPanelVisibility, value);
        }
        private bool _backupPanelVisibility = false;
        public bool OnlineChattingPanelVisibility
        {
            get => _onlineChattingPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _onlineChattingPanelVisibility, value);
        }
        private bool _onlineChattingPanelVisibility = false;
        public bool ExplorerPanelVisibility
        {
            get => _explorerPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _explorerPanelVisibility, value);
        }
        private bool _explorerPanelVisibility = false;
        public bool SettingPanelVisibility
        {
            get => _settingPanelVisibility;
            set => this.RaiseAndSetIfChanged(ref _settingPanelVisibility, value);
        }
        private bool _settingPanelVisibility = false;
        public bool EditingConfig
        {
            get => _editingConfig;
            set => this.RaiseAndSetIfChanged(ref _editingConfig, value);
        }
        private bool _editingConfig = false;
        public string? MCServerName
        {
            get => _mCServerName;
            set => this.RaiseAndSetIfChanged(ref _mCServerName, value);
        }
        private string? _mCServerName = string.Empty;
        public string? MCServerName_Edit
        {
            get => _mCServerName_Edit;
            set => this.RaiseAndSetIfChanged(ref _mCServerName_Edit, value);
        }
        private string? _mCServerName_Edit = string.Empty;
        public string? MCServerType_Edit
        {
            get => _mCServerType_Edit;
            set => this.RaiseAndSetIfChanged(ref _mCServerType_Edit, value);
        }
        private string? _mCServerType_Edit = string.Empty;
        public string? MCServerDirectory_Edit
        {
            get => _mCServerDirectory_Edit;
            set => this.RaiseAndSetIfChanged(ref _mCServerDirectory_Edit, value);
        }
        private string? _mCServerDirectory_Edit = string.Empty;
        public string? JavaPath_Edit
        {
            get => _javaPath_Edit;
            set => this.RaiseAndSetIfChanged(ref _javaPath_Edit, value);
        }
        private string? _javaPath_Edit = string.Empty;
        public string? StartArgument_Edit
        {
            get => _startArgument_Edit;
            set => this.RaiseAndSetIfChanged(ref _startArgument_Edit, value);
        }
        private string? _startArgument_Edit = string.Empty;

        private MCServerManager_LBItemModel? UsingManager;
        public TextDocument ServerLogsDocument
        {
            get => _serverLogsDocument;
            set => this.RaiseAndSetIfChanged(ref _serverLogsDocument, value);
        }
        private TextDocument _serverLogsDocument = new();
        public double ServerLogsAskInterval
        {
            get => _serverLogsAskInterval;
            set => this.RaiseAndSetIfChanged(ref _serverLogsAskInterval, value);
        }
        private double _serverLogsAskInterval = 1000d;
        public IList<DocumentLine> ServerLogsDocumentLines => ServerLogsDocument.Lines;
        public string? Command
        {
            get => _command;
            set => this.RaiseAndSetIfChanged(ref _command, value);
        }
        private string? _command = string.Empty;
        /// <summary>
        /// 命令提示
        /// </summary>
        public ObservableCollection<string> CommandsSupport_IS//item source
        {
            get => _commandsSupport_IS;
            set => this.RaiseAndSetIfChanged(ref _commandsSupport_IS, value);
        }
        private ObservableCollection<string> _commandsSupport_IS = ["stop", "list", "save-on", "save-off", "save-all"];

        public ObservableCollection<string> SupportedMCServerTypes_IS//item source
        {
            get => _supportedMCServerTypes_IS;
            set => this.RaiseAndSetIfChanged(ref _supportedMCServerTypes_IS, value);
        }
        private ObservableCollection<string> _supportedMCServerTypes_IS = [];
        private readonly DispatcherTimer GetNewerLogsTimer = new() { Interval = TimeSpan.FromMicroseconds(500), IsEnabled = false };
        public void OpenSelectedMCServerManagerAction()
        {
            if (SelectedMCServerManager_LBItem is MCServerManager_LBItemModel m)
            {
                UsingManager = m;
                MCServerName = UsingManager.ServerName;
                ServerLogsDocument = new()
                {
                    FileName = "source.log"
                };
                OldestLogID = 0;
                LatestLogID = 0;
                _canRunMCServer.OnNext(!UsingManager.State.IsMCServerRunning);
                _canSendCommand.OnNext(UsingManager.State.IsMCServerRunning);
                _canStopMCServer.OnNext(UsingManager.State.IsMCServerRunning);
                _canKillMCServer.OnNext(UsingManager.State.IsMCServerRunning);
                nativeClient?.RequestBackend(RequestTypeEnum.GetLatestLogs, new Pack_GetLatestMCServerLogs(UsingManager.ManagerID, 200));
                GetNewerLogsTimer.Tick += HandleGetNewerLogsTick;
                LoadCommandsSupport();
                MCServerManagerPanelVisibility = true;

            }
        }
        private void HandleGetNewerLogsTick(object? sender, EventArgs e)
        {
            GetNewerLogsTimer.Stop();
            GetNewerLogsTimer.Interval = TimeSpan.FromMilliseconds(ServerLogsAskInterval);
            if (UsingManager == null)//exit
            {
                GetNewerLogsTimer.Tick -= HandleGetNewerLogsTick;
                return;
            }
            nativeClient?.RequestBackend(RequestTypeEnum.GetNewerLogs, new Pack_GetNewerMCServerLogs(UsingManager.ManagerID, LatestLogID, 200));
        }
        public void ChangePageAction(string pageName)
        {
            switch (pageName)
            {
                case "Console":
                    ConsolePanelVisibility = true;
                    BackupPanelVisibility = false;
                    OnlineChattingPanelVisibility = false;
                    ExplorerPanelVisibility = false;
                    SettingPanelVisibility = false;
                    break;
                case "Backup":
                    ConsolePanelVisibility = false;
                    BackupPanelVisibility = true;
                    OnlineChattingPanelVisibility = false;
                    ExplorerPanelVisibility = false;
                    SettingPanelVisibility = false;
                    break;
                case "OnlineChatting":
                    ConsolePanelVisibility = false;
                    BackupPanelVisibility = false;
                    OnlineChattingPanelVisibility = true;
                    ExplorerPanelVisibility = false;
                    SettingPanelVisibility = false;
                    break;
                case "Explorer":
                    ConsolePanelVisibility = false;
                    BackupPanelVisibility = false;
                    OnlineChattingPanelVisibility = false;
                    ExplorerPanelVisibility = true;
                    SettingPanelVisibility = false;
                    break;
                case "Settings":
                    MCServerName_Edit = UsingManager?.Config.MCServerName;
                    MCServerType_Edit = UsingManager?.Config.MCServerType;
                    MCServerDirectory_Edit = UsingManager?.Config.MCServerDirectory;
                    JavaPath_Edit = UsingManager?.Config.JavaPath;
                    StartArgument_Edit = UsingManager?.Config.StartUpArguments;
                    ConsolePanelVisibility = false;
                    BackupPanelVisibility = false;
                    OnlineChattingPanelVisibility = false;
                    ExplorerPanelVisibility = false;
                    SettingPanelVisibility = true;
                    break;
                default:
                    break;
            }
        }
        public void RunMCServerAction()
        {
            if (nativeClient == null)
            {
                SendMessage("未连接到后端，无法启动服务端", 2);
                return;
            }
            if (UsingManager == null)
            {
                SendMessage("UsingManager为null", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.RunMCServer, new Pack_RunMCServer(UsingManager.ManagerID));
            SendMessage("已发送启动请求", 0);
        }
        public void StopMCServerAction()
        {
            if (nativeClient == null)
            {
                SendMessage("未连接到后端，无法停止服务端", 2);
                return;
            }
            if (UsingManager == null)
            {
                SendMessage("UsingManager为null", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.ShutdownMCServer, new Pack_ShutdownMCServer(UsingManager.ManagerID));
            SendMessage("已发送停止请求", 0);
        }
        public void KillMCServerAction()
        {
            if (nativeClient == null)
            {
                SendMessage("未连接到后端，无法强制终止服务端", 2);
                return;
            }
            if (UsingManager == null)
            {
                SendMessage("UsingManager为null", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.KillMCServer, new Pack_KillMCServer(UsingManager.ManagerID));
            SendMessage("已发送强制终止请求", 0);
        }
        public void SendCommandAction()
        {
            if (nativeClient == null)
            {
                SendMessage("未连接到后端，无法发送命令", 2);
                return;
            }
            if (UsingManager == null)
            {
                SendMessage("UsingManager为null", 2);
                return;
            }
            if (Command == null)
            {
                SendMessage("Command为null", 2);
                return;
            }
            if (Command == string.Empty || Command == " ")
            {
                SendMessage("Command为空", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.SendCommand, new Pack_SendCommand(UsingManager.ManagerID, Command));
            SendMessage("已发送命令", 0);
        }
        public void EditConfigAction()
        {
            if (UsingManager == null) return;
            if (UsingManager.State.IsMCServerRunning)
            {
                SendMessage("服务端仍在运行，请关服后再修改配置", 2);
                return;
            }
            _canEditConfig.OnNext(false);
            _canCommitConfig.OnNext(true);
            EditingConfig = true;
        }
        public void CommitConfigAction()
        {
            _canEditConfig.OnNext(true);
            _canCommitConfig.OnNext(false);
            EditingConfig = false;
            if (nativeClient == null)
            {
                SendMessage("未连接到后端，无法提交配置", 2);
                return;
            }
            nativeClient.RequestBackend(RequestTypeEnum.ModifyMCServerManagerConfig, new Pack_ModifyMCServerManagerConfig(new MCServerManagerConfig()
            {
                ManagerID = UsingManager?.ManagerID ?? "",
                MCServerName = MCServerName_Edit ?? "",
                MCServerType = MCServerType_Edit ?? "",
                MCServerDirectory = MCServerDirectory_Edit ?? "",
                JavaPath = JavaPath_Edit ?? "",
                StartUpArguments = StartArgument_Edit ?? ""
            }));
            SendMessage("已发送修改请求", 0);
        }
        public void ExitManagerPanelAction()
        {
            MCServerManagerPanelVisibility = false;
            UsingManager = null;
        }
        private void LoadCommandsSupport()
        {
            if (UsingManager == null)
            {
                SendMessage("UsingManager为null，无法加载命令支持", 2);
                return;
            }
            //从Json加载
        }
        #endregion
    }
}
