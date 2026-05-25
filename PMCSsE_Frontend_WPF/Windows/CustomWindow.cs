using PMCSsE_Frontend_WPF.Controls;
using PMCSsE_Frontend_WPF.Modules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace PMCSsE_Frontend_WPF.Windows
{
    public class CustomWindow : Window
    {
        private List<Thumb> _resizeThumbs = new List<Thumb>();
        private ContentPresenter? _contentPresenter;

        public CustomWindow()
        {
            InitializeCustomWindow();
        }

        private void InitializeCustomWindow()
        {
            Height = 576;
            Width = 1024;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanMinimize;
            AllowsTransparency = true;
            FontSize = 20;
            WindowStyle = WindowStyle.None;

            // 创建主Grid
            Grid mainGrid = new();
            // 设置列定义
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });

            // 设置行定义
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5) });
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(35) });
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5) });

            // 创建窗口框架Grid
            Grid windowFrameGrid = new()
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60000000"))
            };
            Grid.SetRowSpan(windowFrameGrid, 2);
            Grid.SetColumnSpan(windowFrameGrid, 3);

            // 设置窗口框架Grid的列定义
            windowFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15) });
            windowFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            windowFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            windowFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });

            // 创建标题文本
            TextBlock windowTitleTextBlock = new()
            {
                Name = "WindowTitleTextBlock",
                Text = Title, // 使用窗口的Title属性
                FontSize = 16,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 绑定Title属性，使其能够动态更新
            var titleBinding = new System.Windows.Data.Binding("Title");
            titleBinding.Source = this;
            windowTitleTextBlock.SetBinding(TextBlock.TextProperty, titleBinding);

            Grid.SetColumn(windowTitleTextBlock, 1);
            windowFrameGrid.Children.Add(windowTitleTextBlock);

            // 创建拖拽Thumb
            Thumb dragThumb = new()
            {
                Background = Brushes.Transparent,
                Opacity = 0,
                Cursor = Cursors.SizeAll,
                Margin = new Thickness(5, 5, 5, 0)
            };
            Grid.SetColumnSpan(dragThumb, 4);

            // 绑定事件
            dragThumb.MouseDoubleClick += Thumb_MouseDoubleClick;
            dragThumb.DragDelta += Thumb_DragDelta;
            dragThumb.DragCompleted += Thumb_DragCompleted;

            windowFrameGrid.Children.Add(dragThumb);

            // 创建按钮面板
            StackPanel buttonPanel = new()
            {
                Orientation = Orientation.Horizontal,
                FlowDirection = FlowDirection.RightToLeft
            };
            Grid.SetColumn(buttonPanel, 3);

            // 创建关闭按钮
            ImageButton closeButton = new()
            {
                Width = 24,
                Margin = new Thickness(8)
            };
            closeButton.Click += Close_Click;
            closeButton.ImageButtonImageSource = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_Close.png"));
            closeButton.Style = (Style)FindResource("ImageButtonStyle1");
            buttonPanel.Children.Add(closeButton);

            // 创建最大化按钮
            ImageButton maximizeButton = new()
            {
                Width = 24,
                Margin = new Thickness(8)
            };
            maximizeButton.Click += Maximize_Click;
            maximizeButton.ImageButtonImageSource = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_Maximize.png"));
            maximizeButton.Style = (Style)FindResource("ImageButtonStyle2");
            buttonPanel.Children.Add(maximizeButton);

            // 创建最小化按钮
            ImageButton minimizeButton = new()
            {
                Width = 24,
                Margin = new Thickness(8)
            };
            minimizeButton.Click += Minimize_Click;
            minimizeButton.ImageButtonImageSource = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri("pack://application:,,,/PMCSsE_Frontend_WPF;component/icon/AppIcon_Minimize.png"));
            minimizeButton.Style = (Style)FindResource("ImageButtonStyle2");
            buttonPanel.Children.Add(minimizeButton);

            windowFrameGrid.Children.Add(buttonPanel);
            mainGrid.Children.Add(windowFrameGrid);

            // 创建内容区域 - 这是关键修改！
            _contentPresenter = new ContentPresenter();
            Grid.SetRow(_contentPresenter, 2);
            Grid.SetColumn(_contentPresenter, 1);
            mainGrid.Children.Add(_contentPresenter);

            // 创建各个方向的调整大小Thumb并直接注册事件
            CreateAndRegisterResizeThumbs(mainGrid);

            // 设置窗口内容
            this.Content = mainGrid;
        }

        private bool SettingContent = false;
        // 重写OnContentChanged，将派生类的内容放入_contentPresenter
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (SettingContent)
            {
                return;
            }
            if (oldContent == null)
            {
                base.OnContentChanged(oldContent, newContent);
                return;
            }
            if (oldContent == newContent || newContent==null) { return; }
            SettingContent = true;
            Content = oldContent;
            base.OnContentChanged(newContent, oldContent);
            SettingContent = false;
            if (_contentPresenter != null && newContent != _contentPresenter.Parent)
            {
                _contentPresenter.Content = newContent;
            }
            
        }

        private void CreateAndRegisterResizeThumbs(Grid mainGrid)
        {
            // 清除之前的引用
            _resizeThumbs.Clear();

            // 创建并注册所有调整大小的 Thumb
            CreateResizeThumb(mainGrid, 0, 0, Cursors.SizeNWSE, Resize_TopLeft);      // 左上
            CreateResizeThumb(mainGrid, 0, 1, Cursors.SizeNS, Resize_Top);           // 上
            CreateResizeThumb(mainGrid, 0, 2, Cursors.SizeNESW, Resize_TopRight);    // 右上
            CreateResizeThumb(mainGrid, 1, 2, Cursors.SizeWE, Resize_Right, 2);      // 右
            CreateResizeThumb(mainGrid, 3, 2, Cursors.SizeNWSE, Resize_BottomRight); // 右下
            CreateResizeThumb(mainGrid, 3, 1, Cursors.SizeNS, Resize_Bottom);        // 下
            CreateResizeThumb(mainGrid, 3, 0, Cursors.SizeNESW, Resize_BottomLeft);  // 左下
            CreateResizeThumb(mainGrid, 1, 0, Cursors.SizeWE, Resize_Left, 2);       // 左
        }

        private void CreateResizeThumb(Grid parentGrid, int row, int column, Cursor cursor,
                                     DragDeltaEventHandler eventHandler, int rowSpan = 1)
        {
            Thumb thumb = new()
            {
                Background = Brushes.Transparent,
                Opacity = 0,
                Cursor = cursor
            };

            Grid.SetRow(thumb, row);
            Grid.SetColumn(thumb, column);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(thumb, rowSpan);
            }

            // 直接注册事件
            thumb.DragDelta += eventHandler;

            // 存储引用
            _resizeThumbs.Add(thumb);
            parentGrid.Children.Add(thumb);
        }

        // ... 其余的事件处理方法保持不变
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ?
                          WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool IsRestoredFromMaximized = false;
        private Point MouseInNormalWindowPosition;
        private Point MouseInDesktopPosition;

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                IsRestoredFromMaximized = true;

                Point MouseInMaxmizedWindowPosition = Mouse.GetPosition(this);
                Point K = new(MouseInMaxmizedWindowPosition.X / Width,
                    MouseInMaxmizedWindowPosition.Y / Height);

                WindowState = WindowState.Normal;

                MouseInNormalWindowPosition = new(Width * K.X, Height * K.Y);
                MouseInDesktopPosition = DesktopMousePositionHelperClass.GetDesktopMousePosition();
                Left = MouseInDesktopPosition.X - MouseInNormalWindowPosition.X;
                Top = MouseInDesktopPosition.Y - MouseInNormalWindowPosition.Y;
            }
            if (IsRestoredFromMaximized)
            {
                Point CurrentMouseInDesktopPosition = DesktopMousePositionHelperClass.GetDesktopMousePosition();
                Point Delta = new(CurrentMouseInDesktopPosition.X - MouseInNormalWindowPosition.X,
                    CurrentMouseInDesktopPosition.Y - MouseInNormalWindowPosition.Y);
                Left = Delta.X;
                Top = Delta.Y;
            }
            else
            {
                Left += e.HorizontalChange;
                Top += e.VerticalChange;
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsRestoredFromMaximized = false;
        }

        private void Thumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                return;
            }
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                return;
            }
        }

        // 最小宽高
        private const double MinWidthValue = 800;
        private const double MinHeightValue = 450;

        // 各方向缩放逻辑
        private void Resize_Top(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height - e.VerticalChange;
            double newTop = Top + e.VerticalChange;
            if (newHeight >= MinHeightValue)
            {
                Height = newHeight;
                Top = newTop;
            }
        }

        private void Resize_Bottom(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height + e.VerticalChange;
            if (newHeight >= MinHeightValue)
            {
                Height = newHeight;
            }
        }

        private void Resize_Left(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width - e.HorizontalChange;
            double newLeft = Left + e.HorizontalChange;
            if (newWidth >= MinWidthValue)
            {
                Width = newWidth;
                Left = newLeft;
            }
        }

        private void Resize_Right(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            if (newWidth >= MinWidthValue)
            {
                Width = newWidth;
            }
        }

        private void Resize_TopLeft(object sender, DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Left(sender, e);
        }

        private void Resize_TopRight(object sender, DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Right(sender, e);
        }

        private void Resize_BottomLeft(object sender, DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Left(sender, e);
        }

        private void Resize_BottomRight(object sender, DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Right(sender, e);
        }
    }
}