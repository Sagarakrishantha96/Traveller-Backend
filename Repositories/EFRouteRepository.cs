using Microsoft.EntityFrameworkCore;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;

namespace Traveller_backend.Repositories
{
    public class EFRouteRepository : IRouteRepository
    {
        private readonly TravelDbContext _context;

        public EFRouteRepository(TravelDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransportRoute>> GetRoutesWithStopsAsync()
        {
            return await _context.TransportRoutes
                .AsNoTracking()
                .Include(r => r.Stops)
                .ThenInclude(s => s.Place)
                .ToListAsync();
        }
    }
}
