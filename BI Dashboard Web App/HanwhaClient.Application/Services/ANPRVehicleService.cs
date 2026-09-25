using AutoMapper;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text;

namespace HanwhaClient.Application.Services
{
    public class ANPRVehicleService : IANPRVehicleService
    {
        private readonly IMapper _mapper;
        private readonly IANPRVehicleRepository _anprVehicleRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IUsersRepository _userRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IClientSettingService _clientSettingService;
        private readonly ILicensePlateRecogRepository _licensePlateRecogRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IWidgetService _widgetService;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;
        private readonly ICountryService _countryService;

        public ANPRVehicleService(IMapper mapper,
            IANPRVehicleRepository anprVehicleRepository,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IUsersRepository userRepository,
            ICountryRepository countryRepository,
            IClientSettingService clientSettingService,
            ILicensePlateRecogRepository licensePlateRecogRepository,
            IDeviceMasterRepository deviceMasterRepository,
            IZoneCameraRepository zoneCameraRepository,
            IWidgetService widgetService,
            IUsersService usersService,
            IDateConvert dateConvert,
            ICountryService countryService)
        {
            _mapper = mapper;
            this._anprVehicleRepository = anprVehicleRepository;
            this._vehicleOwnerRepository = vehicleOwnerRepository;
            this._userRepository = userRepository;
            this._countryRepository = countryRepository;
            _clientSettingService = clientSettingService;
            _licensePlateRecogRepository = licensePlateRecogRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _zoneCameraRepository = zoneCameraRepository;
            _widgetService = widgetService;
            _usersService = usersService;
            _dateConvert = dateConvert;
            _countryService = countryService;
        }

        public async Task<(string Id, string ErrorMessage)> SaveANPRVehicleAsync(ANPRVehicleRequest anprVehicleRequest, string userId)
        {
            var vehicle = _mapper.Map<ANPRVehicle>(anprVehicleRequest);
            var owner = await _vehicleOwnerRepository.GetAsync(anprVehicleRequest.VehicleOwnerId);

            var isExist = await _anprVehicleRepository.IsANPRVehicleExistAsync(anprVehicleRequest, owner.RegistrationType);
            if (isExist)
            {
                var countryDetails = await _countryService.GetCountryById(anprVehicleRequest.Country);
                return ("", $"Vehicle {countryDetails.Name}-{anprVehicleRequest.State}-{anprVehicleRequest.PlateCode} is already exist.");
            }
               

            if (string.IsNullOrEmpty(anprVehicleRequest.Id))
            {
                vehicle.CreatedOn = DateTime.UtcNow;
                vehicle.UpdatedOn = DateTime.UtcNow;
                vehicle.CreatedBy = userId;
                vehicle.UpdatedBy = userId;
                var data = await _anprVehicleRepository.InsertAsync(vehicle);
                return await Task.FromResult((data, ""));
            }
            else
            {
                var update = Builders<ANPRVehicle>.Update
                .Set(c => c.VehicleOwnerId, vehicle.VehicleOwnerId)
                .Set(c => c.Country, vehicle.Country)
                .Set(c => c.State, vehicle.State)
                .Set(c => c.Series, vehicle.Series)
                .Set(c => c.VehicleNumber, vehicle.VehicleNumber)
                .Set(c => c.PlateCode, vehicle.PlateCode)
                .Set(c => c.PlateCategory, vehicle.PlateCategory)
                .Set(c => c.Make, vehicle.Make)
                .Set(c => c.Model, vehicle.Model)
                .Set(c => c.Color, vehicle.Color)
                .Set(c => c.VisitorValidFrom, vehicle.VisitorValidFrom)
                .Set(c => c.VisitorValidTo, vehicle.VisitorValidTo)
                .Set(c => c.UpdatedOn, DateTime.UtcNow)
                .Set(c => c.UpdatedBy, userId);
                await _anprVehicleRepository.UpdateFieldsAsync(vehicle.Id, update);
                return await Task.FromResult((vehicle.Id, ""));
            }
        }
        public async Task<IEnumerable<ANPRVehicleList>> GetAllANPRVehicleByOwnerAsync(AllANPRVehicleRequest request)
        {

            AllANPRVehicleWithSearchRequest searchRequest = new AllANPRVehicleWithSearchRequest();

            searchRequest.SearchText = request.SearchText;
            searchRequest.VehicleOwnerId = request.VehicleOwnerId;

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                searchRequest.VehicleOwnerIds = await _vehicleOwnerRepository.FindOwnerIdsByNameAsync(request.SearchText);
                searchRequest.CountryIds = await _countryRepository.FindCountryIdsByNameAsync(request.SearchText);
            }

