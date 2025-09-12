using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Rules;
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

    private static void ApplyThemeElement(IResourceDictionary resources, string key, ThemeElement element, string prefix, ThemeElementRaw elementRaw = null)
    {
        ThemeElement themeElement= element;

        if (elementRaw != null && element == null)
        {
            themeElement = new ThemeElement()
            {
                Background = elementRaw.Background,
                Foreground = elementRaw.Foreground,
                SubElements = elementRaw.SubElements,
            };
        }
        else
        {
            
        }

        if (!string.IsNullOrEmpty(themeElement.Background))
            addResources(resources, $"{prefix}.background", ConvertHex(themeElement.Background));

        if (!string.IsNullOrEmpty(themeElement.Foreground))
            addResources(resources, $"{prefix}.foreground", ConvertHex(themeElement.Foreground));

        if (element.SubElements != null)
        {
            foreach (var sub in element.SubElements)
            {
                ApplyThemeElement(resources, sub.Key, null, $"{prefix}.{sub.Key}", sub.Value);
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

    private static Color ConvertHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.Parse("#FF0000");
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return Color.Parse(hex);
    }
}
