using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using System.Text.Json;
using TextMateSharp.Internal.Themes.Reader;
using AvaloniaEdit.Editing;

public class EditorPanel : Panel
{
    TabControl tabControl;
    List<EditorTab> tabs;

    RegistryOptions registryOptions;

    public EditorPanel()
    {
        InitializeComponent();

        registryOptions = new RegistryOptions(ThemeName.DarkPlus);
    }

    public void InitializeComponent()
    {
        tabs = new List<EditorTab>();

        tabControl = new TabControl()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        Children.Add(tabControl);
    }

    public void NewTab(string filePath)
    {
        var tab = NewTab();
        tab.Load(filePath);
        tab.LoadSyntaxHighlighting(registryOptions);

        TabItem tabItem = new TabItem()
        {
            Header = tab.GetHeader(),
            Content = tab,
        };
        tabControl.Items.Add(tabItem);
        tabControl.SelectedItem = tabItem;
    }
    public EditorTab NewTab()
    {
        var tab = new EditorTab();
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
        return 0;
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


    public int TabCount => tabs.Count;
}