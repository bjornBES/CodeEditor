
using System.ComponentModel.DataAnnotations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using DynamicData;

public class ExplorerNodeItem
{
    public StackPanel HeaderPanel;

    public StackPanel CreateLayout(ExplorerNode explorerNode, StackPanel childrenPanel)
    {
        HeaderPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            IsVisible = true,
        };

        TextBlock header = new TextBlock()
        {
            Text = explorerNode.Header,
            Foreground = Application.Current.Resources.GetResource("sidepanel.foreground"),
            IsVisible = true,
        };

        Button HeaderButton = new Button()
        {
            Content = header,
        };

        HeaderButton.Click += (s, e) =>
        {
            DebugWriter.WriteLine("Explorer", $"Selected: {explorerNode.Path}");
            if (!explorerNode.IsDirectory)
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    CommandManager.ExecuteCommand("editor.open.file", explorerNode.Path);
                }
            }
            else
            {
                explorerNode.IsExpanded = !explorerNode.IsExpanded;
                // toggle.Text = explorerNode.IsExpanded ? "-" : "+";
                childrenPanel.IsVisible = explorerNode.IsExpanded;
            }
        };

        HeaderPanel.Children.Add(HeaderButton);

        return HeaderPanel;
    }
}

public class ExplorerNode
{
    public string Header { get; set; }
    public string Path { get; set; }
    public int Level { get; set; } = 0;
    public bool IsExpanded { get; set; } = false;
    public List<ExplorerNode> Children { get; set; } = new List<ExplorerNode>();
    public bool IsDirectory { get; set; }
}

public class Explorer : SidePanelElement
{
    public string WorkspacePath;

    StackPanel MainPanel;

    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    StackPanel FolderNotOpened;

    TextBlock textBlock;
    Button button;


    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is not <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    Border FileExplore;

    private int IndentStep = 16;
    public Explorer()
    {
        Header = "Explorer";
        InitializeComponent();
        Application.Current.Resources["TreeViewItemIndent"] = 2.0;
    }

    public void InitializeComponent()
    {
        FolderNotOpened = new StackPanel()
        {
            IsVisible = false,
        };

        MainPanel = new StackPanel()
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
        MainPanel.Children.Add(textBlock);

        button = new Button()
        {
            Margin = new Thickness(0, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            Content = "Open Folder",
            FontSize = 14,
        };
        button.Click += (s, e) => OpenFolder();
        MainPanel.Children.Add(button);

        FolderNotOpened.Children.Add(MainPanel);

        FileExplore = new Border()
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
        string path = MainWindow.OpenDialog(DialogType.SelectFolder);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        WorkspacePath = path;
        DebugWriter.WriteLine("Explorer", $"Opened Workspace: {WorkspacePath}");
        AppPaths.SetWorkspacePath(path);
        MainWindow.EditorConfigsSettingsManager.ChangeLoaclPath(AppPaths.WorkspaceConfigFilePath);
        MainWindow.EditorConfigsSettingsManager.Load();

        UpdateExplore();
        UpdateTreeContents();
    }

    public void UpdateTreeContents()
    {
        ExplorerNode rootNode = BuildNode(WorkspacePath);
        Control control = BuildNodeContents(rootNode, 0);
        ScrollViewer scroll = new ScrollViewer()
        {
            Content = control,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            Height = ElementSize.Height,
        };
        FileExplore.Child = scroll;
    }

    public override void UpdateSettings()
    {
        button.Foreground = Application.Current.Resources.GetResource("button.foreground");
        button.Background = Application.Current.Resources.GetResource("button.background");
        textBlock.Foreground = Application.Current.Resources.GetResource("sidepanel.foreground");
        MainPanel.Background = Application.Current.Resources.GetResource("sidepanel.background");
        Background = Application.Current.Resources.GetResource("sidepanel.background");
    }
    public void UpdateTreeSettings(TreeViewItem treeItem, Brush brush)
    {
        /*
        treeItem.Foreground = brush;
        foreach (var item in treeView.Items)
        {
            if (item.GetType() != typeof(TreeViewItem))
            {
                continue;
            }
            TreeViewItem _treeItem = (TreeViewItem)item;
            UpdateTreeSettings(_treeItem, brush);
        }
        */
    }

    public ExplorerNode BuildNode(string path)
    {
        var node = new ExplorerNode { Header = Path.GetFileName(path), Path = path, IsDirectory = Directory.Exists(path) };


        if (node.IsDirectory)
        {
            foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d)) node.Children.Add(BuildNode(dir));
            foreach (var file in Directory.GetFiles(path).OrderBy(f => f)) node.Children.Add(new ExplorerNode { Header = Path.GetFileName(file), Path = file, IsDirectory = false });
        }


        return node;
    }

    public Control BuildNodeContents(ExplorerNode node, int indent)
    {
        StackPanel panel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var childrenPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(IndentStep, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
        ExplorerNodeItem item = new ExplorerNodeItem();
        StackPanel stackPanel = item.CreateLayout(node, childrenPanel);

        panel.Children.Add(stackPanel);
        // Children container
        foreach (var child in node.Children)
        {
            var childControl = BuildNodeContents(child, 0);
            childrenPanel.Children.Add(childControl);
        }
        childrenPanel.IsVisible = node.IsExpanded; // collapse by default if needed
        panel.Children.Add(childrenPanel);

        return panel;
    }

    public void UpdateNode(string path, string value)
    {

    }
}