using Traveller_backend.APIModels;

namespace Traveller_backend.Contracts
{
    // Business logic for searching places and what is near them
    public interface IPlaceService
    {
        Task<List<PlaceResponse>> SearchPlacesAsync(string? query);
        Task<List<AttractionResponse>> GetNearbyAttractionsAsync(decimal latitude, decimal longitude, double radiusKm, string? category);
        Task<List<StayResponse>> GetNearbyStaysAsync(decimal latitude, decimal longitude, double radiusKm, string? type, decimal? maxPrice);
    }
}
