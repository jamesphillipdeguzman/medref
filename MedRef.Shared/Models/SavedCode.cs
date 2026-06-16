using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MedRef.Shared.Models
{
    // The SavedCode class represents a medical code that a user has saved in their profile. It contains properties for the unique identifier (Id), the user ID (UserId) to associate the code with a specific user, the code value (CodeValue), the disease name (DiseaseName) associated with the code, any custom notes (CustomNotes) the user may have added, the URL to the Medline entry (MedlineUrl) for reference, and the creation timestamp (CreatedAt) to track when the code was saved. This class is used to store and manage saved medical codes in MongoDB, allowing users to easily access and reference important medical information in their profiles.
    public class SavedCode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("CodeValue")]
        public string CodeValue { get; set; } = string.Empty;

        [BsonElement("DiseaseName")]
        public string DiseaseName { get; set; } = string.Empty;

        [BsonElement("CustomNotes")]
        public string CustomNotes { get; set; } = string.Empty;

        [BsonElement("MedlineUrl")]
        public string MedlineUrl { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}