using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `route_stops` table - the places a route stops at, in order
    [Table("route_stops")]
    public class RouteStop
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("route_id")]
        public int RouteId { get; set; }

        public TransportRoute Route { get; set; } = null!;

        [Column("place_id")]
        public int PlaceId { get; set; }

        public Place Place { get; set; } = null!;

        [Column("stop_order")]
        public int StopOrder { get; set; }

        [Column("minutes_from_start")]
        public int MinutesFromStart { get; set; }
    }
}
