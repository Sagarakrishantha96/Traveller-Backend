namespace Traveller_backend.APIModels
{
    // Counts shown at the top of the admin page
    public class AdminSummaryResponse
    {
        public int Places { get; set; }
        public int Attractions { get; set; }
        public int Stays { get; set; }
        public int Routes { get; set; }
        public int Users { get; set; }
        public int Trips { get; set; }
    }

    // Allowed values for the dropdowns (they match the database ENUM columns)
    public class AdminOptionsResponse
    {
        public string[] AttractionCategories { get; set; } = Array.Empty<string>();
        public string[] StayTypes { get; set; } = Array.Empty<string>();
        public string[] RouteModes { get; set; } = Array.Empty<string>();
        public string[] RouteOperators { get; set; } = Array.Empty<string>();
    }

    // A bus/train route with its stops, for the admin page
    public class AdminRouteResponse
    {
        public int Id { get; set; }
        public string Mode { get; set; } = string.Empty;
        public string RouteNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public bool IsAc { get; set; }
        public decimal? Fare { get; set; }
        public int DurationMinutes { get; set; }
        public string DepartureTimes { get; set; } = string.Empty;
        public List<AdminRouteStopResponse> Stops { get; set; } = new();
    }

    public class AdminRouteStopResponse
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public int MinutesFromStart { get; set; }
    }
}
