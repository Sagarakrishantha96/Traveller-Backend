using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `activities` table - a visit or a ride in a saved day (ER: TripItems)
    [Table("activities")]
    public class TripItem
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("day_id")]
        public int DayId { get; set; }

        public TripDay Day { get; set; } = null!;

        [Column("attraction_id")]
        public int? AttractionId { get; set; }              // set for a visit

        public Attraction? Attraction { get; set; }

        [Column("route_id")]
        public int? RouteId { get; set; }                   // set for a bus/train ride (first leg)

        public TransportRoute? Route { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }            // the item's note

        [Column("location")]
        public string? Location { get; set; }

        [Column("start_time")]
        public TimeSpan? StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan? EndTime { get; set; }

        [Column("estimated_cost")]
        public decimal? EstimatedCost { get; set; }         // ticket or fare, LKR

        [Column("category")]
        public string Category { get; set; } = "Other";     // Sightseeing / Transport / ...

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }
    }
}
