using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class ANPRVehicleRepository : RepositoryBase<ANPRVehicle>, IANPRVehicleRepository
    {
        public ANPRVehicleRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.ANPRVehicle)
        {
        }    

        public async Task<bool> IsANPRVehicleExistAsync(ANPRVehicleRequest anprVehicleRequest, string ownerRegistrationType)
        {
            var now = DateTime.UtcNow;
            
            var filter = Builders<ANPRVehicle>.Filter.And(
                             Builders<ANPRVehicle>.Filter.Eq(x => x.State, anprVehicleRequest.State),
                             Builders<ANPRVehicle>.Filter.Eq(x => x.Country, anprVehicleRequest.Country),
                             Builders<ANPRVehicle>.Filter.Eq(x => x.PlateCode, anprVehicleRequest.PlateCode),
                             Builders<ANPRVehicle>.Filter.Eq(x => x.IsDeleted, false)
                             );

            //var visitorDateFilter = Builders<ANPRVehicle>.Filter.And(
            //    Builders<ANPRVehicle>.Filter.Lte(x => x.VisitorValidFrom, now),
            //    Builders<ANPRVehicle>.Filter.Gte(x => x.VisitorValidTo, now)
            //);
            //var filter = ownerRegistrationType == "permanent"
                //        ? baseFilter
                //        : Builders<ANPRVehicle>.Filter.And(baseFilter, visitorDateFilter);
            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive issue
            };

            if (!string.IsNullOrEmpty(anprVehicleRequest.Id))
            {
                filter = Builders<ANPRVehicle>.Filter.And(
                    filter,
                    Builders<ANPRVehicle>.Filter.Ne(x => x.Id, anprVehicleRequest.Id));
            }
            var data = await dbEntity.Find(filter, options).AnyAsync();
            return data;
        }
        public async Task<IEnumerable<ANPRVehicle>> GetAllANPRVehicleByOwner(AllANPRVehicleWithSearchRequest request)
        {
            FilterDefinition<ANPRVehicle> combinedOrFilter = null;
            var builder = Builders<ANPRVehicle>.Filter;

            var baseFilter = builder.Ne(x => x.IsDeleted, true);
            baseFilter = builder.And(baseFilter, builder.Eq(x => x.VehicleOwnerId, request.VehicleOwnerId));

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var text = request.SearchText;
                var regex = new BsonRegularExpression(text, "i");

                combinedOrFilter = builder.Or(
                    builder.Regex(x => x.State, regex),
                    builder.Regex(x => x.Series, regex),
                    builder.Regex(x => x.PlateCode, regex),
                    builder.Regex(x => x.PlateCategory, regex),
                    builder.Regex(x => x.Make, regex),
                    builder.Regex(x => x.Model, regex),
                    builder.Regex(x => x.Color, regex)
                );                
                if (int.TryParse(text, out int num)) // vehicle number match
                {
                    combinedOrFilter = builder.Or(combinedOrFilter, builder.Eq(x => x.VehicleNumber, num));
                }
            }
            // 2. OR with OwnerIds
            if (request.VehicleOwnerIds != null && request.VehicleOwnerIds.Any())
            {
                var ownerIdFilter = builder.In(x => x.VehicleOwnerId, request.VehicleOwnerIds);

                combinedOrFilter = combinedOrFilter == null
                    ? ownerIdFilter
                    : builder.Or(combinedOrFilter, ownerIdFilter);
            }

            // 3. OR with CountryIds
            if (request.CountryIds != null && request.CountryIds.Any())
            {
                var countryFilter = builder.In(x => x.Country, request.CountryIds);

                combinedOrFilter = combinedOrFilter == null
                    ? countryFilter
                    : builder.Or(combinedOrFilter, countryFilter);
            }
            var finalFilter = combinedOrFilter == null
                       ? baseFilter
                       : builder.And(baseFilter, combinedOrFilter);
                       
            var data = await dbEntity.Find(finalFilter).ToListAsync();
            return data;
        }
        public async Task<IEnumerable<string>> GetANPRVehicleIdsByOwner(string ownerId)
        {
            var filters = new List<FilterDefinition<ANPRVehicle>>();

            filters.Add(Builders<ANPRVehicle>.Filter.Ne(x => x.IsDeleted, true));
            filters.Add(Builders<ANPRVehicle>.Filter.Eq(x => x.VehicleOwnerId, ownerId));

            var finalFilter = filters.Any()
                ? Builders<ANPRVehicle>.Filter.And(filters)
                : Builders<ANPRVehicle>.Filter.Empty;

            var data = await dbEntity.Find(finalFilter).ToListAsync();
            return data.Select(x => x.Id); 
        }

        public async Task<VehicleByPlateNumberLPRDto?> GetVehicleByPlateNumAsync(string plateCode, string country, string state)
        {
            var filter = Builders<ANPRVehicle>.Filter.And(
                Builders<ANPRVehicle>.Filter.Ne(x => x.IsDeleted, true),
                Builders<ANPRVehicle>.Filter.Eq(x => x.PlateCode, plateCode),
                Builders<ANPRVehicle>.Filter.Eq(x => x.Country, country),
                Builders<ANPRVehicle>.Filter.Eq(x => x.State, state)
            );

            var data = await dbEntity
                .Find(filter)
                .FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new VehicleByPlateNumberLPRDto
            {
                AnprVehicleId = data.Id,
                OwnerId = data.VehicleOwnerId,
                VisitorValidFrom = data.VisitorValidFrom,
                VisitorValidTo = data.VisitorValidTo
            };
        }
    }
}
