

using Avalonia;
using Avalonia.Controls;
using DynamicData.Kernel;
using TextMateSharp.Grammars;

public class EditorTabControl : Panel
{
    private readonly EditorGroup _group;
    private readonly TabControl _tabHost;
    private readonly RegistryOptions _registryOptions;
    public Button fileInfoTextBlock = null;

    public EditorTabControl(EditorGroup group, RegistryOptions registryOptions)
    {
        _registryOptions = registryOptions;
        _group = group;
        _tabHost = new TabControl();
        _tabHost.SelectionChanged += (s, e) =>
        {
            if (_tabHost.SelectedContent is FileEditorView)
            {
                (_tabHost.SelectedContent as FileEditorView).Editor.Focus();
                (_tabHost.SelectedContent as FileEditorView).Focus();
            }
        };
        Children.Add(_tabHost);
        UpdateTabs();
        fileInfoTextBlock = null;
    }

    public void UpdateTabs()
    {
        if (fileInfoTextBlock == null)
        {
            fileInfoTextBlock = new Button
            {
                Content = "Hello",
            };
            fileInfoTextBlock.Click += (s, e) =>
            {
                
            };
            CommandManager.ExecuteCommand("view.status.add.button", fileInfoTextBlock, Dock.Right);
        }
        int count = 0;
        foreach (var tab in _tabHost.Items)
        {
            if (tab is TabItem item && item.Content is FileEditorView view)
            {
                view.Editor.TextArea.OnCaretPositionChanged -= null;
                view.GotFocus -= null;
            }
        }
        _tabHost.Items.Clear();
        TabItem[] tabItems = _group.Tabs.Select(t =>
        {
            var header = t.Input.Title + (t.Input.IsDirty ? "*" : "");
            Control content = t.Input switch
            {
                FileEditorInput fileInput => new FileEditorView(fileInput, _registryOptions).Also((item) =>
                {
                    item.Editor.TextArea.OnCaretPositionChanged += (s, e) =>
                    {
                        item.UpdateFileInfo(fileInfoTextBlock);
                    };
                    item.GotFocus += (s, e) =>
                    {
                        item.UpdateFileInfo(fileInfoTextBlock);
                    };
                }),
                _ => new TextBlock { Text = $"Unsupported editor: {t.Input.Title}" }
            };

            return makeNewTabItem(header, content, count++);
        }).AsArray();
        foreach (var tab in tabItems)
        {
            _tabHost.Items.Add(tab);
        }
    }

    private TabItem makeNewTabItem(string header, Control control, int count)
    {
        TabItem tabItem = new TabItem
        {
            Header = header,
            Content = control,
            FontSize = 16,
            Height = 10,
            BorderThickness = new Thickness(1, 2, 1, 0),
            BorderBrush = Application.Current.Resources.GetResource("editor.tabs.items.border.background"),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Name = "tabItem" + count
        };

        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.hover.background", "pointerover"); // adding hover effect
        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.selected.background", "selected"); // adding selected effect

        return tabItem;
    }
}
