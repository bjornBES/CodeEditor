using System.Text.Json;

public static class ThemeConverter
{
    /// <summary>
    /// This function will convert the theme file into a vscode format
    /// </summary>
    /// <param name="myTheme"></param>
    /// <returns></returns>
    public static string ConvertToVSCodeTheme(Theme myTheme)
    {
        var tokenColors = new List<object>();

        if (myTheme.SemanticTokenColors != null)
        {
            foreach (var kv in myTheme.SemanticTokenColors)
            {
                var scope = kv.Key;
                var style = kv.Value;

                string fontStyle = "";
                if (style.FontStyle != null)
                {
                    var parts = new List<string>();
                    if (style.FontStyle.Bold) parts.Add("bold");
                    if (style.FontStyle.Italic) parts.Add("italic");
                    if (style.FontStyle.Underline) parts.Add("underline");
                    if (style.FontStyle.Strikethrough) parts.Add("strikethrough");
                    fontStyle = string.Join(" ", parts);
                }

                tokenColors.Add(new
                {
                    scope = scope,
                    settings = new
                    {
                        foreground = string.IsNullOrEmpty(style.Foreground) ? null : "#" + style.Foreground,
                        background = string.IsNullOrEmpty(style.Background) ? null : "#" + style.Background,
                        fontStyle = fontStyle
                    }
                });
            }
        }

        // Flatten your UI colors (optional mapping)
        var colors = new Dictionary<string, string>();
        if (myTheme.Colors != null)
        {
            foreach (var kv in myTheme.Colors)
            {
                FlattenColors(kv.Key, kv.Value, colors);
            }
        }

        var vsCodeTheme = new
        {
            name = myTheme.Name,
            type = myTheme.Type,
            colors = colors,
            tokenColors = tokenColors
        };

        return JsonSerializer.Serialize(vsCodeTheme, new JsonSerializerOptions { WriteIndented = true });
    }

    private static void FlattenColors(string prefix, ThemeElement element, Dictionary<string, string> dict)
    {
        if (!string.IsNullOrEmpty(element.Background))
            dict[$"{prefix}.background"] = "#" + element.Background;

        if (!string.IsNullOrEmpty(element.Foreground))
            dict[$"{prefix}.foreground"] = "#" + element.Foreground;

        if (element.SubElements != null)
        {
            foreach (var sub in element.SubElements)
            {
                FlattenColors($"{prefix}.{sub.Key}", sub.Value, dict);
            }
        }
    }
}
