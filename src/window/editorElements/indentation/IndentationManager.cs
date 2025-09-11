
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation;

public static class IndentationManager
{
    private static readonly Dictionary<string, IIndentationProvider> providers = new();

    public static void RegisterProvider(IIndentationProvider provider)
        => providers[provider.LanguageId] = provider;

    public static void IndentLine(string languageId, API.TextDocument document, DocumentLine line, int tabSize, bool useTabs)
    {
        if (providers.TryGetValue(languageId, out var provider))
            provider.IndentLine(document.avaloniaDocument, line, tabSize, useTabs);
        else
            new DefaultIndentationStrategy().IndentLine(document.avaloniaDocument, line);
    }
    public static void IndentAfterEnter(string languageId, API.TextDocument document, int lineNumber, int tabSize, bool useTabs)
    {
        if (lineNumber <= 0 || lineNumber > document.LineCount)
            return;

        var line = document.avaloniaDocument.GetLineByNumber(lineNumber);

        IndentLine(languageId, document, line, tabSize, useTabs);
    }
}