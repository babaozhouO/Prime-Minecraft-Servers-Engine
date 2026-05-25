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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace PMCSsE_Frontend_WPF.Controls
{
    /// <summary>  
    /// CustomListBoxControl.xaml 的交互逻辑  
    /// </summary>  
    public partial class CustomListBoxControl : UserControl
    {
        private bool IsOpened { get; set; } = false;
        public List<CustomListBoxItem> customListBoxItems = [];

        private readonly ImageSource OpenedImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_FunctionListOpened.png"));
        private readonly ImageSource ClosedImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_FunctionListClosed.png"));

        public CustomListBoxControl(string FunctionText, List<CustomListBoxItem> customListBoxItems)
        {
            InitializeComponent();
            this.FunctionDescriptionTextBlock.Text = FunctionText;
            this.customListBoxItems = customListBoxItems;
            // 初始化时不添加，等打开时再添加
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            IsOpened = !IsOpened;
            if (IsOpened)
            {
                ImageButton.ImageButtonImageSource = OpenedImage;
                ItemsPanel.Children.Clear();
                foreach (var item in customListBoxItems)
                {
                    ItemsPanel.Children.Add(item);
                }

                // 关键：强制布局更新，确保测量到内容高度
                ItemsPanelContainer.UpdateLayout();
                ItemsPanel.Measure(new Size(ItemsPanelContainer.ActualWidth, double.PositiveInfinity));
                double targetHeight = ItemsPanel.DesiredSize.Height;

                // 动画到内容高度
                var heightAnimation = new DoubleAnimation
                {
                    To = targetHeight,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                ItemsPanelContainer.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

                var opacityAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                ItemsPanelContainer.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            }
            else
            {
                ImageButton.ImageButtonImageSource = ClosedImage;

                var heightAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                heightAnimation.Completed += (s, ev) => ItemsPanel.Children.Clear();
                ItemsPanelContainer.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

                var opacityAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                ItemsPanelContainer.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            }
        }
    }
}
