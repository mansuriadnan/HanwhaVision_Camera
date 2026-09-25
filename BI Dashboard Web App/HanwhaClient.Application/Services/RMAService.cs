using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class RMAService : IRMAService
    {
        private readonly IRMARepository _rmaRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;

        public RMAService(IRMARepository repository, IDeviceMasterRepository deviceMasterRepository, IZoneCameraRepository zoneCameraRepository, IUsersService usersService, IDateConvert dateConvert)
        {
            _rmaRepository = repository;
            _deviceMasterRepository = deviceMasterRepository;
            _zoneCameraRepository = zoneCameraRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
        }

        public async Task<PagedResult<RMAResponseDto>> GetRMAAsync(RMASerachModel model)
        {
            // 1. Resolve device IDs by floor & zone
            var deviceIds = await _zoneCameraRepository
                .GetDevicebyFloorAndZoneAsync(model.FloorIds, model.ZoneIds);

            var deviceIdSet = (deviceIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            model.DeviceIds = deviceIdSet;
            // 2. Fetch paged maintenance schedules
            var rmaResult = await _rmaRepository.GetAllRMA(model);

            if (rmaResult.TotalCount == 0)
            {
                return new PagedResult<RMAResponseDto>
                {
                    Items = Array.Empty<RMAResponseDto>(),
                    TotalCount = 0
                };
            }

            var rma = rmaResult.Data;

            // 3. Collect distinct device & plan IDs from current page
            var pageDeviceIds = rma
                .Select(x => x.DeviceId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 4. Define projections
            var deviceProjection = Builders<DeviceMaster>.Projection
                .Include(x => x.Id)
                .Include(x => x.DeviceName)
                .Include(x => x.DeviceType)
                .Include(x => x.Model)
                .Include(x => x.Location)
                .Include(x => x.SerialNumber)
                .Include(x => x.IpAddress);

            // 5. Fetch related data
            var deviceDetailsTask = _deviceMasterRepository
                .GetManyAsync(pageDeviceIds, deviceProjection);


            await Task.WhenAll(deviceDetailsTask);

            var deviceById = deviceDetailsTask.Result
                .Where(d => d != null)
                .ToDictionary(d => d.Id, d => d, StringComparer.OrdinalIgnoreCase);


            var items = rma.Select(schedule =>
            {
                deviceById.TryGetValue(schedule.DeviceId?.Trim() ?? string.Empty, out var device);


                return new RMAResponseDto
                {
                    Id = schedule.Id?.ToString(),
                    DeviceId = device?.Id ?? string.Empty,
                    DeviceName = device?.DeviceName ?? string.Empty,
                    IpAddress = device.IpAddress ?? string.Empty,
                    DeviceType = device?.DeviceType ?? string.Empty,
                    Model = device?.Model ?? string.Empty,
                    Location = device?.Location ?? string.Empty,
                    SerialNumber = device?.SerialNumber ?? string.Empty,

                    InProgressNotes = schedule.InProgressNotes ?? "",
                    CompletedNotes = schedule.CompletedNotes ?? "",
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate,
                    RMAStatus = schedule.RMAStatus,
                    FloorId = schedule.FloorId,
                    ZoneId = schedule.ZoneId,
                };
            }).ToList();


            // 7. Return paged result
            return new PagedResult<RMAResponseDto>
            {
                Items = items,
                TotalCount = rmaResult.TotalCount
            };

        }

        public async Task<bool> DeleteRMAAsync(string id, string userId)
        {
            var data = await _rmaRepository.GetAsync(id);
            if (data != null)
            {
                var dataResult = await _rmaRepository.SoftDeleteAsync(id, userId);
                return dataResult;
            }
            return false;
        }

        public async Task<bool> SaveRMAsync(RmaRequestDto rmaDto, string userId)
        {
            // Determine if this is Create or Update
            if (string.IsNullOrEmpty(rmaDto.Id))
            {
                var plan = new Rma
                {
                    RMAStatus = rmaDto.RMAStatus,
                    StartDate = rmaDto.StartDate,
                    EndDate = rmaDto.EndDate,
                    DeviceId = rmaDto.DeviceId,
                    InProgressNotes = rmaDto.InProgressNotes,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = userId,
                    FloorId = rmaDto.FloorId,
                    ZoneId = rmaDto.ZoneId
                };

                // CREATE
                // plan.InProgressNotes = rmaDto.InProgressNotes;
                var rmaId = await _rmaRepository.InsertAsync(plan);
                return !string.IsNullOrEmpty(rmaId) ? true : false;
            }
            else
            {
                // UPDATE
                var exists = await _rmaRepository.GetAsync(rmaDto.Id);
                if (exists == null)
                {
                    return false;
                }
                var updateMaintanance = Builders<Rma>.Update
                    .Set(c => c.RMAStatus, rmaDto.RMAStatus)
                    .Set(c => c.StartDate, rmaDto.StartDate)
                    .Set(c => c.EndDate, rmaDto.EndDate)
                    .Set(c => c.DeviceId, rmaDto.DeviceId)
                    .Set(c => c.CompletedNotes, rmaDto.CompletedNotes)
                    .Set(c => c.InProgressNotes, rmaDto.InProgressNotes)
                    .Set(c => c.FloorId, rmaDto.FloorId)
                    .Set(c => c.ZoneId, rmaDto.ZoneId)
                    .Set(c => c.UpdatedOn, DateTime.UtcNow)
                    .Set(c => c.UpdatedBy, userId);
                var result = await _rmaRepository.UpdateFieldsAsync(rmaDto.Id, updateMaintanance);
                return result;
            }
        }

        public async Task<IEnumerable<RMAWidgetResponse>> GetRmaMaintenanceCount(WidgetRequest widgetRequest)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            RMAWidgetRequest rMAWidgetRequest = new RMAWidgetRequest();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget(false);
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            rMAWidgetRequest.FloorIds = widgetRequest.FloorIds;
            rMAWidgetRequest.ZoneIds = widgetRequest.ZoneIds;
            rMAWidgetRequest.StartDate = widgetRequest.StartDate;
            rMAWidgetRequest.EndDate = widgetRequest.EndDate;
            rMAWidgetRequest.DeviceIds = deviceIds;

            var rmaResult = await _rmaRepository.GetRmaMaintenanceWidgetData(rMAWidgetRequest);
            return rmaResult;

        }

        public async Task<StringBuilder> DownloadRmaMaintenanceCSV(WidgetRequest widgetRequest)
        {
            StringBuilder csvBuilder = new StringBuilder();
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var getRMAData = await GetRmaMaintenanceCount(widgetRequest);

            if (getRMAData != null)
            {
                getRMAData = getRMAData.Select(r => new RMAWidgetResponse
                {
                    Id = r.Id,
                    DeviceId = r.DeviceId,
                    RMAStatus = r.RMAStatus,
                    // Apply offset ONLY to StartDate
                    StartDate = r.StartDate.HasValue ? r.StartDate.Value + offsetTimeStamp : (DateTime?)null,
                    EndDate = r.EndDate.HasValue ? r.EndDate.Value + offsetTimeStamp : (DateTime?)null,
                    // EndDate remains unchanged
                    //EndDate = r.EndDate
                }).ToList();

                List<CsvRmaMaintenanceResponseModel> lstRMAMaintenance = new List<CsvRmaMaintenanceResponseModel>();

                var dateRange = GenerateDatePoints(widgetRequest.StartDate + offsetTimeStamp, widgetRequest.EndDate + offsetTimeStamp, widgetRequest.AverageIntervalMinute);

                //foreach (var date in dateRange)
                //{
                //    var count = 0;
                //    foreach (var rmaData in getRMAData)
                //    {
                //        if (rmaData.StartDate <= date && (rmaData.EndDate != null || rmaData.EndDate > date))
                //        {
                //            CsvRmaMaintenanceResponseModel model = new CsvRmaMaintenanceResponseModel();
                //            model.DateTime = date;
                //            model.InProgress = count + 1;
                //            model.Completed = 0;
                //            lstRMAMaintenance.Add(model);
                //            count++;
                //        }
                //    }

                //    foreach (var rmaData in getRMAData)
                //    {
                //        if (rmaData.RMAStatus == "Completed" && rmaData.EndDate <= date)
                //        {
                //            CsvRmaMaintenanceResponseModel model = new CsvRmaMaintenanceResponseModel();
                //            model.DateTime = date;
                //            model.InProgress = 0;
                //            model.Completed = 1;
                //            lstRMAMaintenance.Add(model);
                //        }
                //    }
                //}

                for (int i = 0; i < dateRange.Count(); i++)
                {
                    int inProgressCount = 0;
                    int completedCount = 0;
                    var bucketStart = dateRange[i]; // 12/24/2025 1:00 PM

                    var intervalInMinutes =
                       (int)(dateRange[1] - dateRange[0]).TotalMinutes;

                    var bucketEnd = bucketStart.AddMinutes(intervalInMinutes);

                    foreach (var rmaData in getRMAData)
                    {

                        // Completed
                        if (rmaData.RMAStatus == "Completed" &&
                            rmaData.EndDate.HasValue &&
                            rmaData.EndDate >= bucketStart && 
                            rmaData.EndDate < bucketEnd)
                        {
                            completedCount++;
                            continue;
                        }

                        // In Progress
                        if (rmaData.StartDate <= dateRange[i] && (rmaData.EndDate == null || rmaData.EndDate >= dateRange[i]))
                        {
                            inProgressCount++;
                            continue;
                        }


                        //if (rmaData.RMAStatus != "Completed" || rmaData.EndDate != null)
                        //{
                        //    continue;
                        //}

                       
                    }

                    // ✅ Single record per date
                    lstRMAMaintenance.Add(new CsvRmaMaintenanceResponseModel
                    {
                        DateTime = dateRange[i],
                        InProgress = inProgressCount,
                        Completed = completedCount
                    });
                }

                if (lstRMAMaintenance.Count() > 0)
                {
                    csvBuilder = await GenerateCSVForRMAMaintenance(lstRMAMaintenance, widgetRequest.WidgetName, widgetRequest);
                }

            }
            return csvBuilder;
        }


        // Parses ISO 8601 strings (e.g., "2025-12-20T17:00:00.000Z") and calls the UTC range generator.
        // Default step is 10 minutes if not provided.
        //  private static List<DateTime> CreateDateTimeRange(
        //DateTime startDate,
        //DateTime endDate,
        //int intervalInMinutes)
        //  {
        //      if (startDate > endDate)
        //          throw new ArgumentException("Start date must be less than end date.");

        //      if (intervalInMinutes <= 0)
        //          throw new ArgumentException("Interval must be greater than zero.");

        //      var result = new List<DateTime>();
        //      var current = startDate;

        //      while (current.AddMinutes(intervalInMinutes) <= endDate.AddMinutes(1))
        //      {
        //          result.Add(current);
        //          current = current.AddMinutes(intervalInMinutes);
        //      }

        //      return result;
        //  }

        public static List<DateTime> GenerateDatePoints(DateTime startDate,DateTime endDate,int intervalMinutes)
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


        private async Task<TimeSpan> GetOffset(string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            return offsetTimeStamp;
        }

        private async Task<StringBuilder> GenerateCSVForRMAMaintenance(List<CsvRmaMaintenanceResponseModel> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));


            // List of vehicle types
            var inOutTypes = new List<string> { "In Progress", "Completed" };

            // Process all data once and group by vehicle type
            var groupedByType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                groupedByType[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            data = data.Select(x => new CsvRmaMaintenanceResponseModel
            {
                DateTime = x.DateTime.Value,
                InProgress = x.InProgress,
                Completed = x.Completed
            }).GroupBy(x => x.DateTime)
            .Select(x => new CsvRmaMaintenanceResponseModel
            {
                DateTime = x.Key,
                InProgress = x.Sum(y => y.InProgress),
                Completed = x.Sum(y => y.Completed)
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

        private string BindHourDataForCsv(Dictionary<DateTime, Dictionary<int, int>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "")
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

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }

        private double GetCountByInOutType(CsvRmaMaintenanceResponseModel entry, string vehicleType)
        {
            return vehicleType switch
            {
                "In Progress" => entry.InProgress,
                "Completed" => entry.Completed,
                _ => 0
            };
        }

        public async Task<StringBuilder> ExportRMACSV(RMASerachModel model, string userId)
        {
            StringBuilder sb = new StringBuilder();
            var offsetTimeStamp = await GetOffset(userId);
            model.PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
            model.PageSize = model.PageSize <= 0 ? 10000000 : model.PageSize;

            var data = await GetRMAAsync(model);
            var rmaData = data.Items;

            if (rmaData.Count() > 0)
            {
                sb.AppendLine("Device Name,IP Address,Model,Location,Device Type,RMA Status,Start Date,End Date");
            }

            foreach (var item in rmaData)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(item.DeviceName),
                    EscapeCsv(item.IpAddress),
                    EscapeCsv(item.Model),
                    EscapeCsv(item.Location),
                    EscapeCsv(item.DeviceType),
                    EscapeCsv(item.RMAStatus),
                    EscapeCsv(FormatDate(item.StartDate, offsetTimeStamp)),
                    EscapeCsv(FormatDate(item.EndDate, offsetTimeStamp, "N/A"))
                     ));
            }
            return sb;
        }


        private static string FormatDate(DateTime? dt, TimeSpan offset, string nullText = "N/A")
        {
            if (!dt.HasValue) return nullText;

            // Apply offset (assuming offset is a TimeSpan from your GetOffset)
            DateTime local = dt.Value + offset;

            // Use invariant culture to avoid locale differences
            return local.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }


        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
