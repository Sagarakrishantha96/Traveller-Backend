using Microsoft.EntityFrameworkCore;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;

namespace Traveller_backend.Repositories
{
    public class EFTripRepository : ITripRepository
    {
        private readonly TravelDbContext _context;

        public EFTripRepository(TravelDbContext context)
        {
            _context = context;
        }

        public async Task<Trip> AddTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<List<Trip>> GetTripsByUserAsync(int userId)
        {
            return await _context.Trips
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.FromPlace)
                .Include(t => t.ToPlace)
                .Include(t => t.Days).ThenInclude(d => d.Items).ThenInclude(i => i.Attraction)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.StartDate)
                .ToListAsync();
        }

        // Only returns the trip if it belongs to this user
        public async Task<Trip?> GetTripAsync(int userId, int tripId)
        {
            return await _context.Trips
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.FromPlace)
                .Include(t => t.ToPlace)
                .Include(t => t.Days).ThenInclude(d => d.Stay!).ThenInclude(s => s.Place)
                .Include(t => t.Days).ThenInclude(d => d.Items).ThenInclude(i => i.Attraction!).ThenInclude(a => a.Place)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
        }

        // Days and activities are removed by the database (ON DELETE CASCADE)
        public async Task<bool> DeleteTripAsync(int userId, int tripId)
        {
            var deleted = await _context.Trips
                .Where(t => t.Id == tripId && t.UserId == userId)
                .ExecuteDeleteAsync();
            return deleted > 0;
        }
    }
}
