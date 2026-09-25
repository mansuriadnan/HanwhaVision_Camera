using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;

namespace HanwhaClient.Application.Interfaces
{
    public interface IFloorService
    {
        Task<(string data, string errorMessage)> AddUpdateFloorAsync(AddFloorRequest floorRequest, string UserId);
        Task<IEnumerable<GetFloorDto>> GetAllFloorsAsync();
        Task<bool> DeleteFloorAsync(string id, string userId);
        Task<bool> UploadFloorPlanImageAsync(string floorId, IFormFile file, string userId);
        Task<string> GetFloorPlanImageAsync(string floorId);
        Task<IEnumerable<FloorZoneByPermissionDto>> GetFloorZoneByPermissionAsync(IEnumerable<string> userRoles, IEnumerable<string> floorIds);
        Task<IEnumerable<FloorZoneByPermissionDto>> GetFloorZonesAsync();
        Task<IEnumerable<GetFloorDto>> GetFloorsByPermissionAsync(IEnumerable<string> userRoles, bool IncludeMultiServer);
        Task<GetFloorDto> GetFloorByFloorIdAsync(string floorId);
        Task<FloorZonesNameByIdResponse> GetFloorZonesByIdsAsync(FloorZonesNameByIdRequest request);
        Task<IEnumerable<FloorDataAccessPermission>> GetLinkedFloorsZoneByPermissionAsync();
        Task<IEnumerable<GetFloorDto>> GetFloorByServerIdsAsync(IEnumerable<string> userRoles, IEnumerable<string> serverIds);
    }
}
