namespace Traveller_backend.APIModels
{
    // The full day-by-day plan returned by POST /api/trips/plan
    public class TripPlanResponse
    {
        public PlaceResponse From { get; set; } = new();
        public PlaceResponse To { get; set; } = new();
        public int NumberOfDays { get; set; }
        public string StartDate { get; set; } = string.Empty;       // "yyyy-MM-dd"
        public string? CoverImageUrl { get; set; }
        public RouteOptionResponse? Outbound { get; set; }           // null = no bus/train, go by car
        public RouteOptionResponse? Return { get; set; }
        public CarEstimateResponse Car { get; set; } = new();
        public List<TripDayResponse> Days { get; set; } = new();
        public TripCostResponse EstimatedCost { get; set; } = new();
    }

    public class TripDayResponse
    {
        public int DayNumber { get; set; }
        public string Date { get; set; } = string.Empty;            // "yyyy-MM-dd"
        public string Title { get; set; } = string.Empty;
        public List<TripItemResponse> Items { get; set; } = new();
        public StayResponse? Stay { get; set; }                     // null on the last day
        public List<StayResponse> OtherStays { get; set; } = new();
    }

    public class TripItemResponse
    {
        public string Type { get; set; } = string.Empty;            // Travel / Attraction / Return
        public string StartTime { get; set; } = string.Empty;       // "HH:mm"
        public string EndTime { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int TravelMinutes { get; set; }                      // getting here from the previous stop
        public AttractionResponse? Attraction { get; set; }
        public RouteOptionResponse? Route { get; set; }
    }

    public class TripCostResponse
    {
        public decimal Transport { get; set; }
        public decimal Tickets { get; set; }
        public decimal Stays { get; set; }
        public decimal Total => Transport + Tickets + Stays;
    }
}
