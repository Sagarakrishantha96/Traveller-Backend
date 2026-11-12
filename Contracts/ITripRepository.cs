using Traveller_backend.EntityModels;

namespace Traveller_backend.Contracts
{
    // Data access for saved trips
    public interface ITripRepository
    {
        Task<Trip> AddTripAsync(Trip trip);
        Task<List<Trip>> GetTripsByUserAsync(int userId);
        Task<Trip?> GetTripAsync(int userId, int tripId);
        Task<bool> DeleteTripAsync(int userId, int tripId);
    }
}
