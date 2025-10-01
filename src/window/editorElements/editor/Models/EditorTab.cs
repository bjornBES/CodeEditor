public class EditorTab : ControlElement<EditorTab>
{
    public EditorInput Input { get; }
    public object View { get; set; }
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public bool IsPreview { get; set; }

    public EditorTab(EditorInput input)
    {
        Initialize();
        AddContext("activeEditorIsPinned", GetPropertyInfo(nameof(IsPinned)), this);
        AddContext("editorFocus", GetPropertyInfo(nameof(IsFocused)), this);
        AddContext("editorActive", GetPropertyInfo(nameof(IsActive)), this);
        Input = input;
    }
}
