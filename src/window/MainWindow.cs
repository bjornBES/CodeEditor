
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using NativeFileDialogSharp;

public enum DialogType
{
    OpenFile,
    SaveFile,
    SelectFolder
}

public class MainWindow : Window
{
    public static SettingsManager<EditorConfigs> EditorConfigsSettingsManager { get; set; }
    public static SettingsManager<GlobalStorageSettings> GlobalStorageSettingsManager { get; set; }

    SidePanel leftSidePanel;
    Editor CodeEditor;
    SidePanel rightSidePanel;

    Explorer explorer;

    public MainWindow()
    {
        EditorConfigsSettingsManager = new SettingsManager<EditorConfigs>(AppPaths.WorkspaceConfigFilePath, AppPaths.GlobalConfigFilePath);
        GlobalStorageSettingsManager = new SettingsManager<GlobalStorageSettings>(AppPaths.GlobalStorageFilePath);

        EditorConfigsSettingsManager.Current.Editor = new EditorSection();
        EditorConfigsSettingsManager.Current.Editor.FontSize = 12;
        EditorConfigsSettingsManager.Current.Editor.FontFamily = "Consolas";
        EditorConfigsSettingsManager.Current.Editor.IndentWidth = 4;
        EditorConfigsSettingsManager.Current.Editor.InsertSpaces = true;
        EditorConfigsSettingsManager.Load();

        AppPaths.EnsureDirectoriesExist();
        AppPaths.EnsureFilesExist();

        InitializeComponent();

        SetTheme("normalDark");
        CodeEditor.UpdateSettings();
        leftSidePanel.UpdateSettings();
        rightSidePanel.UpdateSettings();
        string json = ThemeConverter.ConvertToVSCodeTheme(ThemeService.CurrentTheme);
        File.WriteAllText("./vscodeTemp.json", json);
    }

    public void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Title = "Code Editor";

        Application.Current.Resources.Add("editor.fontsize", EditorConfigsSettingsManager.Current.Editor.FontSize);
        Application.Current.Resources.Add("editor.font", EditorConfigsSettingsManager.Current.Editor.FontFamily);

        KeyDown += UpdateKeyDown;

        Content = CreateLayout();

        if (!string.IsNullOrEmpty(GlobalStorageSettingsManager.Current.DefaultTheme))
        {
            ThemeService.SetTheme(GlobalStorageSettingsManager.Current.DefaultTheme, CodeEditor);
        }

