
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;

public class SidePanel : Grid
{
    List<SidePanelElement> panelElements = new List<SidePanelElement>();

    private TabControl tabControl;
    private Button toggleButton;

    public GridSplitter Splitter;
    public Dock Dock;
    private double originalWidth;
    private bool isCollapsed = false;

    // Events
    public event Action Collapsed;
    public event Action Expanded;

    public SidePanel(Dock dock, double width = 250)
    {
        Dock = dock;
        originalWidth = width;

        Background = Application.Current.Resources.GetResource("sidePanel.background");

        // Create a grid with two rows: button row and tab row
        RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // button
        RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // tabs

        // TabControl for tabs
        tabControl = new TabControl();
        SetColumn(tabControl, 1);
        Children.Add(tabControl);

        // Toggle button
        toggleButton = new Button
        {
            Width = 20,
            Height = 20,
            Content = isCollapsed ? "▶" : "◀", // arrows for collapse/expand
            HorizontalAlignment = dock == Dock.Left ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(2)
        };
        toggleButton.Click += (_, __) =>
        {
            Toggle();
        };
        Grid.SetRow(toggleButton, 0);
        Children.Add(toggleButton);

        // GridSplitter
        Splitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Gray)
        };
    }

    private ColumnDefinition parentColumn;

    public void AttachToColumn(ColumnDefinition column)
    {
        parentColumn = column;
    }


    public void AddItem(string header, Control control)
    {
        TabItem tabItem = new TabItem()
        {
            Header = header,
            Content = control
        };
        tabControl.Items.Add(tabItem);
    }

    public void AddItem(SidePanelElement element)
    {
        TabItem tabItem = new TabItem()
        {
            Header = element.Header,
            Content = element
        };
        tabControl.Items.Add(tabItem);
        panelElements.Add(element);
    }

    public void UpdateSettings()
    {
        Background = Application.Current.Resources.GetResource("sidePanel.background");

        foreach (var item in panelElements)
        {
            item.UpdateSettings();
        }
    }

    // Collapse the panel
    public void Collapse(ColumnDefinition column = null)
    {
        var col = column ?? parentColumn;
        if (col == null) return;
        
        originalWidth = col.Width.Value;
        col.Width = new GridLength(0);
        Splitter.IsEnabled = false;
        isCollapsed = true;
        toggleButton.Content = Dock == Dock.Left ? "▶" : "◀";
        Collapsed?.Invoke();
    }

    // Expand the panel
    public void Expand(ColumnDefinition column = null)
    {
        var col = column ?? parentColumn;
        if (col == null) return;

        col.Width = new GridLength(originalWidth);
        Splitter.IsEnabled = true;
        isCollapsed = false;
        toggleButton.Content = Dock == Dock.Left ? "◀" : "▶";
        Expanded?.Invoke();
    }

    public void Toggle()
    {
        if (isCollapsed)
            Expand(parentColumn);
        else
            Collapse(parentColumn);
    }
}