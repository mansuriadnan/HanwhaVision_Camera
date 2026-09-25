using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Application.Interfaces
{
    public interface IViMultiServerManagementService
    {
        Task<(bool isSuccess, string errorMessage)> AddUpdateServerDetails(ViMultiServerManagementDTO serverManagementRequest, string userId);
        Task<IEnumerable<ViMultiServerManagement>> GetAllServerDetails();
        Task<bool> DeleteServerManagement(DeleteServerManagement request, string userId);
        Task<(bool isSuccess, string errorMessage)> EnableServerManagement(EnabledServerRequest request, string userId);
        Task<IEnumerable<GetViMultiServerDashDto>> GetAllActiveServerForDashboardAsync();
    }
}
