using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaEdit.TextMate;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;
using ReactiveUI.Avalonia;
using TextMateSharp.Grammars;

namespace PMCSsE_Frontend_AvaloniaUI.Views;

/// <summary>
/// 主视图控件，承载前端的主要 UI 布局和交互逻辑。
/// </summary>
public partial class MainView : ReactiveUserControl<MainViewModel>
{
    /// <summary>
    /// 初始化主视图，设置 DataContext 并配置日志编辑器的语法高亮。
    /// </summary>
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
        ServerLogsShower.DocumentChanged += ServerLogsShower_DocumentChanged;
        ServerLogsShower.Document.LineCountChanged += Document_LineCountChanged;
        ServerLogsShower.Document.Changing += Document_Changing;
    }

    private void Document_Changing(object? sender, AvaloniaEdit.Document.DocumentChangeEventArgs e)
    {
        
    }

    private void ServerLogsShower_DocumentChanged(object? sender, AvaloniaEdit.Document.DocumentChangedEventArgs e)
    {
        e.OldDocument.LineCountChanged -= Document_LineCountChanged;
        e.OldDocument.Changing -= Document_Changing;
        e.NewDocument.LineCountChanged += Document_LineCountChanged;
        e.NewDocument.Changing += Document_Changing;
    }

    private void Document_LineCountChanged(object? sender, System.EventArgs e)
    {
        
    }

    private void ServerLogsShower_TextChanged(object? sender, System.EventArgs e)
    {
        if (ServerLogsShower.CaretOffset == ServerLogsShower.Document.TextLength)//光标在末尾
        {
            ServerLogsShower.ScrollToLine(ServerLogsShower.LineCount - 1);
        }
        else
        {

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

    private void ResetChartZoom_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (var axis in CPUChart.XAxes)
        {
            axis.MinLimit = null;
            axis.MaxLimit = null;
        }

        foreach (var axis in MemoryChart.XAxes)
        {
            axis.MinLimit = null;
            axis.MaxLimit = null;
        }
    }
}