        Closed += OnWindowClosed;
    }

    public void OnWindowClosed(object sender, EventArgs e)
    {
        GlobalStorageSettingsManager.SaveGlobal();
        EditorConfigsSettingsManager.SaveGlobal();
    }

    Control CreateLayout()
    {
        Grid mainGrid = new Grid();

        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(250))); // Left panel
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));     // Left splitter
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Editor
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));     // Right splitter
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(250))); // Right panel


        // Left panel
        leftSidePanel = new SidePanel(Dock.Left);

        explorer = new Explorer();
        leftSidePanel.AddItem(explorer);
        leftSidePanel.AddItem("Search", new TextBlock { Text = "Search content" });
        Grid.SetColumn(leftSidePanel, 0);
        mainGrid.Children.Add(leftSidePanel);

        // Attach panel to its parent column so collapse/expand works automatically
        leftSidePanel.AttachToColumn(mainGrid.ColumnDefinitions[0]);

        // Add splitter from SidePanel to parent Grid
        Grid.SetColumn(leftSidePanel.Splitter, 1);
        mainGrid.Children.Add(leftSidePanel.Splitter);

        // Editor
        CodeEditor = new Editor();
        // Grid.SetColumn(CodeEditor, 2);
        // mainGrid.Children.Add(CodeEditor);

        StackPanel stackPanel = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        IBrush normalBackground = (IBrush)Application.Current.Resources["button.background"];
        IBrush hoverBackground = (IBrush)Application.Current.Resources["button.hoverbackground"];

        TextBlock button = new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "testButton25",
        };
        HoverStyleHelper.CreateHoverBackgroundStyle<TextBlock>(normalBackground, hoverBackground, button.Name);
        Grid.SetColumn(button, 2);
        mainGrid.Children.Add(button);

        // Right panel
        rightSidePanel = new SidePanel(Dock.Right);
        rightSidePanel.AddItem("Outline", new TextBlock { Text = "Outline content" });
        rightSidePanel.AddItem("Properties", new TextBlock { Text = "Properties content" });
        Grid.SetColumn(rightSidePanel, 4);
        mainGrid.Children.Add(rightSidePanel);

        // Add right splitter
        Grid.SetColumn(rightSidePanel.Splitter, 3);
        mainGrid.Children.Add(rightSidePanel.Splitter);

        rightSidePanel.AttachToColumn(mainGrid.ColumnDefinitions[4]);

        return mainGrid;
    }

    public void SetTheme(string theme)
    {
        ThemeService.SetTheme(theme, CodeEditor);
        GlobalStorageSettingsManager.Current.DefaultTheme = theme;
        GlobalStorageSettingsManager.SaveGlobal();
    }

    void UpdateKeyDown(object sender, KeyEventArgs e)
    {
        if (isKeyDown(Key.O, e, KeyModifiers.Control))
        {
            string path = OpenDialog(DialogType.OpenFile);
            if (!string.IsNullOrEmpty(path))
            {
                CodeEditor.LoadFile(path);
            }
        }
        if (isKeyDown(Key.OemPlus, e, KeyModifiers.Control))
        {
            int fontsize = (int)Application.Current.Resources["editor.fontsize"];
            fontsize += 1;
            Application.Current.Resources["editor.fontsize"] = fontsize;
            CodeEditor.UpdateSettings();
        }
        if (isKeyDown(Key.OemMinus, e, KeyModifiers.Control))
        {
            int fontsize = (int)Application.Current.Resources["editor.fontsize"];
            fontsize -= 1;
            if (fontsize <= 0) fontsize = 1;
            Application.Current.Resources["editor.fontsize"] = fontsize;
            CodeEditor.UpdateSettings();
        }
        if (isKeyDown(Key.S, e, KeyModifiers.Control))
        {
            CodeEditor.SaveFile();
        }
        if (isKeyDown(Key.S, e, KeyModifiers.Control, KeyModifiers.Shift))
        {
            string path = OpenDialog(DialogType.SaveFile);
            CodeEditor.SaveFile(path);
        }
        if (isKeyDown(Key.W, e, KeyModifiers.Control))
        {
            CodeEditor.CloseTab();
        }
        if (isKeyDown(Key.N, e, KeyModifiers.Control))
        {
            CodeEditor.NewTab();
        }
        if (isKeyDown(Key.B, e, KeyModifiers.Control))
        {
            rightSidePanel.Toggle();
        }
        if (isKeyDown(Key.B, e, KeyModifiers.Control, KeyModifiers.Alt))
        {
            leftSidePanel.Toggle();
        }
        if (isKeyDown(Key.F, e, KeyModifiers.Control, KeyModifiers.Alt))
        {
            CodeEditor.IndentDocument();
        }
    }

    bool isKeyDown(Key key, KeyEventArgs e, params KeyModifiers[] modifiers)
    {
        if (e.Key != key) return false;
        int mods = 0;
        foreach (var mod in modifiers)
        {
            mods |= (int)mod;
        }
        if (mods != (int)e.KeyModifiers) return false;
        return true;
    }

    public string OpenDialog(DialogType type, string defaultPath = null)
    {
        DialogResult dialog;
        string result = "";
        if (type == DialogType.OpenFile)
        {
            dialog = Dialog.FileOpen(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }
        else if (type == DialogType.SaveFile)
        {
            dialog = Dialog.FileSave(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }
        else if (type == DialogType.SelectFolder)
        {
            dialog = Dialog.FolderPicker(defaultPath: defaultPath);
            if (dialog.IsOk) result = dialog.Path;
        }

        return result;
    }
}