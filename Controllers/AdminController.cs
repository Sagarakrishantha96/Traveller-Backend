using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.Models;

namespace Traveller_backend.Controllers
{
    // Add, edit and delete travel data. Only users with role 'Admin' (users.role) can call these.
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET api/admin/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary() => Ok(await _adminService.GetSummaryAsync());

        // GET api/admin/options - values for the category / type / mode / operator dropdowns
        [HttpGet("options")]
        public IActionResult GetOptions() => Ok(_adminService.GetOptions());

        // ---------- Places ----------

        [HttpGet("places")]
        public async Task<IActionResult> GetPlaces() => Ok(await _adminService.GetPlacesAsync());

        [HttpPost("places")]
        public async Task<IActionResult> AddPlace([FromBody] PlaceRequest request) =>
            ToResult(await _adminService.SavePlaceAsync(null, request), created: true);

        [HttpPut("places/{id:int}")]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] PlaceRequest request) =>
            ToResult(await _adminService.SavePlaceAsync(id, request));

        [HttpDelete("places/{id:int}")]
        public async Task<IActionResult> DeletePlace(int id) => ToResult(await _adminService.DeletePlaceAsync(id));

        // ---------- Attractions ----------

        // GET api/admin/attractions?placeId=6
        [HttpGet("attractions")]
        public async Task<IActionResult> GetAttractions([FromQuery] int? placeId) =>
            Ok(await _adminService.GetAttractionsAsync(placeId));

        [HttpPost("attractions")]
        public async Task<IActionResult> AddAttraction([FromBody] AttractionRequest request) =>
            ToResult(await _adminService.SaveAttractionAsync(null, request), created: true);

        [HttpPut("attractions/{id:int}")]
        public async Task<IActionResult> UpdateAttraction(int id, [FromBody] AttractionRequest request) =>
            ToResult(await _adminService.SaveAttractionAsync(id, request));

        [HttpDelete("attractions/{id:int}")]
        public async Task<IActionResult> DeleteAttraction(int id) => ToResult(await _adminService.DeleteAttractionAsync(id));

        // ---------- Stays ----------

        // GET api/admin/stays?placeId=6
        [HttpGet("stays")]
        public async Task<IActionResult> GetStays([FromQuery] int? placeId) =>
            Ok(await _adminService.GetStaysAsync(placeId));

        [HttpPost("stays")]
        public async Task<IActionResult> AddStay([FromBody] StayRequest request) =>
            ToResult(await _adminService.SaveStayAsync(null, request), created: true);

        [HttpPut("stays/{id:int}")]
        public async Task<IActionResult> UpdateStay(int id, [FromBody] StayRequest request) =>
            ToResult(await _adminService.SaveStayAsync(id, request));

        [HttpDelete("stays/{id:int}")]
        public async Task<IActionResult> DeleteStay(int id) => ToResult(await _adminService.DeleteStayAsync(id));

        // ---------- Routes ----------

        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes() => Ok(await _adminService.GetRoutesAsync());

        [HttpPost("routes")]
        public async Task<IActionResult> AddRoute([FromBody] RouteRequest request) =>
            ToResult(await _adminService.SaveRouteAsync(null, request), created: true);

        [HttpPut("routes/{id:int}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteRequest request) =>
            ToResult(await _adminService.SaveRouteAsync(id, request));

        [HttpDelete("routes/{id:int}")]
        public async Task<IActionResult> DeleteRoute(int id) => ToResult(await _adminService.DeleteRouteAsync(id));

        // 201/200 with the saved item, 204 after a delete, 404 not found, 400 with the reason otherwise
        private IActionResult ToResult<T>(ServiceResult<T> result, bool created = false)
        {
            if (!result.Success) return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);
            if (result.Data is bool) return NoContent();
            return created ? StatusCode(StatusCodes.Status201Created, result.Data) : Ok(result.Data);
        }
    }
}
