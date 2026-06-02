using Avalonia;
using Avalonia.Controls;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class MCServerManager_LBItem : UserControl
{
    public MCServerManager_LBItem()
    {
        InitializeComponent();
    }
    public string ManagerID
    {
        get => GetValue(ManagerIDProperty);
        set => SetValue(ManagerIDProperty, value);
    }

    public static readonly StyledProperty<string> ManagerIDProperty =
        AvaloniaProperty.Register<MCServerManager_LBItem, string>(nameof(ManagerID));
    public string ServerName
    {
        get => GetValue(ServerNameProperty);
        set => SetValue(ServerNameProperty, value);
    }

    public static readonly StyledProperty<string> ServerNameProperty =
        AvaloniaProperty.Register<MCServerManager_LBItem, string>(nameof(ServerName));
    public string MCServerType
    {
        get => GetValue(MCServerTypeProperty);
        set => SetValue(MCServerTypeProperty, value);
    }

    public static readonly StyledProperty<string> MCServerTypeProperty =
        AvaloniaProperty.Register<MCServerManager_LBItem, string>(nameof(MCServerType));

    public string MCServerRunningState
    {
        get => GetValue(MCServerRunningStateProperty);
        set => SetValue(MCServerRunningStateProperty, value);
    }

    public static readonly StyledProperty<string> MCServerRunningStateProperty =
        AvaloniaProperty.Register<MCServerManager_LBItem, string>(nameof(MCServerRunningState));
}