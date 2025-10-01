using System.Xml.Serialization;
using Avalonia;
using Avalonia.Controls;

public abstract class SidePanelElement : StackPanel
{
    public Size ElementSize { get; set; }
    public string Header { get; set; }
    public string IconPath { get; set; }

    public abstract void UpdateSettings();

}