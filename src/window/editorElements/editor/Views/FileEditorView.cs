

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

public class FileEditorView : Panel
{
    private readonly FileEditorInput Input;
    public readonly TextEditor Editor;

    private TextMate.Installation textMateInstallation;
    private Language Language;
    private RegistryOptions registryOptions;

    public FileEditorView(FileEditorInput input, RegistryOptions options)
    {
        Input = input;
        registryOptions = options;
        Editor = new TextEditor
        {
            ShowLineNumbers = true,
            FontSize = 14,
            FontFamily = "Consolas",
            Text = input.TextContent
        };

        Editor.TextChanged += (s, e) =>
        {
            Input.UpdateContent(Editor.Text);
        };

        Editor.TextArea.TextEntered += (sender, e) =>
        {
            if (e.Text == "\n")
            {
                var caret = Editor.TextArea.Caret;
                IndentationManager.IndentAfterEnter("csharp", Editor.Document, caret.Line, 4, false);
            }
        };

        Children.Add(Editor);

        LoadSyntaxHighlighting(options, input.FilePath);
    }

    public void UpdateFileInfo(Button textBlock)
    {
        var caret = Editor.TextArea.Caret;
        var line = caret.Line;
        var column = caret.Column;
        textBlock.Content = $"Ln {line}, Col {column}";
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
    }

    public void LoadSyntaxHighlighting(RegistryOptions registryOptions, string path)
    {
        this.registryOptions = registryOptions;

        textMateInstallation = Editor.InstallTextMate(registryOptions);
        var registry = new Registry(registryOptions);

        var language = registryOptions.GetLanguageByExtension(Path.GetExtension(path));
        if (language != null)
        {
            string scopeName = registryOptions.GetScopeByLanguageId(language.Id);
            textMateInstallation.SetGrammar(scopeName);

            IGrammar grammar = registry.LoadGrammar(scopeName);
            var blockProvider = new BlockIndentationProvider(language.Id, grammar);
            IndentationManager.RegisterProvider(blockProvider);

            Language = language;
        }
    }

    public void ApplyTheme(RegistryOptions registryOptions)
    {
        if (Language == null) return;

        textMateInstallation = Editor.InstallTextMate(registryOptions);
        string scope = registryOptions.GetScopeByLanguageId(Language.Id);
        textMateInstallation.SetGrammar(scope);

        this.registryOptions = registryOptions;
    }

    public void OnConfigChanged()
    {
        var fontFamily = Application.Current.Resources["editor.font"];
        Editor.FontFamily = fontFamily == null ? "Consolas" : fontFamily.ToString();

        var fontSize = Application.Current.Resources["editor.fontsize"];
        Editor.FontSize = fontSize == null ? 14 : Convert.ToDouble(fontSize);
    }

    public void UpdateSettings()
    {
var background = Application.Current.Resources.GetResource("editor.background");
        Editor.Background = background == null ? "#1f1f1f".GetColoredBrush() : background.ToString().GetColoredBrush();

        Editor.Foreground = Application.Current.Resources.GetResource("editor.foreground");

        Editor.WordWrap = MainWindow.EditorConfigsSettingsManager.Current.Editor.WordWrap;
    }

}