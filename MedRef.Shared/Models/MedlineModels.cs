using System.Text.Json.Serialization;

namespace MedRef.Shared.Models;

public class MedlineRoot
{
    [JsonPropertyName("feed")]
    public MedlineFeed Feed { get; set; } = new();
}

public class MedlineFeed
{
    [JsonPropertyName("base")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public MedlineTextProperty Title { get; set; } = new();

    [JsonPropertyName("updated")]
    public MedlineValueProperty Updated { get; set; } = new();

    [JsonPropertyName("subtitle")]
    public MedlineTextProperty Subtitle { get; set; } = new();

    [JsonPropertyName("category")]
    public List<MedlineCategory> Categories { get; set; } = new();

    [JsonPropertyName("entry")]
    public List<MedlineEntry> Entries { get; set; } = new();
}

public class MedlineEntry
{
    [JsonPropertyName("title")]
    public MedlineTextProperty Title { get; set; } = new();

    [JsonPropertyName("link")]
    public List<MedlineLink> Links { get; set; } = new();

    [JsonPropertyName("id")]
    public MedlineValueProperty Id { get; set; } = new();

    [JsonPropertyName("summary")]
    public MedlineTextProperty Summary { get; set; } = new();

    [JsonPropertyName("updated")]
    public MedlineValueProperty Updated { get; set; } = new();
}

public class MedlineTextProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("_value")]
    public string Value { get; set; } = string.Empty;
}

public class MedlineValueProperty
{
    [JsonPropertyName("_value")]
    public string Value { get; set; } = string.Empty;
}

public class MedlineCategory
{
    [JsonPropertyName("scheme")]
    public string Scheme { get; set; } = string.Empty;

    [JsonPropertyName("term")]
    public string Term { get; set; } = string.Empty;
}

public class MedlineLink
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("rel")]
    public string Rel { get; set; } = string.Empty;
}