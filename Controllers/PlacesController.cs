using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Traveller_backend.Contracts;

namespace Traveller_backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlacesController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        // GET api/places?q=sig
        [HttpGet("places")]
        public async Task<IActionResult> SearchPlaces([FromQuery] string? q)
        {
            return Ok(await _placeService.SearchPlacesAsync(q));
        }

        // GET api/attractions/nearby?lat=7.95&lng=80.76&radiusKm=40&category=Heritage
        [HttpGet("attractions/nearby")]
        public async Task<IActionResult> GetNearbyAttractions(
            [FromQuery, Required(ErrorMessage = "lat is required"), Range(-90, 90, ErrorMessage = "lat must be between -90 and 90")] decimal? lat,
            [FromQuery, Required(ErrorMessage = "lng is required"), Range(-180, 180, ErrorMessage = "lng must be between -180 and 180")] decimal? lng,
            [FromQuery, Range(1, 200, ErrorMessage = "radiusKm must be between 1 and 200")] double radiusKm = 40,
            [FromQuery] string? category = null)
        {
            return Ok(await _placeService.GetNearbyAttractionsAsync(lat!.Value, lng!.Value, radiusKm, category));
        }

        // GET api/stays/nearby?lat=7.95&lng=80.76&radiusKm=20&type=Homestay&maxPrice=10000
        [HttpGet("stays/nearby")]
        public async Task<IActionResult> GetNearbyStays(
            [FromQuery, Required(ErrorMessage = "lat is required"), Range(-90, 90, ErrorMessage = "lat must be between -90 and 90")] decimal? lat,
            [FromQuery, Required(ErrorMessage = "lng is required"), Range(-180, 180, ErrorMessage = "lng must be between -180 and 180")] decimal? lng,
            [FromQuery, Range(1, 200, ErrorMessage = "radiusKm must be between 1 and 200")] double radiusKm = 20,
            [FromQuery] string? type = null,
            [FromQuery, Range(0, double.MaxValue, ErrorMessage = "maxPrice cannot be negative")] decimal? maxPrice = null)
        {
            return Ok(await _placeService.GetNearbyStaysAsync(lat!.Value, lng!.Value, radiusKm, type, maxPrice));
        }
    }
}
