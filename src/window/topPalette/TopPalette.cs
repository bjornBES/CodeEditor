
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Input;
using lib.debug;

public class TopPalette : ControlElement<TopPalette>
{
    public TopPalette()
    {
        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Initialize();
        MinWidth = 400;
        MaxWidth = 800;
        IsVisible = false;
        Background = Brushes.Red;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Top;
    }

    public void WindowChangedSize(Size windowSize, PixelSize screenSize, Size topbarMenuSize)
    {
        // calculate desired width/height
        double desiredWidth = windowSize.Width * 0.60d;
        double desiredHeight = windowSize.Height * 0.50d;

        // apply min/max before assigning
        Width = Math.Clamp(desiredWidth, MinWidth, MaxWidth);
        Height = desiredHeight;

        DebugWriter.WriteLine("Top palette", $"Size {Width}, {Height}");

        // center horizontally
        double left = (windowSize.Width - Width) / 2;
        Canvas.SetLeft(this, left);

        // place near top (e.g. 15% down from window top)
        double top = topbarMenuSize.Height; // coming from the top bar menu
        Canvas.SetTop(this, top);
    }

    public void OnKeyDownPalette(object sender, KeyEventArgs e)
    {
        if (IsVisible == true)
        {
            if (e.Key == Key.Escape)
            {
                IsVisible = false;
            }
        }
    }

    public void OpenPalette()
    {
        IsVisible = true;
        Focus();
        GotFocus += (sender, e) => { KeybindingManager.ActiveContext = "topPalette"; };
    }
}