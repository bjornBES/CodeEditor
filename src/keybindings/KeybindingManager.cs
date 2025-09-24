using Avalonia;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Input;
using lib.debug;
using System.Reactive;

public class KeybindingManager
{
    private static readonly HashSet<Key> _pressedKeys = new();
    private static readonly Dictionary<(Key, KeyModifiers, string context), string> _bindings = new();

    private static string _activeContext { get; set; } = "global";
    public static string ActiveContext { get { return _activeContext; } set { _activeContext = value; DebugWriter.WriteLine("KeybindingManager", $"Context switched {value}"); } }

    public static Window mainWindow { get; set; }

    public static void BindKey(Key key, KeyModifiers modifiers, string context, string commandId)
    {
        _bindings[(key, modifiers, context)] = commandId;

        ReactiveCommand<string, Unit> command;
        command = ReactiveCommand.Create<string>(PerformCommand);
        KeyBinding keyBinding = new KeyBinding() { Gesture = new KeyGesture(key, modifiers), Command = command, CommandParameter = $"{commandId},{context}" };
        mainWindow.KeyBindings.Add(keyBinding);
    }

    public static void PerformCommand(string data)
    {
        string commandId = data.Split(',')[0];
        string context = data.Split(',')[1];
        CommandEntry commandEntry = CommandManager.GetCommandEntry(commandId);
        if (commandEntry == null)
        {
            DebugWriter.WriteLine("KeybindingManager", $"command Entry is null from {commandId}");
            throw new ArgumentNullException("commandEntry");
        }

        object[] args = { };

        if (commandEntry.Types.Length > 0)
        {
            // TODO use the entry to get the args
        }

        if (context == ActiveContext || context == "global")
        {
            CommandManager.ExecuteCommand(commandId, args);
        }
    }


    // Convenience: attach manager to a Window (wires KeyDown/KeyUp and deactivation)
    public static void AttachToWindow(Window window)
    {
        mainWindow = window;
    }
}
