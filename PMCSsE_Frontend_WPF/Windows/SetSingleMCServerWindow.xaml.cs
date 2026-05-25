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

using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using PMCSsE_FrontendAndBackendCommunicator;

namespace PMCSsE_Frontend_WPF.Windows
{
    public partial class SetSingleMCServerWindow : Window
    {
        private bool IsButtonClicked = false;
        internal event Action SetSingleMCServer = delegate { };
        private readonly MCServerManagerConfig MCServerManagerConfig;
        internal SetSingleMCServerWindow(MCServerManagerConfig mCServerManagerConfig)
        {
            InitializeComponent();
            this.MCServerManagerConfig = mCServerManagerConfig;
            LoadConfig();
        }

        private void LoadConfig()
        {
            this.Title = $"MC服务端管理器 - ID:[{MCServerManagerConfig.ManagerID}] - 名称:[{MCServerManagerConfig.MCServerName}] - 设置此MC服务端";
            MCServerNameTextBox.Text = MCServerManagerConfig.MCServerName;
            MCServerTypeComboBox.Text = MCServerManagerConfig.MCServerType;
            ThisMCServerDirectoryTextBox.Text = MCServerManagerConfig.MCServerDirectory;
            JavaPathTextBox.Text = MCServerManagerConfig.JavaPath;
            ThisMCServerStartUpArgumentTextBox.Text = MCServerManagerConfig.StartUpArgument;
        }

        private void SetServerConfigButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("确认设置好了吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            MCServerManagerConfig.MCServerName = MCServerNameTextBox.Text;
            MCServerManagerConfig.MCServerType = MCServerTypeComboBox.Text;
            MCServerManagerConfig.MCServerDirectory = ThisMCServerDirectoryTextBox.Text;
            MCServerManagerConfig.JavaPath = JavaPathTextBox.Text;
            MCServerManagerConfig.StartUpArgument = ThisMCServerStartUpArgumentTextBox.Text;
            SetSingleMCServer();
            IsButtonClicked = true;
            this.Close();
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("真的要放弃吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            IsButtonClicked = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsButtonClicked)
            {
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("真的要放弃吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
        }
        private void OpenPaperArgumentGeneratorImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "https://docs.papermc.io/misc/tools/start-script-gen/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开链接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new() { Title = "选择服务端目录", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
            openFolderDialog.FolderOk += (sender, e) =>
            {
                ThisMCServerDirectoryTextBox.Text = openFolderDialog.FolderName;
            };
            openFolderDialog.ShowDialog();
        }


        private void ImageButton_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new() { Title = "选择Java安装目录下bin文件夹中的javaw.exe", Filter = "Java可执行文件(javaw.exe)|javaw.exe", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) };
            openFileDialog.FileOk += (sender, e) =>
            {
                JavaPathTextBox.Text = openFileDialog.FileName;
            };
            openFileDialog.ShowDialog();
        }

        private void ImageButton_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("说明", "输入服务端目录后可自动识别类型\n若识别失败或不在列表中\n请选择Vanilla(原版)或基于的上游服务端", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ThisMCServerDirectoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
