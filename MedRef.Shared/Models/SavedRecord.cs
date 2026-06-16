namespace MedRef.Shared.Models;

using MongoDB.Bson.Serialization.Attributes;

public class SavedRecord
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string IcdCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string DiseaseName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string MedlineUrl { get; set; } = string.Empty;
}