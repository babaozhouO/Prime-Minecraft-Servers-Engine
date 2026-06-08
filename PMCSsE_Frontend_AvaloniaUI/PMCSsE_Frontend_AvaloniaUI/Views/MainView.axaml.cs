using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using TextMateSharp.Grammars;

namespace PMCSsE_Frontend_AvaloniaUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        ServerLogsShower.Encoding = System.Text.Encoding.UTF8;
        ServerLogsShower.Options.HighlightCurrentLine = true;
        RegistryOptions options = new(ThemeName.VisualStudioDark);
        var installation = ServerLogsShower.InstallTextMate(options);
        installation.SetGrammar("source.log");
        ServerLogsShower.TextChanged += ServerLogsShower_TextChanged;
        ServerLogsShower.Loaded += (s, e) =>
        {
            var scrollViewer = ServerLogsShower.FindDescendantOfType<ScrollViewer>();
            scrollViewer?.ScrollChanged += OnEditorScrollChanged;
        };
    }

    private void ServerLogsShower_TextChanged(object? sender, System.EventArgs e)
    {
        if (ServerLogsShower.CaretOffset == ServerLogsShower.Document.TextLength)//光标在末尾
        {
            ServerLogsShower.ScrollToLine(ServerLogsShower.LineCount - 1);
        }
    }
    private void OnEditorScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.OffsetDelta.Y == 0)
            return;
        if (sender is ScrollViewer scrollViewer)
        {
            // 检查垂直偏移量是否小于或等于0，考虑到浮点数精度，使用一个小的容差进行判断。
            bool isAtTop = scrollViewer.Offset.Y <= 0.1;
            if (isAtTop)
            {
                if (GetOlderLogsButton.Command == null || GetOlderLogsButton.Command.CanExecute(null) != true)
                    return;
                GetOlderLogsButton.Command.Execute(null);
            }
        }
    }
}