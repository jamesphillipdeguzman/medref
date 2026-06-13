using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MedRef.Shared.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Unassigned"; // Default to Unassigned until set by admin

    }
}