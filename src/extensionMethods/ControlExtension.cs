
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

public static class ControlExtension
{
    public static void AddHoverBackground(this TemplatedControl control, IBrush normal, IBrush hover)
    {
        HoverStyleHelper.CreateHoverBackgroundStyle<Button>(normal, hover, control.Name);
    }
    public static void AddHoverBackground(this TemplatedControl control, string normalKey, string hoverKey)
    {
        IBrush normal = (IBrush)Application.Current.Resources[normalKey];
        IBrush hover = (IBrush)Application.Current.Resources[hoverKey];
        AddHoverBackground(control, normal, hover);
    }
}