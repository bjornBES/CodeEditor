
using System.ComponentModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using NativeFileDialogSharp;
using ReactiveUI;
using Tmds.DBus.Protocol;

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

    TopPalette topPalette;

    Explorer explorer;

    Canvas Overlay;
    Panel topMenu;

    public MainWindow()
    {
        CommandManager.RegisterCommand("Say Hello World", "helloworld", () => { DebugWriter.WriteLine("Window", "Hello world editor command"); });
        CommandManager.RegisterCommand("Say Hello World", "helloworld.global", () => { DebugWriter.WriteLine("Window", "Hello world global command"); });

        PixelSize screenSize = Screens.Primary.Bounds.Size;
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
        Application.Current.Resources["Button.hoverBackground"] = Color.Parse("#FFFFFF");
        Application.Current.Styles.Add(ThemeService.themeStyles);

        EditorConfigsSettingsManager.OnConfigChanged += OnEditorConfigsChanged;

        ReactiveCommand<Unit, Unit> OpenPaltteCommand = ReactiveCommand.Create(OpenPalette);
        KeyBindings.Add(new KeyBinding() { Gesture = new KeyGesture(Key.P, KeyModifiers.Control | KeyModifiers.Shift), Command = OpenPaltteCommand });

        SizeChanged += (sender, args) =>
        {
            if (topPalette != null)
            {
                Size topbarSize = topMenu.Bounds.Size;
                topPalette.WindowChangedSize(args.NewSize, screenSize, topbarSize);

                Size editorSize = CodeEditor.Bounds.Size;

                Size leftSideSize = leftSidePanel.Bounds.Size;
                Size rightSizeSize = rightSidePanel.Bounds.Size;
                leftSidePanel.WindowChangedSize(args.NewSize, rightSizeSize);
                rightSidePanel.WindowChangedSize(args.NewSize, leftSideSize);
            }
        };

        CommandManager.RegisterCommand("Open file", "editor.open.file", CodeEditor.LoadFile);
    }

    public void InitializeComponent()
    {
        Width = 800;
        Height = 600;

        MinWidth = 600;
        MinHeight = 700;
        Title = "Code Editor";

        Application.Current.Resources.Add("editor.fontsize", EditorConfigsSettingsManager.Current.Editor.FontSize);
        Application.Current.Resources.Add("editor.font", EditorConfigsSettingsManager.Current.Editor.FontFamily);

        KeyDown += UpdateKeyDown;

        var rootGrid = new Grid
        {
            // Background = new SolidColorBrush(Color.Parse("#333333")) // window background
            Background = Brushes.White, // window background
        };
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // top menu height
        rootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star))); // rest of window

        topMenu = new Panel()
        {
            MinHeight = 40
        };
        Grid.SetRow(topMenu, 0);
        rootGrid.Children.Add(topMenu);

        DockPanel mainDock = new DockPanel()
        {
            Background = Brushes.Transparent,
        };
        Grid.SetRow(mainDock, 1);
        rootGrid.Children.Add(mainDock);

        Control mainContents = CreateLayout();
        mainDock.Children.Add(mainContents);

        Overlay = new Canvas
        {
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            IsVisible = true // only visible when a submenu is open
        };

        topPalette = new TopPalette();
        Overlay.Children.Add(topPalette);

        // Overlay spans both rows
        Grid.SetRowSpan(Overlay, 2);
        rootGrid.Children.Add(Overlay);

        Content = rootGrid;

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
        CodeEditor = new Editor()
        {
            Name = "Editor"
        };
        Grid.SetColumn(CodeEditor, 2);
        mainGrid.Children.Add(CodeEditor);

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

        if (KeybindingManager.HandleKeyPress(e.Key, e.KeyModifiers))
        {
            e.Handled = true;
        }

        topPalette.OnKeyDownPalette(sender, e);
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

    public void OpenPalette()
    {
        topPalette.OpenPalette();
    }

    public void OnEditorConfigsChanged()
    {
        CodeEditor.OnConfigChanged();
    }

    public static string OpenDialog(DialogType type, string defaultPath = null)
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