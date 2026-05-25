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

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PMCSsE_Frontend_WPF.Modules;
using PMCSsE_FrontendAndBackendCommunicator;

namespace PMCSsE_Frontend_WPF.Controls
{
    /// <summary>
    /// SingleMCServerViewerControl.xaml 的交互逻辑
    /// </summary>
    public partial class SingleMCServerViewerControl : UserControl
    {

        private DispatcherTimer OneSecondTimer;
        internal readonly MCServerManagerConfig MCServerManagerConfig;
        internal SingleMCServerViewerControl(MCServerManagerConfig MCServerManagerConfig)
        {
            InitializeComponent();
            this.MCServerManagerConfig = MCServerManagerConfig;
            ThisServerID.Text = MCServerManagerConfig.ManagerID;
            ThisServerName.Text = MCServerManagerConfig.MCServerName;
            MCServerTypeTextBox.Text = MCServerManagerConfig.MCServerType;
            switch (MCServerManagerConfig.MCServerType)
            {
                case "Forge":
                    MCServerTypeTextBox.Foreground = new SolidColorBrush() { Color = Color.FromArgb(200, 00, 00, 00) };
                    MCServerTypeimage.Source = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_Forge.png"));
                    break;
                default:
                    break;
            }
            OneSecondTimer = new() { Interval = TimeSpan.FromSeconds(1) };
            OneSecondTimer.Tick += (sender, e) =>
            {
                ThisServerID.Text = MCServerManagerConfig.ManagerID;
                ThisServerName.Text = MCServerManagerConfig.MCServerName;
                ThisServerType.Text = MCServerManagerConfig.MCServerType;

            };
            OneSecondTimer.Start();
        }
    }
}
