using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PMCSsE_Frontend_AvaloniaUI.Modules;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class MCServerManagerConfig_LBItem : UserControl
{
    public MCServerManagerConfig_LBItem()
    {
        InitializeComponent();
    }
    public string ManagerID
    {
        get => GetValue(ManagerIDProperty);
        set => SetValue(ManagerIDProperty, value);
    }

    public static readonly StyledProperty<string> ManagerIDProperty =
        AvaloniaProperty.Register<MCServerManagerConfig_LBItem, string>(nameof(ManagerID));
    public string ServerName
    {
        get => GetValue(ServerNameProperty);
        set => SetValue(ServerNameProperty, value);
    }

    public static readonly StyledProperty<string> ServerNameProperty =
        AvaloniaProperty.Register<MCServerManagerConfig_LBItem, string>(nameof(ServerName));
    public string MCServerType
    {
        get => GetValue(MCServerTypeProperty);
        set => SetValue(MCServerTypeProperty, value);
    }

    public static readonly StyledProperty<string> MCServerTypeProperty =
        AvaloniaProperty.Register<MCServerManagerConfig_LBItem, string>(nameof(MCServerType));


}