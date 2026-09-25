using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IArchiveRepository<TArchive>
    {
        Task InsertManyAsync(List<TArchive> archiveList);
    }

}
