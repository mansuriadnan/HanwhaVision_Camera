using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;
using System.Threading.Channels;

namespace HanwhaClient.Infrastructure.Repository
{
    public class VehicleCurrentParkingCountRepository : RepositoryBase<VehicleCurrentParkingCount>, IVehicleCurrentParkingCountRepository
    {
        public VehicleCurrentParkingCountRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.VehicleCurrentParkingCount)
        {
        }

        public async Task<VehicleCurrentParkingCount> GetCurrentParkingCountId(string DeviceId, int ChannelNo, int LineIndex)
        {
            string currentParkingCountId = "";
            var filter = Builders<VehicleCurrentParkingCount>.Filter.And(
                Builders<VehicleCurrentParkingCount>.Filter.Eq(x => x.DeviceId, DeviceId),
                Builders<VehicleCurrentParkingCount>.Filter.Eq(x => x.Channel, ChannelNo),
                Builders<VehicleCurrentParkingCount>.Filter.Eq(x => x.LineIndex, LineIndex));

            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();

            if (data != null && !string.IsNullOrEmpty(data.Id)) {
                return data;
            }
            else
            {
                var vehicleCurrentParkingCount = new VehicleCurrentParkingCount
                {
                    Channel = ChannelNo,
                    DeviceId = DeviceId,
                    LineIndex = LineIndex,
                    ParkingCount = 0
                };
                await dbEntity.InsertOneAsync(vehicleCurrentParkingCount);
                return vehicleCurrentParkingCount;
            }
        }

        public async Task<bool> ResetParkingCount(ParkingCountResetRequest parkingCountResetRequest)
        {
            var update = Builders<VehicleCurrentParkingCount>.Update
                                         .Set(c => c.ParkingCount, parkingCountResetRequest.CurrentCount)
                                         .Set(c => c.CreatedOn, DateTime.UtcNow)
                                         .Set(c => c.UpdatedOn, DateTime.UtcNow);
            var filter = Builders<VehicleCurrentParkingCount>.Filter.Empty;
            var result = await dbEntity.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
