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
using PMCSsE_FrontendAndBackendCommunicator;
using System.Windows;
using System.Windows.Controls;

namespace PMCSsE_Frontend_WPF.Windows
{
    public partial class MainWindow : CustomWindow
    {
        private readonly NativeClient NativeClient;
        internal MainWindow(NativeClient nativeClient)
        {
            NativeClient = nativeClient;
            InitializeComponent();
            // 初始化MC服务端管理器控件列表
            NativeClient.Disconnected += HandleDisconnected;
            NativeClient.CommunicatorDataReceived += HandleCommunicatorDataReceived;

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            NativeClient.RequestBackend(NativeClient.RequestTypeEnum.GetMCServerManagersList);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

        }

        private void HandleCommunicatorDataReceived(NativeClient.ResponeTypeEnum? type, object? content)
        {
            switch (type, content)
            {
                case (NativeClient.ResponeTypeEnum.MCServerManagerConfigs, MCServerManagerConfigs mCServerManagerConfigs):
                    {
                        StaticMCServerManagerConfigs.ConfigVersion = mCServerManagerConfigs.ConfigVersion;
                        StaticMCServerManagerConfigs.MCServerManagerConfigsList = mCServerManagerConfigs.MCServerManagerConfigsList;

                        UIContextClass.UIContext!.Post(async(state) =>
                        {
                            MCServersListPanel.Items.Clear();
                            foreach (MCServerManagerConfig mCServerManagerConfig in StaticMCServerManagerConfigs.MCServerManagerConfigsList)
                            {
                                MCServersListPanel.Items.Add(new SingleMCServerViewerControl(mCServerManagerConfig));
                            }

                        }, null);
                        break;
                    }
                case (NativeClient.ResponeTypeEnum.AddedNewMCServerManager, MCServerManagerConfig mCServerManagerConfig):
                    {
                        UIContextClass.UIContext!.Post((state) =>
                        {
                            MCServersListPanel.Items.Add(new SingleMCServerViewerControl(mCServerManagerConfig));
                        }, null);
                        break;
                    }
            }
        }

        private void HandleDisconnected(NativeClient.DisconnectedReasonEnum nativeClientDisconnectedReasons)
        {
            NativeClient!.Disconnected -= HandleDisconnected;
            NativeClient.CommunicatorDataReceived -= HandleCommunicatorDataReceived;
            MessageBox.Show("连接断开");
            UIContextClass.UIContext!.Post((state) =>
            {
                this.Close();
            }, null);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NativeClient?.Disconnect();
            NativeClient?.Dispose();
            Application.Current.Shutdown();
            App.Current.Shutdown();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private AppAboutWindow? appAboutWindow;
        private void AboutThisApp_Click(object sender, RoutedEventArgs e)
        {
            // 检查窗口是否已存在且未关闭
            if (appAboutWindow != null && appAboutWindow.IsLoaded)
            {
                // 如果窗口已最小化，恢复显示
                if (appAboutWindow.WindowState == WindowState.Minimized)
                {
                    appAboutWindow.WindowState = WindowState.Normal;
                }

                // 激活并前置窗口
                appAboutWindow.Activate();
                appAboutWindow.Topmost = true;  // 临时置顶
                appAboutWindow.Topmost = false; // 取消置顶
                return;
            }

            // 创建新窗口
            appAboutWindow = new AppAboutWindow();

            // 设置窗口关闭时的清理操作
            appAboutWindow.Closed += (s, args) =>
            {
                appAboutWindow = null; // 清除引用
            };

            appAboutWindow.Show();
        }

        private void AddNewServerButton_Click(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            NativeClient.RequestBackend(NativeClient.RequestTypeEnum.AddNewMCServerManager);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }
        private void OpenThisServerManagerWindow_Click(object sender, RoutedEventArgs e)
        {
            if (MCServersListPanel.SelectedItem == null)
            {
                MessageBox.Show("请先选择一个MC服务端管理器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MCServerManagerConfig? mCServerManagerConfig = null;
            if (MCServersListPanel.SelectedItem is SingleMCServerViewerControl viewerControl)
            {
                mCServerManagerConfig = viewerControl.MCServerManagerConfig;
            }

            MCServerManagerWindow singleMCServerManagerWindow;
            if (mCServerManagerConfig != null)
            {
                singleMCServerManagerWindow = new(mCServerManagerConfig);
                singleMCServerManagerWindow.Show();
            }

        }

        private void RemoveThisServerManager_Click(object sender, RoutedEventArgs e)
        {
            if (MCServersListPanel.SelectedItem == null)
            {
                MessageBox.Show("请先选择一个MC服务端管理器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MCServerManagerConfig? mCServerManagerConfig = null;
            SingleMCServerViewerControl? singleMCServerViewerControl = null;
            if (MCServersListPanel.SelectedItem is SingleMCServerViewerControl viewerControl)
            {
                singleMCServerViewerControl = viewerControl;
                mCServerManagerConfig = viewerControl.MCServerManagerConfig;
            }
            if (singleMCServerViewerControl == null) { return; }
            if (mCServerManagerConfig == null) { return; }
            //确认删除
            MessageBoxResult messageBoxResult = MessageBox.Show($"确认删除ID为[{mCServerManagerConfig.ManagerID}]的服务端吗？\n删除后无法恢复", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                StaticMCServerManagerConfigs.MCServerManagerConfigsList.Remove(mCServerManagerConfig);

                //从UI中删除

                if (singleMCServerViewerControl.Parent is ListBox parentlistbox)
                {
                    parentlistbox.Items.Remove(singleMCServerViewerControl);
                }

            }
        }
    }
}