namespace Traveller_backend.APIModels
{
    // A town or city, used by the search form
    public class PlaceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? District { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
