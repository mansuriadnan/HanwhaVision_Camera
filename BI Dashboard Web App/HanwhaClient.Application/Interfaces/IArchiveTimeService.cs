
using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Application.Interfaces
{
    public interface IArchiveTimeService
    {
        public ClientSettings CurrentClientSettings { get; set; }
        public bool CheckCollectionArchiceTime(DateTime filterTime);
        public Task GetClinetSettings();

    }
}
