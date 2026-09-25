using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Application.Interfaces
{
    public interface ISSMServerManagementService
    {
        public Task<(string Id, string ErrorMessage)> AddUpdateServerDetails(SSMServerManagementRequest request, string userId);
        public Task<(IEnumerable<SsmSiteMapping> data, Dictionary<string, object> referenceData)> GetAllSsmServersAsync();
        public Task<bool> DeleteSsmServerManagement(DeleteSsmServerRequest request, string userId);
    }
}
