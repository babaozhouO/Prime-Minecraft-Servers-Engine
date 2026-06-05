using Avalonia.Controls;
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
    }

    private void ServerLogsShower_TextChanged(object? sender, System.EventArgs e)
    {
        if (ServerLogsShower.CaretOffset == ServerLogsShower.Document.TextLength)//光标在末尾
        {
            ServerLogsShower.ScrollToLine(ServerLogsShower.LineCount-1);
        }
    }
}