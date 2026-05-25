using Avalonia.Controls;
using PMCSsE_Frontend_AvaloniaUI.ViewModels;

namespace PMCSsE_Frontend_AvaloniaUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}