using Traveller_backend.EntityModels;

namespace Traveller_backend.Contracts
{
    // Data access for bus and train routes
    public interface IRouteRepository
    {
        Task<List<TransportRoute>> GetRoutesWithStopsAsync();
    }
}
