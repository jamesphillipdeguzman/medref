using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MedRef.Shared.Models
{
    // The User class represents a user in the application, containing properties for the unique identifier (Id), email address (Email), and role (Role) of the user. The Id property is decorated with the BsonId and BsonRepresentation attributes to indicate that it is the primary key and should be stored as an ObjectId in MongoDB. The Email property stores the user's email address, and the Role property indicates the user's role in the application (e.g., "Patient", "Doctor", "Admin"), defaulting to "Unassigned" until set by an administrator. This class is essential for managing user information and roles within the application, allowing for proper authentication and authorization based on user roles.
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Unassigned"; // Default to Unassigned until set by admin

    }
}