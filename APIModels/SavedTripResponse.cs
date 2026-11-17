namespace Traveller_backend.APIModels
{
    // A saved trip in the "My Trips" list
    public class TripSummaryResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FromPlace { get; set; } = string.Empty;
        public string ToPlace { get; set; } = string.Empty;
        public int NumberOfDays { get; set; }
        public string StartDate { get; set; } = string.Empty;   // "yyyy-MM-dd"
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;      // Upcoming / Ongoing / Completed
        public decimal? EstimatedCost { get; set; }
        public int AttractionCount { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime SavedAt { get; set; }
    }

    // A saved trip opened in full - the same shape as a fresh plan, plus its id
    public class SavedTripResponse : TripPlanResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }
}
