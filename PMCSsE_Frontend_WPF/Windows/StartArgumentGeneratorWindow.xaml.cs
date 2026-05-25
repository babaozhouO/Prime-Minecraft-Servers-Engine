using Microsoft.Win32;

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PMCSsE_Frontend_WPF.Modules;

namespace PMCSsE_Frontend_WPF.Windows
{
    /// <summary>
    /// StartArgumentGeneratorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartArgumentGeneratorWindow : Window
    {
        internal event Action<string> GeneratedArgu = delegate { };
        private readonly string ServerDir;
        private readonly DispatcherTimer OneSecondTimer = new() { Interval = TimeSpan.FromSeconds(10) };
        private readonly SolidColorBrush SolidColorBrushN = new(Color.FromArgb(48, 59, 224, 228));
        private readonly SolidColorBrush SolidColorBrushW = new(Color.FromArgb(153, 247, 237, 50));
        private readonly SolidColorBrush SolidColorBrushWW = new(Color.FromArgb(153, 255, 0, 0));
        private readonly SolidColorBrush SolidColorBrushWBN = new(Color.FromArgb(153, 59, 224, 228));
        private readonly SolidColorBrush SolidColorBrushWBW = new(Color.FromArgb(190, 247, 237, 50));
        private readonly SolidColorBrush SolidColorBrushWBWW = new(Color.FromArgb(190, 255, 0, 0));
        private const string Aikar_sArg = "-XX:+AlwaysPreTouch -XX:+DisableExplicitGC -XX:+ParallelRefProcEnabled -XX:+PerfDisableSharedMem -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1HeapRegionSize=8M -XX:G1HeapWastePercent=5 -XX:G1MaxNewSizePercent=40 -XX:G1MixedGCCountTarget=4 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1NewSizePercent=30 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:G1ReservePercent=20 -XX:InitiatingHeapOccupancyPercent=15 -XX:MaxGCPauseMillis=200 -XX:MaxTenuringThreshold=1 -XX:SurvivorRatio=32 -Dfile.encoding=UTF-8 -Dsun.stdout.encoding=UTF-8 -Dsun.stderr.encoding=UTF-8 -Djdk.lang.Process.ignoreStdin=true -Dusing.aikars.flags=https://mcflags.emc.gs -Daikars.new.flags=true -Dterminal.jline=false -Djline.terminal=jline.UnsupportedTerminal";
        private const string VelocityArg = "-XX:+AlwaysPreTouch -XX:+ParallelRefProcEnabled -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1HeapRegionSize=4M -XX:MaxInlineLevel=15 -Dfile.encoding=UTF-8 -Dsun.stdout.encoding=UTF-8 -Dsun.stderr.encoding=UTF-8 -Djdk.lang.Process.ignoreStdin=true -Dterminal.jline=false -Djline.terminal=jline.UnsupportedTerminal";
        public StartArgumentGeneratorWindow(string ServerDir)
        {
            InitializeComponent();
            RegisterResizeThumbEvents();
            this.ServerDir = ServerDir;
            OneSecondTimer.Tick += (sender, e) =>
            {
                UpdateMemoryUsage();
            };
        }

        private void UpdateMemoryUsage()
        {
            var (Total, Used, Free) = MemoryHelperClass.GetMemoryInfo();
            double totalwidth = MemoryShowerGrid.ActualWidth;
            UsedMemoryCol.Width = new((Used / Total) * totalwidth);
            WillBeUsedMemoryCol.Width = new(((MemorySlider.Value > Free ? Free : MemorySlider.Value) / Total) * totalwidth);
            switch (Free - MemorySlider.Value)
            {
                case >= 1500:
                    LastMemoryRec.Fill = SolidColorBrushN;
                    break;
                case <= 800:
                    LastMemoryRec.Fill = SolidColorBrushWW;
                    break;
                case < 1500:
                    LastMemoryRec.Fill = SolidColorBrushW;
                    break;
            }
            switch (MemorySlider.Value)
            {
                case >= 1024:
                    WillBeMemoryRec.Fill = SolidColorBrushWBN;
                    break;
                case <= 500:
                    WillBeMemoryRec.Fill = SolidColorBrushWBWW;
                    break;
                case < 1024:
                    WillBeMemoryRec.Fill = SolidColorBrushWBW;
                    break;
            }
        }


        private void MemorySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MemoryShowerGrid == null)
            {
                return;
            }
            MemoryTextBlock.Text = e.NewValue.ToString();
            UpdateMemoryUsage();
            Generate();
        }
        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new() { Title = "选择服务端目录中的.jar服务端核心文件", Filter = "Java可执行文件(*.jar)|*.jar", InitialDirectory = Directory.Exists(ServerDir)?ServerDir:(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)) };
            openFileDialog.FileOk += (sender, e) =>
            {
                FileNameTextBox.Text = openFileDialog.FileName;
            };
            openFileDialog.ShowDialog();
        }


        private void ShowGUICheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void ArgTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Generate();
        }

        private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Generate();
        }
        //生成
        private void Generate()
        {
            if (ArgTypeComboBox == null||MemorySlider==null||FileNameTextBox==null||ShowGUICheckBox==null)
            {
                return;
            }
            string Arg = "";
            if (ShowGUICheckBox.IsChecked == null) { ShowGUICheckBox.IsChecked = false; }
            switch (ArgTypeComboBox.SelectedIndex)
            {
                case 0:
                    Arg = $"-Xms{(int)MemorySlider.Value}M -Xmx{(int)MemorySlider.Value}M {Aikar_sArg} -jar \"{FileNameTextBox.Text}\"{(((bool)ShowGUICheckBox.IsChecked) ? "" : " nogui")}";
                    break;
                case 1:
                    Arg = $"-Xms{(int)MemorySlider.Value}M -Xmx{(int)MemorySlider.Value}M -Dfile.encoding=UTF-8 -Dsun.stdout.encoding=UTF-8 -Dsun.stderr.encoding=UTF-8 -Djdk.lang.Process.ignoreStdin=true -Dterminal.jline=false -Djline.terminal=jline.UnsupportedTerminal -jar \"{FileNameTextBox.Text}\"{(((bool)ShowGUICheckBox.IsChecked) ? "" : " nogui")}";
                    break;
                case 2:
                    Arg = $"-Xms{(int)MemorySlider.Value}M -Xmx{(int)MemorySlider.Value}M -jar \"{FileNameTextBox.Text}\"{(((bool)ShowGUICheckBox.IsChecked) ? "" : " nogui")}";
                    break;
                case 3:
                    Arg = $"-Xms{(int)MemorySlider.Value} -Xmx{(int)MemorySlider.Value} {VelocityArg} -jar \"{FileNameTextBox.Text}\"{(((bool)ShowGUICheckBox.IsChecked) ? "" : " nogui")}";
                    break;
            }
            ArgTextBox.Text = Arg;
        }

        private void CustomButton_Click_1(object sender, RoutedEventArgs e)
        {
            GeneratedArgu(ArgTextBox.Text);
            Close();
        }

        private void CustomButton_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }
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
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
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
        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
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

        private void RegisterResizeThumbEvents()
        {
            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is Grid grid)
                {
                    foreach (var element in grid.Children)
                    {
                        if (element is System.Windows.Controls.Primitives.Thumb thumb)
                        {
                            // 通过 Grid.Row 和 Grid.Column 判断位置
                            int row = Grid.GetRow(thumb);
                            int col = Grid.GetColumn(thumb);
                            int rowSpan = Grid.GetRowSpan(thumb);
                            int colSpan = Grid.GetColumnSpan(thumb);

                            if (row == 0 && col == 0)
                                thumb.DragDelta += Resize_TopLeft;
                            else if (row == 0 && col == 1)
                                thumb.DragDelta += Resize_Top;
                            else if (row == 0 && col == 2)
                                thumb.DragDelta += Resize_TopRight;
                            else if (row == 1 && col == 0 && rowSpan == 2)
                                thumb.DragDelta += Resize_Left;
                            else if (row == 1 && col == 2 && rowSpan == 2)
                                thumb.DragDelta += Resize_Right;
                            else if (row == 3 && col == 0)
                                thumb.DragDelta += Resize_BottomLeft;
                            else if (row == 3 && col == 1)
                                thumb.DragDelta += Resize_Bottom;
                            else if (row == 3 && col == 2)
                                thumb.DragDelta += Resize_BottomRight;
                        }
                    }
                }
            }
        }

        // 最小宽高
        private const double MinWidthValue = 800;
        private const double MinHeightValue = 450;

        // 各方向缩放逻辑
        private void Resize_Top(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = Height - e.VerticalChange;
            double newTop = Top + e.VerticalChange;
            if (newHeight >= MinHeightValue)
            {
                Height = newHeight;
                Top = newTop;
            }
        }
        private void Resize_Bottom(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = Height + e.VerticalChange;
            if (newHeight >= MinHeightValue)
            {
                Height = newHeight;
            }
        }
        private void Resize_Left(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = Width - e.HorizontalChange;
            double newLeft = Left + e.HorizontalChange;
            if (newWidth >= MinWidthValue)
            {
                Width = newWidth;
                Left = newLeft;
            }
        }
        private void Resize_Right(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            if (newWidth >= MinWidthValue)
            {
                Width = newWidth;
            }
        }
        private void Resize_TopLeft(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Left(sender, e);
        }
        private void Resize_TopRight(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Right(sender, e);
        }
        private void Resize_BottomLeft(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Left(sender, e);
        }
        private void Resize_BottomRight(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Right(sender, e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OneSecondTimer.Stop();
            GeneratedArgu = delegate { };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var (Total, Used, Free) = MemoryHelperClass.GetMemoryInfo();
            MemorySlider.Maximum = Total;
            UpdateMemoryUsage();
            OneSecondTimer.Start();
        }

        private void MemorySlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
