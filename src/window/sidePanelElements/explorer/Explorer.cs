
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using DynamicData;
using lib.debug;
using ReactiveUI;

public class ExplorerNode
{
    public string Header { get; set; }
    public string Path { get; set; }
    public List<ExplorerNode> Children { get; set; } = new List<ExplorerNode>();
    public bool IsDirectory { get; set; }
}
public class Explorer : SidePanelElement
{
    public string WorkspacePath;

    ContextMenu treeItemMenu;

    TreeView treeView;
    ScrollViewer scrollViewer;

    StackPanel MainPanel;

    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    StackPanel FolderNotOpened;

    TextBlock textBlock;
    Button button;

    // ScrollViewer scroll;


    /// <summary>
    /// This will visible if the <see cref="WorkspacePath"/> is not <seealso cref="string.IsNullOrEmpty(string?)"/>
    /// </summary>
    Border FileExplore;

    public Explorer()
    {
        Header = "Explorer";
        InitializeComponent();
        Application.Current.Resources["TreeViewItemIndent"] = 2.0;
    }

    public override void EndInit()
    {
        UpdateTreeContents();
        base.EndInit();
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
            Name = "explorerButton",
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

        treeView = new TreeView()
        {
            IsVisible = true,
            SelectionMode = SelectionMode.Multiple,
        };
        scrollViewer = new ScrollViewer()
        {
            IsVisible = true,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            AllowAutoHide = true,
        };
        treeView.SelectionChanged += (s, e) =>
        {
            if (treeView.SelectedItem == null)
            {
                return;
            }
            TreeViewItem viewItem = (TreeViewItem)treeView.SelectedItem;
            ExplorerNode explorerNode = (ExplorerNode)viewItem.Tag;
            if (explorerNode.IsDirectory)
            {
                e.Handled = true;
                viewItem.IsExpanded = !viewItem.IsExpanded;
            }
            else
            {
                e.Handled = true;
                CommandManager.ExecuteCommand("editor.action.open", explorerNode.Path);
            }
        };

        treeItemMenu = new ContextMenu()
        {
        };
        ReactiveCommand<string, Unit> command;
        command = ReactiveCommand.Create<string>(PerformCommand);
        treeItemMenu.Items.Add(new MenuItem()
        {
            Command = command,
            CommandParameter = "helloworld",
            Header = "Hello world"
        });

        UpdateExplore();
    }

    void PerformCommand(string commandId)
    {
        CommandEntry commandEntry = CommandManager.GetCommandEntry(commandId);
        if (commandEntry == null)
        {
            DebugWriter.WriteLine("KeybindingManager", $"command Entry is null from {commandId}");
            throw new ArgumentNullException("commandEntry");
        }

        CommandManager.ExecuteCommandGetArgs(commandId);
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

    public void OpenFolder(string path = "")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        if (string.IsNullOrEmpty(path))
        {
            path = MainWindow.OpenDialog(DialogType.SelectFolder);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            CommandManager.ExecuteCommand("editor.action.open.folder", path);
        }

        AppPaths.SetWorkspacePath(path);
        WorkspacePath = path;
        DebugWriter.WriteLine("Explorer", $"Opened Workspace: {WorkspacePath}");
        UpdateExplore();
        UpdateTreeContents();
        stopwatch.Stop();
        DebugWriter.WriteLine("Explorer", $"OpenFolder: Elapsed {stopwatch.ElapsedMilliseconds} ms path = {path}");
    }

    public void UpdateTreeContents()
    {
        treeView.Items.Clear();
        Stopwatch stopwatch = Stopwatch.StartNew();
        Stopwatch build = Stopwatch.StartNew();
        ExplorerNode rootNode = BuildNode(WorkspacePath, true);
        TreeViewItem control = BuildNodeContents(rootNode, 0, true);
        // control.IsExpanded = true;
        build.Stop();
        DebugWriter.WriteLine("Explorer", $"builds: Elapsed {build.ElapsedMilliseconds} ms");
        Stopwatch other1 = Stopwatch.StartNew();
        treeView.Items.Add(control);
        scrollViewer.Content = treeView;
        FileExplore.Child = scrollViewer;
        other1.Stop();
        DebugWriter.WriteLine("Explorer", $"other1: Elapsed {other1.ElapsedMilliseconds} ms");

        stopwatch.Stop();
        DebugWriter.WriteLine("Explorer", $"UpdateTreeContents: Elapsed {stopwatch.ElapsedMilliseconds} ms");
        UpdateSettings();
    }

    public override void UpdateSettings()
    {
        button.Foreground = Application.Current.Resources.GetResource("button.foreground");
        button.Background = Application.Current.Resources.GetResource("button.background");
        button.AddHoverBackground("button.background", "button.hover.background");
        textBlock.Foreground = Application.Current.Resources.GetResource("sidepanel.foreground");
        MainPanel.Background = Application.Current.Resources.GetResource("sidepanel.background");
        Background = Application.Current.Resources.GetResource("sidepanel.background");

        if (FileExplore.Child != null)
        {
            // ScrollViewer scrollViewer = (ScrollViewer)FileExplore.Child;
            FileExplore.Height = ElementSize.Height;
            scrollViewer.Height = ElementSize.Height;
            treeView.Height = ElementSize.Height;
        }
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

    public ExplorerNode BuildNode(string path, bool first = false)
    {
        Stopwatch stopwatch = null;
        if (first == true)
        {
            stopwatch = Stopwatch.StartNew();
        }
        var node = new ExplorerNode { Header = Path.GetFileName(path), Path = path, IsDirectory = Directory.Exists(path) };

        if (node.IsDirectory)
        {
            foreach (var dir in Directory.GetDirectories(path).OrderBy(d => d))
            {
                node.Children.Add(BuildNode(dir));
            }
            foreach (var file in Directory.GetFiles(path).OrderBy(f => f))
            {
                node.Children.Add(new ExplorerNode
                {
                    Header = Path.GetFileName(file),
                    Path = file,
                    IsDirectory = false
                });
            }
        }

        if (first == true)
        {
            stopwatch.Stop();
            DebugWriter.WriteLine("Explorer", $"BuildNode: Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }
        return node;
    }

    public TreeViewItem BuildNodeContents(ExplorerNode node, int indent, bool first = false)
    {
        Stopwatch stopwatch = null;
        if (first == true)
        {
            stopwatch = Stopwatch.StartNew();
        }

        TreeViewItem root = new TreeViewItem() { Header = node.Header, Tag = node };
        root.ContextMenu = treeItemMenu;

        foreach (var child in node.Children)
        {
            var childControl = BuildNodeContents(child, indent + 1);
            root.Items.Add(childControl);
        }

        if (first == true)
        {
            stopwatch.Stop();
            DebugWriter.WriteLine("Explorer", $"BuildNodeContents: Elapsed {stopwatch.ElapsedMilliseconds} ms");
        }
        return root;
    }

    public void UpdateNode(string path, string value)
    {

    }
}