using Traveller_backend.APIModels;
using Traveller_backend.Models;

namespace Traveller_backend.Contracts
{
    // Builds a day-by-day itinerary
    public interface ITripPlanService
    {
        Task<ServiceResult<TripPlanResponse>> PlanTripAsync(TripPlanRequest request);
    }
}
