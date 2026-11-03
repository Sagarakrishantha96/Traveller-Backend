using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `stays` table - hotels, villas and homestays (ER: Accommodations)
    [Table("stays")]
    public class Stay
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("place_id")]
        public int PlaceId { get; set; }

        public Place Place { get; set; } = null!;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("price_per_night")]
        public decimal PricePerNight { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("booking_url")]
        public string? BookingUrl { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("latitude")]
        public decimal Latitude { get; set; }

        [Column("longitude")]
        public decimal Longitude { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
