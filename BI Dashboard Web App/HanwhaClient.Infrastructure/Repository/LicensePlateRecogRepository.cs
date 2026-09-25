using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class LicensePlateRecogRepository : RepositoryBase<LicensePlateRecogDetails>, ILicensePlateRecogRepository
    {
        public LicensePlateRecogRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.LicensePlateRecogDetails)
        {
        }

        public async Task<int> CountVehiclesInByOwnerIdAsync(string vehicleOwnerId)
        {
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.VehicleOwnerId, vehicleOwnerId),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false)
            );

            var count = await dbEntity.CountDocumentsAsync(filter);
            return (int)count;
        }

        public async Task<LicensePlateRecogDetails> GetLatestInRecordByVehicleAsync(string plate, string country, string state, string vehicleOwnerId)
        {
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq("plate", plate),
                Builders<LicensePlateRecogDetails>.Filter.Eq("country", country),
                Builders<LicensePlateRecogDetails>.Filter.Eq("state", state),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.VehicleOwnerId, vehicleOwnerId),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false)
            );

            var sort = Builders<LicensePlateRecogDetails>.Sort.Descending(x => x.EntryTime);

            return await dbEntity.Find(filter).Sort(sort).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LicensePlateRecogDetails>> GetAllInRecordsByVehicleAsync(string plate, string country, string state)
        {
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq("plate", plate),
                Builders<LicensePlateRecogDetails>.Filter.Eq("country", country),
                Builders<LicensePlateRecogDetails>.Filter.Eq("state", state),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await dbEntity.Find(filter).ToListAsync();
        }        

        public async Task<(IEnumerable<LicensePlateRecogDetails> Data, int TotalCount)> GetAllLprDetailsAsync(IEnumerable<string>? deviceIds, IEnumerable<string>? ownerIds, AllLprRequest request)
        {
            // Validate / normalize paging
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var skip = (pageNumber - 1) * pageSize;

            // Whitelist & map sort fields (prevent injection)
            var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "EntryTime", "entryTime" },
                { "ExitTime", "exitTime" },
                { "CreatedOn", "createdOn" },
                { "CreatedAt", "createdAt" }
            };

            // If SortBy not allowed → default to createdOn
            var sortByField = "createdOn";
            if (!string.IsNullOrWhiteSpace(request.SortBy) && sortMap.TryGetValue(request.SortBy, out var mapped))
            {
                sortByField = mapped;
            }

            // Only accept 1 (asc) or -1 (desc); default to -1 (desc)
            var sortOrder = (request.SortOrder == 1 || request.SortOrder == -1) ? request.SortOrder.Value : -1;

            // Build filters
            var filters = new List<FilterDefinition<LicensePlateRecogDetails>>();

            // Base filter - not deleted
            filters.Add(Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false));

            // Device Filter: EntryGate OR ExitGate match with deviceIds
            if ((deviceIds != null && deviceIds.Any()) || (request.FloorIds != null && request.FloorIds.Any()))
            {
                var deviceFilter = Builders<LicensePlateRecogDetails>.Filter.Or(
                    Builders<LicensePlateRecogDetails>.Filter.In(x => x.EntryGate, deviceIds),
                    Builders<LicensePlateRecogDetails>.Filter.In(x => x.ExitGate, deviceIds)
                );
                filters.Add(deviceFilter);
            }

            // Owner & Search Filter (OR Condition): OwnerId with ownerIds OR State matches SearchText
            if ((ownerIds != null && ownerIds.Any()) || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                var ownerSearchFilters = new List<FilterDefinition<LicensePlateRecogDetails>>();

                if (ownerIds != null && ownerIds.Any())
                {
                    ownerSearchFilters.Add(Builders<LicensePlateRecogDetails>.Filter.In(x => x.VehicleOwnerId, ownerIds));
                }

                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    // State matches SearchText (from DynamicFields)
                    var stateFilter = Builders<LicensePlateRecogDetails>.Filter.Regex("state", new MongoDB.Bson.BsonRegularExpression(request.SearchText, "i"));
                    ownerSearchFilters.Add(stateFilter);
                    // PlateCode filter
                    var plateCodeFilter = Builders<LicensePlateRecogDetails>.Filter.Regex("plate", new MongoDB.Bson.BsonRegularExpression(request.SearchText, "i"));
                    ownerSearchFilters.Add(plateCodeFilter);
                }

                if (ownerSearchFilters.Any())
                {
                    filters.Add(Builders<LicensePlateRecogDetails>.Filter.Or(ownerSearchFilters));
                }
            }

            FilterDefinition<LicensePlateRecogDetails> dateFilter = null;

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                dateFilter = Builders<LicensePlateRecogDetails>.Filter.And(
                    Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.EntryTime, null),
                    Builders<LicensePlateRecogDetails>.Filter.Gte(x => x.EntryTime, request.FromDate.Value),
                    Builders<LicensePlateRecogDetails>.Filter.Or(
                        Builders<LicensePlateRecogDetails>.Filter.Lte(x => x.ExitTime, request.ToDate.Value),
                        Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.ExitTime, null)
                    )
                );
            }
            else if (request.FromDate.HasValue)
            {
                dateFilter = Builders<LicensePlateRecogDetails>.Filter.And(
                    Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.EntryTime, null),
                    Builders<LicensePlateRecogDetails>.Filter.Gte(x => x.EntryTime, request.FromDate.Value)
                );
            }
            else if (request.ToDate.HasValue)
            {
                dateFilter = Builders<LicensePlateRecogDetails>.Filter.And(
                    Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.ExitTime, null),
                    Builders<LicensePlateRecogDetails>.Filter.Lte(x => x.ExitTime, request.ToDate.Value)
                );
            }

            if (dateFilter != null)
            {
                filters.Add(dateFilter);
            }
            // Country Filter: Country = CountryName (from DynamicFields)
            if (!string.IsNullOrWhiteSpace(request.CountryName))
            {
                var countryFilter = Builders<LicensePlateRecogDetails>.Filter.Eq("country", request.CountryName);
                filters.Add(countryFilter);
            }

            // Combine all filters
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(filters);

            // Build sort
            var sort = sortOrder == 1
                ? Builders<LicensePlateRecogDetails>.Sort.Ascending(sortByField)
                : Builders<LicensePlateRecogDetails>.Sort.Descending(sortByField);

            // Get total count
            var totalCount = (int)await dbEntity.CountDocumentsAsync(filter);

            // Get paged data
            var data = await dbEntity
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<IEnumerable<ANPRParkingResponse>> ANPRVehicleParkingCountAsync(IEnumerable<string> deviceIds, DateTime startTime, DateTime endTime)
        {
            var filters = new List<FilterDefinition<LicensePlateRecogDetails>>();

            filters.Add(Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false));
            filters.Add(Builders<LicensePlateRecogDetails>.Filter.In(x => x.EntryGate, deviceIds));
            filters.Add(Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true));
            
            if (startTime != null && endTime != null)
            {
                filters.Add(
                    Builders<LicensePlateRecogDetails>.Filter.And(
                        Builders<LicensePlateRecogDetails>.Filter.Lte(x => x.EntryTime, endTime),
                        Builders<LicensePlateRecogDetails>.Filter.Or(
                            Builders<LicensePlateRecogDetails>.Filter.Gte(x => x.ExitTime, startTime),
                            Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.ExitTime, null)
                        )
                    )
                );
            }

            var filter = Builders<LicensePlateRecogDetails>.Filter.And(filters);

            var docs = await dbEntity.Find(filter).ToListAsync();

            if (docs == null)
            {
                docs = new List<LicensePlateRecogDetails>();
            }

            await QueryDataFromLinkedServers(filter, docs);

            var res =  docs.Select(x => new ANPRParkingResponse
                        {
                            DeviceId = x.EntryGate,
                            Id = x.Id,
                            ExitTime = x.ExitTime,
                            EntryTime = x.EntryTime,
                            ANPRVehicleId = x.AnprVehicleId
                        })
                       .ToList();
            
            return res;
        }

        public async Task<IEnumerable<LicensePlateRecogDetails>> GetAllVehiclesCurrentlyInAsync()
        {
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await dbEntity.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<LicensePlateRecogDetails>> GetVehiclesCurrentlyInByOwnerIdsAsync(IEnumerable<string> vehicleOwnerIds)
        {
            if (vehicleOwnerIds == null || !vehicleOwnerIds.Any())
            {
                return new List<LicensePlateRecogDetails>();
            }

            // Optimize: Only get vehicles where overstay notification hasn't been sent yet
            // This significantly reduces the number of records to process
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false),
                Builders<LicensePlateRecogDetails>.Filter.In(x => x.VehicleOwnerId, vehicleOwnerIds),
                Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.VehicleOwnerId, null),
                // Only get vehicles where notification hasn't been sent (false or not set)
                Builders<LicensePlateRecogDetails>.Filter.Or(
                    Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.OverstayNotificationSent, false),
                    Builders<LicensePlateRecogDetails>.Filter.Exists(x => x.OverstayNotificationSent, false)
                )
            );

            return await dbEntity.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<LicensePlateRecogDetails>> GetVehiclesInFor24HByOwnerIdsAsync(IEnumerable<string> vehicleOwnerIds, DateTime thresholdTime)
        {
            if (vehicleOwnerIds == null || !vehicleOwnerIds.Any())
            {
                return new List<LicensePlateRecogDetails>();
            }

            // Optimize: Get vehicles that have been in for 24+ hours AND notification hasn't been sent yet
            // thresholdTime = currentTime - 24 hours, so EntryTime < thresholdTime means vehicle has been in for 24+ hours
            var filter = Builders<LicensePlateRecogDetails>.Filter.And(
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsIn, true),
                Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.IsDeleted, false),
                Builders<LicensePlateRecogDetails>.Filter.In(x => x.VehicleOwnerId, vehicleOwnerIds),
                Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.VehicleOwnerId, null),
                Builders<LicensePlateRecogDetails>.Filter.Ne(x => x.EntryTime, null),
                // EntryTime must be before threshold (24 hours ago) - vehicle has been in for 24+ hours
                Builders<LicensePlateRecogDetails>.Filter.Lt(x => x.EntryTime, thresholdTime),
                // Only get vehicles where 24H notification hasn't been sent (false or not set)
                Builders<LicensePlateRecogDetails>.Filter.Or(
                    Builders<LicensePlateRecogDetails>.Filter.Eq(x => x.Overstay24hNotificationSent, false),
                    Builders<LicensePlateRecogDetails>.Filter.Exists(x => x.Overstay24hNotificationSent, false)
                )
            );

            return await dbEntity.Find(filter).ToListAsync();
        }
    }
}
