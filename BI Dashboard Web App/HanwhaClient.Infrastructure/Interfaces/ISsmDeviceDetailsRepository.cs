using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.SSM;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISSMDeviceDetailsRepository : IRepositoryBase<SsmDeviceDetails>
    {
        Task<(bool success, string id)> AddUpdateSsmDeviceDetails(List<SsmCameraDto> deviceDto, string serverId);
        Task<(IEnumerable<SsmDeviceDetails> deviceDetails, int count)> GetSsmDeviceDetailsAsync(SSMDeviceDetailsRequest request, DateTime startOfDayUTC, DateTime endOfDayUTC);
        Task<List<SsmDeviceDetails>> GetDevicesByServerIdsAsync(List<string> serverIds);
    }
}
