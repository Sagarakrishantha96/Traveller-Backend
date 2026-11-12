using System.ComponentModel.DataAnnotations;

namespace Traveller_backend.APIModels
{
    // Bodies of the admin add/edit endpoints. Coordinates must be inside Sri Lanka.

    public class PlaceRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be 100 characters or less")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "District must be 100 characters or less")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        [Range(5.5, 10.0, ErrorMessage = "Latitude must be inside Sri Lanka (5.5 to 10)")]
        public decimal? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(79.0, 82.5, ErrorMessage = "Longitude must be inside Sri Lanka (79 to 82.5)")]
        public decimal? Longitude { get; set; }
    }

    public class AttractionRequest
    {
        [Required(ErrorMessage = "Place is required")]
        public int? PlaceId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name must be 150 characters or less")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        [Range(0.25, 12, ErrorMessage = "Visit hours must be between 0.25 and 12")]
        public decimal VisitHours { get; set; } = 1;

        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Open time must look like 07:30")]
        public string? OpenTime { get; set; }

        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Close time must look like 17:30")]
        public string? CloseTime { get; set; }

        [Range(0, 1000000, ErrorMessage = "Ticket price cannot be negative")]
        public decimal? TicketPrice { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal? Rating { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        [Range(5.5, 10.0, ErrorMessage = "Latitude must be inside Sri Lanka (5.5 to 10)")]
        public decimal? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(79.0, 82.5, ErrorMessage = "Longitude must be inside Sri Lanka (79 to 82.5)")]
        public decimal? Longitude { get; set; }

        [Url(ErrorMessage = "Image URL must be a full link (https://...)")]
        [StringLength(500, ErrorMessage = "Image URL must be 500 characters or less")]
        public string? ImageUrl { get; set; }
    }

    public class StayRequest
    {
        [Required(ErrorMessage = "Place is required")]
        public int? PlaceId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name must be 150 characters or less")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price per night is required")]
        [Range(0, 10000000, ErrorMessage = "Price per night cannot be negative")]
        public decimal? PricePerNight { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal? Rating { get; set; }

        [StringLength(255, ErrorMessage = "Address must be 255 characters or less")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Phone must be 20 characters or less")]
        public string? Phone { get; set; }

        [Url(ErrorMessage = "Booking URL must be a full link (https://...)")]
        [StringLength(500, ErrorMessage = "Booking URL must be 500 characters or less")]
        public string? BookingUrl { get; set; }

        [Url(ErrorMessage = "Image URL must be a full link (https://...)")]
        [StringLength(500, ErrorMessage = "Image URL must be 500 characters or less")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        [Range(5.5, 10.0, ErrorMessage = "Latitude must be inside Sri Lanka (5.5 to 10)")]
        public decimal? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(79.0, 82.5, ErrorMessage = "Longitude must be inside Sri Lanka (79 to 82.5)")]
        public decimal? Longitude { get; set; }
    }

    public class RouteRequest
    {
        [Required(ErrorMessage = "Mode is required")]
        public string Mode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route number is required")]
        [StringLength(20, ErrorMessage = "Route number must be 20 characters or less")]
        public string RouteNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name must be 150 characters or less")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Operator is required")]
        public string Operator { get; set; } = string.Empty;

        public bool IsAc { get; set; }

        [Range(0, 100000, ErrorMessage = "Fare cannot be negative")]
        public decimal? Fare { get; set; }

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int DurationMinutes { get; set; }

        // e.g. "05:30,07:00,09:15"
        [Required(ErrorMessage = "Departure times are required")]
        [RegularExpression(@"^\d{2}:\d{2}(\s*,\s*\d{2}:\d{2})*$", ErrorMessage = "Departure times must look like 05:30,07:00,09:15")]
        public string DepartureTimes { get; set; } = string.Empty;

        [MinLength(2, ErrorMessage = "A route needs at least 2 stops")]
        public List<RouteStopRequest> Stops { get; set; } = new();
    }

    public class RouteStopRequest
    {
        [Required(ErrorMessage = "Every stop needs a place")]
        public int? PlaceId { get; set; }

        [Range(0, 1440, ErrorMessage = "Minutes from start must be between 0 and 1440")]
        public int MinutesFromStart { get; set; }
    }
}
