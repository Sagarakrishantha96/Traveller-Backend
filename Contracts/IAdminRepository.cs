using Traveller_backend.EntityModels;

namespace Traveller_backend.Contracts
{
    // Data access for the admin page
    public interface IAdminRepository
    {
        Task<int> CountAsync<T>() where T : class;
        Task<int> CountTripsAsync();

        Task<T?> FindAsync<T>(int id) where T : class;
        void Add<T>(T entity) where T : class;
        void Remove<T>(T entity) where T : class;
        Task SaveChangesAsync();

        Task<bool> PlaceNameExistsAsync(string name, int? exceptId);
        Task<bool> PlaceIsUsedAsync(int placeId);
        Task<int> CountExistingPlacesAsync(IEnumerable<int> placeIds);

        Task<List<Attraction>> GetAttractionsAsync(int? placeId);
        Task<Attraction?> GetAttractionAsync(int id);
        Task<List<Stay>> GetStaysAsync(int? placeId);
        Task<Stay?> GetStayAsync(int id);
        Task<List<TransportRoute>> GetRoutesAsync();
        Task<TransportRoute?> GetRouteAsync(int id);

        Task AddRouteAsync(TransportRoute route, List<RouteStop> stops);
        Task UpdateRouteAsync(TransportRoute route, List<RouteStop> stops);
    }
}
