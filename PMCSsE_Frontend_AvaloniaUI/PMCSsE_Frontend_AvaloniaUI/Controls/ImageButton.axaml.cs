using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

/// <summary>
/// 支持显示图片的自定义按钮控件。
/// </summary>
public partial class ImageButton : Button
{
    /// <summary>
    /// 初始化 ImageButton 控件。
    /// </summary>
    public ImageButton()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Image_IB 属性的 StyledProperty 定义。
    /// </summary>
    public static readonly StyledProperty<IImage> Image_IBProperty =AvaloniaProperty.Register<ImageButton, IImage>(nameof(Image_IB));

    /// <summary>
    /// 按钮上显示的图片。
    /// </summary>
    public IImage Image_IB
    {
        get => GetValue(Image_IBProperty);
        set => SetValue(Image_IBProperty, value);
    }
}