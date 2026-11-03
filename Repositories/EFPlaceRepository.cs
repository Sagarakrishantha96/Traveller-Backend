using Microsoft.EntityFrameworkCore;
using Traveller_backend.Contracts;
using Traveller_backend.EntityModels;
using Traveller_backend.Models;

namespace Traveller_backend.Repositories
{
    public class EFPlaceRepository : IPlaceRepository
    {
        private readonly TravelDbContext _context;

        public EFPlaceRepository(TravelDbContext context)
        {
            _context = context;
        }

        public async Task<List<Place>> SearchPlacesAsync(string? query, int limit)
        {
            var places = _context.Places.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                places = places.Where(p => p.Name.Contains(query) || (p.District != null && p.District.Contains(query)));
            }

            return await places
                .OrderBy(p => p.Name)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Place?> GetPlaceByIdAsync(int id)
        {
            return await _context.Places.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Attraction>> GetAttractionsWithImagesAsync()
        {
            return await _context.Attractions
                .AsNoTracking()
                .Where(a => a.ImageUrl != null)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetAttractionCountsAsync()
        {
            return await _context.Attractions
                .GroupBy(a => a.PlaceId)
                .Select(g => new { PlaceId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PlaceId, x => x.Count);
        }

        public async Task<List<Attraction>> GetAttractionsInAreaAsync(GeoBounds bounds, string? category)
        {
            var attractions = _context.Attractions
                .AsNoTracking()
                .Include(a => a.Place)
                .Where(a => a.Latitude >= bounds.MinLatitude && a.Latitude <= bounds.MaxLatitude
                         && a.Longitude >= bounds.MinLongitude && a.Longitude <= bounds.MaxLongitude);

            if (!string.IsNullOrWhiteSpace(category))
            {
                attractions = attractions.Where(a => a.Category == category);
            }

            return await attractions.ToListAsync();
        }

        public async Task<List<Stay>> GetStaysInAreaAsync(GeoBounds bounds, string? type, decimal? maxPrice)
        {
            var stays = _context.Stays
                .AsNoTracking()
                .Include(s => s.Place)
                .Where(s => s.Latitude >= bounds.MinLatitude && s.Latitude <= bounds.MaxLatitude
                         && s.Longitude >= bounds.MinLongitude && s.Longitude <= bounds.MaxLongitude);

            if (!string.IsNullOrWhiteSpace(type))
            {
                stays = stays.Where(s => s.Type == type);
            }

            if (maxPrice.HasValue)
            {
                stays = stays.Where(s => s.PricePerNight <= maxPrice.Value);
            }

            return await stays.ToListAsync();
        }
    }
}
