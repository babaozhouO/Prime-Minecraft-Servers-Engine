/*Copyright 2025 【Babao Zhou (Legal Name: RenJie Zhou) <Contact: 1749861851@qq.com>】

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using PMCSsE_Frontend_AvaloniaUI.Views;
using System.Linq;

namespace PMCSsE_Frontend_AvaloniaUI
{
    /// <summary>
    /// Avalonia 应用程序入口类，负责初始化框架并根据平台创建对应的主窗口/视图。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 全局 MainViewModel 引用，供退出清理使用。
        /// </summary>
        internal static MainViewModel? MainViewModel { get; set; }

        /// <summary>
        /// 加载 XAML 资源并在调试模式下附加开发者工具。
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        /// <summary>
        /// 框架初始化完成后的回调。根据应用程序生命周期类型创建对应平台的主视图。
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.Exit += (s, e) => MainViewModel?.CleanupOnExit();

            }
            else if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
            {
                activityLifetime.MainViewFactory = () => new MainView();
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}