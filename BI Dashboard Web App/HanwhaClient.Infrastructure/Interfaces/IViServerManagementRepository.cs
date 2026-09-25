using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IViServerManagementRepository : IRepositoryBase<ViMultiServerManagement>
    {
        Task<bool> CheckMultiServerExists(ViMultiServerManagementDTO serverManagementRequest);
        Task<IEnumerable<ViMultiServerManagement>> GetAllActiveServer();
        string GetApplicationConnectionString();
    }
}
