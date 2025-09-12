using System.Formats.Asn1;
using System.Text.Json.Serialization;

public class ThemeElement
{
    [JsonPropertyName("foreground")]
    public string Foreground { get; set; }
    [JsonPropertyName("background")]
    public string Background { get; set; }

    [JsonPropertyName("events")]
    public ThemeEventOptions Events { get; set; }

    [JsonPropertyName("subElements")]
    public Dictionary<string, ThemeElementRaw> SubElements { get; set; }
}

public class ThemeElementRaw
{
    [JsonPropertyName("foreground")]
    public string Foreground { get; set; }
    [JsonPropertyName("background")]
    public string Background { get; set; }
    [JsonPropertyName("subElements")]
    public Dictionary<string, ThemeElementRaw> SubElements { get; set; }
}

public class ThemeEventOptions
{
    [JsonPropertyName("clickedBackground")]
    public string ClickedBackground { get; set; }
    [JsonPropertyName("clickedForeground")]
    public string ClickedForeground { get; set; }

    [JsonPropertyName("hoverForeground")]
    public string HoverForeground { get; set; }
    [JsonPropertyName("hoverBackground")]
    public string HoverBackground { get; set; }
}
