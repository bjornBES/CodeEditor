
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

public static class IResourceDictionaryExtension
{
    public static Brush GetResource(this IResourceDictionary resource, string key)
    {
        if (!resource.ContainsKey(key))
        {
            return new SolidColorBrush(Color.Parse("#FF00FF"));
        }
        Color color = (Color)resource[key];
        return new SolidColorBrush(color);
    }
}