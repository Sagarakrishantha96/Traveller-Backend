using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _authRepository.EmailExistsAsync(email))
            {
                return ServiceResult<UserResponse>.Fail("An account with this email already exists");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.Now
            };

            var savedUser = await _authRepository.AddUserAsync(user);

            return ServiceResult<UserResponse>.Ok(new UserResponse
            {
                Id = savedUser.Id,
                FullName = savedUser.FullName,
                Email = savedUser.Email,
                Phone = savedUser.Phone,
                CreatedAt = savedUser.CreatedAt
            });
        }
    }
}
