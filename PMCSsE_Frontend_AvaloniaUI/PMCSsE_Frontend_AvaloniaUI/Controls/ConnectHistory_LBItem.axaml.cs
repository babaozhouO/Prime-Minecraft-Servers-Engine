using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using PMCSsE_Frontend_AvaloniaUI.Models;
using PMCSsE_Frontend_AvaloniaUI.Modules;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using PMCSsE_Frontend_AvaloniaUI.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class ConnectHistory_LBItem : UserControl
{

    public ConnectHistory_LBItem()
    {
        InitializeComponent();
        SetButtonImage = SetImage;
    }
    public static readonly StyledProperty<ICommand> ConnectCommandProperty =
    AvaloniaProperty.Register<ConnectHistory_LBItem, ICommand>(nameof(ConnectCommand));
    public ICommand ConnectCommand
    {
        get => GetValue(ConnectCommandProperty);
        set => SetValue(ConnectCommandProperty, value);
    }
    public static readonly StyledProperty<NativeServerHistory> MyNativeServerHistoryProperty =
        AvaloniaProperty.Register<ConnectHistory_LBItem, NativeServerHistory>(nameof(MyNativeServerHistory));
    public NativeServerHistory MyNativeServerHistory
    {
        get => GetValue(MyNativeServerHistoryProperty);
        set => SetValue(MyNativeServerHistoryProperty, value);
    }

    public static readonly StyledProperty<string> DisplayTextProperty =
        AvaloniaProperty.Register<ConnectHistory_LBItem, string>(nameof(DisplayText));
    public string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public static readonly StyledProperty<string> PasswordProperty =
    AvaloniaProperty.Register<ConnectHistory_LBItem, string>(nameof(Password));
    public string Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly StyledProperty<bool?> SetControlVisibilityProperty =
    AvaloniaProperty.Register<ConnectHistory_LBItem, bool?>(nameof(SetControlVisibility),
defaultValue: false);

    public bool? SetControlVisibility
    {
        get => GetValue(SetControlVisibilityProperty);
        set => SetValue(SetControlVisibilityProperty, value);
    }
    public static readonly StyledProperty<IImage> SetButtonImageProperty =
    AvaloniaProperty.Register<ConnectHistory_LBItem, IImage>(nameof(SetButtonImage));
    public IImage SetButtonImage
    {
        get => GetValue(SetButtonImageProperty);
        set => SetValue(SetButtonImageProperty, value);
    }
    private bool Setting = false;
    private readonly IImage SetImage = new Bitmap(AssetLoader.Open(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/Settings.png")));
    private readonly IImage ApplyImage = new Bitmap(AssetLoader.Open(new Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/Finish.png")));
    private void Set_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!Setting)
        {
            SetButtonImage = ApplyImage;
            SetControlVisibility = true;
            Setting = true;
        }
        else
        {
            MyNativeServerHistory.Password = Password;
            StaticConfigManagerClass.SaveAPPConfig();
            Password = string.Empty;
            SetButtonImage = SetImage;
            SetControlVisibility = false;
            Setting = false;
        }
    }
    private void Delete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.DataContext is ConnectHistory_LBItemModel m)
        {
            if (this.Parent is ListBoxItem item)
            {
                if (item.Parent is ListBox lb)
                {
                    if (lb.DataContext is MainViewModel vm)
                    {
                        vm.DeleteConnectHistory_LBItemModel(m);
                    }
                }
            }
        }
    }
}