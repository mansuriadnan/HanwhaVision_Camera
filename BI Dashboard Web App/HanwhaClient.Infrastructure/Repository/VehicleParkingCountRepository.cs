using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using System.Threading.Channels;

namespace HanwhaClient.Infrastructure.Repository
{
    public class VehicleParkingCountRepository : RepositoryBase<VehicleParkingCount>, IVehicleParkingCountRepository
    {
        public VehicleParkingCountRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.VehicleParkingCount)
        {
        }

        public async Task<IEnumerable<VehicleParkingCount>> VehicleParkingAnalysisDataAsync(IEnumerable<ZoneCamera> zoneCamerasList, DateTime startdate, DateTime enddate)
        {
            var filters = Builders<VehicleParkingCount>.Filter.And(
                        Builders<VehicleParkingCount>.Filter.In(x => x.DeviceId, zoneCamerasList.Select(x => x.DeviceId)),
                        Builders<VehicleParkingCount>.Filter.Gte(x => x.CreatedOn, startdate),
                        Builders<VehicleParkingCount>.Filter.Lte(x => x.CreatedOn, enddate)); 

            var result = await dbEntity.Find(filters).ToListAsync();
            await QueryDataFromLinkedServers(filters, result);
            return result;
        }

        public async Task<VehicleParkingCount> GetLatestVehicleParkingCountAsync(string DeviceId, int ChannelNo, int LineIndex, DateTime startdate)
        {
            var filters = Builders<VehicleParkingCount>.Filter.And(
                Builders<VehicleParkingCount>.Filter.Eq(x => x.DeviceId, DeviceId),
                Builders<VehicleParkingCount>.Filter.Eq(x => x.Channel, ChannelNo),
                Builders<VehicleParkingCount>.Filter.Eq(x => x.LineIndex, LineIndex),
                Builders<VehicleParkingCount>.Filter.Lte(x => x.CreatedOn, startdate));

            var result = await dbEntity.Find(filters).SortByDescending(x => x.CreatedOn).FirstOrDefaultAsync();
            return result;
        }
    }
}
