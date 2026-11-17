using Microsoft.EntityFrameworkCore;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;

namespace Traveller_backend.Repositories
{
    public class EFAdminRepository : IAdminRepository
    {
        private readonly TravelDbContext _context;

        public EFAdminRepository(TravelDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountAsync<T>() where T : class
        {
            return await _context.Set<T>().CountAsync();
        }

        // Saved trips have no entity on this branch yet, so count them directly
        public async Task<int> CountTripsAsync()
        {
            return await _context.Database.SqlQuery<int>($"SELECT COUNT(*) AS Value FROM trips").SingleAsync();
        }

        public async Task<T?> FindAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public void Add<T>(T entity) where T : class => _context.Set<T>().Add(entity);

        public void Remove<T>(T entity) where T : class => _context.Set<T>().Remove(entity);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<bool> PlaceNameExistsAsync(string name, int? exceptId)
        {
            return await _context.Places.AnyAsync(p => p.Name == name && p.Id != exceptId);
        }

        // A place can't be deleted while attractions, stays or routes still use it
        public async Task<bool> PlaceIsUsedAsync(int placeId)
        {
            return await _context.Attractions.AnyAsync(a => a.PlaceId == placeId)
                || await _context.Stays.AnyAsync(s => s.PlaceId == placeId)
                || await _context.RouteStops.AnyAsync(s => s.PlaceId == placeId);
        }

        public async Task<int> CountExistingPlacesAsync(IEnumerable<int> placeIds)
        {
            var ids = placeIds.Distinct().ToList();
            return await _context.Places.CountAsync(p => ids.Contains(p.Id));
        }

        public async Task<List<Attraction>> GetAttractionsAsync(int? placeId)
        {
            return await _context.Attractions
                .AsNoTracking()
                .Include(a => a.Place)
                .Where(a => placeId == null || a.PlaceId == placeId)
                .OrderBy(a => a.Place.Name).ThenBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Attraction?> GetAttractionAsync(int id)
        {
            return await _context.Attractions.Include(a => a.Place).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Stay>> GetStaysAsync(int? placeId)
        {
            return await _context.Stays
                .AsNoTracking()
                .Include(s => s.Place)
                .Where(s => placeId == null || s.PlaceId == placeId)
                .OrderBy(s => s.Place.Name).ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Stay?> GetStayAsync(int id)
        {
            return await _context.Stays.Include(s => s.Place).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<TransportRoute>> GetRoutesAsync()
        {
            return await _context.TransportRoutes
                .AsNoTracking()
                .Include(r => r.Stops).ThenInclude(s => s.Place)
                .OrderBy(r => r.Mode).ThenBy(r => r.RouteNumber).ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<TransportRoute?> GetRouteAsync(int id)
        {
            return await _context.TransportRoutes
                .Include(r => r.Stops).ThenInclude(s => s.Place)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddRouteAsync(TransportRoute route, List<RouteStop> stops)
        {
            route.Stops = stops;
            _context.TransportRoutes.Add(route);
            await _context.SaveChangesAsync();
        }

        // Old stops are deleted before the new ones are added, so the unique (route, order)
        // and (route, place) keys never clash; both steps run in one transaction
        public async Task UpdateRouteAsync(TransportRoute route, List<RouteStop> stops)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var old in route.Stops.ToList()) _context.RouteStops.Remove(old);
            await _context.SaveChangesAsync();

            foreach (var stop in stops)
            {
                stop.RouteId = route.Id;
                _context.RouteStops.Add(stop);
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
    }
}
