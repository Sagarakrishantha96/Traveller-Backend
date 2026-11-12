using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    public class PlaceService : IPlaceService
    {
        // Max results when the user is typing; an empty search returns every place
        private const int SearchLimit = 10;

        private readonly IPlaceRepository _placeRepository;

        public PlaceService(IPlaceRepository placeRepository)
        {
            _placeRepository = placeRepository;
        }

        public async Task<List<PlaceResponse>> SearchPlacesAsync(string? query)
        {
            var limit = string.IsNullOrWhiteSpace(query) ? int.MaxValue : SearchLimit;
            var places = await _placeRepository.SearchPlacesAsync(query?.Trim(), limit);

            // Each place uses the photo of its best-rated attraction
            var images = (await _placeRepository.GetAttractionsWithImagesAsync())
                .GroupBy(a => a.PlaceId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.Rating ?? 0).First().ImageUrl);
            var counts = await _placeRepository.GetAttractionCountsAsync();

            return places
                .Select(p => ResponseMapper.ToPlaceResponse(p, images.GetValueOrDefault(p.Id), counts.GetValueOrDefault(p.Id)))
                .ToList();
        }

        public async Task<List<AttractionResponse>> GetNearbyAttractionsAsync(decimal latitude, decimal longitude, double radiusKm, string? category)
        {
            // The box filter runs in SQL; the exact distance is checked here
            var bounds = GeoHelper.GetBounds(latitude, longitude, radiusKm);
            var attractions = await _placeRepository.GetAttractionsInAreaAsync(bounds, category);

            return attractions
                .Select(a => ResponseMapper.ToAttractionResponse(a, latitude, longitude))
                .Where(a => a.DistanceKm <= radiusKm)
                .OrderBy(a => a.DistanceKm)
                .ToList();
        }

        public async Task<List<StayResponse>> GetNearbyStaysAsync(decimal latitude, decimal longitude, double radiusKm, string? type, decimal? maxPrice)
        {
            var bounds = GeoHelper.GetBounds(latitude, longitude, radiusKm);
            var stays = await _placeRepository.GetStaysInAreaAsync(bounds, type, maxPrice);

            return stays
                .Select(s => ResponseMapper.ToStayResponse(s, latitude, longitude))
                .Where(s => s.DistanceKm <= radiusKm)
                .OrderBy(s => s.DistanceKm)
                .ToList();
        }
    }
}
