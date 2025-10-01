
using Avalonia.Controls;

public abstract class TopPaletteElement : Panel
{
    public abstract string ElementName { get; protected set; }
    public Action ClosePalette;
    public abstract void OpenElement();
}