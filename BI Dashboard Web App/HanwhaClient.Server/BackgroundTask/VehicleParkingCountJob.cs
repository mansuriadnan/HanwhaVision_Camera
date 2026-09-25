using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using Quartz;
using static OpenCvSharp.Stitcher;

namespace HanwhaClient.Server.BackgroundTask
{
    public class VehicleParkingCountJob : IJob
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IVehicleCurrentParkingCountRepository _vehicleCurrentParkingCountRepository;
        private readonly IVehicleParkingCountRepository _vehicleParkingCountRepository;
        public VehicleParkingCountJob(IVehicleRepository vehicleRepository,IDeviceMasterService deviceMasterService, IVehicleCurrentParkingCountRepository vehicleCurrentParkingCountRepository, IVehicleParkingCountRepository vehicleParkingCountRepository)
        {
            _vehicleRepository = vehicleRepository;
            _deviceMasterService = deviceMasterService;
            _vehicleCurrentParkingCountRepository = vehicleCurrentParkingCountRepository;
            _vehicleParkingCountRepository = vehicleParkingCountRepository;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var deviceDetail = await _deviceMasterService.GetVehicleDeviceListAsync();
            var datetime = DateTime.UtcNow;
            foreach (var device in deviceDetail)
            {
                if (device.ApiModel == "SUNAPI")
                {
                    VehicleCount vehicleCount = new VehicleCount();
                    vehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.Id, 0, datetime);
                    if (vehicleCount != null)
                    {
                        foreach(var line in vehicleCount.VehicleCounts.SelectMany(x => x.Lines))
                        {
                            var currentId = await _vehicleCurrentParkingCountRepository.GetCurrentParkingCountId(device.Id, 0,line.LineIndex);
                            var lastUpdatedparkingCount = await _vehicleParkingCountRepository.GetLatestVehicleParkingCountAsync(device.Id, 0, line.LineIndex, datetime);
                            if (lastUpdatedparkingCount != null)
                            {
                                var update = Builders<VehicleCurrentParkingCount>.Update
                                         .Set(c => c.ParkingCount, lastUpdatedparkingCount.ParkingCount)
                                         .Set(c => c.CreatedOn, datetime)
                                         .Set(c => c.UpdatedOn, datetime);

                                var result = await _vehicleCurrentParkingCountRepository.UpdateFieldsAsync(currentId.Id, update);
                            }
                            
                        }
                    }
                }
                else if (device.ApiModel == "WiseAI")
                {
                    foreach (var channel in device.ChannelIndexList.Select(x => x.Channel).Distinct())
                    {
                        VehicleCount vehicleCount = new VehicleCount();
                        vehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.Id, channel, datetime);
                        if (vehicleCount != null) {

                            foreach (var line in vehicleCount.VehicleCounts.SelectMany(x => x.Lines))
                            {
                                var currentId = await _vehicleCurrentParkingCountRepository.GetCurrentParkingCountId(device.Id, channel, line.LineIndex);
                                var lastUpdatedparkingCount = await _vehicleParkingCountRepository.GetLatestVehicleParkingCountAsync(device.Id, channel, line.LineIndex, datetime);
                                if (lastUpdatedparkingCount != null)
                                {
                                    var update = Builders<VehicleCurrentParkingCount>.Update
                                             .Set(c => c.ParkingCount, lastUpdatedparkingCount.ParkingCount)
                                             .Set(c => c.CreatedOn, datetime)
                                             .Set(c => c.UpdatedOn, datetime);

                                    var result = await _vehicleCurrentParkingCountRepository.UpdateFieldsAsync(currentId.Id, update);
                                }
                            }

                        }
                    }
                }
            }
            return ; 
        }
    }
}
