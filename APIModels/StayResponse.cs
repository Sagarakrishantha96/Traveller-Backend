namespace Traveller_backend.APIModels
{
    // A place to stay with its distance from the searched location
    public class StayResponse
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal? Rating { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? BookingUrl { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double DistanceKm { get; set; }
    }
}
