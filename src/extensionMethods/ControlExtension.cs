
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
        IBrush normal = Application.Current.Resources.GetResource(normalKey);
        IBrush hover = Application.Current.Resources.GetResource(hoverKey);
        AddHoverBackground(control, normal, hover);
    }
}