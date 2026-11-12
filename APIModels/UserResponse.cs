namespace Traveller_backend.APIModels
{
    // What the API returns about a user (never includes the password hash)
    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "User";      // User / Admin
        public DateTime CreatedAt { get; set; }
    }
}
