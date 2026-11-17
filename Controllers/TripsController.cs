using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ITripService _tripService;

        public TripsController(ITripPlanService tripPlanService, IRouteService routeService, ITripService tripService)
        {
            _tripPlanService = tripPlanService;
            _routeService = routeService;
            _tripService = tripService;
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

        // POST api/trips - save a plan to My Trips
        [Authorize]
        [HttpPost("trips")]
        public async Task<IActionResult> SaveTrip([FromBody] TripPlanRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _tripService.SaveTripAsync(userId.Value, request);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetTrip), new { id = result.Data!.Id }, result.Data);
        }

        // GET api/trips - the logged-in user's saved trips
        [Authorize]
        [HttpGet("trips")]
        public async Task<IActionResult> GetTrips()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _tripService.GetTripsAsync(userId.Value));
        }

        // GET api/trips/5 - one saved trip in full
        [Authorize]
        [HttpGet("trips/{id:int}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _tripService.GetTripAsync(userId.Value, id);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // DELETE api/trips/5
        [Authorize]
        [HttpDelete("trips/{id:int}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (!await _tripService.DeleteTripAsync(userId.Value, id))
            {
                return NotFound("Trip not found");
            }

            return NoContent();
        }

        // The user id from the login token ("sub" claim)
        private int? GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(id, out var userId) ? userId : null;
        }
    }
}
