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
    /// CustomButton.xaml 的交互逻辑
    /// </summary>
    public partial class CustomButton : UserControl
    {
        public static readonly RoutedEvent CustomButtonClick =
            EventManager.RegisterRoutedEvent("CustomButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomButton));
        public event RoutedEventHandler Click
        {
            add { AddHandler(CustomButtonClick, value); }
            remove { RemoveHandler(CustomButtonClick, value); }
        }


        public CustomButton()
        {
            InitializeComponent();
        }

        public string CustomButtonText
        {
            get { return (string)GetValue(CustomButtonTextProperty); }
            set { SetValue(CustomButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonTextProperty =
            DependencyProperty.Register("CustomButtonText", typeof(string), typeof(CustomButton), new PropertyMetadata("按钮"));





        public double CustomButtonFontSize
        {
            get { return (double)GetValue(CustomButtonFontSizeProperty); }
            set { SetValue(CustomButtonFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonFontSizeProperty =
            DependencyProperty.Register("CustomButtonFontSize", typeof(double), typeof(CustomButton), new PropertyMetadata(16d));





        public FontWeight CustomButtonFontWeight
        {
            get { return (FontWeight)GetValue(CustomButtonFontWeightProperty); }
            set { SetValue(CustomButtonFontWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonFontWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonFontWeightProperty =
            DependencyProperty.Register("CustomButtonFontWeight", typeof(FontWeight), typeof(CustomButton));

        private static readonly CornerRadius cornerRadius = new(10d);

        public CornerRadius CustomButtonBorderCornerRadius
        {
            get { return (CornerRadius)GetValue(CustomButtonBorderCornerRadiusProperty); }
            set { SetValue(CustomButtonBorderCornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonBoderCornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonBorderCornerRadiusProperty =
            DependencyProperty.Register("CustomButtonBorderCornerRadius", typeof(CornerRadius), typeof(CustomButton), new PropertyMetadata(cornerRadius));

        private static readonly Brush borderBrush = new SolidColorBrush(Color.FromRgb(125, 164, 196));

        public Brush CustomButtonBorderBrush
        {
            get { return (Brush)GetValue(CustomButtonBorderBrushProperty); }
            set { SetValue(CustomButtonBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonBorderBrushProperty =
            DependencyProperty.Register("CustomButtonBorderBrush", typeof(Brush), typeof(CustomButton), new PropertyMetadata(borderBrush));

        private static readonly Thickness thickness = new(4d);

        public Thickness CustomButtonBorderThickness
        {
            get { return (Thickness)GetValue(CustomButtonBorderThicknessProperty); }
            set { SetValue(CustomButtonBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonBorderThicknessProperty =
            DependencyProperty.Register("CustomButtonBorderThickness", typeof(Thickness), typeof(CustomButton), new PropertyMetadata(thickness));

        private static readonly Brush backgroundBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        public Brush CustomButtonBorderBackground
        {
            get { return (Brush)GetValue(CustomButtonBorderBackgroundProperty); }
            set { SetValue(CustomButtonBorderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonBorderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonBorderBackgroundProperty =
            DependencyProperty.Register("CustomButtonBorderBackground", typeof(Brush), typeof(CustomButton), new PropertyMetadata(backgroundBrush));



        public bool IsMousePressed
        {
            get { return (bool)GetValue(IsMousePressedProperty); }
            set { SetValue(IsMousePressedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMousePressed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMousePressedProperty =
            DependencyProperty.Register("IsMousePressed", typeof(bool), typeof(CustomButton), new PropertyMetadata(false));



        private void CustomButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMousePressed = false;
            ReleaseMouseCapture();
            RaiseEvent(new RoutedEventArgs(CustomButtonClick, this));
        }

        private void CustomButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMousePressed = true;
            CaptureMouse();
        }



        public bool IsMouseOverTheButton
        {
            get { return (bool)GetValue(IsMouseOverTheButtonProperty); }
            set { SetValue(IsMouseOverTheButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMouseOverTheButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMouseOverTheButtonProperty =
            DependencyProperty.Register("IsMouseOverTheButton", typeof(bool), typeof(CustomButton), new PropertyMetadata(false));



        private void CustomButton_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseOverTheButton = true;
        }

        private void CustomButton_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverTheButton = false;
        }

        private void CustomButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 这里可以处理按钮大小变化的逻辑
            // 例如，调整内部元素的布局或样式
        }

        private static readonly ImageSource defaultImage = new BitmapImage(new Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/Author_BBZ.ico"));

        public ImageSource CustomButtonDisplayImage
        {
            get { return (ImageSource)GetValue(CustomButtonDisplayImageProperty); }
            set { SetValue(CustomButtonDisplayImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomButtonDisplayImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomButtonDisplayImageProperty =
            DependencyProperty.Register("CustomButtonDisplayImage", typeof(ImageSource), typeof(CustomButton), new PropertyMetadata(defaultImage));


    }
}
