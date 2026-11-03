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

            return places.Select(p => new PlaceResponse
            {
                Id = p.Id,
                Name = p.Name,
                District = p.District,
                Latitude = p.Latitude,
                Longitude = p.Longitude
            }).ToList();
        }

        public async Task<List<AttractionResponse>> GetNearbyAttractionsAsync(decimal latitude, decimal longitude, double radiusKm, string? category)
        {
            // The box filter runs in SQL; the exact distance is checked here
            var bounds = GeoHelper.GetBounds(latitude, longitude, radiusKm);
            var attractions = await _placeRepository.GetAttractionsInAreaAsync(bounds, category);

            return attractions
                .Select(a => new AttractionResponse
                {
                    Id = a.Id,
                    PlaceId = a.PlaceId,
                    PlaceName = a.Place.Name,
                    Name = a.Name,
                    Description = a.Description,
                    Category = a.Category,
                    VisitHours = a.VisitHours,
                    OpenTime = a.OpenTime?.ToString(@"hh\:mm"),
                    CloseTime = a.CloseTime?.ToString(@"hh\:mm"),
                    TicketPrice = a.TicketPrice,
                    Rating = a.Rating,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    ImageUrl = a.ImageUrl,
                    DistanceKm = Math.Round(GeoHelper.DistanceKm(latitude, longitude, a.Latitude, a.Longitude), 1)
                })
                .Where(a => a.DistanceKm <= radiusKm)
                .OrderBy(a => a.DistanceKm)
                .ToList();
        }

        public async Task<List<StayResponse>> GetNearbyStaysAsync(decimal latitude, decimal longitude, double radiusKm, string? type, decimal? maxPrice)
        {
            var bounds = GeoHelper.GetBounds(latitude, longitude, radiusKm);
            var stays = await _placeRepository.GetStaysInAreaAsync(bounds, type, maxPrice);

            return stays
                .Select(s => new StayResponse
                {
                    Id = s.Id,
                    PlaceId = s.PlaceId,
                    PlaceName = s.Place.Name,
                    Name = s.Name,
                    Type = s.Type,
                    PricePerNight = s.PricePerNight,
                    Rating = s.Rating,
                    Address = s.Address,
                    Phone = s.Phone,
                    BookingUrl = s.BookingUrl,
                    ImageUrl = s.ImageUrl,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    DistanceKm = Math.Round(GeoHelper.DistanceKm(latitude, longitude, s.Latitude, s.Longitude), 1)
                })
                .Where(s => s.DistanceKm <= radiusKm)
                .OrderBy(s => s.DistanceKm)
                .ToList();
        }
    }
}
