using System.ComponentModel.DataAnnotations;

namespace Traveller_backend.APIModels
{
    // Body of POST /api/auth/register
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name must be 100 characters or less")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [StringLength(150, ErrorMessage = "Email must be 150 characters or less")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^\+?\d{9,15}$", ErrorMessage = "Enter a valid phone number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
