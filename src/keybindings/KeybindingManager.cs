using Avalonia.Input;

public class KeybindingManager
{
    private static readonly Dictionary<(Key, KeyModifiers, string context), string> _bindings = new();
    private static string _activeContext { get; set; } = "global";
    public static string ActiveContext { get { return _activeContext; } set { _activeContext = value; DebugWriter.WriteLine("KeybindingManager", $"Context switched {value}"); } }

    public static void BindKey(Key key, KeyModifiers modifiers, string context, string commandId)
    {
        _bindings[(key, modifiers, context)] = commandId;
    }

    public static bool HandleKeyPress(Key key, KeyModifiers modifiers, params object[] args)
    {
        if (_bindings.TryGetValue((key, modifiers, ActiveContext), out var commandId) ||
                    _bindings.TryGetValue((key, modifiers, "global"), out commandId))
        {
            CommandManager.ExecuteCommand(commandId, args);
            return true;
        }

        return false;
    }
}
