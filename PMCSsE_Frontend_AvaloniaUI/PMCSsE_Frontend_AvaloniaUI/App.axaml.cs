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
using PMCSsE_Frontend_AvaloniaUI.Views;
using System.Linq;

namespace PMCSsE_Frontend_AvaloniaUI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                desktop.MainWindow = new MainWindow();

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