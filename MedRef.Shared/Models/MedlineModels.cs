using System.Text.Json.Serialization;

namespace MedRef.Shared.Models;
// These classes represent the structure of the JSON response from the Medline API. They are used to deserialize the API response into C# objects for easier access and manipulation within the application.
public class MedlineRoot
{
    [JsonPropertyName("feed")]
    public MedlineFeed Feed { get; set; } = new();
}

// The MedlineFeed class represents the main structure of the feed returned by the Medline API, including properties for the base URL, language, title, update time, subtitle, categories, and entries. Each entry in the feed is represented by the MedlineEntry class, which contains properties for the title, links, ID, summary, and update time. The MedlineTextProperty and MedlineValueProperty classes are used to represent text and value properties in the feed, while the MedlineCategory and MedlineLink classes represent categories and links associated with each entry.
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

// The MedlineEntry class represents an individual entry in the Medline feed, containing properties for the title, links, ID, summary, and update time. The MedlineTextProperty and MedlineValueProperty classes are used to represent text and value properties in the feed, while the MedlineCategory and MedlineLink classes represent categories and links associated with each entry. These classes are essential for deserializing the JSON response from the Medline API into C# objects that can be easily accessed and manipulated within the application.
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
    // The Type property indicates the type of text (e.g., "text", "html") and the Value property contains the actual text content. These properties are decorated with the JsonPropertyName attribute to specify the corresponding JSON property names during deserialization.
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("_value")]
    public string Value { get; set; } = string.Empty;
}


public class MedlineValueProperty
{
    // The Value property contains the actual value. This property is decorated with the JsonPropertyName attribute to specify the corresponding JSON property name during deserialization.
    [JsonPropertyName("_value")]
    public string Value { get; set; } = string.Empty;
}


public class MedlineCategory
{
    // The Scheme property indicates the categorization scheme (e.g., "http://www.w3.org/2005/Atom") and the Term property contains the specific category term. These properties are decorated with the JsonPropertyName attribute to specify the corresponding JSON property names during deserialization.
    [JsonPropertyName("scheme")]
    public string Scheme { get; set; } = string.Empty;

    [JsonPropertyName("term")]
    public string Term { get; set; } = string.Empty;
}

public class MedlineLink
{
    // The Href property contains the URL of the link, and the Rel property indicates the relationship type of the link (e.g., "self", "related"). These properties are decorated with the JsonPropertyName attribute to specify the corresponding JSON property names during deserialization.
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("rel")]
    public string Rel { get; set; } = string.Empty;
}