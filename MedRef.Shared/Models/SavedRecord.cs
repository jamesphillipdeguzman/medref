namespace MedRef.Shared.Models;

using MongoDB.Bson.Serialization.Attributes;

// The SavedRecord class represents a medical record that a user has saved in their profile. It contains properties for the unique identifier (Id), the user ID (UserId) to associate the record with a specific user, the ICD code (IcdCode) and code value (Code) for the medical condition, the disease name (DiseaseName) associated with the record, any notes (Notes) the user may have added, and the URL to the Medline entry (MedlineUrl) for reference. This class is used to store and manage saved medical records in MongoDB, allowing users to easily access and reference important medical information in their profiles.
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