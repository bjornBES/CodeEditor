using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Search;
using AvaloniaEdit.TextMate;
using lib.debug;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

public class EditorTab : ControlElement<EditorTab>
{
    TextMate.Installation textMateInstallation;
    private TextEditor textEditor;
    internal Language Language;
    public API.TextDocument TextDocument { get; private set; }

    public string FilePath { get; set; }
    public string FileName { get; private set; }
    public bool IsModified { get; set; }
    public string FileExtension { get; private set; }
    public string TextBuffer => textEditor.Text;
    public Caret Caret => textEditor.TextArea.Caret;

    public int TabPlacement { get; set; } = -1;
    public bool IsPinned { get; set; } = false;
    public bool IsTabFocused { get; private set; } = false;

    public bool IsTextFocus { get => textEditor.IsFocused; }

    public Action<int> GetFocusAction;
    public Action<int> LostFocusAction;

    public EditorTab()
    {
        Initialize();
        Background = Application.Current.Resources.GetResource("editor.background");
        IsModified = false;
        InitializeComponent();
        // GotFocus += (sender, e) => { KeybindingManager.ActiveContext = "editor"; };
        AddContext("activeEditorIsPinned", GetPropertyInfo(nameof(IsPinned)), this);
        AddContext("editorTextFocus", GetPropertyInfo(nameof(IsTextFocus)), this);
        AddContext("editorFocus", GetPropertyInfo(nameof(IsFocused)), this);
    }

    public void AttachedToControl(TabControl tabControl)
    {
        if (TabPlacement != -1)
        {
            return;
        }

        TabPlacement = tabControl.ItemCount;
        DebugWriter.WriteLine("Editor", $"Tab placement = {TabPlacement}");
    }

    internal TextEditor GetEditor() => textEditor;

    public void Load(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(FilePath);
        FileExtension = Path.GetExtension(FilePath);
        if (File.Exists(filePath))
        {
            string contents = File.ReadAllText(filePath);
            textEditor.Text = contents;
            IsModified = false;
        }
    }

    void InitializeComponent()
    {
        textEditor = new TextEditor()
        {
            ShowLineNumbers = true,
            FontSize = 14,
            Foreground = Application.Current.Resources.GetResource("editor.foreground"),
            WordWrap = false,
        };
        GotFocus += (sender, e) => { KeybindingManager.ActiveContext = "editor";  };

        Application.Current.Resources["SearchPanelBackgroundBrush"] = textEditor.Background;
        Application.Current.Resources["SearchPanelBorderBrush"] = Application.Current.Resources.GetResource("editor.searchPanel.border.background");
        Application.Current.Resources["BoxTextBackgroundBrush"] = Application.Current.Resources.GetResource("editor.searchPanel.textbox.background");
        Application.Current.Resources["BoxTextForegroundBrush"] = Application.Current.Resources.GetResource("editor.searchPanel.textbox.foreground");

        textEditor.TextChanged += OnTextChanged;
        textEditor.TextArea.TextEntered += (sender, e) =>
        {
            if (e.Text == "\n")
            {
                var caret = textEditor.TextArea.Caret;
                IndentationManager.IndentAfterEnter("csharp", TextDocument, caret.Line, 4, false);
            }
            else if (e.Text == "{")
            {
                textEditor.TextArea.PerformTextInput("}");
            }
            else if (e.Text == "[")
            {
                textEditor.TextArea.PerformTextInput("]");
            }
            else if (e.Text == "(")
            {
                textEditor.TextArea.PerformTextInput(")");
            }
        };
        textEditor.TextArea.OnCaretPositionChanged += (sender, e) =>
        {
            var caret = textEditor.TextArea.Caret;
            colLineStatus.Text = $"Ln {caret.Line}, Col {caret.Column}";
        };

        textEditor.KeyDown += (sender, e) =>
        {
            e.Handled = false;
        };

        TextDocument = new API.TextDocument();
        TextDocument.avaloniaDocument = textEditor.Document;
        TextDocument.editorTab = this;

        UpdateSettings();

        if (File.Exists(FilePath))
        {
            string contents = File.ReadAllText(FilePath);
            textEditor.Text = contents;
        }

        Children.Add(textEditor);

        textEditor.GotFocus += (sender, e) => { KeybindingManager.ActiveContext = "editor"; IsTabFocused = true; GetFocusAction.Invoke(TabPlacement); };
        textEditor.LostFocus += (sender, e) => { IsTabFocused = false; LostFocusAction.Invoke(TabPlacement); };
    }

    public override void EndInit()
    {
        base.EndInit();
        // ApplicationCommands.Find.ChangeBinding(Key.F, KeyModifiers.Control);        
        // search.CommandBindings.RemoveItem(ApplicationCommands.Find);
    }

    void OnTextChanged(object sender, EventArgs e)
    {
        IsModified = true;
    }

    public void UpdateSettings()
    {
        var background = Application.Current.Resources.GetResource("editor.background");
        textEditor.Background = background == null ? "#1f1f1f".GetColoredBrush() : background.ToString().GetColoredBrush();

        textEditor.Foreground = Application.Current.Resources.GetResource("editor.foreground");

        textEditor.WordWrap = MainWindow.EditorConfigsSettingsManager.Current.Editor.WordWrap;
    }

    public void Save()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            File.WriteAllText(FilePath, textEditor.Text);
            IsModified = false;
        }
    }

    public void Close()
    {
        FilePath = "";
        textEditor.Text = "";
        IsModified = false;
    }

    public string GetHeader()
    {
        string name = FileName;

        if (IsPinned)
        {
            name = "P " + name;
        }

        return $"{name}";
    }

    public void LoadSyntaxHighlighting(RegistryOptions registryOptions)
    {
        textMateInstallation = textEditor.InstallTextMate(registryOptions);
        Registry registry = new Registry(registryOptions);
        Language = registryOptions.GetLanguageByExtension(FileExtension);
        if (Language != null)
        {
            string scopeName = registryOptions.GetScopeByLanguageId(Language.Id);
            textMateInstallation.SetGrammar(scopeName);

            IGrammar grammar = registry.LoadGrammar(scopeName);
            var blockProvider = new BlockIndentationProvider(Language.Id, grammar);
            IndentationManager.RegisterProvider(blockProvider);
        }
    }

    public void ApplyTheme(RegistryOptions registryOptions)
    {
        textMateInstallation = textEditor.InstallTextMate(registryOptions);

        Language = registryOptions.GetLanguageByExtension(FileExtension);
        if (Language != null)
        {
            string scope = registryOptions.GetScopeByLanguageId(Language.Id);
            textMateInstallation.SetGrammar(scope);
        }
    }

    public void OnConfigChanged()
    {
        var fontFamily = Application.Current.Resources["editor.font"];
        textEditor.FontFamily = fontFamily == null ? "Consolas" : fontFamily.ToString();

        var fontSize = Application.Current.Resources["editor.fontsize"];
        textEditor.FontSize = fontSize == null ? 14 : Convert.ToDouble(fontSize);
    }

    public void PinTab()
    {
        CommandManager.ExecuteCommand("editor.action.pinEditor.index", TabPlacement);
    }
}