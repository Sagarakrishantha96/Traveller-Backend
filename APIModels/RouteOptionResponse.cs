using System.Text.Json.Serialization;

namespace Traveller_backend.APIModels
{
    // One way to get from A to B by bus/train - one leg, or two legs with a change
    public class RouteOptionResponse
    {
        public List<RouteLegResponse> Legs { get; set; } = new();
        public string DepartureTime { get; set; } = string.Empty;   // "HH:mm"
        public string ArrivalTime { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal? Fare { get; set; }                          // LKR, all legs
        public int Changes => Math.Max(0, Legs.Count - 1);

        // Minutes after midnight, used by the planner (not sent to the frontend)
        [JsonIgnore] public int DepartureMinutes { get; set; }
        [JsonIgnore] public int ArrivalMinutes { get; set; }
    }

    // One bus or train ride
    public class RouteLegResponse
    {
        public string Mode { get; set; } = string.Empty;            // Bus / Train
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public bool IsAc { get; set; }
        public string FromPlace { get; set; } = string.Empty;
        public string ToPlace { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal? Fare { get; set; }
    }

    // Driving estimate when going by car / taxi
    public class CarEstimateResponse
    {
        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }
    }
}
