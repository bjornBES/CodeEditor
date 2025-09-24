
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
        KeyGesture gesture = KeyGesture.Parse(keyString);

        modifiers = gesture.KeyModifiers;
        key = gesture.Key;
        
        return key != Key.None;
    }

}