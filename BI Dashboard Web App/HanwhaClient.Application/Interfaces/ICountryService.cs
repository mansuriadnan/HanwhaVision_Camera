using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryResponse>> GetAllCountryAsync();
        Task<Country> GetCountryById(string countryId);
    }
}
