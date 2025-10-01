using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using System.Text.Json;
using TextMateSharp.Internal.Themes.Reader;
using AvaloniaEdit.Editing;
using Avalonia.LogicalTree;

public class EditorPanel : ControlElement<EditorPanel>
{
    public int PanelIndex = -1;
    TabControl tabControl;
    List<EditorTab> tabs;
    List<EditorTab> pinnedEditorTabs;

    RegistryOptions registryOptions;
    int count = 0;
    bool IsFocused = false;

    public EditorPanel(int panelIndex)
    {
        PanelIndex = panelIndex;
        InitializeComponent();

        registryOptions = new RegistryOptions(ThemeName.DarkPlus);
    }

    public void InitializeComponent()
    {
        Initialize();

        CommandManager.RegisterCommand("Pin editor", "editor.action.pinEditor.index", PinTabWithIndex);

        tabs = new List<EditorTab>();
        pinnedEditorTabs = new List<EditorTab>();

        tabControl = new TabControl()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            AutoScrollToSelectedItem = true,
            TabStripPlacement = Dock.Top,
        };
        tabControl.SelectionChanged += (sender, e) =>
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            TabItem tab = (TabItem)e.AddedItems[0];
            if (tab == null)
            {
                return;
            }
            EditorTab editorTab = tab.Content as EditorTab;

            TextEditor textEditor = editorTab.GetEditor();

