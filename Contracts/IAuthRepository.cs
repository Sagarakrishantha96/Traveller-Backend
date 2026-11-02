using Traveller_backend.EntityModels;

namespace Traveller_backend.Contracts
{
    // Data access for users
    public interface IAuthRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<User> AddUserAsync(User user);
    }
}
