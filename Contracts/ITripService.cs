using Traveller_backend.APIModels;
using Traveller_backend.Models;

namespace Traveller_backend.Contracts
{
    // Saving, listing, opening and deleting a user's trips
    public interface ITripService
    {
        Task<ServiceResult<TripSummaryResponse>> SaveTripAsync(int userId, TripPlanRequest request);
        Task<List<TripSummaryResponse>> GetTripsAsync(int userId);
        Task<ServiceResult<SavedTripResponse>> GetTripAsync(int userId, int tripId);
        Task<bool> DeleteTripAsync(int userId, int tripId);
    }
}
