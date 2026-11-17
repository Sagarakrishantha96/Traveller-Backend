using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `itinerary_days` table - one day of a saved trip (ER: TripDays)
    [Table("itinerary_days")]
    public class TripDay
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("trip_id")]
        public int TripId { get; set; }

        public Trip Trip { get; set; } = null!;

        [Column("destination_id")]
        public int? DestinationId { get; set; }

        [Column("stay_id")]
        public int? StayId { get; set; }                    // the night's stay

        public Stay? Stay { get; set; }

        [Column("day_number")]
        public int DayNumber { get; set; }

        [Column("day_date")]
        public DateTime? DayDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }                  // day title, e.g. "Sigiriya & Habarana"

        public List<TripItem> Items { get; set; } = new();
    }
}
