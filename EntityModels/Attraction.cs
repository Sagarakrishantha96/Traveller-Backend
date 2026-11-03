using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `attractions` table - things to see near a place
    [Table("attractions")]
    public class Attraction
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("place_id")]
        public int PlaceId { get; set; }

        public Place Place { get; set; } = null!;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("visit_hours")]
        public decimal VisitHours { get; set; }

        [Column("open_time")]
        public TimeSpan? OpenTime { get; set; }

        [Column("close_time")]
        public TimeSpan? CloseTime { get; set; }

        [Column("ticket_price")]
        public decimal? TicketPrice { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("latitude")]
        public decimal Latitude { get; set; }

        [Column("longitude")]
        public decimal Longitude { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
