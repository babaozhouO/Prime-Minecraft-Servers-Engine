using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace PMCSsE_Frontend_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // 在App.xaml.cs中启用硬件加速
        protected override void OnStartup(StartupEventArgs e)
        {
            // 强制使用硬件渲染
            RenderOptions.ProcessRenderMode = RenderMode.Default;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            base.OnStartup(e);
        }
    }

}