            var vehicles = await _anprVehicleRepository.GetAllANPRVehicleByOwner(searchRequest);

            var ownerIds = vehicles
                .Select(v => v.VehicleOwnerId)
                .Distinct()
                .ToList();

            var countryIds = vehicles
                 .Select(o => o.Country)
                 .Distinct()
                 .ToList();

            ProjectionDefinition<VehicleOwner> projection = Builders<VehicleOwner>.Projection
            .Include("RegistrationType")
            .Include("OwnerName")
            .Include("_id");

            var owners = await _vehicleOwnerRepository.GetManyAsync(ownerIds, projection);
            var ownerDict = owners.ToDictionary(
                            o => o.Id,
                            o => new OwnerInfo
                            {
                                OwnerName = o.OwnerName,
                                RegistrationType = o.RegistrationType
                            });

            ProjectionDefinition<Country> projectionCountry = Builders<Country>.Projection
            .Include("Name")
            .Include("code")
            .Include("_id");

            var countries = await _countryRepository.GetManyAsync(countryIds, projectionCountry);
            var countryDict = countries.ToDictionary(
                    c => c.Id,
                    c => $"{c.Code} - {c.Name}");


            var aNPRVehicle = vehicles.Select(v => new ANPRVehicleList
            {
                Id = v.Id,
                VehicleOwnerType = ownerDict.ContainsKey(v.VehicleOwnerId)
                    ? ownerDict[v.VehicleOwnerId].RegistrationType
                    : null,
                VehicleOwnerName = ownerDict.ContainsKey(v.VehicleOwnerId)
                    ? ownerDict[v.VehicleOwnerId].OwnerName
                    : null,
                CountryId = v.Country,
                CountryName = countryDict.ContainsKey(v.Country)
                    ? countryDict[v.Country]
                    : null,
                State = v.State,
                Series = v.Series,
                VehicleNumber = v.VehicleNumber,
                PlateCode = v.PlateCode,
                PlateCategory = v.PlateCategory,
                Make = v.Make,
                Model = v.Model,
                Color = v.Color,
                VisitorValidFrom = v.VisitorValidFrom,
                VisitorValidTo = v.VisitorValidTo
            }).ToList();
            return aNPRVehicle;
        }
        public async Task<bool> DeleteANPRVehiclesAsync(string id, string userId)
        {
            var data = await _anprVehicleRepository.SoftDeleteAsync(id, userId);
            return data;
        }
        public async Task<IEnumerable<OwnerIdAndRegistrationTypeDto>> GetOwnerIdAndRegistrationTypeAsync(string building, string buildingUnit)
        {
            var data = await _vehicleOwnerRepository.GetOwnerIdAndRegistrationType(building, buildingUnit);
            return data;
        }

        public async Task<IEnumerable<string>> CountryIdsByNameAsync(string countryName)
        {
            var countryIds = await _countryRepository.FindCountryIdsByNameAsync(countryName);
            return countryIds;
        }

