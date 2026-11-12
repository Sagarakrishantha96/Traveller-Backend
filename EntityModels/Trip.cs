using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `trips` table - a trip a user has saved (ER: Trips)
    [Table("trips")]
    public class Trip
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("from_place_id")]
        public int? FromPlaceId { get; set; }

        public Place? FromPlace { get; set; }

        [Column("to_place_id")]
        public int? ToPlaceId { get; set; }

        public Place? ToPlace { get; set; }

        [Column("number_of_days")]
        public int NumberOfDays { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("budget")]
        public decimal? Budget { get; set; }                // estimated cost when saved, LKR

        [Column("currency")]
        public string Currency { get; set; } = "LKR";

        [Column("status")]
        public string Status { get; set; } = "Planning";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public List<TripDay> Days { get; set; } = new();
    }
}
