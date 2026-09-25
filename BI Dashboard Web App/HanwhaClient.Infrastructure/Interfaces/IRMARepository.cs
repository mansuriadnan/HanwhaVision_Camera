using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IRMARepository : IRepositoryBase<Rma>
    {
        Task<(IEnumerable<Rma> Data, int TotalCount)> GetAllRMA(RMASerachModel model);
        Task<IEnumerable<RMAWidgetResponse>> GetRmaMaintenanceWidgetData(RMAWidgetRequest model);
        Task<IEnumerable<string>> GetRMAByDeviceIds(IEnumerable<string> deviceIds);
    }
}
