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
using PMCSsE_Frontend_WPF.Controls;
using PMCSsE_Frontend_WPF.Modules;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PMCSsE_FrontendAndBackendCommunicator;

namespace PMCSsE_Frontend_WPF.Windows
{
    public partial class MCServerManagerWindow : CustomWindow
    {
        private MCServerManagerConfig MCServerManagerConfig;
        #region 初始化相关函数
        internal MCServerManagerWindow(MCServerManagerConfig mCServerManagerConfig)
        {
            MCServerManagerConfig = mCServerManagerConfig;
            Initialize();
        }
        private void Initialize()
        {
            InitializeComponent();
            LogListBox_SG.ItemsSource = logListBoxItemModels_SG;
            LogListBox_BG.ItemsSource = logListBoxItemModels_BG;
            LogListBox_OG.ItemsSource = logListBoxItemModels_OG;
            //更新UI
            this.Title = $"MC服务端管理器 - ID:[{MCServerManagerConfig.ManagerID}] - 名称:[{MCServerManagerConfig.MCServerName}]";
            ManagerIDAndName_SG.Text = $"{MCServerManagerConfig.ManagerID} - {MCServerManagerConfig.MCServerName}  当前页面:MC服务端控制台";
            //添加功能控件
            AddFunctions(MCServerManagerConfig.MCServerType);
            //---------------------------------------------------------------------------------
            AppendLog("成功", "管理面板", "太棒了！程序没爆炸！全都加载完了");
            AppendLog("信息", "管理面板", $"当前MC服务端管理器ID: {MCServerManagerConfig.ManagerID}");
            AppendLog("信息", "管理面板", $"当前MC服务器名称: {MCServerManagerConfig.MCServerName}");
        }
        /// <summary>
        /// 添加功能列表项
        /// </summary>
        private void AddFunctions(string MCServerType)
        {
            List<CustomListBoxItem> customListBoxItems;
            customListBoxItems = GeneralFunction_SG();
            CustomListBoxControl customListBoxControl = new("通用功能", customListBoxItems);
            FunctionCollectionList_SG.Children.Add(customListBoxControl);
            List<CustomListBoxItem> customListBoxItems1;
            switch (MCServerType)
            {
                case "Forge":
                    customListBoxItems1 = ForgeFunction();
                    break;
                default:
                    customListBoxItems1 = [];
                    break;
            }
            CustomListBoxControl customListBoxControl1 = new("专有功能", customListBoxItems1);
            FunctionCollectionList_SG.Children.Add(customListBoxControl1);
        }
        /// <summary>
        /// MC服务端的通用功能
        /// </summary>
        private List<CustomListBoxItem> GeneralFunction_SG()
        {
            List<CustomListBoxItem> customListBoxItems = []; CancellationTokenSource cancellationTokenSource = new();

            CustomListBoxItem item1 = new("打开服务端目录");
            item1.OnItemClick += () =>
            {
                string directoryPath = MCServerManagerConfig.MCServerDirectory;
                if (System.IO.Directory.Exists(directoryPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", directoryPath);
                }
                else
                {
                    MessageBox.Show("服务端目录不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            customListBoxItems.Add(item1);

            return customListBoxItems;
        }
        private List<CustomListBoxItem> GeneralFunction_BG()
        {
            List<CustomListBoxItem> customListBoxItems = [];

            CustomListBoxItem item1 = new("");
            item1.OnItemClick += () =>
            {

            };
            customListBoxItems.Add(item1);

            CustomListBoxItem item2 = new("");
            item2.OnItemClick += () =>
            {
            };
            customListBoxItems.Add(item2);

            return customListBoxItems;
        }
        /// <summary>
        /// Forge功能
        /// </summary>
        private List<CustomListBoxItem> ForgeFunction()
        {
            return [];
        }
        #endregion
        #region 服务端操作
        /// <summary>
        /// 启动服务端
        /// </summary>
        private void StartButton_SG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行启动MC服务端操作");

        }
        /// <summary>
        /// 关闭服务端
        /// </summary>
        private void ShutdownButton_SG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行关闭MC服务端操作");
            ;
        }
        /// <summary>
        /// 杀死服务端
        /// </summary>
        private void KillButton_SG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行强行停止服务端进程操作");

        }
        private void OpenSettingsWindowButton_SG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户打开设置界面");

            //MessageBox.Show("请先关闭服务端后再进行设置。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //AppendLog("错误", "管理面板", "服务端正在运行时不能进行设置");
            //AppendLog("失败", "管理面板", "打开请求已取消");
            //return;

            SetSingleMCServerWindow setSingleMCServerWindow = new(MCServerManagerConfig);

            setSingleMCServerWindow.SetSingleMCServer += () =>
            {
                // 更新窗口标题
                this.Title = $"MC服务端管理器 - ID:[{this.MCServerManagerConfig.ManagerID}] - 名称:[{MCServerManagerConfig.MCServerName}]";
                ManagerIDAndName_SG.Text = $"{MCServerManagerConfig.ManagerID} - {MCServerManagerConfig.MCServerName}  当前页面:MC服务端控制台";
            };
            setSingleMCServerWindow.Show();
        }
        #endregion
        #region 备份工具操作
        private void StartButton_BG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行启动自动备份操作");
        }
        private void ShutdownButton_BG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行停止自动备份操作");
        }
        private void KillButton_BG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户进行终止备份操作");
        }
        private void OpenSettingsWindowButton_BG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "管理面板", "用户打开备份工具设置界面");

        }
        private bool IsBackupHelperInitialized = false;
        private readonly CancellationTokenSource CancellationTokenSource = new();
        #endregion
        #region 互联工具操作
        private bool OnlineCAMCInitialized = false;
        /// <summary>
        /// 启动
        /// </summary>
        private void StartButton_OG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "实时服内外通信和远程服务器管理器", "用户进行启动实时服内外通信与管理工具操作");

        }
        /// <summary>
        /// 关闭服务端
        /// </summary>
        private void ShutdownButton_OG_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("用户操作", "实时服内外通信和远程服务器管理器", "用户进行关闭实时服内外通信与管理工具操作");

        }
        /// <summary>
        /// 停止
        /// </summary>
        private void KillButton_OG_Click(object sender, RoutedEventArgs e)
        {
        }
        private void OpenSettingsWindowButton_OG_Click(object sender, RoutedEventArgs e)
        {
        }
        #endregion
        #region 命令输入框相关
        /// <summary>
        /// 命令输入框的文本变化,在这里制作命令补全相关功能
        /// </summary>
        private void CommandEnterTextBox_SG_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private bool IsEnterKeyDownInCommandEnterTextBox_SG = false;
        private void CommandEnterTextBox_SG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsEnterKeyDownInCommandEnterTextBox_SG = true;
            }
        }
        private void CommandEnterTextBox_SG_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && IsEnterKeyDownInCommandEnterTextBox_SG)
            {
                IsEnterKeyDownInCommandEnterTextBox_SG = false;
                string Command = CommandEnterTextBox_SG.Text;
                CommandEnterTextBox_SG.Clear();

                AppendLog("用户操作", "管理面板", $"用户发送[{Command}]命令");
            }
        }
        // 在TextBox获得焦点时设置输入法状态
        private void CommandEnterTextBox_SG_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox CommandEnterTextBox)
            {
                // 启用输入法并设置为英文模式
                InputMethod.SetIsInputMethodEnabled(CommandEnterTextBox, true);
                InputMethod.SetPreferredImeState(CommandEnterTextBox, InputMethodState.Off);
            }
        }

        #endregion
        #region 消息或命令输入框相关
        private bool IsEnterKeyDownInCommandEnterTextBox_OG = false;
        private void CommandEnterTextBox_OG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsEnterKeyDownInCommandEnterTextBox_OG = true;
            }
        }
        private void CommandEnterTextBox_OG_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && IsEnterKeyDownInCommandEnterTextBox_OG)
            {
                IsEnterKeyDownInCommandEnterTextBox_OG = false;
                if (!string.IsNullOrEmpty(CommandEnterTextBox_OG.Text))
                {
                    string Command = CommandEnterTextBox_OG.Text;

                    CommandEnterTextBox_OG.Clear();
                }
            }
        }

        private void CommandEnterTextBox_OG_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CommandEnterTextBox_OG_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion
        #region 日志处理逻辑
        public class LogListBoxItemModel
        {
            public required string TimeText { get; set; }
            public required string LogText { get; set; }
            public required Brush TextColor { get; set; }
        }
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModels_SG = [];
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModelsWithoutTime_SG = [];
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModels_BG = [];
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModelsWithoutTime_BG = [];
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModels_OG = [];
        private readonly ObservableCollection<LogListBoxItemModel> logListBoxItemModelsWithoutTime_OG = [];
        /// <summary>
        /// (类别，发送者，内容)
        /// </summary>
        private void AppendLog(string Type, string Sender, string Log)
        {
            if (Sender == "管理面板" || Sender == "MC服务端管理器" || Sender == $"{MCServerManagerConfig.MCServerName}(服务端)")
            {
                if (!ShowDebugLog_SG && Type == "调试") return;
                UIContextClass.UIContext?.Post(state =>
                {
                    LogListBoxItemModel logListBoxItemModel = GetLogListBoxItemModelFromTextAndType(Type, Sender, Log);
                    logListBoxItemModels_SG.Add(logListBoxItemModel);
                    LogListBoxItemModel logListBoxItemModelWithoutTime = new() { TimeText = "", LogText = logListBoxItemModel.LogText, TextColor = logListBoxItemModel.TextColor };
                    logListBoxItemModelsWithoutTime_SG.Add(logListBoxItemModelWithoutTime);
                    if (logListBoxItemModels_SG.Count > 1000)
                    {
                        logListBoxItemModels_SG.RemoveAt(0);
                        logListBoxItemModelsWithoutTime_SG.RemoveAt(0);
                    }
                    LogListBox_SG.ScrollIntoView(LogListBox_SG.Items[^1]);
                }, null);

            }
            else if (Sender == "MC服务端备份工具")
            {
                if (!ShowDebugLog_BG && Type == "调试") return;
                UIContextClass.UIContext?.Post(state =>
                {
                    LogListBoxItemModel logListBoxItemModel = GetLogListBoxItemModelFromTextAndType(Type, Sender, Log);
                    logListBoxItemModels_BG.Add(logListBoxItemModel);
                    LogListBoxItemModel logListBoxItemModelWithoutTime = new() { TimeText = "", LogText = logListBoxItemModel.LogText, TextColor = logListBoxItemModel.TextColor };
                    logListBoxItemModelsWithoutTime_BG.Add(logListBoxItemModelWithoutTime);
                    if (logListBoxItemModels_BG.Count > 300)
                    {
                        logListBoxItemModels_BG.RemoveAt(0);
                        logListBoxItemModelsWithoutTime_BG.RemoveAt(0);
                    }
                    LogListBox_BG.ScrollIntoView(LogListBox_BG.Items[^1]);
                }, null);
            }
            else if (Sender == "实时服内外通信和远程服务器管理器" || Sender == "聊天记录管理器")
            {
                if (!ShowDebugLog_OG && Type == "调试") return;
                UIContextClass.UIContext?.Post(state =>
                {
                    LogListBoxItemModel logListBoxItemModel = GetLogListBoxItemModelFromTextAndType(Type, Sender, Log);
                    logListBoxItemModels_OG.Add(logListBoxItemModel);
                    LogListBoxItemModel logListBoxItemModelWithoutTime = new() { TimeText = "", LogText = logListBoxItemModel.LogText, TextColor = logListBoxItemModel.TextColor };
                    logListBoxItemModelsWithoutTime_OG.Add(logListBoxItemModelWithoutTime);
                    if (logListBoxItemModels_OG.Count > 300)
                    {
                        logListBoxItemModels_OG.RemoveAt(0);
                        logListBoxItemModelsWithoutTime_OG.RemoveAt(0);
                    }
                    LogListBox_OG.ScrollIntoView(LogListBox_OG.Items[^1]);
                }, null);
            }
        }

        private static LogListBoxItemModel GetLogListBoxItemModelFromTextAndType(string Type, string Sender, string Log)
        {
            Brush ForeBrush = Brushes.Black;
            switch (Type)
            {
                case "信息":
                    Type += "💬";
                    ForeBrush = Brushes.Black;
                    break;
                case "警告":
                    Type += "⚠";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(255, 128, 0));
                    break;
                case "错误":
                    Type += "💥";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    break;
                case "失败":
                    Type += "❌";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    break;
                case "成功":
                    Type += "✔";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(21, 173, 63));
                    break;
                case "调试":
                    Type += "🛠";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(218, 37, 209));
                    break;
                case "用户操作":
                    Type += "👉";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(0, 221, 221));
                    break;
                case "程序操作":
                    Type += "⚙";
                    ForeBrush = new SolidColorBrush(Color.FromRgb(228, 68, 201));
                    break;

            }
            string Time = $"[{DateTime.Now:G}] |";
            string FullLog = $"[{Type}] | [{Sender}]:{Log}";

            return new() { TimeText = Time, LogText = FullLog, TextColor = ForeBrush };
        }
        private void ShowTimeCheckBox_SG_Click(object sender, RoutedEventArgs e)
        {
            ShowTimeCheckBox_SG.IsChecked = ShowTimeCheckBox_SG.IsChecked == true;
            LogListBox_SG.ItemsSource = ((bool)ShowTimeCheckBox_SG.IsChecked ? logListBoxItemModels_SG : logListBoxItemModelsWithoutTime_SG);
        }

        private void ShowTimeCheckBox_BG_Click(object sender, RoutedEventArgs e)
        {
            ShowTimeCheckBox_BG.IsChecked = ShowTimeCheckBox_BG.IsChecked == true;
            LogListBox_BG.ItemsSource = ((bool)ShowTimeCheckBox_BG.IsChecked ? logListBoxItemModels_BG : logListBoxItemModelsWithoutTime_BG);
        }

        private void ShowTimeCheckBox_OG_Click(object sender, RoutedEventArgs e)
        {
            ShowTimeCheckBox_OG.IsChecked = ShowTimeCheckBox_OG.IsChecked == true;
            LogListBox_OG.ItemsSource = ((bool)ShowTimeCheckBox_OG.IsChecked ? logListBoxItemModels_OG : logListBoxItemModelsWithoutTime_OG);
        }
        private bool ShowDebugLog_SG = false;
        private bool ShowDebugLog_BG = false;
        private bool ShowDebugLog_OG = false;
        private void ShowDebugLogCheckBox_SG_Click(object sender, RoutedEventArgs e)
        {
            ShowDebugLog_SG = ShowDebugLogCheckBox_SG.IsChecked == true;
        }
        private void ShowDebugLogCheckBox_BG_Click(object sender, RoutedEventArgs e)
        {
            ShowDebugLog_BG = ShowDebugLogCheckBox_BG.IsChecked==true;
        }
        private void ShowDebugLogCheckBox_OG_Click(object sender, RoutedEventArgs e)
        {
            ShowDebugLog_OG = ShowDebugLogCheckBox_OG.IsChecked==true;
        }
        private void CleanLogButton_Click(object sender, RoutedEventArgs e)
        {
            switch (PageIndex)
            {
                case 0:
                    logListBoxItemModels_SG.Clear();
                    logListBoxItemModelsWithoutTime_SG.Clear();
                    break;
                case 1:
                    logListBoxItemModels_BG.Clear();
                    logListBoxItemModelsWithoutTime_BG.Clear();
                    break;
                case 2:
                    logListBoxItemModels_OG.Clear();
                    logListBoxItemModelsWithoutTime_OG.Clear();
                    break;
            }
        }
        #endregion
        #region 页面改变逻辑
        private byte PageIndex = 0;

        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            PageIndex = 0;
            ChangePage();
        }

        private void CustomButton_Click_1(object sender, RoutedEventArgs e)
        {
            PageIndex = 1;
            if (!IsBackupHelperInitialized)
            {
                IsBackupHelperInitialized = true;
            }
            ChangePage();
        }

        private void CustomButton_Click_2(object sender, RoutedEventArgs e)
        {
            PageIndex = 2;
            if (!OnlineCAMCInitialized)
            {
                AppendLog("用户操作", "实时服内外通信和远程服务器管理器", "输入/help可查看命令帮助");
                OnlineCAMCInitialized = true;
            }
            ChangePage();
        }

        private void ChangePage()
        {
            switch (PageIndex)
            {
                case 0:
                    ServerConsoleGrid.Visibility = Visibility.Visible;
                    BackupHelperConsoleGrid.Visibility = Visibility.Collapsed;
                    OnlineChatAndManagerConsoleGrid.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    ServerConsoleGrid.Visibility = Visibility.Collapsed;
                    BackupHelperConsoleGrid.Visibility = Visibility.Visible;
                    OnlineChatAndManagerConsoleGrid.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    ServerConsoleGrid.Visibility = Visibility.Collapsed;
                    BackupHelperConsoleGrid.Visibility = Visibility.Collapsed;
                    OnlineChatAndManagerConsoleGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
        #endregion
    }
}
