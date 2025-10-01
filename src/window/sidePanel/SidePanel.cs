
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;

public class SidePanel : ControlElement<SidePanel>
{
    List<SidePanelElement> panelElements = new List<SidePanelElement>();

    public Grid mainGrid;
    private TabControl tabControl;
    private Button toggleButton;

    public GridSplitter Splitter;
    public Dock Dock;
    private double originalWidth;
    private bool isCollapsed = false;

    public double innerHeight;

    // Events
    public event Action Collapsed;
    public event Action Expanded;

    public SidePanel(Dock dock, double width = 250)
    {
        Initialize();
        Dock = dock;
        originalWidth = width;
        MinWidth = 170;

        Background = Application.Current.Resources.GetResource("sidepanel.background");
        mainGrid = new Grid();
        // Create a grid with two rows: button row and tab row
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // button
        mainGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // tabs

        // TabControl for tabs
        tabControl = new TabControl();
        Grid.SetColumn(tabControl, 1);
        mainGrid.Children.Add(tabControl);

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
        mainGrid.Children.Add(toggleButton);

        // GridSplitter
        Splitter = new GridSplitter
        {
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Gray)
        };

        SizeChanged += (sender, args) =>
        {
            foreach (var item in panelElements)
            {
                item.ElementSize = args.NewSize - new Size(0, tabControl.Height / 2);
            }
        };

        Children.Add(mainGrid);
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
        element.GotFocus += (sender, e) => { KeybindingManager.ActiveContext = element.Header; };
        tabControl.Items.Add(tabItem);
        panelElements.Add(element);
    }

    public void UpdateSettings()
    {
        Background = Application.Current.Resources.GetResource("sidepanel.background");

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

    public void WindowChangedSize(Size windowSize, Size otherSideSize)
    {
        double availableWidth = windowSize.Width - otherSideSize.Width;

        // Enforce min/max range
        double maxAllowedWidth = availableWidth * 0.45d;
        if (maxAllowedWidth < 170)
        {
            maxAllowedWidth = 170; // sidebar can't shrink below min
        }

        MaxWidth = maxAllowedWidth;
        MinWidth = 170;

        // Also apply to parent grid column if needed
        if (parentColumn != null)
        {
            parentColumn.MaxWidth = MaxWidth;
            parentColumn.MinWidth = MinWidth;
        }
    }
}