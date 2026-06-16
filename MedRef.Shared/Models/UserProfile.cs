using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MedRef.Shared.Models
{
    // The UserProfile class represents a user's profile in the application, containing properties for the unique identifier (Id), user ID (UserId) to link back to the User model, full name (FullName), phone number (PhoneNumber), last updated timestamp (UpdatedAt), and a dictionary for dynamic profile data (ProfileData) that can store role-specific details. The Id property is decorated with the BsonId and BsonRepresentation attributes to indicate that it is the primary key and should be stored as an ObjectId in MongoDB. The UserId property is also decorated with BsonRepresentation to ensure it is stored as an ObjectId, linking it directly to the User model's Id. This class is essential for managing user profiles and allowing users to view and update their profile information in the application.
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