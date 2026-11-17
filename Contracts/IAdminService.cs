using Traveller_backend.APIModels;
using Traveller_backend.Models;

namespace Traveller_backend.Contracts
{
    // Business logic for the admin page: add, edit and delete travel data
    public interface IAdminService
    {
        Task<AdminSummaryResponse> GetSummaryAsync();
        AdminOptionsResponse GetOptions();

        Task<List<PlaceResponse>> GetPlacesAsync();
        Task<ServiceResult<PlaceResponse>> SavePlaceAsync(int? id, PlaceRequest request);
        Task<ServiceResult<bool>> DeletePlaceAsync(int id);

        Task<List<AttractionResponse>> GetAttractionsAsync(int? placeId);
        Task<ServiceResult<AttractionResponse>> SaveAttractionAsync(int? id, AttractionRequest request);
        Task<ServiceResult<bool>> DeleteAttractionAsync(int id);

        Task<List<StayResponse>> GetStaysAsync(int? placeId);
        Task<ServiceResult<StayResponse>> SaveStayAsync(int? id, StayRequest request);
        Task<ServiceResult<bool>> DeleteStayAsync(int id);

        Task<List<AdminRouteResponse>> GetRoutesAsync();
        Task<ServiceResult<AdminRouteResponse>> SaveRouteAsync(int? id, RouteRequest request);
        Task<ServiceResult<bool>> DeleteRouteAsync(int id);
    }
}
