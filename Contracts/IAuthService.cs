using Traveller_backend.APIModels;
using Traveller_backend.Models;

namespace Traveller_backend.Contracts
{
    // Business logic for authentication
    public interface IAuthService
    {
        Task<ServiceResult<UserResponse>> RegisterAsync(RegisterRequest request);
        Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
