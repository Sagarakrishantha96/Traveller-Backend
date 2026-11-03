namespace Traveller_backend.APIModels
{
    // An attraction with its distance from the searched location
    public class AttractionResponse
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal VisitHours { get; set; }
        public string? OpenTime { get; set; }   // "HH:mm", null = open all day
        public string? CloseTime { get; set; }
        public decimal? TicketPrice { get; set; }
        public decimal? Rating { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public double DistanceKm { get; set; }
    }
}
