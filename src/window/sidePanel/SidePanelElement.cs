using System.Xml.Serialization;
using Avalonia.Controls;

public abstract class SidePanelElement : StackPanel
{
    public string Header { get; set; }

    public abstract void UpdateSettings();
}