using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    // Builds the itinerary: travel there, fill each day with the best nearby
    // attractions (greedy), a stay each night, and travel home on the last day.
    public class TripPlanService : ITripPlanService
    {
        private const int EarliestDeparture = 5 * 60;        // first bus/train we'd suggest
        private const int CarDeparture = 6 * 60;
        private const int DayStart = 8 * 60;
        private const int DayEnd = 18 * 60;
        private const int ReturnAfter = 14 * 60;            // last day: sightsee in the morning, leave after lunch
        private const int SettleInMinutes = 30;             // after arriving, before the first visit
        private const int MaxAttractionsPerDay = 4;
        private const double AttractionRadiusKm = 40;
        private const double StayRadiusKm = 15;
        private const double LocalSpeedKmh = 35;            // tuk-tuk / car between sights
        private const double RoadFactor = 1.35;

        private readonly IPlaceRepository _placeRepository;
        private readonly IPlaceService _placeService;
        private readonly IRouteService _routeService;

        public TripPlanService(IPlaceRepository placeRepository, IPlaceService placeService, IRouteService routeService)
        {
            _placeRepository = placeRepository;
            _placeService = placeService;
            _routeService = routeService;
        }

        public async Task<ServiceResult<TripPlanResponse>> PlanTripAsync(TripPlanRequest request)
        {
            var from = await _placeRepository.GetPlaceByIdAsync(request.FromPlaceId!.Value);
            var to = await _placeRepository.GetPlaceByIdAsync(request.ToPlaceId!.Value);

            if (from == null || to == null) return ServiceResult<TripPlanResponse>.Fail("Place not found");
            if (from.Id == to.Id) return ServiceResult<TripPlanResponse>.Fail("Start and destination must be different");

            var car = _routeService.EstimateCar(from, to);

            // Getting there: earliest-arriving bus/train, or drive if there is none
            var outbound = (await _routeService.FindRoutesAsync(from.Id, to.Id, EarliestDeparture)).FirstOrDefault();
            var arrival = outbound?.ArrivalMinutes ?? CarDeparture + car.DurationMinutes;

            // Getting home: leave after lunch on the last day (later on a one-day trip)
            var leaveAfter = request.Days == 1 ? Math.Max(arrival + 3 * 60, ReturnAfter) : ReturnAfter;
            var returnTrip = (await _routeService.FindRoutesAsync(to.Id, from.Id, leaveAfter)).FirstOrDefault();
            var returnDeparture = returnTrip?.DepartureMinutes ?? leaveAfter + 60;

            var remaining = await _placeService.GetNearbyAttractionsAsync(to.Latitude, to.Longitude, AttractionRadiusKm, null);

            var plan = new TripPlanResponse
            {
                From = ToPlaceResponse(from),
                To = ToPlaceResponse(to),
                NumberOfDays = request.Days,
                StartDate = request.StartDate!.Value.ToString("yyyy-MM-dd"),
                CoverImageUrl = remaining.FirstOrDefault(a => a.ImageUrl != null)?.ImageUrl,
                Outbound = outbound,
                Return = returnTrip,
                Car = car
            };

            // Each day starts from the destination town or last night's stay
            (decimal Lat, decimal Lng) position = (to.Latitude, to.Longitude);

            for (var dayNumber = 1; dayNumber <= request.Days; dayNumber++)
            {
                var isFirst = dayNumber == 1;
                var isLast = dayNumber == request.Days;
                var day = new TripDayResponse
                {
                    DayNumber = dayNumber,
                    Date = request.StartDate.Value.AddDays(dayNumber - 1).ToString("yyyy-MM-dd")
                };

                if (isFirst) day.Items.Add(TravelItem("Travel", $"Travel to {to.Name}", outbound, CarDeparture, arrival, car));

                var time = isFirst ? arrival + SettleInMinutes : DayStart;
                var dayEnd = isLast ? returnDeparture - SettleInMinutes : DayEnd;

                // Greedy: keep adding the best-scoring attraction that still fits today
                while (day.Items.Count(i => i.Type == "Attraction") < MaxAttractionsPerDay)
                {
                    var next = remaining
                        .Select(a => Schedule(a, position, time, dayEnd, isLast ? to : null))
                        .Where(s => s != null)
                        .OrderByDescending(s => s!.Score)
                        .FirstOrDefault();

                    if (next == null) break;

                    day.Items.Add(next.Item);
                    remaining.Remove(next.Item.Attraction!);
                    position = (next.Item.Attraction!.Latitude, next.Item.Attraction.Longitude);
                    time = next.End;
                }

                if (isLast)
                {
                    day.Items.Add(TravelItem("Return", $"Return to {from.Name}", returnTrip, returnDeparture, returnDeparture + car.DurationMinutes, car));
                }
                else
                {
                    // Tonight's stay near where the day ends
                    var stays = await _placeService.GetNearbyStaysAsync(position.Lat, position.Lng, StayRadiusKm, null, null);
                    if (stays.Count == 0) stays = await _placeService.GetNearbyStaysAsync(to.Latitude, to.Longitude, AttractionRadiusKm, null, null);

                    // Suggest the best-rated stay at or under the middle price, so the default isn't always a luxury resort
                    var medianPrice = stays.Select(s => s.PricePerNight).OrderBy(p => p).ElementAtOrDefault(stays.Count / 2);
                    day.Stay = stays
                        .Where(s => s.PricePerNight <= medianPrice)
                        .OrderByDescending(s => s.Rating ?? 0)
                        .ThenBy(s => s.DistanceKm)
                        .FirstOrDefault();
                    day.OtherStays = stays
                        .Where(s => s != day.Stay)
                        .OrderByDescending(s => s.Rating ?? 0)
                        .ThenBy(s => s.DistanceKm)
                        .Take(3)
                        .ToList();
                    if (day.Stay != null) position = (day.Stay.Latitude, day.Stay.Longitude);
                }

                day.Title = DayTitle(day, isFirst, isLast, to.Name);
                plan.Days.Add(day);
            }

            plan.EstimatedCost = new TripCostResponse
            {
                Transport = (outbound?.Fare ?? 0) + (returnTrip?.Fare ?? 0),
                Tickets = plan.Days.SelectMany(d => d.Items).Sum(i => i.Attraction?.TicketPrice ?? 0),
                Stays = plan.Days.Sum(d => d.Stay?.PricePerNight ?? 0)
            };

            return ServiceResult<TripPlanResponse>.Ok(plan);
        }

        private record ScheduledVisit(TripItemResponse Item, int End, double Score);

        // When we could visit this attraction next, or null if it doesn't fit today
        private static ScheduledVisit? Schedule(AttractionResponse a, (decimal Lat, decimal Lng) position, int time, int dayEnd, Place? mustReturnTo)
        {
            var travel = TravelMinutes(position.Lat, position.Lng, a.Latitude, a.Longitude);
            var start = time + travel;
            var open = ParseTime(a.OpenTime);
            var close = ParseTime(a.CloseTime);

            if (open.HasValue && start < open) start = open.Value;      // wait for it to open

            var end = start + (int)Math.Round(a.VisitHours * 60);
            if (close.HasValue && end > close) return null;

            // On the last day we also need time to get back to the bus/train
            var finish = mustReturnTo == null ? end : end + TravelMinutes(a.Latitude, a.Longitude, mustReturnTo.Latitude, mustReturnTo.Longitude);
            if (finish > dayEnd) return null;

            // Rating counts most (4.8 is far better than 4.2), and far-away places lose points.
            // Visit length isn't penalised, so headline sights like Sigiriya Rock aren't skipped.
            var score = Math.Pow((double)(a.Rating ?? 4m), 4) / (1 + 2 * travel / 60.0);

            return new ScheduledVisit(new TripItemResponse
            {
                Type = "Attraction",
                StartTime = RouteService.FormatTime(start),
                EndTime = RouteService.FormatTime(end),
                Title = a.Name,
                TravelMinutes = travel,
                Note = open.HasValue && time + travel < open ? $"Opens at {a.OpenTime}" : null,
                Attraction = a
            }, end, score);
        }

        private static TripItemResponse TravelItem(string type, string title, RouteOptionResponse? route, int carStart, int carEnd, CarEstimateResponse car)
        {
            if (route != null)
            {
                var first = route.Legs.First();
                var note = route.Legs.Count == 1
                    ? $"{first.Mode} {first.RouteNumber} · {first.RouteName}"
                    : $"{first.Mode} {first.RouteNumber}, change at {first.ToPlace}";

                return new TripItemResponse
                {
                    Type = type, Title = title, Route = route, Note = note,
                    StartTime = route.DepartureTime, EndTime = route.ArrivalTime
                };
            }

            return new TripItemResponse
            {
                Type = type, Title = title,
                Note = $"No direct bus or train found - about {car.DistanceKm} km by car or taxi",
                StartTime = RouteService.FormatTime(carStart),
                EndTime = RouteService.FormatTime(carEnd)
            };
        }

        private static string DayTitle(TripDayResponse day, bool isFirst, bool isLast, string destination)
        {
            var places = day.Items
                .Where(i => i.Attraction != null)
                .Select(i => i.Attraction!.PlaceName)
                .Distinct()
                .ToList();

            if (places.Count > 0) return string.Join(" & ", places.Take(2));
            if (isFirst) return $"Arrive in {destination}";
            if (isLast) return "Head home";
            return $"Relax in {destination}";
        }

        // Time by tuk-tuk/car between two points, rounded up to 5 minutes (at least 10)
        private static int TravelMinutes(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var km = GeoHelper.DistanceKm(lat1, lng1, lat2, lng2) * RoadFactor;
            return Math.Max(10, (int)Math.Ceiling(km / LocalSpeedKmh * 60 / 5) * 5);
        }

        private static int? ParseTime(string? time) =>
            TimeSpan.TryParse(time, out var t) ? (int)t.TotalMinutes : null;

        private static PlaceResponse ToPlaceResponse(Place p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            District = p.District,
            Latitude = p.Latitude,
            Longitude = p.Longitude
        };
    }
}
