using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IMaintenanceScheduleRepository _maintenanceScheduleRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IMaintenancePlanRepository _maintenancePlanRepository;
        private readonly IWidgetService _widgetService;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;

        public MaintenanceScheduleService(IMaintenanceScheduleRepository repository, IZoneCameraRepository zoneCameraRepository, IDeviceMasterRepository deviceMasterRepository, IMaintenancePlanRepository maintenancePlanRepository, IWidgetService widgetService, IUsersService usersService, IDateConvert dateConvert)
        {
            _maintenanceScheduleRepository = repository;
            _zoneCameraRepository = zoneCameraRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _maintenancePlanRepository = maintenancePlanRepository;
            _widgetService = widgetService;
            _usersService = usersService;
            _dateConvert = dateConvert;
        }

        public async Task<string> AddUpdateMaintenanceScheduleAsync(MaintenanceScheduleDto scheduleDto, string userId)
        {
            if (scheduleDto.IsScheduleManually)
            {
                var deviceExits = await _maintenanceScheduleRepository.CheckMaintenanceDeviceExits(scheduleDto.DeviceId);
                if (deviceExits)
                {
                    return "";
                }

                var notes = scheduleDto.StatusHistory.FirstOrDefault()?.Notes;

                var last = scheduleDto.StatusHistory.LastOrDefault();
                if (last != null)
                {
                    // Update the existing last item
                    last.Status = "Not Started";
                    last.Notes = notes;
                    last.StatusDatetime = DateTime.UtcNow;
                }
            }

            var schedule = MapToModel(scheduleDto);
            if (scheduleDto.IsScheduleManually)
            {

                schedule.CreatedOn = DateTime.UtcNow;
                schedule.CreatedBy = userId;
                schedule.UpdatedOn = DateTime.UtcNow;
                schedule.UpdatedBy = userId;
                schedule.MaintenancePlanId = null;
            }

            // Ensure the latest status is set from the most recent status history
            if (schedule.StatusHistory != null && schedule.StatusHistory.Any())
            {
                schedule.LatestStatus = schedule.StatusHistory
                    .OrderByDescending(x => x.StatusDatetime)
                    .First()
                    .Status;
            }

            var result = await _maintenanceScheduleRepository.InsertAsync(schedule);
            return result.ToString();
            // return MapToDto(result);
        }

        //public async Task<List<MaintenanceScheduleSearchResponseDto>> GetAllAsync(MaintenanceScheduleSerachModel model)
        //{
        //    // 1) Get device IDs for requested floors/zones
        //    var deviceIds = await _zoneCameraRepository
        //        .GetDevicebyFloorAndZoneAsync(model.FloorIds, model.ZoneIds);

        //    var response = new List<MaintenanceScheduleSearchResponseDto>();
        //    // 2) Get all schedules
        //    try
        //    {
        //        var schedules = await _maintenanceScheduleRepository.GetAllAsync();


        //        // 3) Build deviceId set for filtering
        //        var deviceIdSet = (deviceIds ?? Enumerable.Empty<string>())
        //            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        //        // 4) Compute target due date (use UTC or local based on how DueDate is stored)
        //        DateTime? targetDueDate = null;
        //        if (model.DueFilter.HasValue)
        //        {
        //            var today = DateTime.UtcNow.Date; // or DateTime.Now.Date
        //            targetDueDate = today.AddDays(model.DueFilter.Value);
        //        }

        //        // 5) Normalize StatusFilter (compare against LatestStatus)
        //        var statusFilter = string.IsNullOrWhiteSpace(model.StatusFilter)
        //            ? null
        //            : model.StatusFilter.Trim();


        //        // 6) Apply filters to schedules
        //        var filteredSchedules = schedules
        //            .Where(s => s != null)
        //            .Where(s => deviceIdSet.Count == 0 ||
        //                        (s.DeviceId != null && deviceIdSet.Contains(s.DeviceId)))
        //            .Where(s => !targetDueDate.HasValue ||
        //                        (s.DueDate.HasValue && s.DueDate.Value.Date == targetDueDate.Value))
        //            .Where(s => statusFilter == null ||
        //                        (!string.IsNullOrEmpty(s.LatestStatus) &&
        //                         string.Equals(s.LatestStatus, statusFilter, StringComparison.OrdinalIgnoreCase)))
        //            .ToList();
        //        if (filteredSchedules.Count() == 0)
        //            return new List<MaintenanceScheduleSearchResponseDto>();

        //        // 7) Collect distinct deviceIds and planIds from filtered schedules
        //        var filteredDeviceIds = filteredSchedules
        //            .Select(s => s.DeviceId)
        //            .Where(id => !string.IsNullOrWhiteSpace(id))
        //            .Distinct(StringComparer.OrdinalIgnoreCase)
        //            .ToList();

        //        var filteredPlanIds = filteredSchedules
        //            .Select(s => s.MaintenancePlanId)
        //            .Where(id => id != null) // adjust if string or ObjectId
        //            .Distinct()
        //            .ToList();

        //        // 8) Prepare projections (include join keys!)
        //        var deviceProjection = Builders<DeviceMaster>.Projection
        //            .Include(x => x.Id)                  // ensure join key is included
        //            .Include("deviceName")
        //            .Include("model")
        //            .Include("location")
        //            .Include("serialNumber");

        //        var planProjection = Builders<MaintenancePlan>.Projection
        //            .Include(x => x.Id)                  // ensure join key is included
        //            .Include("planName");                 // if your field is "name", add .Include("name") too

        //        // 9) Fetch device + plan details based on filtered IDs
        //        var deviceDetails = await _deviceMasterRepository.GetManyAsync(filteredDeviceIds, deviceProjection);
        //        var planDetails = await _maintenancePlanRepository.GetManyAsync(filteredPlanIds, planProjection);

        //        // 10) Build lookup dictionaries
        //        // NOTE: If your join key is different (e.g., DeviceId instead of Id), change the key selector.
        //        var deviceById = deviceDetails
        //            .Where(d => d != null)
        //            .ToDictionary(
        //                d => d.Id, // or d.DeviceId
        //                d => d,
        //                StringComparer.OrdinalIgnoreCase);

        //        var planById = planDetails
        //            .Where(p => p != null)
        //            .ToDictionary(
        //                p => p.Id, // or p.MaintenancePlanId
        //                p => p);

        //        // 11) Project to response DTO
        //        response = filteredSchedules.Select(s =>
        //        {
        //            DeviceMaster device = null;
        //            MaintenancePlan plan = null;

        //            if (!string.IsNullOrWhiteSpace(s.DeviceId))
        //                deviceById.TryGetValue(s.DeviceId, out device); // change key if needed

        //            if (s.MaintenancePlanId != null)
        //                planById.TryGetValue(s.MaintenancePlanId, out plan); // change key if needed

        //            // Try both "PlanName" and "Name" properties depending on schema
        //            var planName =
        //                (plan?.PlanName) ?? string.Empty;

        //            return new MaintenanceScheduleSearchResponseDto
        //            {
        //                Id = s.Id?.ToString(), // adjust if your Id is not nullable/string
        //                DeviceName = device?.DeviceName ?? string.Empty,
        //                Model = device?.Model ?? string.Empty,
        //                Location = device?.Location ?? string.Empty,
        //                SerialNumber = device?.SerialNumber ?? string.Empty,
        //                PlanName = planName,
        //                MaintenanceDueDate = s.DueDate.HasValue
        //                    ? s.DueDate.Value.ToString("dd/MM/yyyy")
        //                    : string.Empty
        //            };
        //        })
        //        .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        var msg = ex.Message;
        //        throw;
        //    }

        //    return response;
        //}


        public async Task<PagedResult<MaintenanceScheduleSearchResponseDto>> GetAllAsync(MaintenanceScheduleSerachModel model)
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
            var schedulesResult = await _maintenanceScheduleRepository.GetAllMaintenanceSchedule(model);

            if (schedulesResult.TotalCount == 0)
            {
                return new PagedResult<MaintenanceScheduleSearchResponseDto>
                {
                    Items = Array.Empty<MaintenanceScheduleSearchResponseDto>(),
                    TotalCount = 0
                };
            }

            var schedules = schedulesResult.Data;

            // 3. Collect distinct device & plan IDs from current page
            var pageDeviceIds = schedules
                .Select(x => x.DeviceId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pagePlanIds = schedules
                .Select(x => x.MaintenancePlanId)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            // 4. Define projections
            var deviceProjection = Builders<DeviceMaster>.Projection
                .Include(x => x.Id)
                .Include(x => x.DeviceName)
                .Include(x => x.DeviceType)
                .Include(x => x.Model)
                .Include(x => x.Location)
                .Include(x => x.IpAddress)
                .Include(x => x.SerialNumber);

            var planProjection = Builders<MaintenancePlan>.Projection
                .Include(x => x.Id)
                .Include(x => x.PlanName);

            // 5. Fetch related data
            var deviceDetailsTask = _deviceMasterRepository
                .GetManyAsync(pageDeviceIds, deviceProjection);

            var planDetailsTask = _maintenancePlanRepository
                .GetManyAsync(pagePlanIds, planProjection);

            await Task.WhenAll(deviceDetailsTask, planDetailsTask);

            var deviceById = deviceDetailsTask.Result
                .Where(d => d != null)
                .ToDictionary(d => d.Id, d => d, StringComparer.OrdinalIgnoreCase);

            var planById = planDetailsTask.Result?
                           .Where(p => p != null)
                           .ToDictionary(p => p.Id, p => p)
                           ?? new Dictionary<string, MaintenancePlan>();


            // 6. Map to response DTO
            var items = schedules.Select(schedule =>
            {
                deviceById.TryGetValue(schedule.DeviceId?.Trim() ?? string.Empty, out var device);

                MaintenancePlan plan = null;
                if (planById.Count > 0 && schedule.MaintenancePlanId != null)
                {
                    planById.TryGetValue(schedule.MaintenancePlanId, out plan);
                }

                return new MaintenanceScheduleSearchResponseDto
                {
                    Id = schedule.Id?.ToString(),
                    DeviceId = device?.Id ?? string.Empty,
                    DeviceName = device?.DeviceName ?? string.Empty,
                    DeviceType = device?.DeviceType ?? string.Empty,
                    Model = device?.Model ?? string.Empty,
                    Location = device?.Location ?? string.Empty,
                    SerialNumber = device?.SerialNumber ?? string.Empty,
                    IpAddress = device?.IpAddress ?? string.Empty,

                    // ✅ Blank if plan is missing or dictionary is null
                    PlanName = plan?.PlanName ?? string.Empty,
                    BeforeCameraImage = schedule?.BeforeCameraImage ?? string.Empty,
                    AfterCameraImage = schedule?.AfterCameraImage ?? string.Empty,

                    MaintenanceDueDate = schedule.DueDate,
                    Status = schedule.LatestStatus,
                    StatusHistory = schedule.StatusHistory
                };
            }).ToList();

          
            var sortedItems = SortItems(items, model.SortBy, model.SortOrder).ToList();
            // 7. Return paged result
            return new PagedResult<MaintenanceScheduleSearchResponseDto>
            {
                Items = sortedItems,
                TotalCount = schedulesResult.TotalCount
            };
        }



        IEnumerable<MaintenanceScheduleSearchResponseDto> SortItems(
            IEnumerable<MaintenanceScheduleSearchResponseDto> source,
            string sortBy,
            int? sortOrder)
        {
            // Skip sorting for these columns
            if (string.Equals(sortBy, "dueDate", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sortBy, "latestStatus", StringComparison.OrdinalIgnoreCase))
            {
                return source; // return as-is
            }

            var sortSelectors = new Dictionary<string, Func<MaintenanceScheduleSearchResponseDto, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["deviceName"] = x => x.DeviceName ?? string.Empty,
                ["model"] = x => x.Model ?? string.Empty,
                ["location"] = x => x.Location ?? string.Empty,
                ["deviceType"] = x => x.DeviceType ?? string.Empty,
                ["ipAddress"] = x => x.IpAddress ?? string.Empty,
                ["planName"] = x => x.PlanName ?? string.Empty,
            };

            if (!sortSelectors.TryGetValue(sortBy ?? string.Empty, out var selector))
            {
                selector = x => x.DeviceName ?? string.Empty; // default
            }

            var comparer = StringComparer.OrdinalIgnoreCase;
            var descending = string.Equals(sortOrder == 1 ? "asc" : "desc", "desc", StringComparison.OrdinalIgnoreCase);

            var ordered = descending
                ? source.OrderByDescending(selector, comparer)
                : source.OrderBy(selector, comparer);

            // Optional: secondary sort for stability
            return ordered.ThenBy(x => x.Id ?? string.Empty, comparer);
        }

        //private static IEnumerable<MaintenanceScheduleSearchResponseDto> SortDto(
        //    IEnumerable<MaintenanceScheduleSearchResponseDto> source,
        //    string sortBy,
        //    bool asc)
        //{
        //    // Map SortBy to DTO properties (case-insensitive)
        //    Func<MaintenanceScheduleSearchResponseDto, object> keySelector = sortBy.ToLower() switch
        //    {
        //        "devicename" => (x => x.DeviceName ?? string.Empty),
        //        "deviceType" => (x => x.DeviceType ?? string.Empty),
        //        "model" => (x => x.Model ?? string.Empty),
        //        "location" => (x => x.Location ?? string.Empty),
        //        "serialnumber" => (x => x.SerialNumber ?? string.Empty),
        //        "planname" => (x => x.PlanName ?? string.Empty),
        //        "maintenanceduedate" => (x => x.MaintenanceDueDate ?? string.Empty),
        //        "id" => (x => x.Id ?? string.Empty),
        //        _ => (x => x.MaintenanceDueDate ?? string.Empty) // default sort
        //    };

        //    return asc ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        //}



        private MaintenanceSchedule MapToModel(MaintenanceScheduleDto dto)
        {
            return new MaintenanceSchedule
            {
                Id = dto.Id,
                DeviceId = dto.DeviceId,
                MaintenancePlanId = dto.MaintenancePlanId,
                StatusHistory = dto.StatusHistory?.Select(sh => new StatusHistoryItem
                {
                    Status = sh.Status,
                    Notes = sh.Notes,
                    StatusDatetime = sh.StatusDatetime
                }).ToList() ?? new List<StatusHistoryItem>(),
                LatestStatus = dto.LatestStatus,
                DueDate = dto.DueDate,
                BeforeCameraImage = dto.BeforeCameraImage,
                AfterCameraImage = dto.AfterCameraImage,
                ImageMatchingPercentage = dto.ImageMatchingPercentage,
                IsScheduleManually = dto.IsScheduleManually,
                FloorId = dto.FloorId,
                ZoneId = dto.ZoneId

            };
        }

        private MaintenanceScheduleDto MapToDto(MaintenanceSchedule model)
        {
            return new MaintenanceScheduleDto
            {
                Id = model.Id,
                DeviceId = model.DeviceId,
                MaintenancePlanId = model.MaintenancePlanId,
                StatusHistory = model.StatusHistory?.Select(sh => new StatusHistoryItemDto
                {
                    Status = sh.Status,
                    Notes = sh.Notes,
                    StatusDatetime = sh.StatusDatetime
                }).ToList() ?? new List<StatusHistoryItemDto>(),
                LatestStatus = model.LatestStatus,
                DueDate = model.DueDate,
                BeforeCameraImage = model.BeforeCameraImage,
                AfterCameraImage = model.AfterCameraImage,
                ImageMatchingPercentage = model.ImageMatchingPercentage,
                IsScheduleManually = model.IsScheduleManually
            };
        }


        public async Task<string> UpdateMaintenanceScheduleStatusAsync(UpdateMaintenanceScheduleStatusDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(dto.MaintenanceScheduleId))
                throw new ArgumentException("MaintenanceScheduleId is required.", nameof(dto.MaintenanceScheduleId));

            var deviceId = _maintenanceScheduleRepository.GetAsync(dto.MaintenanceScheduleId).Result.DeviceId;

            // If we need a camera image, get it once
            string? liveImageBase64 = null;
            string? getFullPath = null;
            if (dto.Status == "In Progress" || dto.Status == "Done")
            {
                RequestCurrentImageDto requestCurrentImageDto = new RequestCurrentImageDto();
                requestCurrentImageDto.DeviceId = deviceId;
                liveImageBase64 = await GetLiveImageAsync(requestCurrentImageDto);
                getFullPath = await SaveBase64ImageAsync(liveImageBase64, "", "maintenance");
            }

            var item = new StatusHistoryItem
            {
                Status = dto.Status,
                Notes = dto.Notes,
                StatusDatetime = dto.StatusDatetime ?? DateTime.UtcNow,
            };

            // Build update definition
            var updates = new List<UpdateDefinition<MaintenanceSchedule>>
                          {
                              // Always push status history
                              Builders<MaintenanceSchedule>.Update.Push(s => s.StatusHistory, item)
                              .Set(s => s.UpdatedBy, userId)
                              .Set(s => s.UpdatedOn, DateTime.UtcNow)
                          };

            // Conditionally set the image fields
            if (dto.Status == "In Progress" && !string.IsNullOrEmpty(liveImageBase64))
            {
                updates.Add(Builders<MaintenanceSchedule>.Update.Set(s => s.BeforeCameraImage, getFullPath));
            }
            else if (dto.Status == "Done" && !string.IsNullOrEmpty(liveImageBase64))
            {
                updates.Add(Builders<MaintenanceSchedule>.Update.Set(s => s.AfterCameraImage, getFullPath));
            }
            updates.Add(Builders<MaintenanceSchedule>.Update.Set(s => s.LatestStatus, dto.Status));

            // Combine into a single atomic update
            var update = Builders<MaintenanceSchedule>.Update.Combine(updates);

            await _maintenanceScheduleRepository.UpdateFieldsAsync(dto.MaintenanceScheduleId, update);

            return dto.MaintenanceScheduleId!;
        }

        private async Task<string> SaveBase64ImageAsync(
        string base64Image,
        string? folderPath,
        string fileNameWithoutExtension)
        {
            // If folderPath is empty → use "MaintenanceImages" in current project
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "MaintenanceImages"
                );
            }

            // Ensure directory exists
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (base64Image == null)
            {
                return null;
            }

            // Remove data:image/...;base64, if present
            if (base64Image.Contains(","))
                base64Image = base64Image.Split(',')[1];

            byte[] imageBytes = Convert.FromBase64String(base64Image);

            string fileName = $"{fileNameWithoutExtension}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            string fullPath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return fullPath; // saved file path
        }

        public async Task<(string imageBase64, string ErrorMessage)> GetBeforeAfterImageAsync(string imageFullpath)
        {
            if (string.IsNullOrWhiteSpace(imageFullpath))
                return await Task.FromResult(("Invalid file path.", ""));

            if (!File.Exists(imageFullpath))
                return await Task.FromResult(("Image not found.", ""));

            byte[] imageBytes = await File.ReadAllBytesAsync(imageFullpath);

            // Detect MIME type from extension
            string extension = Path.GetExtension(imageFullpath).ToLower();
            string mimeType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "image/jpeg" // default fallback
            };

            var imageBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
            return await Task.FromResult((imageBase64, imageFullpath));

        }

        public async Task<string> GetLiveImageAsync(RequestCurrentImageDto dto)
        {
            GetCurrentImageDto img = new GetCurrentImageDto();
            var device = await _deviceMasterRepository.GetAsync(dto.DeviceId);
            return await GetDeviceHeatmapImage(device);
        }

        private async Task<string> GetDeviceHeatmapImage(DeviceMaster device)
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(device.UserName, device.Password),
                PreAuthenticate = true
            };

            using (var httpClient = new HttpClient(handler))
            {
                try
                {
                    var response = await httpClient.GetAsync((device.IsHttps ? "https://" : "http://") + device.IpAddress + SunapiAPIConstant.HeatmapImage);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch snapshot. Status: {response.StatusCode}");
                        return null;
                    }

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64Image = Convert.ToBase64String(imageBytes);

                    return $"data:image/jpeg;base64,{base64Image}";
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public async Task<StringBuilder> ExportMaintenanceScheduleCSV(MaintenanceScheduleSerachModel model)
        {
            StringBuilder sb = new StringBuilder();

            model.PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
            model.PageSize = model.PageSize <= 0 ? 10000000 : model.PageSize;
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();

            if (model.FloorIds != null && model.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(model.FloorIds, model.ZoneIds);
            }

            // 1. Resolve device IDs by floor & zone
            //var deviceIds = await _zoneCameraRepository
            //    .GetDevicebyFloorAndZoneAsync(model.FloorIds, model.ZoneIds);

            var deviceIdSet = (deviceIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            model.DeviceIds = deviceIdSet;

            // 2. Fetch paged maintenance schedules
            var schedulesResult = await _maintenanceScheduleRepository.GetAllMaintenanceSchedule(model);

            if (schedulesResult.TotalCount == 0)
            {
                return sb;
            }

            var schedules = schedulesResult.Data;

            // 3. Collect distinct device & plan IDs from current page
            var pageDeviceIds = schedules
                .Select(x => x.DeviceId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pagePlanIds = schedules
                .Select(x => x.MaintenancePlanId)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            // 4. Define projections
            var deviceProjection = Builders<DeviceMaster>.Projection
                .Include(x => x.Id)
                .Include(x => x.DeviceName)
                .Include(x => x.DeviceType)
                .Include(x => x.Model)
                .Include(x => x.Location)
                .Include(x => x.SerialNumber);

            var planProjection = Builders<MaintenancePlan>.Projection
                .Include(x => x.Id)
                .Include(x => x.PlanName);

            // 5. Fetch related data
            var deviceDetailsTask = _deviceMasterRepository
                .GetManyAsync(pageDeviceIds, deviceProjection);

            var planDetailsTask = _maintenancePlanRepository
                .GetManyAsync(pagePlanIds, planProjection);

            await Task.WhenAll(deviceDetailsTask, planDetailsTask);

            var deviceById = deviceDetailsTask.Result
                .Where(d => d != null)
                .ToDictionary(d => d.Id, d => d, StringComparer.OrdinalIgnoreCase);

            var planById = planDetailsTask.Result?
                           .Where(p => p != null)
                           .ToDictionary(p => p.Id, p => p)
                           ?? new Dictionary<string, MaintenancePlan>();


            // 6. Map to response DTO
            var items = schedules.Select(schedule =>
            {
                deviceById.TryGetValue(schedule.DeviceId?.Trim() ?? string.Empty, out var device);

                MaintenancePlan plan = null;
                if (planById.Count > 0 && schedule.MaintenancePlanId != null)
                {
                    planById.TryGetValue(schedule.MaintenancePlanId, out plan);
                }

                return new MaintenanceScheduleSearchResponseDto
                {
                    DeviceName = device?.DeviceName ?? string.Empty,
                    Model = device?.Model ?? string.Empty,
                    Location = device?.Location ?? string.Empty,
                    SerialNumber = device?.SerialNumber ?? string.Empty,
                    DeviceType = device?.DeviceType ?? string.Empty,
                    PlanName = plan?.PlanName ?? string.Empty,
                    Status = schedule.LatestStatus,
                    MaintenanceDueDate = schedule.DueDate,
                };
            }).ToList();

            // 7. Write CSV Header
            if (items.Count() > 0)
            {
                sb.AppendLine("Device Name,Model,Location,Serial Number,Device Type,Plan Name,Status,Due Date");
            }

            // 8. Write CSV Rows
            foreach (var item in items)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(item.DeviceName),
                    EscapeCsv(item.Model),
                    EscapeCsv(item.Location),
                    EscapeCsv(item.SerialNumber),
                    EscapeCsv(item.DeviceType),
                    EscapeCsv(item.PlanName),
                    EscapeCsv(item.Status),
                    EscapeCsv(item.MaintenanceDueDate.Value.ToString("dd/MM/yyyy"))

                ));
            }

            return sb;

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
