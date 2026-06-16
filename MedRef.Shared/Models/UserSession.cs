namespace MedRef.Shared.Models
{
    // The UserSession class represents the session information for a user in the application. It contains properties for the user's email (Email), role (Role), and authentication status (IsAuthenticated). The Email property stores the user's email address, while the Role property indicates the user's role in the application (e.g., "Patient", "Doctor"), defaulting to "Patient". The IsAuthenticated property is a boolean that indicates whether the user is currently authenticated or not. This class is used to manage user sessions and provide relevant information about the user's authentication status and role within the application.
    public class UserSession
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient"; // Default to Patient
        public bool IsAuthenticated { get; set; }
    }
}