            UpdateInstance("activeEditorIsPinned", editorTab);
            UpdateInstance("editorTextFocus", editorTab);
            UpdateInstance("editorFocus", editorTab);
        };
        Children.Add(tabControl);

        Focusable = true;
        GotFocus += (sender, e) => { KeybindingManager.ActiveContext = "editor"; };
    }

    public void NewTab(string filePath)
    {
        EditorTab tab = NewTab();
        tab.Load(filePath);
        tab.LoadSyntaxHighlighting(registryOptions);

        TabItem tabItem = new TabItem()
        {
            Header = tab.GetHeader(),
            Content = tab,
            FontSize = 16,
            Height = 10,
            BorderThickness = new Thickness(1, 2, 1, 0),
            BorderBrush = Application.Current.Resources.GetResource("editor.tabs.items.border.background"),
            CornerRadius = new CornerRadius(5, 5, 0, 0),
            Name = $"TabItem{count}"
        };
        count++;
        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.hover.background", "pointerover");
        tabItem.AddPseudoClassesBackground("editor.tabs.background", "editor.tabs.items.selected.background", "selected");
        // tabItem.Background = Application.Current.Resources.GetResource("editor.tabs.background");


        tab.AttachedToControl(tabControl);
        tabControl.Items.Add(tabItem);
        tabControl.SelectedItem = tabItem;
    }

    public EditorTab NewTab()
    {
        var tab = new EditorTab()
        {
        };
        tab.GetFocusAction += (tabPlacement) =>
        { 
            
        };
        tab.LostFocusAction += (tabPlacement) =>
        { 
            
        };
        tabs.Add(tab);
        return tab;
    }

    public EditorTab GetTab(int index)
    {
        if (index >= 0 && index < tabs.Count)
        {
            return tabs[index];
        }
        return null;
    }

    public EditorTab GetFocusedTab()
    {
        return tabs.FirstOrDefault(t => t.IsTabFocused);
    }

    public void UpdateSettings()
    {
        foreach (var tab in tabs)
        {
            tab.UpdateSettings();
        }
    }
    public void CloseCurrentTab()
    {
        int index = getCurrentTabIndex();
        CloseTab(index);
    }
    public void CloseTab(int index)
    {
        if (index >= 0 && index < tabs.Count)
        {
            var tab = tabs[index];
            if (tab.IsModified)
            {
                // Prompt to save changes
            }
            Children.Remove(tab);
            tabs.RemoveAt(index);
        }
    }
    public void SaveCurrentTab(string path = null)
    {
        int index = getCurrentTabIndex();
        SaveTab(index, path);
    }
    public void SaveTab(int index, string path = null)
    {
        if (index >= 0 && index < tabs.Count)
        {
            var tab = tabs[index];
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, tab.TextBuffer);
                tab.IsModified = false;
                tab.Load(path);
            }
            else if (!string.IsNullOrEmpty(tab.FilePath))
            {
                tab.Save();
            }
        }
    }

    public void ApplyRegistryOptions(RegistryOptions newOptions)
    {
        registryOptions = newOptions;

        foreach (var tab in tabs)
        {
            tab.ApplyTheme(registryOptions);
        }
    }
    private int getCurrentTabIndex()
    {
        return tabControl.SelectedIndex;
    }
    public void IndentCurrentDocument()
    {
        int index = getCurrentTabIndex();
        IndentDocument(index);
    }
    public void IndentDocument(int index)
    {
        EditorTab tab = GetTab(index);
        if (tab == null) return;

        API.TextDocument document = tab.TextDocument;
        EditorConfigs editor = MainWindow.EditorConfigsSettingsManager.Current;
        var avaloniaDoc = document.avaloniaDocument;
        int tabSize = editor.Editor.IndentWidth;
        bool useTabs = !editor.Editor.InsertSpaces;

        // Loop through all lines in the document
        for (int i = 1; i <= avaloniaDoc.LineCount; i++)
        {
            var line = avaloniaDoc.GetLineByNumber(i);
            IndentationManager.IndentLine("csharp", document, line, tabSize, useTabs);
        }
    }
    public void IndentLine()
    {
        int index = getCurrentTabIndex();
        IndentLine(index);
    }
    public void IndentLine(int index)
    {
        EditorTab tab = GetTab(index);
        if (tab != null)
        {
            API.TextDocument document = tab.TextDocument;
            EditorConfigs editor = MainWindow.EditorConfigsSettingsManager.Current;
            Caret caret = document.editorTab.Caret;
            IndentationManager.IndentLine("csharp", document, document.avaloniaDocument.GetLineByNumber(caret.Line), editor.Editor.IndentWidth, !editor.Editor.InsertSpaces);
        }
    }

    public void OnConfigChanged()
    {
        IBrush tabBackgroundColor = Application.Current.Resources.GetResource("editor.tabs.background");
        IBrush tabBorderBackgroundColor = Application.Current.Resources.GetResource("editor.tabs.items.border.background");
        tabControl.Background = tabBackgroundColor;
        // Application.Current.Resources["TabItemHeaderBackgroundUnselected"] = tabBackgroundColor;
        // Application.Current.Resources["TabItemHeaderBackgroundSelected"] = Application.Current.Resources.GetResource("editor.tabs.selected.background");
        // Application.Current.Resources["TabItemHeaderBackgroundUnselectedPointerOver"] = Application.Current.Resources.GetResource("editor.tabs.hover.background");
        // Application.Current.Resources["TabItemHeaderBackgroundSelectedPointerOver"] = Application.Current.Resources.GetResource("editor.tabs.hover+selected.background");

        for (int i = 0; i < tabControl.Items.Count; i++)
        {
            TabItem item = (TabItem)tabControl.Items[i];
            // item.Background = tabBackgroundColor;
            item.BorderBrush = tabBorderBackgroundColor;
        }

        foreach (EditorTab tab in tabs)
        {
            tab.OnConfigChanged();
        }
    }

    public void UpdateInfo()
    {
        Application.Current.Resources["tab.count"] = tabControl.ItemCount;
    }
    public void PinTab()
    {
        int index = getCurrentTabIndex();
        PinTab(tabs[index]);
    }
    public void PinTabWithIndex(int index)
    {
        PinTab(tabs[index]);
    }
    public void PinTab(EditorTab editorTab)
    {
        if (editorTab.IsPinned)
        {
            return;
        }

        pinnedEditorTabs.Add(editorTab);
        tabs.Remove(editorTab);
        TabItem tabItem = (TabItem)tabControl.Items.FirstOrDefault((x) =>
        {
            TabItem tab = (TabItem)x;
            EditorTab editor = (EditorTab)tab.Content;
            return editor != null && !editor.IsPinned && editor.TabPlacement == editorTab.TabPlacement;
        });
        tabControl.Items.Remove(tabItem);
        tabControl.Items.Insert(pinnedEditorTabs.Count - 1, tabItem);
        tabs.Insert(pinnedEditorTabs.Count - 1, editorTab);
        editorTab.IsPinned = true;
        editorTab.TabPlacement = pinnedEditorTabs.Count - 1;
        tabItem.Header = editorTab.GetHeader();
    }

    public void UnpinTab()
    {
        int index = getCurrentTabIndex();
        UnpinTab(tabs[index]);
    }
    public void UnpinTabWithIndex(int index)
    {
        UnpinTab(tabs[index]);
    }
    public void UnpinTab(EditorTab editorTab)
    {
        if (!editorTab.IsPinned)
        {
            return;
        }

        int index = pinnedEditorTabs.IndexOf(editorTab);
        pinnedEditorTabs.Remove(editorTab);
        tabs.Remove(editorTab);
        TabItem tabItem = (TabItem)tabControl.Items.FirstOrDefault((x) =>
        {
            TabItem tab = (TabItem)x;
            EditorTab editor = (EditorTab)tab.Content;
            return editor != null && editor.IsPinned && editor.TabPlacement == editorTab.TabPlacement;
        });
        tabControl.Items.Remove(tabItem);
        tabControl.Items.Add(tabItem);
        tabs.Add(editorTab);
        editorTab.IsPinned = false;
        editorTab.TabPlacement = pinnedEditorTabs.Count - 1;
        tabItem.Header = editorTab.GetHeader();
    }

    public int TabCount => tabs.Count;

}