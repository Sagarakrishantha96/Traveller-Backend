using Traveller_backend.APIModels;
using Traveller_backend.EntityModels;

namespace Traveller_backend.Contracts
{
    // Finds bus/train options between two places
    public interface IRouteService
    {
        // Options leaving at or after earliestDeparture (minutes after midnight), earliest arrival first
        Task<List<RouteOptionResponse>> FindRoutesAsync(int fromPlaceId, int toPlaceId, int earliestDeparture);
        CarEstimateResponse EstimateCar(Place from, Place to);
    }
}
