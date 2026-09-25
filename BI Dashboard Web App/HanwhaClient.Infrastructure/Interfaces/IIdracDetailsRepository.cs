using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IIdracDetailsRepository : IRepositoryBase<IdracDetails>
    {
        Task UpsertIdracDetailsAsync(IdracDetails idracDetails);
        Task<IdracDetails> GetIdracDetails(string serverId);
        Task<List<IdracStatusReponse>> GetIdracServerHealth(IEnumerable<string> serverId);
    }
}
