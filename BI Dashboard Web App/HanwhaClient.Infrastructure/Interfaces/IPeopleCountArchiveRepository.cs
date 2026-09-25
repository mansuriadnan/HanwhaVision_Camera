using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IPeopleCountArchiveRepository : IRepositoryBase<PeopleCountArchive>
    {
    }
}
