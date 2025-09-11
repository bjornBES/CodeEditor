
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using TextMateSharp.Grammars;
using AvaloniaEditTextDocument = AvaloniaEdit.Document.TextDocument;

namespace API;

public class TextDocument
{
    internal AvaloniaEditTextDocument avaloniaDocument;
    internal EditorTab editorTab;

    public string Encoding { get; private set; }
    public string EOL { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }

    public string LanguageId { get => editorTab.Language.Id; }
    public int LineCount { get; private set; }
}