using Avalonia;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using PMCSsE_Frontend_AvaloniaUI.Views;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Builder;
using System;
using System.Reflection;

namespace PMCSsE_Frontend_AvaloniaUI.Desktop
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            Avalonia.AppBuilder appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithSystemFontSource(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Fonts/SourceHanSansSC-Regular.otf#Source Han Sans SC"))
            .LogToTrace()
            .UseReactiveUI(rxAppBuilder =>
            {
                rxAppBuilder.RegisterView<MainView, MainViewModel>();
            });
            return appBuilder;
        }
    }
}
