using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MedRef.Shared.Models
{
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