        public async Task<bool> UploadANPRImages(IEnumerable<ANPRImageUpload> aNPRImageUploads)
        {
            try
            {
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "ANPRImages");
                var clientSettings = await _clientSettingService.GetClientSetting();

                if (clientSettings != null && clientSettings.ANPRImageConfiguration != null && !string.IsNullOrEmpty(clientSettings.ANPRImageConfiguration.ImagePath))
                {
                    rootPath = clientSettings.ANPRImageConfiguration.ImagePath;
                }

                foreach (var imageDetails in aNPRImageUploads)
                {
                    string imagePath = rootPath;
                    string imageName = imageDetails.ImageName.Split('#')[0];
                    string imageSize = imageDetails.ImageName.Split('#')[1];

                    if (imageSize.ToLower() == "large")
                    {
                        imagePath = Path.Combine(rootPath, "Large");
                    }
                    else if (imageSize.ToLower() == "small")
                    {
                        imagePath = Path.Combine(rootPath, "Small");
                    }
                    else
                    {
                        return false;
                    }

                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    imagePath = Path.Combine(imagePath, imageName + ".png");
                    var base64Data = imageDetails.ImageBase64.Contains(",")
                                    ? imageDetails.ImageBase64.Split(',')[1]
                                    : imageDetails.ImageBase64;

                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    await File.WriteAllBytesAsync(imagePath, imageBytes);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<VehicleByPlateNumberLPRDto> GetVehicleByPlateNumAsync(string plateNumber, string Country, string state)
        {
            var country = await _countryRepository.FindSingleCountryIdsByNameAsync(Country);
            var data = await _anprVehicleRepository.GetVehicleByPlateNumAsync(plateNumber, country, state);
            return data;
        }

        public async Task<IEnumerable<ANPRParkingResponse>> ANPRVehicleParkingCountAsync(WidgetRequest widgetRequest)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            var parkingResult = await _licensePlateRecogRepository.ANPRVehicleParkingCountAsync(deviceIds, widgetRequest.StartDate, widgetRequest.EndDate);
            return parkingResult;
        }

        public async Task<IEnumerable<VehicleParking>> ANPRVehicleParkingCountByZonesAsync(WidgetRequest widgetRequest)
        {
            List<VehicleParking> vehicleParkingCountByZones = new List<VehicleParking>();
            var zoneData = await _widgetService.GetAllZoneByPermission(widgetRequest);
            zoneData = zoneData.Where(x => x.IsParkingZone == true);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }

                var data = await _licensePlateRecogRepository.ANPRVehicleParkingCountAsync(zoneCameraList.Select(x => x.DeviceId), widgetRequest.StartDate, widgetRequest.EndDate);


                vehicleParkingCountByZones.Add(new VehicleParking
                {
                    ZoneName = zone.ZoneName,
                    ParkingCount = data.Count(),
                    TotalParkingOccupancy = zone.VehicleOccupancy ?? 0
                });
            }

