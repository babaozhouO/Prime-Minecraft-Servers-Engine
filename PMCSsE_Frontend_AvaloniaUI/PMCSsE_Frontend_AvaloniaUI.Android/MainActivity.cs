using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using PMCSsE_Frontend_AvaloniaUI.Views;
using ReactiveUI.Avalonia;
using System;
using System.Diagnostics;

namespace PMCSsE_Frontend_AvaloniaUI.Android
{
    /// <summary>
    /// Android 平台的主 Activity，继承自 AvaloniaMainActivity，配置全屏沉浸式体验。
    /// </summary>
    [Activity(
        Label = "PMCSsE-前端",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/PMCSsE",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity
    {
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            RequestedOrientation = ScreenOrientation.SensorLandscape;
            if (Window != null)
            {
                Window.ClearFlags(WindowManagerFlags.TranslucentStatus | WindowManagerFlags.TranslucentNavigation);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

                var decorView = Window.DecorView;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11(API 30)+
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    var controller = decorView.WindowInsetsController;
                    if (controller != null)
                    {
                        controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                        controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                    }
#pragma warning restore CA1416 // 验证平台兼容性
                }
                else // Android 5.0~10
                {
                    var uiOptions =
                        SystemUiFlags.LayoutStable
                        | SystemUiFlags.LayoutFullscreen
                        | SystemUiFlags.LayoutHideNavigation
                        | SystemUiFlags.HideNavigation
                        | SystemUiFlags.Fullscreen
                        | SystemUiFlags.ImmersiveSticky;
#pragma warning disable CA1422 // 验证平台兼容性
                    decorView.SystemUiFlags = uiOptions;
#pragma warning restore CA1422 // 验证平台兼容性
                }
                if ((int)Build.VERSION.SdkInt < 35)
                {
#pragma warning disable CA1422 // 验证平台兼容性
                    Window.SetStatusBarColor(Color.Transparent);
                    Window.SetNavigationBarColor(Color.Transparent);
#pragma warning restore CA1422 // 验证平台兼容性
                }
                // TODO: Android 14+ (API 35+) 可用新API，待Avalonia/AndroidX支持后补充
                Window.DecorView.SetFitsSystemWindows(true);
            }
            base.OnWindowFocusChanged(hasFocus);
        }
        protected override void OnNightModeChanged(int mode)
        {
            base.OnNightModeChanged(mode);
        }
        protected override void OnStart()
        {
            RequestedOrientation = ScreenOrientation.SensorLandscape;
            if (Window != null)
            {
                Window.ClearFlags(WindowManagerFlags.TranslucentStatus | WindowManagerFlags.TranslucentNavigation);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

                var decorView = Window.DecorView;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11(API 30)+
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    var controller = decorView.WindowInsetsController;
                    if (controller != null)
                    {
                        controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                        controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                    }
#pragma warning restore CA1416 // 验证平台兼容性
                }
                else // Android 5.0~10
                {
                    var uiOptions =
                        SystemUiFlags.LayoutStable
                        | SystemUiFlags.LayoutFullscreen
                        | SystemUiFlags.LayoutHideNavigation
                        | SystemUiFlags.HideNavigation
                        | SystemUiFlags.Fullscreen
                        | SystemUiFlags.ImmersiveSticky;
#pragma warning disable CA1422 // 验证平台兼容性
                    decorView.SystemUiFlags = uiOptions;
#pragma warning restore CA1422 // 验证平台兼容性
                }
                if ((int)Build.VERSION.SdkInt < 35)
                {
#pragma warning disable CA1422 // 验证平台兼容性
                    Window.SetStatusBarColor(Color.Transparent);
                    Window.SetNavigationBarColor(Color.Transparent);
#pragma warning restore CA1422 // 验证平台兼容性
                }
                // TODO: Android 14+ (API 35+) 可用新API，待Avalonia/AndroidX支持后补充
                Window.DecorView.SetFitsSystemWindows(true);
            }
            base.OnStart();
        }
    }
    [Application]
    public class AndroidApp : AvaloniaAndroidApplication<App>
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            Avalonia.AppBuilder appBuilder = AppBuilder.Configure<App>()
                .UseAndroid()
                .WithSystemFontSource(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Fonts/SourceHanSansSC-Regular.otf#Source Han Sans SC"))
                .LogToTrace()
                .UseReactiveUI(rxAppBuilder =>
                    {
                        rxAppBuilder.RegisterView<MainView, MainViewModel>();
                    });
            return appBuilder;
        }
        protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }
    }
}
