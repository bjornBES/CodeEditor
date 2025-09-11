using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;

public static class HoverStyleHelper
{
    public static void CreateHoverBackgroundStyle<T>(IBrush normal, IBrush hover, string name) where T : Control
    {
        Application.Current.Styles.Add(new Styles
        {
            // Normal background
            new Style(x => x.OfType<T>().Name(name))
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty, normal),
                },
                Children = {
                    new Style(x => x.OfType<T>().Nesting().Class(":pointerover"))
                    {
                        Setters =
                        {
                            new Setter(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty, hover),
                        }
                    }
                }
            },
        });

    }
}

// Usage example:
// var style = HoverStyleHelper.CreateHoverBackgroundStyle<Button>(Brushes.White, Brushes.LightBlue);
// button.Styles.Add(style);
