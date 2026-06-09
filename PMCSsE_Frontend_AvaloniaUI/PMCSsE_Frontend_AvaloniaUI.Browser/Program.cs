using Avalonia;
using Avalonia.Browser;
using PMCSsE_Frontend_AvaloniaUI;
using System.Runtime.Versioning;
using System.Threading.Tasks;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    /// <summary>
    /// 构建并配置 Avalonia 浏览器应用程序的 AppBuilder。
    /// </summary>
    /// <returns>配置好的 AppBuilder 实例。</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}