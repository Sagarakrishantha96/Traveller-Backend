using System.ComponentModel.DataAnnotations.Schema;

namespace Traveller_backend.EntityModels
{
    // Maps to the `transport_routes` table - bus and train routes
    [Table("transport_routes")]
    public class TransportRoute
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("mode")]
        public string Mode { get; set; } = string.Empty;

        [Column("route_number")]
        public string RouteNumber { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("operator")]
        public string Operator { get; set; } = string.Empty;

        [Column("is_ac")]
        public bool IsAc { get; set; }

        [Column("fare")]
        public decimal? Fare { get; set; }

        [Column("duration_minutes")]
        public int DurationMinutes { get; set; }

        // Departure times from the first stop, e.g. "05:30,07:00,09:15"
        [Column("departure_times")]
        public string DepartureTimes { get; set; } = string.Empty;

        public List<RouteStop> Stops { get; set; } = new();
    }
}
