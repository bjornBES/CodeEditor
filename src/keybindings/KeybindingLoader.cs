
using System.Text.Json;
using Avalonia.Input;

public static class KeybindingLoader
{
    public static void LoadKeybindings()
    {
        var json = File.ReadAllText(AppPaths.KeybindingsFilePath);
        var bindings = JsonSerializer.Deserialize<List<KeybindingConfig>>(json);

        foreach (var binding in bindings)
        {
            if (TryParseKey(binding.Key, out var key, out var modifiers))
            {
                KeybindingManager.BindKey(key, modifiers, binding.Context, binding.CommandId);
            }
        }
    }

    private static bool TryParseKey(string keyString, out Key key, out KeyModifiers modifiers)
    {
        modifiers = KeyModifiers.None;
        key = Key.None;

        var parts = keyString.Split('+', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            switch (part.Trim().ToLowerInvariant())
            {
                case "ctrl": modifiers |= KeyModifiers.Control; break;
                case "alt": modifiers |= KeyModifiers.Alt; break;
                case "shift": modifiers |= KeyModifiers.Shift; break;
                default:
                    Enum.TryParse(part, true, out key);
                    break;
            }
        }
        return key != Key.None;
    }

}