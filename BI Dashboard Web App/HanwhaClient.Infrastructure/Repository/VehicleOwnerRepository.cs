using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class VehicleOwnerRepository : RepositoryBase<VehicleOwner>, IVehicleOwnerRepository
    {
        public VehicleOwnerRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.VehicleOwner)
        {

        }

        public async Task<bool> IsVehicleOwnerExistAsync(string building, string buildingUnit, string vehicleOwnerId = null)
        {
            var filter = Builders<VehicleOwner>.Filter.Where(x => x.Building == building && x.BuildingUnit == buildingUnit && x.IsDeleted != true && x.RegistrationType == "permanent");

            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive issue
            };

            if (!string.IsNullOrEmpty(vehicleOwnerId))
            {
                filter = Builders<VehicleOwner>.Filter.And(
                    filter,
                    Builders<VehicleOwner>.Filter.Ne(x => x.Id, vehicleOwnerId));
            }
            var data = await dbEntity.Find(filter, options).AnyAsync();
            return data;
        }
        public async Task<(IEnumerable<VehicleOwner> ownerDetails, int totalCount)> GetAllOwnerAsync(AllVehicleOwnerRequest request, List<string> filteredDeviceIds)
        {
            var builder = Builders<VehicleOwner>.Filter;

            var baseFilter = builder.Ne(x => x.IsDeleted, true);

            FilterDefinition<VehicleOwner> combinedOrFilter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText;

                combinedOrFilter = builder.Or(
                    builder.Regex(x => x.RegistrationType, new BsonRegularExpression(searchText, "i")),
                    builder.Regex(x => x.OwnerName, new BsonRegularExpression(searchText, "i")),
                    builder.Regex(x => x.Building, new BsonRegularExpression(searchText, "i")),
                    builder.Regex(x => x.BuildingUnit, new BsonRegularExpression(searchText, "i")),
                    builder.Regex(x => x.Email, new BsonRegularExpression(searchText, "i")),
                    builder.Regex(x => x.ContactNumber, new BsonRegularExpression(searchText, "i"))
                );
            }
            if (filteredDeviceIds.Any())
            {
                var gateFilter = builder.AnyIn(x => x.AllowedGates, filteredDeviceIds);

                combinedOrFilter = combinedOrFilter == null
                    ? gateFilter
                    : builder.Or(combinedOrFilter, gateFilter);
            }

            var finalFilter = combinedOrFilter == null
                ? baseFilter
                : builder.And(baseFilter, combinedOrFilter);

            string sortField = request.SortBy ?? "createdOn";
            bool sortDescending = request.SortOrder == -1;

            var sortDefinition = sortDescending
                ? Builders<VehicleOwner>.Sort.Descending(sortField)
                : Builders<VehicleOwner>.Sort.Ascending(sortField);

            var data = await dbEntity.Find(finalFilter).Sort(sortDefinition).Skip((request.PageNumber - 1) * request.PageSize).Limit(request.PageSize).ToListAsync();
            int totalCount = (int)await dbEntity.CountDocumentsAsync(finalFilter);
            return (data, totalCount);
        }
        public async Task<List<string>> FindOwnerIdsByNameAsync(string ownerName)
        {
            var filter = Builders<VehicleOwner>.Filter
                .Regex("ownerName", new BsonRegularExpression(ownerName, "i"));

            return await dbEntity.Find(filter).Project(d => d.Id).ToListAsync();
        }

        public async Task<IEnumerable<OwnerIdAndRegistrationTypeDto>> GetOwnerIdAndRegistrationType(string building, string buildingUnit)
        {
            var filter = Builders<VehicleOwner>.Filter.Where(x => x.Building == building && x.BuildingUnit == buildingUnit && x.IsDeleted == false);

            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive issue
            };

            var data = await dbEntity.Find(filter, options).ToListAsync();

            var newResult = data.Select(x => new OwnerIdAndRegistrationTypeDto
            {
                OwnerId = x.Id,
                RegistrationType = x.RegistrationType
            });
            return newResult;
        }

        public async Task<IEnumerable<VehicleOwner>> GetOwnerByOwnerId(string ownerId)
        {
            var filter = Builders<VehicleOwner>.Filter.Where(x => x.Id == ownerId && x.IsDeleted == false);
            var data = await dbEntity.Find(filter).ToListAsync();
            return data;
        }
        public async Task<List<string>> FindOwnerIdsByNameTypeEmailAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            var regex = new BsonRegularExpression(searchText, "i");

            var filter = Builders<VehicleOwner>.Filter.Or(
                Builders<VehicleOwner>.Filter.Regex(x => x.OwnerName, regex),
                Builders<VehicleOwner>.Filter.Regex(x => x.Email, regex),
                Builders<VehicleOwner>.Filter.Regex(x => x.RegistrationType, regex)
            );

            return await dbEntity
                .Find(filter)
                .Project(x => x.Id)
                .ToListAsync();
        }

        public async Task<VehicleOwner> GetSingleOwnerByOwnerId(string ownerId)
        {
            var filter = Builders<VehicleOwner>.Filter.Where(x => x.Id == ownerId && x.IsDeleted == false);
            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
            return data;
        }

        public async Task<List<string>> GetVehicleOwnerIdsWithOverstayEnabledAsync()
        {
            var filter = Builders<VehicleOwner>.Filter.And(
                Builders<VehicleOwner>.Filter.Eq(x => x.EnabledAlarmForOverstay, true),
                Builders<VehicleOwner>.Filter.Ne(x => x.IsDeleted, true)
            );

            return await dbEntity
                .Find(filter)
                .Project(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<string>> GetVehicleOwnerIdsWith24HStayEnabledAsync()
        {
            var filter = Builders<VehicleOwner>.Filter.And(
                Builders<VehicleOwner>.Filter.Eq(x => x.EnabledAlarmFor24HStay, true),
                Builders<VehicleOwner>.Filter.Ne(x => x.IsDeleted, true)
            );

            return await dbEntity
                .Find(filter)
                .Project(x => x.Id)
                .ToListAsync();
        }
    }
}
