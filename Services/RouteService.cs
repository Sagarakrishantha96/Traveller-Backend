using Traveller_backend.APIModels;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Services
{
    public class RouteService : IRouteService
    {
        private const int MinutesPerDay = 24 * 60;
        private const int ChangeBufferMinutes = 15;     // time to change bus/train
        private const int MaxOptions = 5;
        private const double RoadFactor = 1.35;         // roads are longer than a straight line
        private const double CarSpeedKmh = 45;

        private readonly IRouteRepository _routeRepository;

        public RouteService(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        // A route run in one direction: its stops in travel order with minutes from the first stop
        private record StopTime(int PlaceId, string PlaceName, int Offset);
        private record Direction(TransportRoute Route, List<StopTime> Stops, List<int> Departures);
        private record Leg(Direction Direction, int FromIndex, int ToIndex, int Depart, int Arrive);

        public async Task<List<RouteOptionResponse>> FindRoutesAsync(int fromPlaceId, int toPlaceId, int earliestDeparture)
        {
            var routes = await _routeRepository.GetRoutesWithStopsAsync();
            var directions = routes.Where(r => r.Stops.Count >= 2).SelectMany(GetDirections).ToList();

            var options = new List<List<Leg>>();

            // Direct: one bus or train from A to B
            options.AddRange(FindLegs(directions, fromPlaceId, toPlaceId, earliestDeparture).Select(l => new List<Leg> { l }));

            // One change: A -> X, then the first connection X -> B on a different route
            foreach (var first in FindLegs(directions, fromPlaceId, null, earliestDeparture))
            {
                var changeAt = first.Direction.Stops[first.ToIndex].PlaceId;
                if (changeAt == toPlaceId) continue;

                var second = FindLegs(directions, changeAt, toPlaceId, first.Arrive + ChangeBufferMinutes)
                    .Where(l => l.Direction.Route.Id != first.Direction.Route.Id)
                    .OrderBy(l => l.Arrive)
                    .FirstOrDefault();

                if (second != null) options.Add(new List<Leg> { first, second });
            }

            return options
                .Where(o => o.Last().Arrive < MinutesPerDay)        // same-day trips only
                .OrderBy(o => o.Last().Arrive)
                .ThenBy(o => o.Count)
                .ThenByDescending(o => o.First().Depart)
                .DistinctBy(o => o.Last().Arrive)                   // keep the best option per arrival time
                .Take(MaxOptions)
                .Select(ToResponse)
                .ToList();
        }

        public CarEstimateResponse EstimateCar(Place from, Place to)
        {
            var distanceKm = GeoHelper.DistanceKm(from.Latitude, from.Longitude, to.Latitude, to.Longitude) * RoadFactor;
            var minutes = (int)Math.Ceiling(distanceKm / CarSpeedKmh * 60 / 5) * 5;

            return new CarEstimateResponse { DistanceKm = Math.Round(distanceKm), DurationMinutes = minutes };
        }

        private static IEnumerable<Direction> GetDirections(TransportRoute route)
        {
            var departures = route.DepartureTimes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => TimeSpan.TryParse(t, out var time) ? (int)time.TotalMinutes : -1)
                .Where(m => m >= 0)
                .ToList();

            var stops = route.Stops
                .OrderBy(s => s.StopOrder)
                .Select(s => new StopTime(s.PlaceId, s.Place.Name, s.MinutesFromStart))
                .ToList();

            yield return new Direction(route, stops, departures);

            // The timetable only lists one direction; assume the way back runs at the same times from the last stop
            var total = stops.Last().Offset;
            var reversed = stops.AsEnumerable().Reverse().Select(s => s with { Offset = total - s.Offset }).ToList();
            yield return new Direction(route, reversed, departures);
        }

        // Every ride leaving `fromPlaceId` at or after `earliest`; toPlaceId null = to any later stop
        private static IEnumerable<Leg> FindLegs(List<Direction> directions, int fromPlaceId, int? toPlaceId, int earliest)
        {
            foreach (var direction in directions)
            {
                var fromIndex = direction.Stops.FindIndex(s => s.PlaceId == fromPlaceId);
                if (fromIndex < 0) continue;

                for (var toIndex = fromIndex + 1; toIndex < direction.Stops.Count; toIndex++)
                {
                    if (toPlaceId.HasValue && direction.Stops[toIndex].PlaceId != toPlaceId) continue;

                    foreach (var departure in direction.Departures)
                    {
                        var depart = departure + direction.Stops[fromIndex].Offset;
                        if (depart < earliest) continue;

                        yield return new Leg(direction, fromIndex, toIndex, depart, departure + direction.Stops[toIndex].Offset);
                    }
                }
            }
        }

        private static RouteOptionResponse ToResponse(List<Leg> legs)
        {
            var legResponses = legs.Select(l =>
            {
                var route = l.Direction.Route;
                return new RouteLegResponse
                {
                    Mode = route.Mode,
                    RouteNumber = route.RouteNumber,
                    RouteName = route.Name,
                    Operator = route.Operator,
                    IsAc = route.IsAc,
                    FromPlace = l.Direction.Stops[l.FromIndex].PlaceName,
                    ToPlace = l.Direction.Stops[l.ToIndex].PlaceName,
                    DepartureTime = FormatTime(l.Depart),
                    ArrivalTime = FormatTime(l.Arrive),
                    DurationMinutes = l.Arrive - l.Depart,
                    Fare = PartFare(route, l.Arrive - l.Depart)
                };
            }).ToList();

            var fares = legResponses.Select(l => l.Fare).ToList();

            return new RouteOptionResponse
            {
                Legs = legResponses,
                DepartureMinutes = legs.First().Depart,
                ArrivalMinutes = legs.Last().Arrive,
                DepartureTime = FormatTime(legs.First().Depart),
                ArrivalTime = FormatTime(legs.Last().Arrive),
                DurationMinutes = legs.Last().Arrive - legs.First().Depart,
                Fare = fares.All(f => f.HasValue) ? fares.Sum() : null
            };
        }

        // Fare for part of a route, in proportion to the time on board, rounded to LKR 10
        private static decimal? PartFare(TransportRoute route, int minutes)
        {
            if (route.Fare == null || route.DurationMinutes <= 0) return null;
            var fare = route.Fare.Value * minutes / route.DurationMinutes;
            return Math.Max(10, Math.Round(fare / 10) * 10);
        }

        public static string FormatTime(int minutes)
        {
            minutes = ((minutes % MinutesPerDay) + MinutesPerDay) % MinutesPerDay;
            return $"{minutes / 60:00}:{minutes % 60:00}";
        }
    }
}
