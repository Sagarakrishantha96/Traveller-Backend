using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Traveller_backend.APIModels;
using Traveller_backend.Contracts;

namespace Traveller_backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class TripsController : ControllerBase
    {
        private readonly ITripPlanService _tripPlanService;
        private readonly IRouteService _routeService;

        public TripsController(ITripPlanService tripPlanService, IRouteService routeService)
        {
            _tripPlanService = tripPlanService;
            _routeService = routeService;
        }

        // POST api/trips/plan
        [HttpPost("trips/plan")]
        public async Task<IActionResult> PlanTrip([FromBody] TripPlanRequest request)
        {
            var result = await _tripPlanService.PlanTripAsync(request);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // GET api/routes?fromPlaceId=1&toPlaceId=6&after=06:00
        [HttpGet("routes")]
        public async Task<IActionResult> FindRoutes(
            [FromQuery, Required(ErrorMessage = "fromPlaceId is required")] int? fromPlaceId,
            [FromQuery, Required(ErrorMessage = "toPlaceId is required")] int? toPlaceId,
            [FromQuery] string? after = null)
        {
            var earliest = TimeSpan.TryParse(after, out var time) ? (int)time.TotalMinutes : 0;
            return Ok(await _routeService.FindRoutesAsync(fromPlaceId!.Value, toPlaceId!.Value, earliest));
        }
    }
}
