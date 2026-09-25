using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ICountryRepository : IRepositoryBase<Country>
    {
        Task<List<string>> FindCountryIdsByNameAsync(string countryName);
        Task<string> FindSingleCountryIdsByNameAsync(string countryName);
    }
}
