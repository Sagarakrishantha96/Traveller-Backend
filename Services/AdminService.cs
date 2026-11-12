using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    public class AdminService : IAdminService
    {
        // Must match the ENUM columns in the database
        private static readonly string[] AttractionCategories = { "Heritage", "Nature", "Wildlife", "Religious", "Adventure", "Beach", "Viewpoint", "Other" };
        private static readonly string[] StayTypes = { "Hotel", "Villa", "Homestay", "Guesthouse", "Resort" };
        private static readonly string[] RouteModes = { "Bus", "Train" };
        private static readonly string[] RouteOperators = { "CTB", "Private", "Expressway", "SriLankaRailways" };

        private readonly IAdminRepository _adminRepository;
        private readonly IPlaceService _placeService;

        public AdminService(IAdminRepository adminRepository, IPlaceService placeService)
        {
            _adminRepository = adminRepository;
            _placeService = placeService;
        }

        public async Task<AdminSummaryResponse> GetSummaryAsync() => new()
        {
            Places = await _adminRepository.CountAsync<Place>(),
            Attractions = await _adminRepository.CountAsync<Attraction>(),
            Stays = await _adminRepository.CountAsync<Stay>(),
            Routes = await _adminRepository.CountAsync<TransportRoute>(),
            Users = await _adminRepository.CountAsync<User>(),
            Trips = await _adminRepository.CountTripsAsync()
        };

        public AdminOptionsResponse GetOptions() => new()
        {
            AttractionCategories = AttractionCategories,
            StayTypes = StayTypes,
            RouteModes = RouteModes,
            RouteOperators = RouteOperators
        };

        // ---------- Places ----------

        public async Task<List<PlaceResponse>> GetPlacesAsync() => await _placeService.SearchPlacesAsync(null);

        public async Task<ServiceResult<PlaceResponse>> SavePlaceAsync(int? id, PlaceRequest request)
        {
            var name = request.Name.Trim();
            if (await _adminRepository.PlaceNameExistsAsync(name, id))
                return ServiceResult<PlaceResponse>.Fail($"A place called \"{name}\" already exists");

            var place = id.HasValue ? await _adminRepository.FindAsync<Place>(id.Value) : new Place { CreatedAt = DateTime.Now };
            if (place == null) return ServiceResult<PlaceResponse>.NotFound("Place not found");

            place.Name = name;
            place.District = Clean(request.District);
            place.Latitude = request.Latitude!.Value;
            place.Longitude = request.Longitude!.Value;

            if (!id.HasValue) _adminRepository.Add(place);
            await _adminRepository.SaveChangesAsync();

            return ServiceResult<PlaceResponse>.Ok(new PlaceResponse
            {
                Id = place.Id, Name = place.Name, District = place.District,
                Latitude = place.Latitude, Longitude = place.Longitude
            });
        }

        public async Task<ServiceResult<bool>> DeletePlaceAsync(int id)
        {
            var place = await _adminRepository.FindAsync<Place>(id);
            if (place == null) return ServiceResult<bool>.NotFound("Place not found");

            // Deleting would also delete its attractions, stays and route stops, so block it
            if (await _adminRepository.PlaceIsUsedAsync(id))
                return ServiceResult<bool>.Fail($"{place.Name} still has attractions, stays or route stops. Delete or move those first.");

            _adminRepository.Remove(place);
            await _adminRepository.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        // ---------- Attractions ----------

        public async Task<List<AttractionResponse>> GetAttractionsAsync(int? placeId)
        {
            var attractions = await _adminRepository.GetAttractionsAsync(placeId);
            return attractions.Select(ToAttractionResponse).ToList();
        }

        public async Task<ServiceResult<AttractionResponse>> SaveAttractionAsync(int? id, AttractionRequest request)
        {
            if (!AttractionCategories.Contains(request.Category))
                return ServiceResult<AttractionResponse>.Fail("Choose a valid category");

            var hours = ParseOpeningHours(request.OpenTime, request.CloseTime);
            if (hours.Error != null) return ServiceResult<AttractionResponse>.Fail(hours.Error);

            var place = await _adminRepository.FindAsync<Place>(request.PlaceId!.Value);
            if (place == null) return ServiceResult<AttractionResponse>.Fail("Choose a valid place");

            var attraction = id.HasValue ? await _adminRepository.GetAttractionAsync(id.Value) : new Attraction { CreatedAt = DateTime.Now };
            if (attraction == null) return ServiceResult<AttractionResponse>.NotFound("Attraction not found");

            attraction.PlaceId = place.Id;
            attraction.Place = place;
            attraction.Name = request.Name.Trim();
            attraction.Description = Clean(request.Description);
            attraction.Category = request.Category;
            attraction.VisitHours = request.VisitHours;
            attraction.OpenTime = hours.Open;
            attraction.CloseTime = hours.Close;
            attraction.TicketPrice = request.TicketPrice;
            attraction.Rating = request.Rating;
            attraction.Latitude = request.Latitude!.Value;
            attraction.Longitude = request.Longitude!.Value;
            attraction.ImageUrl = Clean(request.ImageUrl);

            if (!id.HasValue) _adminRepository.Add(attraction);
            await _adminRepository.SaveChangesAsync();

            return ServiceResult<AttractionResponse>.Ok(ToAttractionResponse(attraction));
        }

        // Saved trips keep the day but lose this visit (activities.attraction_id is set to NULL)
        public async Task<ServiceResult<bool>> DeleteAttractionAsync(int id)
        {
            var attraction = await _adminRepository.FindAsync<Attraction>(id);
            if (attraction == null) return ServiceResult<bool>.NotFound("Attraction not found");

            _adminRepository.Remove(attraction);
            await _adminRepository.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        // ---------- Stays ----------

        public async Task<List<StayResponse>> GetStaysAsync(int? placeId)
        {
            var stays = await _adminRepository.GetStaysAsync(placeId);
            return stays.Select(ToStayResponse).ToList();
        }

        public async Task<ServiceResult<StayResponse>> SaveStayAsync(int? id, StayRequest request)
        {
            if (!StayTypes.Contains(request.Type))
                return ServiceResult<StayResponse>.Fail("Choose a valid stay type");

            var place = await _adminRepository.FindAsync<Place>(request.PlaceId!.Value);
            if (place == null) return ServiceResult<StayResponse>.Fail("Choose a valid place");

            var stay = id.HasValue ? await _adminRepository.GetStayAsync(id.Value) : new Stay { CreatedAt = DateTime.Now };
            if (stay == null) return ServiceResult<StayResponse>.NotFound("Stay not found");

            stay.PlaceId = place.Id;
            stay.Place = place;
            stay.Name = request.Name.Trim();
            stay.Type = request.Type;
            stay.PricePerNight = request.PricePerNight!.Value;
            stay.Rating = request.Rating;
            stay.Address = Clean(request.Address);
            stay.Phone = Clean(request.Phone);
            stay.BookingUrl = Clean(request.BookingUrl);
            stay.ImageUrl = Clean(request.ImageUrl);
            stay.Latitude = request.Latitude!.Value;
            stay.Longitude = request.Longitude!.Value;

            if (!id.HasValue) _adminRepository.Add(stay);
            await _adminRepository.SaveChangesAsync();

            return ServiceResult<StayResponse>.Ok(ToStayResponse(stay));
        }

        // Saved trips keep the night but lose this stay (itinerary_days.stay_id is set to NULL)
        public async Task<ServiceResult<bool>> DeleteStayAsync(int id)
        {
            var stay = await _adminRepository.FindAsync<Stay>(id);
            if (stay == null) return ServiceResult<bool>.NotFound("Stay not found");

            _adminRepository.Remove(stay);
            await _adminRepository.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        // ---------- Routes ----------

        public async Task<List<AdminRouteResponse>> GetRoutesAsync()
        {
            var routes = await _adminRepository.GetRoutesAsync();
            return routes.Select(ToRouteResponse).ToList();
        }

        public async Task<ServiceResult<AdminRouteResponse>> SaveRouteAsync(int? id, RouteRequest request)
        {
            var error = await ValidateRouteAsync(request);
            if (error != null) return ServiceResult<AdminRouteResponse>.Fail(error);

            var route = id.HasValue ? await _adminRepository.GetRouteAsync(id.Value) : new TransportRoute();
            if (route == null) return ServiceResult<AdminRouteResponse>.NotFound("Route not found");

            route.Mode = request.Mode;
            route.RouteNumber = request.RouteNumber.Trim();
            route.Name = request.Name.Trim();
            route.Operator = request.Operator;
            route.IsAc = request.IsAc;
            route.Fare = request.Fare;
            route.DurationMinutes = request.DurationMinutes;
            route.DepartureTimes = string.Join(",", request.DepartureTimes.Split(',').Select(t => t.Trim()));

            // Stops are numbered in the order they were sent
            var stops = request.Stops.Select((s, index) => new RouteStop
            {
                PlaceId = s.PlaceId!.Value,
                StopOrder = index + 1,
                MinutesFromStart = s.MinutesFromStart
            }).ToList();

            if (id.HasValue) await _adminRepository.UpdateRouteAsync(route, stops);
            else await _adminRepository.AddRouteAsync(route, stops);

            var saved = await _adminRepository.GetRouteAsync(route.Id);
            return ServiceResult<AdminRouteResponse>.Ok(ToRouteResponse(saved!));
        }

        // Stops are removed by the database; saved trips keep the ride text (activities.route_id is set to NULL)
        public async Task<ServiceResult<bool>> DeleteRouteAsync(int id)
        {
            var route = await _adminRepository.FindAsync<TransportRoute>(id);
            if (route == null) return ServiceResult<bool>.NotFound("Route not found");

            _adminRepository.Remove(route);
            await _adminRepository.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        private async Task<string?> ValidateRouteAsync(RouteRequest request)
        {
            if (!RouteModes.Contains(request.Mode)) return "Choose a valid mode";
            if (!RouteOperators.Contains(request.Operator)) return "Choose a valid operator";

            var times = request.DepartureTimes.Split(',').Select(t => t.Trim()).ToList();
            if (times.Any(t => !TimeSpan.TryParse(t, out var time) || time.TotalHours >= 24))
                return "Departure times must be real times, e.g. 05:30,07:00";

            var stops = request.Stops;
            if (stops.Count < 2) return "A route needs at least 2 stops";
            if (stops.Select(s => s.PlaceId).Distinct().Count() != stops.Count) return "Each place can only be a stop once";
            if (stops[0].MinutesFromStart != 0) return "The first stop must be at 0 minutes";

            for (var i = 1; i < stops.Count; i++)
            {
                if (stops[i].MinutesFromStart <= stops[i - 1].MinutesFromStart)
                    return "Each stop must be later than the one before it";
            }

            if (stops[^1].MinutesFromStart > request.DurationMinutes)
                return "The last stop can't be after the route's total duration";

            if (await _adminRepository.CountExistingPlacesAsync(stops.Select(s => s.PlaceId!.Value)) != stops.Count)
                return "One of the stops is not a valid place";

            return null;
        }

        // ---------- Helpers ----------

        private record OpeningHours(TimeSpan? Open, TimeSpan? Close, string? Error);

        // Both times or neither (neither = open all day); closing must be after opening
        private static OpeningHours ParseOpeningHours(string? open, string? close)
        {
            var hasOpen = !string.IsNullOrWhiteSpace(open);
            var hasClose = !string.IsNullOrWhiteSpace(close);
            if (!hasOpen && !hasClose) return new OpeningHours(null, null, null);
            if (hasOpen != hasClose) return new OpeningHours(null, null, "Enter both open and close times, or leave both empty for open all day");

            if (!TimeSpan.TryParse(open, out var openTime) || !TimeSpan.TryParse(close, out var closeTime) || closeTime.TotalHours >= 24)
                return new OpeningHours(null, null, "Opening times must be real times, e.g. 07:30");
            if (closeTime <= openTime) return new OpeningHours(null, null, "Close time must be after open time");

            return new OpeningHours(openTime, closeTime, null);
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static AttractionResponse ToAttractionResponse(Attraction a) => new()
        {
            Id = a.Id,
            PlaceId = a.PlaceId,
            PlaceName = a.Place.Name,
            Name = a.Name,
            Description = a.Description,
            Category = a.Category,
            VisitHours = a.VisitHours,
            OpenTime = a.OpenTime?.ToString(@"hh\:mm"),
            CloseTime = a.CloseTime?.ToString(@"hh\:mm"),
            TicketPrice = a.TicketPrice,
            Rating = a.Rating,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            ImageUrl = a.ImageUrl,
            DistanceKm = Math.Round(GeoHelper.DistanceKm(a.Place.Latitude, a.Place.Longitude, a.Latitude, a.Longitude), 1)
        };

        private static StayResponse ToStayResponse(Stay s) => new()
        {
            Id = s.Id,
            PlaceId = s.PlaceId,
            PlaceName = s.Place.Name,
            Name = s.Name,
            Type = s.Type,
            PricePerNight = s.PricePerNight,
            Rating = s.Rating,
            Address = s.Address,
            Phone = s.Phone,
            BookingUrl = s.BookingUrl,
            ImageUrl = s.ImageUrl,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            DistanceKm = Math.Round(GeoHelper.DistanceKm(s.Place.Latitude, s.Place.Longitude, s.Latitude, s.Longitude), 1)
        };

        private static AdminRouteResponse ToRouteResponse(TransportRoute r) => new()
        {
            Id = r.Id,
            Mode = r.Mode,
            RouteNumber = r.RouteNumber,
            Name = r.Name,
            Operator = r.Operator,
            IsAc = r.IsAc,
            Fare = r.Fare,
            DurationMinutes = r.DurationMinutes,
            DepartureTimes = r.DepartureTimes,
            Stops = r.Stops.OrderBy(s => s.StopOrder).Select(s => new AdminRouteStopResponse
            {
                PlaceId = s.PlaceId,
                PlaceName = s.Place?.Name ?? string.Empty,
                StopOrder = s.StopOrder,
                MinutesFromStart = s.MinutesFromStart
            }).ToList()
        };
    }
}
