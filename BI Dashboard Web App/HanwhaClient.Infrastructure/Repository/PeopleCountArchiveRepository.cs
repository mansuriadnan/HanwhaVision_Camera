using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class PeopleCountArchiveRepository : RepositoryBase<PeopleCountArchive>, IPeopleCountArchiveRepository, IArchiveRepository<PeopleCountArchive>
    {
        public PeopleCountArchiveRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.PeopleCountArchive)
        {
        }

    }
}
