namespace MedRef.Shared.Models
{
    public class UserSession
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient"; // Default to Patient
        public bool IsAuthenticated { get; set; }
    }
}