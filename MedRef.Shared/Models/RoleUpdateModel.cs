namespace MedRef.Shared.Models
{
    // The RoleUpdateModel class is a simple model used to represent the data structure for updating a user's role in the application. It contains a single property, Role, which is a string that indicates the new role to be assigned to the user (e.g., "Patient", "Doctor"). This model is typically used in API requests when a user selects a new role after authentication, allowing the server to process the role update accordingly.
    public class RoleUpdateModel
    {
        public string Role { get; set; } = "Patient";
    }
}