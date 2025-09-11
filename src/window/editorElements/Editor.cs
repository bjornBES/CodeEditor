using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;

public class Editor : Panel
{
    public List<EditorPanel> editorPanels;

    public Editor()
    {
        InitializeComponent();
    }

    public void InitializeComponent()
    {
        Background = (IBrush)Application.Current.Resources["editor.background"];
        editorPanels = new List<EditorPanel>(8);
        AddPanel();
    }

    public void AddPanel()
    {
        EditorPanel editorPanel = new EditorPanel();
        Children.Add(editorPanel);
        editorPanels.Add(editorPanel);
    }

    public void LoadFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            EditorPanel editorPanel = getCurrentPanel();
            editorPanel.NewTab(filePath);
        }
    }

    public void UpdateSettings()
    {
        Background = (IBrush)Application.Current.Resources["editor.background"];
        EditorPanel editorPanel = getCurrentPanel();
        editorPanel.UpdateSettings();
    }

    public void SaveFile()
    {
        EditorPanel editorPanel = getCurrentPanel();
        editorPanel.SaveCurrentTab();
    }
    public void SaveFile(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            EditorPanel editorPanel = getCurrentPanel();
            editorPanel.SaveCurrentTab(path);
        }
    }
    public EditorPanel getCurrentPanel()
    {
        int index = getCurrentPanelIndex();
        return editorPanels[index];
    }
    public EditorPanel getCurrentPanel(int index)
    {
        return editorPanels[index];
    }
    private int getCurrentPanelIndex()
    {
        // TODO for mulipul panels
        return 0;
    }
    public void CloseTab()
    {
        EditorPanel editorPanel = getCurrentPanel();
        editorPanel.CloseCurrentTab();
    }

    public void NewTab()
    {
        EditorPanel editorPanel = getCurrentPanel();
        // TODO for only one tab
        if (editorPanel.TabCount != 0)
        {
            int index = getCurrentPanelIndex();
            EditorTab tab = editorPanel.GetTab(index);
            if (!string.IsNullOrEmpty(tab.TextBuffer))
            {
                editorPanel.SaveTab(index);
                editorPanel.CloseTab(index);
            }
        }
        editorPanel.NewTab();
    }

    public void CloseEditor(int index)
    {
        EditorPanel editorPanel = getCurrentPanel();
        if (index == 0)
        {
            // Only one tab, clear it
            editorPanel.CloseTab(index);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void IndentDocument()
    {
        int index = getCurrentPanelIndex();
        IndentDocument(index);
    }
    public void IndentDocument(int index)
    {
        EditorPanel editorPanel = getCurrentPanel(index);
        editorPanel.IndentCurrentDocument();
    }
}