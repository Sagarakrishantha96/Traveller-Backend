using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    public class TripService : ITripService
    {
        private const string Transport = "Transport";
        private const string Sightseeing = "Sightseeing";

        private readonly ITripRepository _tripRepository;
        private readonly ITripPlanService _tripPlanService;
        private readonly IRouteService _routeService;

        public TripService(ITripRepository tripRepository, ITripPlanService tripPlanService, IRouteService routeService)
        {
            _tripRepository = tripRepository;
            _tripPlanService = tripPlanService;
            _routeService = routeService;
        }

        // Plans the trip on the server (the client only sends the search) and stores it
        public async Task<ServiceResult<TripSummaryResponse>> SaveTripAsync(int userId, TripPlanRequest request)
        {
            var planResult = await _tripPlanService.PlanTripAsync(request);
            if (!planResult.Success) return ServiceResult<TripSummaryResponse>.Fail(planResult.ErrorMessage!);

            var plan = planResult.Data!;
            var startDate = request.StartDate!.Value.Date;

            var trip = new Trip
            {
                UserId = userId,
                FromPlaceId = plan.From.Id,
                ToPlaceId = plan.To.Id,
                NumberOfDays = plan.NumberOfDays,
                Title = $"{plan.From.Name} to {plan.To.Name}",
                StartDate = startDate,
                EndDate = startDate.AddDays(plan.NumberOfDays - 1),
                Budget = plan.EstimatedCost.Total,
                Status = "Upcoming",
                CreatedAt = DateTime.Now,
                Days = plan.Days.Select(d => new TripDay
                {
                    DayNumber = d.DayNumber,
                    DayDate = DateTime.Parse(d.Date),
                    Notes = d.Title,
                    StayId = d.Stay?.Id,
                    Items = d.Items.Select((item, index) => ToTripItem(item, index, plan)).ToList()
                }).ToList()
            };

            var saved = await _tripRepository.AddTripAsync(trip);

            var summary = ToSummary(saved, plan.CoverImageUrl);
            summary.FromPlace = plan.From.Name;
            summary.ToPlace = plan.To.Name;
            return ServiceResult<TripSummaryResponse>.Ok(summary);
        }

        public async Task<List<TripSummaryResponse>> GetTripsAsync(int userId)
        {
            var trips = await _tripRepository.GetTripsByUserAsync(userId);
            return trips.Select(t => ToSummary(t, null)).ToList();
        }

        // Rebuilds the saved plan in the same shape as POST /api/trips/plan
        public async Task<ServiceResult<SavedTripResponse>> GetTripAsync(int userId, int tripId)
        {
            var trip = await _tripRepository.GetTripAsync(userId, tripId);
            if (trip == null) return ServiceResult<SavedTripResponse>.Fail("Trip not found");
            if (trip.FromPlace == null || trip.ToPlace == null) return ServiceResult<SavedTripResponse>.Fail("This trip's places no longer exist");

            var from = trip.FromPlace;
            var to = trip.ToPlace;
            var response = new SavedTripResponse
            {
                Id = trip.Id,
                Title = trip.Title,
                Status = GetStatus(trip),
                SavedAt = trip.CreatedAt,
                From = ResponseMapper.ToPlaceResponse(from),
                To = ResponseMapper.ToPlaceResponse(to),
                NumberOfDays = trip.NumberOfDays,
                StartDate = trip.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Car = _routeService.EstimateCar(from, to)
            };

            (decimal Lat, decimal Lng) position = (to.Latitude, to.Longitude);
            var sawOutbound = false;

            foreach (var day in trip.Days.OrderBy(d => d.DayNumber))
            {
                var dayResponse = new TripDayResponse
                {
                    DayNumber = day.DayNumber,
                    Date = day.DayDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    Title = day.Notes ?? $"Day {day.DayNumber}"
                };

                foreach (var item in day.Items.OrderBy(i => i.SortOrder))
                {
                    if (item.Attraction != null)
                    {
                        var attraction = ResponseMapper.ToAttractionResponse(item.Attraction, to.Latitude, to.Longitude);
                        dayResponse.Items.Add(new TripItemResponse
                        {
                            Type = "Attraction",
                            StartTime = FormatTime(item.StartTime),
                            EndTime = FormatTime(item.EndTime),
                            Title = item.Title,
                            Note = item.Description,
                            TravelMinutes = GeoHelper.LocalTravelMinutes(position.Lat, position.Lng, attraction.Latitude, attraction.Longitude),
                            Attraction = attraction
                        });
                        position = (attraction.Latitude, attraction.Longitude);
                    }
                    else
                    {
                        // The first ride is the way there, the other one is the way home
                        var isReturn = sawOutbound;
                        sawOutbound = true;
                        var route = isReturn
                            ? await FindSavedRouteAsync(to.Id, from.Id, item.StartTime)
                            : await FindSavedRouteAsync(from.Id, to.Id, item.StartTime);

                        if (isReturn) response.Return = route; else response.Outbound = route;

                        dayResponse.Items.Add(new TripItemResponse
                        {
                            Type = isReturn ? "Return" : "Travel",
                            StartTime = FormatTime(item.StartTime),
                            EndTime = FormatTime(item.EndTime),
                            Title = item.Title,
                            Note = item.Description,
                            Route = route
                        });
                    }
                }

                if (day.Stay != null)
                {
                    dayResponse.Stay = ResponseMapper.ToStayResponse(day.Stay, to.Latitude, to.Longitude);
                    position = (day.Stay.Latitude, day.Stay.Longitude);
                }

                response.Days.Add(dayResponse);
            }

            var items = trip.Days.SelectMany(d => d.Items).ToList();
            response.CoverImageUrl = CoverImage(trip);
            response.EstimatedCost = new TripCostResponse
            {
                Transport = items.Where(i => i.Category == Transport).Sum(i => i.EstimatedCost ?? 0),
                Tickets = items.Where(i => i.AttractionId != null).Sum(i => i.EstimatedCost ?? 0),
                Stays = trip.Days.Sum(d => d.Stay?.PricePerNight ?? 0)
            };

            return ServiceResult<SavedTripResponse>.Ok(response);
        }

        public async Task<bool> DeleteTripAsync(int userId, int tripId)
        {
            return await _tripRepository.DeleteTripAsync(userId, tripId);
        }

        private static TripItem ToTripItem(TripItemResponse item, int index, TripPlanResponse plan)
        {
            var isVisit = item.Attraction != null;
            return new TripItem
            {
                AttractionId = item.Attraction?.Id,
                RouteId = item.Route?.Legs.FirstOrDefault()?.RouteId,
                Title = item.Title,
                Description = item.Note,
                Location = isVisit ? item.Attraction!.PlaceName
                    : item.Type == "Return" ? $"{plan.To.Name} → {plan.From.Name}" : $"{plan.From.Name} → {plan.To.Name}",
                StartTime = TimeSpan.Parse(item.StartTime),
                EndTime = TimeSpan.Parse(item.EndTime),
                EstimatedCost = isVisit ? item.Attraction!.TicketPrice : item.Route?.Fare,
                Category = isVisit ? Sightseeing : Transport,
                SortOrder = index
            };
        }

        // Finds the same bus/train again by its departure time; null if it was a car trip or the timetable changed
        private async Task<RouteOptionResponse?> FindSavedRouteAsync(int fromPlaceId, int toPlaceId, TimeSpan? departure)
        {
            if (departure == null) return null;
            var minutes = (int)departure.Value.TotalMinutes;
            var options = await _routeService.FindRoutesAsync(fromPlaceId, toPlaceId, minutes);
            return options.FirstOrDefault(o => o.DepartureMinutes == minutes);
        }

        private static TripSummaryResponse ToSummary(Trip trip, string? coverImageUrl) => new()
        {
            Id = trip.Id,
            Title = trip.Title,
            FromPlace = trip.FromPlace?.Name ?? string.Empty,
            ToPlace = trip.ToPlace?.Name ?? string.Empty,
            NumberOfDays = trip.NumberOfDays,
            StartDate = trip.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            EndDate = trip.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            Status = GetStatus(trip),
            EstimatedCost = trip.Budget,
            AttractionCount = trip.Days.SelectMany(d => d.Items).Count(i => i.AttractionId != null),
            CoverImageUrl = coverImageUrl ?? CoverImage(trip),
            SavedAt = trip.CreatedAt
        };

        // Photo of the best-rated attraction in the trip
        private static string? CoverImage(Trip trip) => trip.Days
            .SelectMany(d => d.Items)
            .Select(i => i.Attraction)
            .Where(a => a?.ImageUrl != null)
            .OrderByDescending(a => a!.Rating ?? 0)
            .FirstOrDefault()?.ImageUrl;

        // Worked out from the dates, so it is always current
        private static string GetStatus(Trip trip)
        {
            var today = DateTime.Today;
            if (trip.StartDate > today) return "Upcoming";
            if (trip.EndDate < today) return "Completed";
            return "Ongoing";
        }

        private static string FormatTime(TimeSpan? time) => time?.ToString(@"hh\:mm") ?? string.Empty;
    }
}
