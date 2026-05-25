using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class ImageButton : Button
{
    public ImageButton()
    {
        InitializeComponent();
    }
    public static readonly StyledProperty<IImage> Image_IBProperty =AvaloniaProperty.Register<ImageButton, IImage>(nameof(Image_IB));

    public IImage Image_IB
    {
        get => GetValue(Image_IBProperty);
        set => SetValue(Image_IBProperty, value);
    }
}