using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

public class EditorTab : Panel
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

    public EditorTab()
    {
        Background = Application.Current.Resources.GetResource("editor.background");
        IsModified = false;
        InitializeComponent();

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
            Foreground = (IBrush)Application.Current.Resources["editor.foreground"],
            WordWrap = false,
        };

        textEditor.TextChanged += OnTextChanged;
        textEditor.TextArea.TextEntered += (sender, e) =>
        {
            if (e.Text == "\n")
            {
                var caret = textEditor.TextArea.Caret;
                IndentationManager.IndentAfterEnter("csharp", TextDocument, caret.Line, 4, false);
            }
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
    }

    void OnTextChanged(object sender, EventArgs e)
    {
        IsModified = true;
    }

    public void UpdateSettings()
    {
        var fontFamily = Application.Current.Resources["editor.font"];
        textEditor.FontFamily = fontFamily == null ? "Consolas" : fontFamily.ToString();

        var fontSize = Application.Current.Resources["editor.fontsize"];
        textEditor.FontSize = fontSize == null ? 14 : Convert.ToDouble(fontSize);

        var background = Application.Current.Resources.GetResource("editor.background");
        textEditor.Background = background == null ? "#1f1f1f".GetColoredBrush() : background.ToString().GetColoredBrush();

        textEditor.Foreground = Application.Current.Resources.GetResource("editor.foreground");
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
        return FileName;
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
}