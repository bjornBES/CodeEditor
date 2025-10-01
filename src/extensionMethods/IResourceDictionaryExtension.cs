
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

public static class IResourceDictionaryExtension
{
    public static Brush GetResource(this IResourceDictionary resource, string key)
    {
        if (key.Contains('+'))
        {
            return GetMergedResource(resource, key);
        }

        if (!resource.ContainsKey(key))
        {
            return new SolidColorBrush(Color.Parse("#FF00FF"));
        }
        Color color = (Color)resource[key];
        return new SolidColorBrush(color);
    }


    private static Brush GetMergedResource(IResourceDictionary resource, string key)
    {
        // Example: "tabs.hover+selected.background"
        var parts = key.Split('.');
        if (parts.Length < 2)
            return new SolidColorBrush(Color.Parse("#FF00FF"));

        string prefix = string.Join(".", parts.Take(parts.Length - 1)); // everything except property
        string prop = parts.Last(); // "background" or "foreground"

        // Extract states from last prefix part (e.g. "hover+selected")
        var prefixParts = prefix.Split('.');
        string statePart = prefixParts.Last();
        prefix = string.Join(".", prefixParts.Take(prefixParts.Length - 1));

        var states = statePart.Split('+');

        // Merge via ThemeService
        var merged = ThemeService.GetMergedState(prefix, states);
        if (merged.TryGetValue(prop, out var color))
        {
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Color.Parse("#FF00FF"));
    }
}