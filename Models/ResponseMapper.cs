using Traveller_backend.APIModels;
using Traveller_backend.EntityModels;

namespace Traveller_backend.Models
{
    // Turns entities into API responses, shared by the place, planner and saved-trip services
    public static class ResponseMapper
    {
        public static PlaceResponse ToPlaceResponse(Place p, string? imageUrl = null, int attractionCount = 0) => new()
        {
            Id = p.Id,
            Name = p.Name,
            District = p.District,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            ImageUrl = imageUrl,
            AttractionCount = attractionCount
        };

        // Needs a.Place loaded; distance is measured from (fromLat, fromLng)
        public static AttractionResponse ToAttractionResponse(Attraction a, decimal fromLat, decimal fromLng) => new()
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
            DistanceKm = Math.Round(GeoHelper.DistanceKm(fromLat, fromLng, a.Latitude, a.Longitude), 1)
        };

        // Needs s.Place loaded; distance is measured from (fromLat, fromLng)
        public static StayResponse ToStayResponse(Stay s, decimal fromLat, decimal fromLng) => new()
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
            DistanceKm = Math.Round(GeoHelper.DistanceKm(fromLat, fromLng, s.Latitude, s.Longitude), 1)
        };
    }
}
