using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.SSM;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;


namespace HanwhaClient.Infrastructure.Repository
{
    public class SSMDeviceDetailsRepository : RepositoryBase<SsmDeviceDetails>, ISSMDeviceDetailsRepository
    {
        private readonly ISsmOfflinedeviceRepository _ssmOfflinedeviceRepository;
        public SSMDeviceDetailsRepository(MongoDbConnectionService mongoDbConnectionService, ISsmOfflinedeviceRepository ssmOfflinedeviceRepository) : base(mongoDbConnectionService, AppDBConstants.SsmDeviceDetails)
        {
            _ssmOfflinedeviceRepository = ssmOfflinedeviceRepository;
        }

        public async Task<(bool success, string id)> AddUpdateSsmDeviceDetails(List<SsmCameraDto> deviceDto, string serverId)
        {
            var filter = Builders<SsmDeviceDetails>.Filter.And(
                Builders<SsmDeviceDetails>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmDeviceDetails>.Filter.Eq(x => x.ServerId, serverId)
            );

            var existingDevices = await dbEntity.Find(filter).ToListAsync();

            var existingDeviceList = existingDevices
                .Where(x => !string.IsNullOrEmpty(x.IpAddress) && !string.IsNullOrEmpty(x.Name))
                .ToList();

            var devicesToInsert = new List<SsmDeviceDetails>();
            var deviceUpdates = new List<WriteModel<SsmDeviceDetails>>();
            var offlineDeviceTasks = new List<Task>();

            foreach (var device in deviceDto)
            {
                if (device.Data == null || string.IsNullOrEmpty(device.Data.Address))
                    continue;

                var existingDevice = existingDeviceList.FirstOrDefault(x => x.IpAddress == device.Data.Address && x.Name == device.Data.Name);

                if (existingDevice == null)
                {
                    devicesToInsert.Add(new SsmDeviceDetails
                    {
                        ServerId = serverId,
                        IpAddress = device.Data.Address,
                        Name = device.Data.Name,
                        Status = device.Status,
                        Location = device.Data.Location,
                        CreatedOn = DateTime.UtcNow,
                        CameraStatus = "Connected", // get the value from the api response
                        RecordingStatus = "Disconnected" // get the value from the api response
                    });
                }
                else
                {
                    bool isOnlineDevice = existingDevice.CameraStatus == "Connected" || existingDevice.CameraStatus == "Warning";
                    bool isOnlineApi = device.Status != 0;

                    if (isOnlineDevice != isOnlineApi)
                    {
                        if (isOnlineDevice == true && isOnlineApi == false)
                        {
                            var newEntry = new SsmOfflinedevice
                            {
                                ServerId = serverId,
                                DeviceId = existingDevice.Id,
                                OfflineTime = DateTime.UtcNow,
                                OnlineTime = null,
                                CreatedOn = DateTime.UtcNow
                            };
                            existingDevice.CameraStatus = "Disconnected";
                            existingDevice.UpdatedOn = DateTime.UtcNow;
                            
                            offlineDeviceTasks.Add(_ssmOfflinedeviceRepository.InsertAsync(newEntry));
                        }
                        else
                        {
                            existingDevice.CameraStatus = "Warning";
                            existingDevice.UpdatedOn = DateTime.UtcNow;

                            offlineDeviceTasks.Add(Task.Run(async () =>
                            {
                                var offlineRecord = await _ssmOfflinedeviceRepository.GetSsmOfflinedevice(serverId, existingDevice.Id);
                                if (offlineRecord != null)
                                {
                                    var update = Builders<SsmOfflinedevice>.Update
                                    .Set(x => x.OnlineTime, DateTime.UtcNow)
                                    .Set(x => x.UpdatedOn, DateTime.UtcNow);

                                    await _ssmOfflinedeviceRepository.UpdateFieldsAsync(offlineRecord.Id, update);
                                }
                            }));
                        }

                        var updateDefinition = Builders<SsmDeviceDetails>.Update
                            .Set(x => x.CameraStatus, existingDevice.CameraStatus)
                            .Set(x => x.UpdatedOn, DateTime.UtcNow);

                        deviceUpdates.Add(new UpdateOneModel<SsmDeviceDetails>(
                            Builders<SsmDeviceDetails>.Filter.Eq(x => x.Id, existingDevice.Id),
                            updateDefinition
                        ));
                    }
                }
            }

            bool success = true;

            if (devicesToInsert.Any())
            {
                success = await InsertManyAsync(devicesToInsert);
            }

            if (deviceUpdates.Any())
            {
                await dbEntity.BulkWriteAsync(deviceUpdates);
            }

            if (offlineDeviceTasks.Any())
            {
                await Task.WhenAll(offlineDeviceTasks);
            }

            return (success, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<SsmDeviceDetails> deviceDetails, int count)> GetSsmDeviceDetailsAsync(SSMDeviceDetailsRequest request, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filters = new List<FilterDefinition<SsmDeviceDetails>>();

            // Mandatory serverId filter
            filters.Add(
                Builders<SsmDeviceDetails>.Filter.Eq(x => x.ServerId, request.ServerId)
            );

            // Search filter (optional)
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim();

                var searchFilter = Builders<SsmDeviceDetails>.Filter.Or(
                    Builders<SsmDeviceDetails>.Filter.Regex("ipAddress", new BsonRegularExpression(search, "i")),
                    Builders<SsmDeviceDetails>.Filter.Regex("name", new BsonRegularExpression(search, "i")),
                    Builders<SsmDeviceDetails>.Filter.Regex("location", new BsonRegularExpression(search, "i")),
                    Builders<SsmDeviceDetails>.Filter.Regex("cameraStatus", new BsonRegularExpression(search, "i")),
                    Builders<SsmDeviceDetails>.Filter.Regex("recordingStatus", new BsonRegularExpression(search, "i"))
                );

                filters.Add(searchFilter);
            }

            // Final combined filter
            var finalFilter = filters.Any() ? Builders<SsmDeviceDetails>.Filter.And(filters) : Builders<SsmDeviceDetails>.Filter.Empty;
            string sortField = request.SortBy ?? "createdOn"; // default sort field
            bool sortDescending = request.SortOrder == -1;     // -1 = desc, 1 = asc

            var sortDefinition = sortDescending
                ? Builders<SsmDeviceDetails>.Sort.Descending(sortField)
                : Builders<SsmDeviceDetails>.Sort.Ascending(sortField);

            var data = await dbEntity.Find(finalFilter).Sort(sortDefinition).Skip((request.PageNumber - 1) * request.PageSize).Limit(request.PageSize).ToListAsync();
            int totalCount = (int)await dbEntity.CountDocumentsAsync(finalFilter);
            return (data, totalCount);
        }

        public async Task<List<SsmDeviceDetails>> GetDevicesByServerIdsAsync(List<string> serverIds)
        {
            var filter = Builders<SsmDeviceDetails>.Filter.And(
                Builders<SsmDeviceDetails>.Filter.In(x => x.ServerId, serverIds),
                Builders<SsmDeviceDetails>.Filter.Eq(x => x.IsDeleted, false)
            );
            return await dbEntity.Find(filter).ToListAsync();
        }

    }
}

