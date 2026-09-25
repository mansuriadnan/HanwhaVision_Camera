using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Utilities
{
    public static class RetentionHelper
    {
        public static async Task<int> ProcessRetentionData<T, TArchive>(
         IRepositoryBase<T> sourceRepo,
         IRepositoryBase<TArchive> archiveRepo,
         IRetentionRepository<T> retentionRepo,
         Func<T, TArchive> mapToArchive,
         int retentionPeriod,
         int batchSize = 5000)
         where T : BaseModel
         where TArchive : BaseModel
        {
            int totalArchived = 0;

            while (true)
            {
                var batchData = await retentionRepo.GetRetentionPeriodData(retentionPeriod, batchSize);

                if (!batchData.Any())
                    break;

                var archiveList = batchData.Select(mapToArchive).ToList();

                await archiveRepo.InsertManyAsync(archiveList);

                var ids = batchData.Select(x => x.Id).ToList();
                await sourceRepo.DeleteManyAsync(ids);

                totalArchived += batchData.Count();
            }

            return totalArchived;
        }
    }
}
