using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PMCSsE_Frontend_AvaloniaUI.Models;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using PMCSsE_Frontend_AvaloniaUI.Views;
using System;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class Message : UserControl
{
    private readonly DispatcherTimer CountDown = new() { Interval=TimeSpan.FromSeconds(10)};
    public Message()
    {
        InitializeComponent();
        CountDown.Tick += (sender,e) =>
        {
            CountDown.Stop();
            if (this.DataContext is MessageModel m)
            {
                if (this.Parent is ListBoxItem lbt)
                {
                    if (lbt.Parent is ListBox lb)
                    {
                        if (lb.DataContext is MainViewModel vm)
                        {
                            vm.DeleteMessage(m);
                        }
                    }
                }
            }
        };
        CountDown.Start();
    }

    public string MessageString
    {
        get => GetValue(MessageStringProperty);
        set => SetValue(MessageStringProperty, value);
    }

    public static readonly StyledProperty<string> MessageStringProperty =
        AvaloniaProperty.Register<ConnectHistory_LBItem, string>(nameof(MessageString));

}