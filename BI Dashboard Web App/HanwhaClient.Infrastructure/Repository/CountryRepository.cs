using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class CountryRepository : RepositoryBase<Country>, ICountryRepository
    {
        public CountryRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.Country)
        {

        }
        public async Task<List<string>> FindCountryIdsByNameAsync(string countryName)
        {
            var filter = Builders<Country>.Filter
                .Regex("name", new BsonRegularExpression(countryName, "i"));

            return await dbEntity.Find(filter).Project(d => d.Id).ToListAsync();
        }

        public async Task<string> FindSingleCountryIdsByNameAsync(string countryName)
        {
            var filter = Builders<Country>.Filter
               .Regex("name", new BsonRegularExpression(countryName, "i"));

            return await dbEntity.Find(filter).Project(d => d.Id).FirstOrDefaultAsync();
        }
    }
}
