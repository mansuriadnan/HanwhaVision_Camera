using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;
        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }
        public async Task<IEnumerable<CountryResponse>> GetAllCountryAsync()
        {            
            ProjectionDefinition<Country> projection = Builders<Country>.Projection
            .Include("Name")
            .Include("HasStates")
            .Include("Code")
            .Include("_id");
            var countryData = await _countryRepository.GetAllAsync(projection);
            var result = countryData.Select(item => new CountryResponse
            {
                Id = item.Id,
                CountryName = item.Name,
                HasState = item.HasStates,
                CountryCode = item.Code
            });
            return result;
        }

        public async Task<Country> GetCountryById(string countryId)
        {
            var countryData = await _countryRepository.GetAsync(countryId);
            return countryData;
        }
    }
}
