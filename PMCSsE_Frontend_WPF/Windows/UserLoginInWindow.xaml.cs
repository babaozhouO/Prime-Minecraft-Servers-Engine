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

using PMCSsE_Frontend_WPF.Modules;
using PMCSsE_FrontendAndBackendCommunicator;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace PMCSsE_Frontend_WPF.Windows
{
    /// <summary>
    /// UserLoginInWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UserLoginInWindow : CustomWindow
    {
        private bool MainWindowOpened = false;
        private NativeClient? nativeClient;
        private string? Password;
        private NativeServerHistory? ConnectingNativeServer;
        internal UserLoginInWindow()
        {
            InitializeComponent();
            LoadConfig();
            foreach (var item in StaticAPPConfigClass.NativeServerHistories)
            {
                ListBoxItem listBoxItem = new()
                {
                    Content = $"{item.IP}:{item.Port}"
                };
                listBoxItem.MouseLeftButtonUp += (sender, e) =>
                {
                    HostName_TB.Text = item.IP;
                    Port_TB.Text = item.Port.ToString();
                    Password_TB.Password = item.Password;
                    if (string.IsNullOrEmpty(item.Password))
                    {
                        SavePassword_CB.IsChecked = false;
                    }
                    else
                    {
                        SavePassword_CB.IsChecked = true;
                    }
                };
                listBoxItem.MouseRightButtonUp += (sender, e) =>
                {
                    if (listBoxItem.Parent is ListBox listBox)
                    {
                        listBox.Items.Remove(listBoxItem);
                    }
                    StaticAPPConfigClass.NativeServerHistories.Remove(item);
                    StaticConfigManagerClass.SaveAPPConfig();
                };
                History_LB.Items.Add(listBoxItem);
            }
        }

        private static void LoadConfig()
        {
            UIContextClass.UIContext = SynchronizationContext.Current;

            APPConfigClass? APPConfig;

            if (File.Exists(PathsAndDefaultConfigTextClass.APPConfigPath))
            {
                string APPConfigText = System.IO.File.ReadAllText(PathsAndDefaultConfigTextClass.APPConfigPath);
                APPConfig = JsonSerializer.Deserialize<APPConfigClass>(APPConfigText);
            }
            else
            {
                System.IO.File.WriteAllText(PathsAndDefaultConfigTextClass.APPConfigPath, PathsAndDefaultConfigTextClass.DefaultAPPConfigText);
                APPConfig = JsonSerializer.Deserialize<APPConfigClass>(PathsAndDefaultConfigTextClass.DefaultAPPConfigText);
            }

            if (APPConfig == null)
            {
                MessageBox.Show("配置文件错误，无法启动", "", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
                return;
            }

            StaticAPPConfigClass.NativeServerHistories = APPConfig.NativeServerHistories;

        }

        private void Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            string hostName = HostName_TB.Text;
            int port = 0;
            try
            {
                port = Convert.ToInt32(Port_TB.Text);
                if (port < 1 || port > 65535)
                {
                    MessageBox.Show("请输入有效的端口号（1~65535）", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入有效的端口号（1~65535）", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (OverflowException)
            {
                MessageBox.Show("请输入有效的端口号（1~65535）", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Password = Password_TB.Password;
            bool existed = false;
            foreach (var item in StaticAPPConfigClass.NativeServerHistories)
            {
                if (item.IP == hostName && item.Port == port)
                {
                    existed = true;
                    ConnectingNativeServer = item;
                    if ((bool)SavePassword_CB.IsChecked!)
                    {
                        item.Password = Password;
                    }
                    else
                    {
                        item.Password = "";
                    }

                }
            }
            if (!existed)
            {
                NativeServerHistory nativeServerHistory = new(hostName, port, "", (bool)SavePassword_CB!.IsChecked! ? Password : "");
                StaticAPPConfigClass.NativeServerHistories.Add(nativeServerHistory);
                ConnectingNativeServer = nativeServerHistory;
                ListBoxItem listBoxItem = new()
                {
                    Content = $"{hostName}:{port}"
                };

                listBoxItem.MouseLeftButtonUp += (sender, e) =>
                {
                    HostName_TB.Text = nativeServerHistory.IP;
                    Port_TB.Text = nativeServerHistory.Port.ToString();
                    Password_TB.Password = nativeServerHistory.Password;
                    if (string.IsNullOrEmpty(nativeServerHistory.Password))
                    {
                        SavePassword_CB.IsChecked = false;
                    }
                    else
                    {
                        SavePassword_CB.IsChecked = true;
                    }
                };
                listBoxItem.MouseRightButtonUp += (sender, e) =>
                {
                    if (listBoxItem.Parent is ListBox listBox)
                    {
                        listBox.Items.Remove(listBoxItem);
                    }
                    StaticAPPConfigClass.NativeServerHistories.Remove(nativeServerHistory);
                    StaticConfigManagerClass.SaveAPPConfig();
                };
                History_LB.Items.Add(listBoxItem);
            }
            StaticConfigManagerClass.SaveAPPConfig();
            nativeClient = new(hostName, port);
            nativeClient.ReportLog += HandleLog;
            nativeClient.NeedToVerifyRSAPublicKey += HandleVerifyRSAPublicKey;
            nativeClient.NeedPassword += HandleNeedPasswordEvent;
            nativeClient.Connected += HandleConnected;
            nativeClient.Disconnected += HandleDisconnected;
            Connect_btn.Visibility = Visibility.Hidden;
            Verify_btn.Visibility = Visibility.Visible;
            nativeClient.Connect();
        }

        private void HandleVerifyRSAPublicKey(int client, string rsa)
        {
            if (ConnectingNativeServer == null) { return; }
            if (rsa == ConnectingNativeServer.RSAPublicKeyHash)
            {
                nativeClient!.IsRSAPublicKeyRight = true;
            }
            else
            {
                if (ConnectingNativeServer.RSAPublicKeyHash != "")
                {
                    UIContextClass.UIContext!.Post((state) =>
                    {
                        LogTextBox.AppendText($"密钥指纹与记忆的不一致，原因可能为：重启了后端、更换了后端端口{Environment.NewLine}");
                    }, null);
                }
                UIContextClass.UIContext!.Post((state) =>
                {
                    LogTextBox.AppendText($"请比对与RSA公钥指纹是否与原生服务器显示的一致(限时60s){Environment.NewLine}ID:{client}{Environment.NewLine}{rsa}{Environment.NewLine}");
                }, null);
            }
            ConnectingNativeServer.RSAPublicKeyHash = rsa;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
        }
        private void HandleDisconnected(NativeClient.DisconnectedReasonEnum nativeClientDisconnectedReasons)
        {
            nativeClient!.ReportLog -= HandleLog;
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
            nativeClient!.Connected -= HandleConnected;
            nativeClient.Disconnected += HandleDisconnected;
            string reason;
            switch (nativeClientDisconnectedReasons)
            {
                case NativeClient.DisconnectedReasonEnum.DisconnectingCalledByUser:
                    reason = "用户取消了连接";
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
                case NativeClient.DisconnectedReasonEnum.HandShake_UnknownPackFormat:
                    reason = "不匹配的客户端版本";
                    break;
                case NativeClient.DisconnectedReasonEnum.HandShake_VerifyRSAPublicKeyTimeOut:
                    ConnectingNativeServer!.RSAPublicKeyHash = "";
                    reason = "验证超时";
                    break;
                case NativeClient.DisconnectedReasonEnum.HandShake_RSAPublicKeyMismatch:
                    reason = "遭遇中间人攻击，RSA公钥指纹不匹配";
                    break;
                case NativeClient.DisconnectedReasonEnum.HandShake_PasswordMismatch:
                    reason = "密码错误";
                    break;
                case NativeClient.DisconnectedReasonEnum.ConnectionUnexpectedDisconnected:
                    reason = "意外断开了网络连接";
                    break;
                default:
                    reason = "未知原因，通常因为客户端版本不匹配导致";
                    break;
            }
            UIContextClass.UIContext!.Post((state) =>
            {
                LogTextBox.AppendText("连接失败:" + reason + Environment.NewLine);
                Connect_btn.Visibility = Visibility.Visible;
                Verify_btn.Visibility = Visibility.Hidden;
            }, null);
        }

        private void HandleConnected()
        {
            nativeClient!.Disconnected -= HandleDisconnected;
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
            nativeClient!.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
            nativeClient!.ReportLog -= HandleLog;
            nativeClient!.Connected -= HandleConnected;
            UIContextClass.UIContext!.Post((state) =>
            {
                LogTextBox.AppendText("连接成功" + Environment.NewLine);
                MainWindow mainWindow = new(nativeClient!);
                mainWindow.Show();
                MainWindowOpened = true;
                this.Close();
            }, null);
        }

        private void HandleNeedPasswordEvent()
        {
            nativeClient!.TypePassword(Password!);
            nativeClient!.NeedPassword -= HandleNeedPasswordEvent;
        }

        private void HandleLog(string log)
        {
            UIContextClass.UIContext!.Post((state) =>
            {
                LogTextBox.AppendText(log + Environment.NewLine);
            }, null);
        }

        private void Verify_btn_Click(object sender, RoutedEventArgs e)
        {
            Verify_CB.IsChecked ??= false;
            nativeClient!.IsRSAPublicKeyRight = Verify_CB.IsChecked;
            if (!(bool)nativeClient.IsRSAPublicKeyRight)
            {
                ConnectingNativeServer!.RSAPublicKeyHash = "";
            }
            StaticConfigManagerClass.SaveAPPConfig();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (nativeClient != null)
            {
                nativeClient.Disconnected -= HandleDisconnected;
                nativeClient.NeedPassword -= HandleNeedPasswordEvent;
                nativeClient.NeedToVerifyRSAPublicKey -= HandleVerifyRSAPublicKey;
                nativeClient.ReportLog -= HandleLog;
                nativeClient.Connected -= HandleConnected;
            }
            if (MainWindowOpened)
            {
                return; // 如果主窗口已经打开，则不需要再确认退出
            }
            Application.Current.Shutdown(); // 关闭应用程序
        }

    }
}
