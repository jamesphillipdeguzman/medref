using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MedRef.Shared.Models
{
    public class UserProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty; // Links directly back to your User model Id

        [BsonElement("FullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Dynamic metadata storage for role-specific details 
        // (e.g., Nurse: "LicenseNumber", Patient: "EmergencyContact")
        [BsonElement("ProfileData")]
        public Dictionary<string, string> ProfileData { get; set; } = new();
    }
}