            return vehicleParkingCountByZones;
        }

        public async Task<StringBuilder> ANPRVehicleParkingCountCsvAsync(WidgetRequest widgetRequest)
        {
            StringBuilder csvBuilder = new StringBuilder();

            var zoneData = await ANPRVehicleParkingCountByZonesAsync(widgetRequest);

            var totalZone = zoneData.Sum(x => x.TotalParkingOccupancy);

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var getANPRVehicleParkingData = await ANPRVehicleParkingCountAsync(widgetRequest);

            if (getANPRVehicleParkingData != null)
            {
                getANPRVehicleParkingData = getANPRVehicleParkingData.Select(r => new ANPRParkingResponse
                {
                    Id = r.Id,
                    DeviceId = r.DeviceId,
                    // Apply offset ONLY to StartDate
                    EntryTime = r.EntryTime.HasValue ? r.EntryTime.Value + offsetTimeStamp : (DateTime?)null,
                    ExitTime = r.ExitTime.HasValue ? r.ExitTime.Value + offsetTimeStamp : (DateTime?)null,
                    // EndDate remains unchanged
                    //EndDate = r.EndDate
                }).ToList();

                List<CsvANPRParkingResponseModel> lstANPRParking = new List<CsvANPRParkingResponseModel>();

                var dateRange = GenerateDatePoints(widgetRequest.StartDate + offsetTimeStamp, widgetRequest.EndDate + offsetTimeStamp, widgetRequest.AverageIntervalMinute);


                for (int i = 0; i < dateRange.Count(); i++)
                {
                    int availableCount = 0;
                    int occupiedCount = 0;
                    var bucketStart = dateRange[i]; // 12/24/2025 1:00 PM

                    var intervalInMinutes =
                       (int)(dateRange[1] - dateRange[0]).TotalMinutes;

                    var bucketEnd = bucketStart.AddMinutes(intervalInMinutes);

                    foreach (var rmaData in getANPRVehicleParkingData)
                    {

                        // Completed
                        if (rmaData.ExitTime.HasValue &&
                            rmaData.ExitTime >= bucketStart &&
                            rmaData.ExitTime < bucketEnd)
                        {
                            availableCount++;
                            continue;
                        }

                        // In Progress
                        if (rmaData.EntryTime <= dateRange[i] && (rmaData.ExitTime == null || rmaData.ExitTime >= dateRange[i]))
                        {

                            occupiedCount++;
                            continue;
                        }

                    }
                    availableCount = totalZone - occupiedCount;


                    // ✅ Single record per date
                    lstANPRParking.Add(new CsvANPRParkingResponseModel
                    {
                        DateTime = dateRange[i],
                        Available = availableCount,
                        Occupied = occupiedCount
                    });
                }

                if (lstANPRParking.Count() > 0)
                {
                    csvBuilder = await GenerateCSVForANPRParking(lstANPRParking, widgetRequest.WidgetName, widgetRequest);
                }

            }

            return csvBuilder;
        }

        public static List<DateTime> GenerateDatePoints(DateTime startDate, DateTime endDate, int intervalMinutes)
        {
            List<DateTime> datePoints = new List<DateTime>();

            DateTime currentDate = startDate;

            while (currentDate < endDate)
            {
                datePoints.Add(currentDate);
                currentDate = currentDate.AddMinutes(intervalMinutes);
            }

            return datePoints;
        }

        private async Task<StringBuilder> GenerateCSVForANPRParking(List<CsvANPRParkingResponseModel> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));


            // List of vehicle types
            var inOutTypes = new List<string> { "Available", "Occupied" };

            // Process all data once and group by vehicle type
            var groupedByType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                groupedByType[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            data = data.Select(x => new CsvANPRParkingResponseModel
            {
                DateTime = x.DateTime.Value,
                Available = x.Available,
                Occupied = x.Occupied
            }).GroupBy(x => x.DateTime)
            .Select(x => new CsvANPRParkingResponseModel
            {
                DateTime = x.Key,
                Available = x.Sum(y => y.Available),
                Occupied = x.Sum(y => y.Occupied)
            }).ToList();


            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in inOutTypes)
                {
                    var count = GetCountByInOutType(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByType[vehicleType].ContainsKey(date))
                        {
                            groupedByType[vehicleType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByType[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByType[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByType[vehicleType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType));
            }

            return csvBuilder;
        }

        private double GetCountByInOutType(CsvANPRParkingResponseModel entry, string vehicleType)
        {
            return vehicleType switch
            {
                "Available" => entry.Available,
                "Occupied" => entry.Occupied,
                _ => 0
            };
        }

        private async Task<TimeSpan> GetOffset(string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            return offsetTimeStamp;
        }

        private async Task<string> GetTitleForCsv(string widgetName, DateTime startDate, DateTime endDate, string userId)
        {
            var csvBuilder = new StringBuilder();
            var userData = await _usersService.GetUserAsync(userId);

            csvBuilder.AppendLine($"Widget name: {widgetName}");
            csvBuilder.AppendLine($"\"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}\"");
            csvBuilder.AppendLine($"Exported By: {userData.Firstname} {userData.Lastname}");
            csvBuilder.AppendLine($"\"Exported On: {DateTime.Now:MMM dd, yyyy hh:mm tt}\"");

            return csvBuilder.ToString();
        }

        private string GetHourHeaderForCsv(string headerData, int minuteInterval, int slotsPerDay, string otherHeaders = "")
        {
            var csvBuilder = new StringBuilder();

            // Generate header
            var header = new List<string> { headerData };
            for (int i = 0; i < slotsPerDay; i++)
            {
                var startTime = TimeSpan.FromMinutes(i * minuteInterval);
                var endTime = startTime.Add(TimeSpan.FromMinutes(minuteInterval - 1));
                header.Add($"{startTime:hh\\:mm} - {endTime:hh\\:mm}");
            }
            if (!string.IsNullOrEmpty(otherHeaders))
            {
                header.Add(otherHeaders);
            }
            csvBuilder.AppendLine(string.Join(",", header));
            return csvBuilder.ToString();
        }

        private string BindHourDataForCsv(Dictionary<DateTime, Dictionary<int, int>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "", bool addTotal = false)
        {
            var csvBuilder = new StringBuilder();
            var currentDate = startDate.Date;
            //if (data.Count() > 0)
            //{
            while (currentDate <= endDate.Date)
            {
                var row = new List<string>();
                if (string.IsNullOrEmpty(rowName))
                {
                    row = new List<string> { currentDate.ToString("dd-MMM-yy") };
                }
                else
                {
                    row = new List<string> { rowName, currentDate.ToString("dd-MMM-yy") };
                }

                int totalCount = 0;
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    if (data.TryGetValue(currentDate, out var slotData) &&
                        slotData.TryGetValue(slot, out var count))
                    {
                        row.Add(count.ToString());
                        totalCount += count;
                    }
                    else
                    {
                        row.Add("0");
                    }
                }

                if (addTotal)
                {
                    row.Add(totalCount.ToString());
                }

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }
    }
}
