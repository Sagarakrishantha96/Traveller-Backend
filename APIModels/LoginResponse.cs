namespace Traveller_backend.APIModels
{
    // What the API returns after a successful login
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserResponse User { get; set; } = new();
    }
}
