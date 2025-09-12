
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

public class Explorer : SidePanelElement
{
    public string WorkspacePath;

    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    StackPanel FolderNotOpened;

    TextBlock textBlock;
    Button button;

    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is not <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    StackPanel FileExplore;
    public Explorer()
    {
        Header = "Explorer";
        InitializeComponent();
    }

    public void InitializeComponent()
    {
        FolderNotOpened = new StackPanel()
        {
            IsVisible = false,
        };

        StackPanel mainPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };
        textBlock = new TextBlock()
        {
            Margin = new Thickness(0, 20, 0, 10),
            Text = "You have bit yet opened a folder.",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 14,
        };
        mainPanel.Children.Add(textBlock);

        button = new Button()
        {
            Margin = new Thickness(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            Content = "Open Folder",
            FontSize = 14,
        };
        button.Click += (s, e) => OpenFolder();
        mainPanel.Children.Add(button);

        FolderNotOpened.Children.Add(mainPanel);

        FileExplore = new StackPanel()
        {
            IsVisible = false,
        };

        Children.Add(FolderNotOpened);
        Children.Add(FileExplore);

        UpdateExplore();
    }

    public void UpdateExplore()
    {
        if (string.IsNullOrEmpty(WorkspacePath))
        {
            FolderNotOpened.IsVisible = true;
            FileExplore.IsVisible = false;
        }
        else
        {
            FolderNotOpened.IsVisible = false;
            FileExplore.IsVisible = true;
        }
    }

    public void OpenFolder()
    {

    }

    public override void UpdateSettings()
    {
        button.Foreground = Application.Current.Resources.GetResource("button.foreground");
        button.Background = Application.Current.Resources.GetResource("button.background");
        textBlock.Foreground = Application.Current.Resources.GetResource("sidePanel.foreground");
    }
}