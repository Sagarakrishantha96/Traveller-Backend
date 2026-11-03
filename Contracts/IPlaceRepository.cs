using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Contracts
{
    // Data access for places, attractions and stays
    public interface IPlaceRepository
    {
        Task<List<Place>> SearchPlacesAsync(string? query, int limit);
        Task<List<Attraction>> GetAttractionsInAreaAsync(GeoBounds bounds, string? category);
        Task<List<Stay>> GetStaysInAreaAsync(GeoBounds bounds, string? type, decimal? maxPrice);
    }
}
