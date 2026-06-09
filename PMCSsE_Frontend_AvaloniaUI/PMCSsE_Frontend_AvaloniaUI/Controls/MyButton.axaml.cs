using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

/// <summary>
/// 支持显示文本和图片的自定义按钮控件。
/// </summary>
public partial class MyButton : Button
{
    /// <summary>
    /// 初始化 MyButton 控件。
    /// </summary>
    public MyButton()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Text 属性的 StyledProperty 定义。
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MyButton, string>(
            nameof(Text),
            defaultValue: "这是一个按钮");

    /// <summary>
    /// 按钮上显示的文本。
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    /// <summary>
    /// Image 属性的 StyledProperty 定义。
    /// </summary>
    public static readonly StyledProperty<IImage> ImageProperty =
    AvaloniaProperty.Register<MyButton, IImage>(
        nameof(Image),
        defaultValue: new Bitmap(AssetLoader.Open(new System.Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/About.png"))));

    /// <summary>
    /// 按钮上显示的图片。
    /// </summary>
    public IImage Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
}