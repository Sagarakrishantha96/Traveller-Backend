using System.ComponentModel.DataAnnotations;

namespace Traveller_backend.APIModels
{
    // Body of POST /api/trips/plan
    public class TripPlanRequest
    {
        [Required(ErrorMessage = "Starting place is required")]
        public int? FromPlaceId { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        public int? ToPlaceId { get; set; }

        [Range(1, 7, ErrorMessage = "Days must be between 1 and 7")]
        public int Days { get; set; } = 2;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime? StartDate { get; set; }
    }
}
