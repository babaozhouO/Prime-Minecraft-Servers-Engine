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

using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PMCSsE_FrontendAndBackendCommunicator;
using PMCSsE_Frontend_WPF.Modules;

namespace PMCSsE_Frontend_WPF.Windows
{
    /// <summary>
    /// AddNewMCServerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddNewMCServerWindow : Window
    {
        internal event Action AddNewMCServer = delegate { }; // 事件，用于通知添加新服务器
        private readonly MCServerManagerConfig MCServerManagerConfig;

        private bool IsButtonClicked = false;
        internal AddNewMCServerWindow(MCServerManagerConfig mCServerManagerConfig)
        {
            InitializeComponent();
            RegisterResizeThumbEvents();
            MCServerManagerConfig = mCServerManagerConfig;
        }

        private void AddNewServerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("确认设置好了吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            MCServerManagerConfig.MCServerName = MCServerNameTextBox.Text;
            MCServerManagerConfig.MCServerType = MCServerTypeComboBox.Text;
            MCServerManagerConfig.MCServerDirectory = ThisMCServerDirectoryTextBox.Text;
            MCServerManagerConfig.JavaPath = JavaPathTextBox.Text;
            MCServerManagerConfig.StartUpArgument = ThisMCServerStartUpArgumentTextBox.Text;
            // 添加新服务器到配置列表
            StaticMCServerManagerConfigs.MCServerManagerConfigsList.Add(MCServerManagerConfig);

            AddNewMCServer();
            IsButtonClicked = true;
            this.Close();
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("真的要放弃吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            IsButtonClicked = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsButtonClicked)
            {
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("真的要放弃吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        private void OpenArgumentGeneratorWindowImageButton_Click(object sender, RoutedEventArgs e)
        {
            StartArgumentGeneratorWindow startArgumentGeneratorWindow = new(ThisMCServerDirectoryTextBox.Text);
            startArgumentGeneratorWindow.GeneratedArgu += (Argu) =>
            {
                ThisMCServerStartUpArgumentTextBox.Text = Argu;
            };
            startArgumentGeneratorWindow.Show();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new() { Title = "选择服务端目录", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
            openFolderDialog.FolderOk += (sender, e) =>
            {
                ThisMCServerDirectoryTextBox.Text = openFolderDialog.FolderName;
            };
            openFolderDialog.ShowDialog();
        }


        private void ImageButton_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new() { Title = "选择Java安装目录下bin文件夹中的javaw.exe", Filter = "Java可执行文件(javaw.exe)|javaw.exe", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) };
            openFileDialog.FileOk += (sender, e) =>
            {
                JavaPathTextBox.Text = openFileDialog.FileName;
            };
            openFileDialog.ShowDialog();
        }

        private void ImageButton_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("输入服务端目录后可自动识别类型\n若识别失败或不在列表中\n请选择Vanilla(原版)或基于的上游服务端\n例如:Leaves基于Paper，若无Leaves选Paper", "说明", MessageBoxButton.OK, MessageBoxImage.Information);
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

    }
}
