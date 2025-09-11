using System.Text.Json.Serialization;

public class ThemeElement
{
    [JsonPropertyName("foreground")]
    public string Foreground { get; set; }
    [JsonPropertyName("background")]
    public string Background { get; set; }

    [JsonPropertyName("hoverForeground")]
    public string HoverForeground { get; set; }
    [JsonPropertyName("hoverBackground")]
    public string HoverBackground { get; set; }
    [JsonPropertyName("events")]
    public ThemeEventOptions Events { get; set; }

    [JsonPropertyName("subElements")]
    public Dictionary<string, ThemeElement> SubElements { get; set; }
}

public class ThemeEventOptions
{
    [JsonPropertyName("OnClick")]
    public ThemeElement OnClick { get; set; }
}
