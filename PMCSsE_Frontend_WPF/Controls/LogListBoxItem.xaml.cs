
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PMCSsE_Frontend_WPF.Modules;
namespace PMCSsE_Frontend_WPF.Controls
{
    /// <summary>
    /// LogListBoxItem.xaml 的交互逻辑
    /// </summary>
    public partial class LogListBoxItem : UserControl
    {
        private readonly ImageSource DefaultImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/Icon/AppIcon_Copy.png"));
        private readonly ImageSource CopyedImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/Icon/AppIcon_Finish.png"));
        private readonly System.Timers.Timer Timer = new() { AutoReset = false, Interval = 2000d };
        public LogListBoxItem()
        {
            InitializeComponent();
            Timer.Elapsed += (sender, e) =>
            {
                
                UIContextClass.UIContext?.Post(state =>
                {
                    CopyButton.ImageButtonImageSource = DefaultImage;
                    CopyButton.IsEnabled = true;
                }, null);
            };
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string FullLog = $"{TimeTextBox.Text}{LogTextBox.Text}";
                Clipboard.SetText(FullLog);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入剪贴板失败，请检查是否有其它程序占用\n{ex.Message}\n{ex.StackTrace}");
                return;
            }
            CopyButton.ImageButtonImageSource = CopyedImage;
            CopyButton.IsEnabled = false;
            Timer.Start();
        }
    }
}
