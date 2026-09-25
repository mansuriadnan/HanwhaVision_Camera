using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISiteRepository : IRepositoryBase<SiteMaster>
    {
        Task<List<SiteMaster>> GetByFilterAsync(FilterDefinition<SiteMaster> filter,ProjectionDefinition<SiteMaster> projection);
    }
}
