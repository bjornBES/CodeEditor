using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

public class ThemeService
{
    private static Theme _currentTheme;

    public static Theme CurrentTheme => _currentTheme;

    public static void SetTheme(string name, Editor editor)
    {
        string path = Path.Combine(AppPaths.ThemesDirectoryPath, $"{name}.json");

        string json = File.ReadAllText(path);
        _currentTheme = JsonSerializer.Deserialize<Theme>(json);

        ApplyUITheme(_currentTheme);

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        foreach (var panel in editor.editorPanels)
        {
            panel.ApplyRegistryOptions(registryOptions);
        }
    }

    private static void ApplyUITheme(Theme theme)
    {
        if (Application.Current == null) return;

        var resources = Application.Current.Resources;
        resources.MergedDictionaries.Clear();

        if (theme.Colors != null)
        {
            foreach (var kv in theme.Colors)
            {
                ApplyThemeElement(resources, kv.Key, kv.Value, kv.Key);
            }
        }
    }

    private static void ApplyThemeElement(IResourceDictionary resources, string key, ThemeElement element, string prefix)
    {
        if (!string.IsNullOrEmpty(element.Background))
            addResources(resources, $"{prefix}.background", ConvertHex(element.Background));

        if (!string.IsNullOrEmpty(element.Foreground))
            addResources(resources, $"{prefix}.foreground", ConvertHex(element.Foreground));

        if (!string.IsNullOrEmpty(element.HoverBackground))
            addResources(resources, $"{prefix}.hoverBackground", ConvertHex(element.HoverBackground));

        if (!string.IsNullOrEmpty(element.HoverForeground))
            addResources(resources, $"{prefix}.hoverForeground", ConvertHex(element.HoverForeground));

        if (element.SubElements != null)
        {
            foreach (var sub in element.SubElements)
            {
                ApplyThemeElement(resources, sub.Key, sub.Value, $"{prefix}.{sub.Key}");
            }
        }
    }

    private static void addResources(IResourceDictionary resources, string key, object value)
    {
        if (resources.ContainsKey(key))
        {
            resources[key] = value;
        }
        else
        {
            resources.Add(key, value);
        }
    }

    private static IBrush ConvertHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return new SolidColorBrush(Color.Parse(hex));
    }
}
