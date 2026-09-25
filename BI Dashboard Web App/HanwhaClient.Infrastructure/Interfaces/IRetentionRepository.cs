using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IRetentionRepository<T> where T : BaseModel
    {
        Task<IEnumerable<T>> GetRetentionPeriodData(int retentionPeriod, int batchSize);
    }
}
