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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PMCSsE_Frontend_WPF.Controls
{
    /// <summary>
    /// ImageButton.xaml 的交互逻辑
    /// </summary>
    public partial class ImageButton : UserControl
    {
        public static readonly RoutedEvent ImageButtonClick =
     EventManager.RegisterRoutedEvent("ImageButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageButton));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ImageButtonClick, value); }
            remove { RemoveHandler(ImageButtonClick, value); }
        }
        private static readonly ImageSource defaultImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/Author_BBZ.ico"));

        public ImageSource ImageButtonImageSource
        {
            get { return (ImageSource)GetValue(ImageButtonImageSourceProperty); }
            set { SetValue(ImageButtonImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageButtonImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageButtonImageSourceProperty =
            DependencyProperty.Register("ImageButtonImageSource", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(defaultImage));



        public bool IsMouseOverTheImageButton
        {
            get { return (bool)GetValue(IsMouseOverTheImageButtonProperty); }
            set { SetValue(IsMouseOverTheImageButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMouseOverTheImageButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMouseOverTheImageButtonProperty =
            DependencyProperty.Register("IsMouseOverTheImageButton", typeof(bool), typeof(ImageButton), new PropertyMetadata(false));



        public bool IsMousePressedTheImageButton
        {
            get { return (bool)GetValue(IsMousePressedTheImageButtonProperty); }
            set { SetValue(IsMousePressedTheImageButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMousePressed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMousePressedTheImageButtonProperty =
            DependencyProperty.Register("IsMousePressedTheImageButton", typeof(bool), typeof(ImageButton), new PropertyMetadata(false));
        public ImageButton()
        {
            InitializeComponent();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseOverTheImageButton = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverTheImageButton = false;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMousePressedTheImageButton = true;
            CaptureMouse();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMousePressedTheImageButton = false;
            ReleaseMouseCapture();
            RaiseEvent(new RoutedEventArgs(ImageButtonClick, this));
        }
    }
}
