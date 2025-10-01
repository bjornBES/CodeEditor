public class EditorGroup : ControlElement<EditorGroup>
{
    private readonly List<EditorTab> _tabs = new();
    public IReadOnlyList<EditorTab> Tabs => _tabs;
    public int ActiveIndex { get; private set; } = -1;
    public EditorTab ActiveTab
    {
        get
        {
            EditorTab activeTab = (ActiveIndex >= 0 && ActiveIndex < _tabs.Count) ? _tabs[ActiveIndex] : null;
            UpdateInstance("activeEditorIsPinned", activeTab);
            UpdateInstance("editorActive", activeTab);
            UpdateInstance("editorFocus", activeTab);
            return activeTab;
        }
    }

    public void OpenEditor(EditorInput input, object view, bool pinned = false)
    {
        var existing = _tabs.FindIndex(t => t.Input.Resource == input.Resource);
        if (existing >= 0)
        {
            ActiveIndex = existing;
            _tabs[existing].IsPreview = false;
            return;
        }

        if (!pinned && _tabs.Any(t => t.IsPreview))
        {
            var previewIndex = _tabs.FindIndex(t => t.IsPreview);
            _tabs[previewIndex] = new EditorTab(input) { IsPreview = true, View = view };
            ActiveIndex = previewIndex;
        }
        else
        {
            var tab = new EditorTab(input) { IsPinned = pinned, View = view };
            _tabs.Add(tab);
            ActiveIndex = _tabs.Count - 1;
        }
    }

    public void CloseEditor(EditorInput input)
    {
        var index = _tabs.FindIndex(t => t.Input == input);
        if (index >= 0)
        {
            _tabs[index].Input.Dispose();
            _tabs.RemoveAt(index);
            if (ActiveIndex >= _tabs.Count) ActiveIndex = _tabs.Count - 1;
        }
    }
}
