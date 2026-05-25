using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace PMCSsE_Frontend_AvaloniaUI.Controls;

public partial class MyButton : Button
{
    public MyButton()
    {
        InitializeComponent();
    }
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MyButton, string>(
            nameof(Text),
            defaultValue: "这是一个按钮");

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly StyledProperty<IImage> ImageProperty =
    AvaloniaProperty.Register<MyButton, IImage>(
        nameof(Image),
        defaultValue: new Bitmap(AssetLoader.Open(new System.Uri("avares://PMCSsE_Frontend_AvaloniaUI/Icons/About.png"))));

    public IImage Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
}