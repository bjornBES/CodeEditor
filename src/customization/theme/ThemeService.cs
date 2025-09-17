using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Rules;
using TextMateSharp.Themes;

public class ThemeService
{
    private static Theme _currentTheme;

    public static Theme CurrentTheme => _currentTheme;

    public static readonly string[] EventIdent = ["hover", "clicked", "focus", "disabled"];
    public static Styles themeStyles = new Styles();

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

        if (element.Events != null)
        {
            foreach (string eventKey in element.Events.Keys)
            {

                ThemeEventElement eventElement = element.Events[eventKey];

                if (eventKey == "focus")
                {
                    
                }
                if (EventIdent.Contains(eventKey))
                {
                    if (!string.IsNullOrEmpty(element.Background))
                        addResources(resources, $"{prefix}.{eventKey}.background", ConvertHex(eventElement.Background));

                    if (!string.IsNullOrEmpty(element.Foreground))
                        addResources(resources, $"{prefix}.{eventKey}.foreground", ConvertHex(eventElement.Foreground));
                }
            }
        }

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

    private static Color ConvertHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Color.Parse("#FF00FF");
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return Color.Parse(hex);
    }
}
