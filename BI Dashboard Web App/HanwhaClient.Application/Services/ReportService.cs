using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Core.Services;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Auth;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using Microsoft.Web.Administration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using SharpCompress.Common;
using System;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace HanwhaClient.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IFloorRepository _floorRepository;
        private readonly IBase64Generator _base64Generator;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;

        public ReportService(IReportRepository reportRepository, ISiteRepository siteRepository, IUsersRepository usersRepository, IZoneRepository zoneRepository,
            IFloorRepository floorRepository, IBase64Generator base64Generator, IStringLocalizer<AppMessages> localizer
            , IHttpContextAccessor httpContextAccessor, IUsersService usersService, IDateConvert dateConvert)
        {
            _reportRepository = reportRepository;
            _siteRepository = siteRepository;
            _usersRepository = usersRepository;
            _zoneRepository = zoneRepository;
            _floorRepository = floorRepository;
            _base64Generator = base64Generator;
            _localizer = localizer;
            _httpContextAccessor = httpContextAccessor;
            _usersService = usersService;
            _dateConvert = dateConvert;
        }

        public async Task<IEnumerable<ReportResponseModel>> GetAllAsync()
        {
            var reportsData = await _reportRepository.GetAllAsync();

            return reportsData.Select(r => new ReportResponseModel
            {
                Id = r.Id,
                ReportType = r.ReportType,
                ComperisionType = r.ComperisionType,
                SiteReport = r.SiteReport,
                ZoneReport = r.ZoneReport,
                CreatedOn = r.CreatedOn,
                UpdatedOn = r.UpdatedOn
            });
        }

        public async Task<string> AddUpdateReportAsync(ReportRequestModel model, string userId)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                var newReport = new Report
                {
                    ReportType = model.ReportType,
                    ComperisionType = model.ComperisionType,
                    SiteReport = model.SiteReport,
                    ZoneReport = model.ZoneReport,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = userId,
                    UpdatedOn = DateTime.UtcNow
                };

                await _reportRepository.InsertAsync(newReport);
                return newReport.Id.ToString();
            }
            else
            {
                var reportId = new ObjectId(model.Id);
                var existing = await _reportRepository.GetAsync(model.Id);

                if (existing == null)
                    return "Report not found.";

                existing.ReportType = model.ReportType;
                existing.ComperisionType = model.ComperisionType;
                existing.SiteReport = model.SiteReport;
                existing.ZoneReport = model.ZoneReport;
                existing.UpdatedBy = userId;
                existing.UpdatedOn = DateTime.UtcNow;

                var result = await _reportRepository.UpdateAsync(existing);
                return result ? "Report updated successfully." : "Update failed.";
            }
        }

        public async Task<string> DeleteReportByIdAsync(string reportId, string userId)
        {

            if (string.IsNullOrEmpty(reportId))
                return "reportId are required.";

            var reportData = await _reportRepository.GetAsync(reportId);
            if (reportData == null)
                return "Report not found.";


            await _reportRepository.SoftDeleteAsync(reportId, userId);

            return "Group deleted successfully.";

        }

        public async Task<IEnumerable<GetFloorDto>> GetAllFloorBySiteId(string siteId)
        {
            // Get site data from repository
            var siteData = await _siteRepository.GetAsync(siteId);

            if (siteData == null)
            {
                throw new KeyNotFoundException($"Site with ID {siteId} not found");
            }

            // Extract connection parameters (adjust property names as needed)
            var apiBaseUrl = siteData.HostingAddress;
            var username = siteData.Username;
            var password = siteData.Password;

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            // 1. First call the login API to get the token
            var loginResponse = await GetAuthToken(httpClient, username, password);

            if (string.IsNullOrEmpty(loginResponse?.Data.AccessToken))
            {
                throw new UnauthorizedAccessException("Failed to authenticate with the API");
            }

            // 2. Now call the floors API with the token
            try
            {
                //httpClient.DefaultRequestHeaders.Authorization =
                //    new AuthenticationHeaderValue("Bearer ", loginResponse.Data.AccessToken);

                //var response = await httpClient.GetAsync("api/floors"); // Adjust endpoint as needed

                //response.EnsureSuccessStatusCode();

                //var content = await response.Content.ReadAsStringAsync();
                //var floors = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ReportResponseModel>>(
                //    content,
                //    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Step 2: Use token to call the floor API
                //https://localhost:5173/api/Floor/FloorsByPermission
                //https://localhost:5173/api/Floor/FloorZoneByPermission

                if (string.IsNullOrEmpty(loginResponse?.Data.AccessToken))
                {
                    throw new UnauthorizedAccessException("Failed to authenticate with the API");
                }

                WidgetRequest widgetRequest = new WidgetRequest();
                List<string> floorIds = new List<string>();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

                var floorData = await httpClient.GetAsync($"{apiBaseUrl}/api/Floor/FloorsByPermission");
                var floorJson = await floorData.Content.ReadAsStringAsync();
                var floorDataRes = JsonConvert.DeserializeObject<StandardAPIResponse<IEnumerable<GetFloorDto>>>(floorJson);

                return floorDataRes.Data;
            }
            catch (Exception ex)
            {
                // Log error if needed
                // _logger.LogError(ex, "Error calling floors API");
                throw; // Re-throw or handle as appropriate
            }
        }


        public async Task<ReportDto> GenerateReportById(string reportId)
        {
            ReportDto reportDto = new ReportDto();
            var reportData = await _reportRepository.GetAsync(reportId);
            var performanceComparisonEntries = new List<PerformanceComparisonTable>();

            if (reportData != null)
            {
                if (reportData.SiteReport != null && reportData.ReportType == "site report")
                {
                    var sitesIds = reportData.SiteReport.SitesIds;
                    var userData = await _usersRepository.GetAsync(reportData.CreatedBy.ToString());

                    var tasks = sitesIds.Select(async siteId =>
                    {
                        var parentSiteForChild = new SiteMaster();
                        var siteData = await _siteRepository.GetAsync(siteId);
                        if (siteData == null)
                        {
                            var allSiteData = await _siteRepository.GetAllAsync();
                            parentSiteForChild = allSiteData
                                .FirstOrDefault(site => site.ChildSites != null &&
                                                        site.ChildSites.Any(child => child.Id == siteId));

                            if (parentSiteForChild != null)
                            {
                                siteData = parentSiteForChild;
                            }
                            else
                            {
                                throw new KeyNotFoundException($"Site with ID {siteId} not found");
                            }
                        }

                        var childSiteData = parentSiteForChild.ChildSites?.FirstOrDefault(x => x.Id == siteId);
                        var apiBaseUrl = childSiteData?.HostingAddress ?? siteData.HostingAddress;
                        var username = childSiteData?.Username ?? siteData.Username;
                        var password = childSiteData?.Password ?? siteData.Password;

                        using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
                        var loginResponse = await GetAuthToken(httpClient, username, password);
                        if (string.IsNullOrEmpty(loginResponse?.Data.AccessToken))
                        {
                            throw new UnauthorizedAccessException("Failed to authenticate with the API");
                        }

                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

                        // Floors
                        var floorData = await httpClient.GetAsync($"{apiBaseUrl}/api/Floor/FloorsByPermission");
                        var floorJson = await floorData.Content.ReadAsStringAsync();
                        var floorDataRes = JsonConvert.DeserializeObject<StandardAPIResponse<IEnumerable<GetFloorDto>>>(floorJson);

                        var floorIds = floorDataRes?.Data?.Select(f => f.Id).ToList() ?? new List<string>();

                        var widgetRequest = new WidgetRequest
                        {
                            FloorIds = floorIds,
                            StartDate = reportData.SiteReport.StartDate,
                            EndDate = reportData.SiteReport.EndDate
                        };

                        var widgetJson = JsonConvert.SerializeObject(widgetRequest);
                        var widgetContent = new StringContent(widgetJson, Encoding.UTF8, "application/json");

                        PerformanceComparisonTable performanceComparisonTable = new PerformanceComparisonTable();

                        if (reportData.ComperisionType.Contains("people"))
                        {
                            var peopleInOutTotalResponseTask = httpClient.PostAsync($"{apiBaseUrl}/api/PeopleWidget/PeopleInOutTotal", widgetContent);
                            var peopleOccupancyResponseTask = httpClient.PostAsync($"{apiBaseUrl}/api/PeopleWidget/PeopleCapacityUtilization", widgetContent);

                            await Task.WhenAll(peopleInOutTotalResponseTask, peopleOccupancyResponseTask);

                            // Deserialize results
                            var peopleData = JsonConvert.DeserializeObject<StandardAPIResponse<PeopleVehicleInOutTotal>>(
                            await (await peopleInOutTotalResponseTask).Content.ReadAsStringAsync());
                            var utilizationData = JsonConvert.DeserializeObject<StandardAPIResponse<CapacityUtilization>>(
                            await (await peopleOccupancyResponseTask).Content.ReadAsStringAsync());

                            performanceComparisonTable.PeopleCount = (int)(peopleData?.Data?.TotalInCount ?? 0);
                            performanceComparisonTable.PeopleOccupancy = utilizationData?.Data?.Percentage ?? 0;
                            performanceComparisonTable.PeopleUtilization = utilizationData?.Data?.Utilization ?? 0;
                        }

                        if(reportData.ComperisionType.Contains("vehicle"))
                        {
                            var vehicleInOutTotalResponseTask = httpClient.PostAsync($"{apiBaseUrl}/api/Widget/VehicleInOutTotal", widgetContent);
                            var vehicleOccupancyResponseTask = httpClient.PostAsync($"{apiBaseUrl}/api/Widget/VehicleCapacityUtilization", widgetContent);

                            await Task.WhenAll(vehicleInOutTotalResponseTask, vehicleOccupancyResponseTask);

                            // Deserialize results
                            var vehicleData = JsonConvert.DeserializeObject<StandardAPIResponse<PeopleVehicleInOutTotal>>(
                                await (await vehicleInOutTotalResponseTask).Content.ReadAsStringAsync());

                            var vehicleUtilizationData = JsonConvert.DeserializeObject<StandardAPIResponse<CapacityUtilization>>(
                                await (await vehicleOccupancyResponseTask).Content.ReadAsStringAsync());

                            performanceComparisonTable.VehicleCount = (int)(vehicleData?.Data?.TotalInCount ?? 0);
                            performanceComparisonTable.VehicleOccupancy = vehicleUtilizationData?.Data?.Percentage ?? 0;
                            performanceComparisonTable.VehicleUtilization = vehicleUtilizationData?.Data?.Utilization ?? 0;
                        }
                        performanceComparisonTable.SiteZoneName = childSiteData?.SiteName ?? siteData.SiteName;

                        return performanceComparisonTable;
                    });

                    performanceComparisonEntries = (await Task.WhenAll(tasks)).ToList();
                    var totalPeopleCount = performanceComparisonEntries.Sum(x => x.PeopleCount);
                    var avgPeopleOccupancy = performanceComparisonEntries.Any() ? (int)Math.Round(performanceComparisonEntries.Average(x => x.PeopleOccupancy)) : 0;

                    var totalVehicleCount = performanceComparisonEntries.Sum(x => x.VehicleCount);
                    var avgVehicleOccupancy = performanceComparisonEntries.Any() ? (int)Math.Round(performanceComparisonEntries.Average(x => x.VehicleOccupancy)) : 0;

                    var keyPerformanceMetrics = new KeyPerformanceMetrics
                    {
                        TotalPeopleCount = totalPeopleCount,
                        AveragePeopleOccupancyRate = avgPeopleOccupancy,
                        TotalVehicleCount = totalVehicleCount,
                        AverageVehicleOccupancyRate = avgVehicleOccupancy
                    };

                    var siteNames = await Task.WhenAll(reportData.SiteReport.SitesIds.Select(async id =>
                    {
                        var site = await _siteRepository.GetAsync(id);

                        if (site == null)
                        {
                            var allSiteData = await _siteRepository.GetAllAsync();

                            var parentSite = allSiteData
                                .FirstOrDefault(site => site.ChildSites?.Any(child => child.Id == id) == true);

                            var matchingChild = parentSite?.ChildSites?.FirstOrDefault(child => child.Id == id);

                            if (matchingChild != null)
                            {
                                return matchingChild.SiteName;
                            }

                            return "Unknown";
                        }

                        return site.SiteName ?? "Unknown";
                    }));
                    var reportHeader = new ReportHeader
                    {
                        ReportType = reportData.ReportType,
                        ReportName = reportData.SiteReport.ReportName,
                        Sites = string.Join(", ", siteNames),
                        ReportStartDate = reportData.SiteReport.StartDate,
                        ReportEndDate = reportData.SiteReport.EndDate,
                    };

                    if (performanceComparisonEntries.Any())
                    {
                        var averageEntry = new PerformanceComparisonTable
                        {
                            SiteZoneName = "Average",
                            PeopleCount = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleCount)),
                            PeopleOccupancy = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleOccupancy)),
                            VehicleCount = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleCount)),
                            VehicleOccupancy = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleOccupancy)),
                            PeopleUtilization = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleUtilization)),
                            VehicleUtilization = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleUtilization))
                        };

                        performanceComparisonEntries.Add(averageEntry);
                    }

                    reportDto.PerformanceComparisonTable = performanceComparisonEntries;
                    reportDto.ReportHeader = reportHeader;
                    reportDto.KeyPerformanceMetrics = keyPerformanceMetrics;
                    reportDto.CreatedBy = userData.Firstname + " " + userData.Firstname;
                    reportDto.ComperisionType = reportData.ComperisionType;
                    if (reportData.CreatedOn.HasValue)
                    {
                        reportDto.CreatedOn = reportData.CreatedOn;
                    }

                    return reportDto;
                }
                else
                {
                    var siteId = reportData.ZoneReport.SiteId;
                    var userData = await _usersRepository.GetAsync(reportData.CreatedBy.ToString());

                    var siteData = await _siteRepository.GetAsync(siteId);
                    if (siteData == null)
                    {
                        throw new KeyNotFoundException($"Site with ID {siteId} not found");
                    }

                    var apiBaseUrl = siteData.HostingAddress;
                    var username = siteData.Username;
                    var password = siteData.Password;

                    using var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri(apiBaseUrl);

                    var loginResponse = await GetAuthToken(httpClient, username, password);
                    if (string.IsNullOrEmpty(loginResponse?.Data.AccessToken))
                    {
                        throw new UnauthorizedAccessException("Failed to authenticate with the API");
                    }
                    var strZoneName = "";
                    var zoneTasks = reportData.ZoneReport.ZoneIds.Select(async zoneId =>
                    {
                        WidgetRequest widgetRequest = new WidgetRequest();
                        var zoneData = new ZoneResDto();

                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

                        var response = await httpClient.GetAsync($"{apiBaseUrl}/api/Zone/GetZoneByZoneId/{zoneId}");
                        if (!response.IsSuccessStatusCode)
                        {
                            return null; // skip on failure
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<StandardAPIResponse<ZoneResDto>>(json);

                        if (result?.Data == null)
                        {
                            return null;
                        }

                        zoneData = result.Data;

                        widgetRequest.FloorIds = reportData.ZoneReport.FloorIds;
                        widgetRequest.ZoneIds = new List<string> { zoneId.ToString() };
                        widgetRequest.StartDate = reportData.ZoneReport.StartDate;
                        widgetRequest.EndDate = reportData.ZoneReport.EndDate;

                        var widgetJson = JsonConvert.SerializeObject(widgetRequest);
                        var widgetContent = new StringContent(widgetJson, Encoding.UTF8, "application/json");

                        PerformanceComparisonTable performanceComparisonTable = new PerformanceComparisonTable();
                        if (reportData.ComperisionType.Contains("people"))
                        {
                            var peopleInOutTask = httpClient.PostAsync($"{apiBaseUrl}/api/PeopleWidget/PeopleInOutTotal", widgetContent);
                            var peopleOccTask = httpClient.PostAsync($"{apiBaseUrl}/api/PeopleWidget/PeopleCapacityUtilization", widgetContent);

                            // Run widget calls in parallel
                            await Task.WhenAll(peopleInOutTask, peopleOccTask);

                            var peopleData = JsonConvert.DeserializeObject<StandardAPIResponse<PeopleVehicleInOutTotal>>(
                            await (await peopleInOutTask).Content.ReadAsStringAsync());
                            var peopleUtilizationData = JsonConvert.DeserializeObject<StandardAPIResponse<CapacityUtilization>>(
                            await (await peopleOccTask).Content.ReadAsStringAsync());

                            performanceComparisonTable.PeopleCount = (int)(peopleData?.Data?.TotalInCount ?? 0);
                            performanceComparisonTable.PeopleOccupancy = peopleUtilizationData?.Data?.Percentage ?? 0;
                            performanceComparisonTable.PeopleUtilization = peopleUtilizationData?.Data?.Utilization ?? 0;

                        }

                        if (reportData.ComperisionType.Contains("vehicle"))
                        {
                            var vehicleInOutTask = httpClient.PostAsync($"{apiBaseUrl}/api/Widget/VehicleInOutTotal", widgetContent);
                            var vehicleOccTask = httpClient.PostAsync($"{apiBaseUrl}/api/Widget/VehicleCapacityUtilization", widgetContent);

                            // Run widget calls in parallel
                            await Task.WhenAll(vehicleInOutTask, vehicleOccTask);

                            // Parse results
                            var vehicleData = JsonConvert.DeserializeObject<StandardAPIResponse<PeopleVehicleInOutTotal>>(
                            await (await vehicleInOutTask).Content.ReadAsStringAsync());
                            var vehicleUtilizationData = JsonConvert.DeserializeObject<StandardAPIResponse<CapacityUtilization>>(
                                await (await vehicleOccTask).Content.ReadAsStringAsync());

                            performanceComparisonTable.VehicleCount = (int)(vehicleData?.Data?.TotalInCount ?? 0);
                            performanceComparisonTable.VehicleOccupancy = vehicleUtilizationData?.Data?.Percentage ?? 0;
                            performanceComparisonTable.VehicleUtilization = vehicleUtilizationData?.Data?.Utilization ?? 0;
                        }

                        performanceComparisonTable.SiteZoneName = zoneData.ZoneName;

                        return performanceComparisonTable;
                    });
                    var zoneResults = await Task.WhenAll(zoneTasks);
                    performanceComparisonEntries.AddRange(zoneResults.Where(r => r != null));
                    strZoneName += string.Join(",", zoneResults.Where(r => r != null).Select(r => r.SiteZoneName));

                    var totalPeopleCount = performanceComparisonEntries.Sum(x => x.PeopleCount);
                    var avgPeopleOccupancy = performanceComparisonEntries.Any() ? (int)Math.Round(performanceComparisonEntries.Average(x => x.PeopleOccupancy)) : 0;

                    var totalVehicleCount = performanceComparisonEntries.Sum(x => x.VehicleCount);
                    var avgVehicleOccupancy = performanceComparisonEntries.Any() ? (int)Math.Round(performanceComparisonEntries.Average(x => x.VehicleOccupancy)) : 0;

                    var keyPerformanceMetrics = new KeyPerformanceMetrics
                    {
                        TotalPeopleCount = totalPeopleCount,
                        AveragePeopleOccupancyRate = avgPeopleOccupancy,
                        TotalVehicleCount = totalVehicleCount,
                        AverageVehicleOccupancyRate = avgVehicleOccupancy
                    };

                    var floorNames = await Task.WhenAll(reportData.ZoneReport.FloorIds.Select(async floorId =>
                    {
                        var floorData = new GetFloorDto();

                        var response = await httpClient.GetAsync($"{apiBaseUrl}/api/Floor/GetFloorByFloorId/{floorId}");

                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<StandardAPIResponse<GetFloorDto>>(json);

                            if (result?.Data != null)
                            {
                                floorData = result?.Data;
                            }
                            else
                            {
                                // Handle case where data is null
                            }
                        }

                        return floorData?.FloorPlanName ?? "Unknown";
                    }));

                    var reportHeader = new ReportHeader
                    {
                        ReportType = reportData.ReportType,
                        ReportName = reportData.ZoneReport.ReportName,
                        Sites = siteData.SiteName,
                        Floors = string.Join(", ", floorNames),
                        Zones = strZoneName.TrimEnd(',', ' '),
                        ReportStartDate = reportData.ZoneReport.StartDate,
                        ReportEndDate = reportData.ZoneReport.EndDate,
                    };


                    if (performanceComparisonEntries.Any())
                    {
                        var averageEntry = new PerformanceComparisonTable
                        {
                            SiteZoneName = "Average",
                            PeopleCount = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleCount)),
                            PeopleOccupancy = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleOccupancy)),
                            VehicleCount = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleCount)),
                            VehicleOccupancy = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleOccupancy)),
                            PeopleUtilization = (int)Math.Round(performanceComparisonEntries.Average(e => e.PeopleUtilization)),
                            VehicleUtilization = (int)Math.Round(performanceComparisonEntries.Average(e => e.VehicleUtilization))
                        };

                        performanceComparisonEntries.Add(averageEntry);
                    }

                    reportDto.PerformanceComparisonTable = performanceComparisonEntries;
                    reportDto.ReportHeader = reportHeader;
                    reportDto.KeyPerformanceMetrics = keyPerformanceMetrics;
                    reportDto.CreatedBy = userData.Firstname + " " + userData.Firstname;
                    reportDto.ComperisionType = reportData.ComperisionType;
                    if (reportData.CreatedOn.HasValue)
                    {
                        reportDto.CreatedOn = reportData.CreatedOn;
                    }
                }
            }

            return reportDto;
        }
        public async Task<string> ReplaceReportPlaceholders(string htmlTemplate, ReportDto reportData, PdfDataRequest pdfDataRequest, string userId)
        {
            var data = await _usersRepository.GetAsync(userId);
            var timeFormat = data?.UserPreferences?.TimeFormat;

            var timeZoneData = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZoneData.UtcOffset);

            var language = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            var culture = language.ToString().Split(',')[0].Trim();

            var cultureInfo = new CultureInfo(culture);
            var dateTimeFormat = timeFormat != null && timeFormat == "24h"
                ? "dd MMM yyyy HH:mm"   // 24-hour
                : "dd MMM yyyy hh:mm tt"; // 12-hour

            var reportComperisionType = reportData.ComperisionType;
            var floorZoneHtml = GenerateFloorZoneHtml(reportData.ReportHeader.Floors, reportData.ReportHeader.Zones);
            var keyPerformanceMatricsHtml = GenerateKeyPerformanceMatricsHtml(reportData.KeyPerformanceMetrics, reportComperisionType);
            var filteredTableData = reportData.PerformanceComparisonTable
            .Where(item => !string.Equals(item.SiteZoneName, "Average", StringComparison.OrdinalIgnoreCase))
            .ToList();
            var tableHeaderHtml = GenerateTableHeaderHtml(reportComperisionType);
            var tableRowHtml = GenerateTableRowsHtml(reportData.PerformanceComparisonTable, reportComperisionType);
            var summaryInsightsHtml = GenerateSummaryInsightsHtml(filteredTableData, reportComperisionType);
            int index = 1;
            foreach (var svgData in pdfDataRequest.SVGData)
            {
                var placeholder = $"{{{{SVG_WIDGET{index}}}}}";
                htmlTemplate = htmlTemplate.Replace(placeholder, svgData.SVGData);
                index++;
            }

            return htmlTemplate
                .Replace("{{CreatedBy}}", reportData.CreatedBy)
                .Replace("{{CreatedOn}}", offsetTimeStamp != TimeSpan.Zero ? reportData.CreatedOn.Value.Add(offsetTimeStamp).ToString(dateTimeFormat, cultureInfo) : reportData.CreatedOn.Value.ToString(dateTimeFormat, cultureInfo))
                .Replace("{{ReportType}}", reportData.ReportHeader.ReportType)
                .Replace("{{ReportName}}", reportData.ReportHeader.ReportName)
                .Replace("{{Sites}}", reportData.ReportHeader.Sites)
                .Replace("{{ReportFloorZone}}", reportData.ReportHeader.ReportType != "site report" ? floorZoneHtml : "")
                .Replace("{{ReportDateTimeRange}}", offsetTimeStamp != TimeSpan.Zero ?
                    $"{reportData.ReportHeader.ReportStartDate.Value.Add(offsetTimeStamp).ToString(dateTimeFormat, cultureInfo)}-{reportData.ReportHeader.ReportEndDate.Value.Add(offsetTimeStamp).ToString(dateTimeFormat, cultureInfo)}" :
                    $"{reportData.ReportHeader.ReportStartDate.Value.ToString(dateTimeFormat, cultureInfo)}-{reportData.ReportHeader.ReportEndDate.Value.ToString(dateTimeFormat, cultureInfo)}")
                .Replace("{{tableHeaderHtml}}", tableHeaderHtml)
                .Replace("{{KeyPerformanceMetrics}}", keyPerformanceMatricsHtml)
                .Replace("{{SiteZoneTableHeader}}", reportData.ReportHeader.ReportType == "site report" ? "Site" : "Zone")
                .Replace("{{PerformanceComparisonRows}}", tableRowHtml)
                .Replace("{{SummaryInsights}}", summaryInsightsHtml)
                .Replace("{{WIDGET1_TITLE}}", GetChartTitle(reportData.ReportHeader.ReportType, reportComperisionType))
                .Replace("{{FilterApplied}}", _localizer[MessageKeys.FilterApplied])
                .Replace("{{CreatedByLabel}}", _localizer[MessageKeys.CreatedBy])
                .Replace("{{CreatedOnLabel}}", _localizer[MessageKeys.CreatedOn])
                .Replace("{{ReportTypeLabel}}", _localizer[MessageKeys.ReportType])
                .Replace("{{ReportNameLabel}}", _localizer[MessageKeys.ReportName])
                .Replace("{{SitesLabel}}", _localizer[MessageKeys.Sites])
                .Replace("{{DateTime}}", _localizer[MessageKeys.DateTimeLabel])
                .Replace("{{KeyPerformanceMetric}}", _localizer[MessageKeys.KeyPerformanceMetric])
                .Replace("{{PerformanceComparisonTable}}", _localizer[MessageKeys.PerformanceComparisonTable])
                .Replace("{{SummaryInsightsLabel}}", _localizer[MessageKeys.SummaryInsights])
                .Replace("{{OccupancyRateComparison}}", _localizer[MessageKeys.OccupancyRateComparison])
                .Replace("{{TrafficCompositionPeopleVsVehicles}}", _localizer[MessageKeys.TrafficCompositionPeopleVsVehicles])
                .Replace("{{lang}}", culture)
                .Replace("{{direction}}", culture == "ar" ? "rtl" : "ltr")
                ;
        }
        public string GetChartTitle(string reportType, IEnumerable<string> comperisionType)
        {
            return (comperisionType == null || comperisionType.Count() == 0 || (comperisionType.Contains("people") && comperisionType.Contains("vehicle")))
                ? $"{_localizer[MessageKeys.PeopleAndVehicleCount]} {(reportType?.ToLower() == "site report" ? _localizer[MessageKeys.BySite] : _localizer[MessageKeys.ByZone])}"
                : (comperisionType.Count() == 1 && comperisionType.Contains("people"))
                    ? $"{_localizer[MessageKeys.PeopleCount]} {(reportType?.ToLower() == "site report" ? _localizer[MessageKeys.BySite] : _localizer[MessageKeys.BySite])}"
                    : (comperisionType.Count() == 1 && comperisionType.Contains("vehicle"))
                        ? $"{_localizer[MessageKeys.VehicleCount]} {(reportType?.ToLower() == "site report" ? _localizer[MessageKeys.BySite] : _localizer[MessageKeys.ByZone])}"
                        : $"{_localizer[MessageKeys.PeopleAndVehicleCount]} {(reportType?.ToLower() == "site report" ? _localizer[MessageKeys.BySite] : _localizer[MessageKeys.ByZone])}";
        }
        public string GenerateKeyPerformanceMatricsHtml(KeyPerformanceMetrics keyPerformanceMetrics, IEnumerable<string> reportComperisionType)
        {
            var sb = new System.Text.StringBuilder();
            var PeopleCount = $"<img src=\"{_base64Generator.ConvertImageToBase64String("wwwroot/Report/Images/PeopleCountReportIcon.svg")}\" alt=\"People Count\" />";
            var PeopleOccupancy = $"<img src=\"{_base64Generator.ConvertImageToBase64String("wwwroot/Report/Images/PeopleOccupancyReportIcon.svg")}\" alt=\"People Count\" />";
            var VehicleCount = $"<img src=\"{_base64Generator.ConvertImageToBase64String("wwwroot/Report/Images/VehicleCountReportIcon.svg")}\" alt=\"People Count\" />";
            var VehicleOccupancy = $"<img src=\"{_base64Generator.ConvertImageToBase64String("wwwroot/Report/Images/VehicleOccupancyReportIcon.svg")}\" alt=\"People Count\" />";
            var showPeople = reportComperisionType == null || reportComperisionType.Count() == 0 || reportComperisionType.Contains("people");
            var showVehicle = reportComperisionType == null || reportComperisionType.Count() == 0 || reportComperisionType.Contains("vehicle");
            sb.Append("<table>");
            sb.Append("<tr class=\"performance-wrapper\">");

            // Total People Count
            if (showPeople)
            {
                sb.Append("<td>");
                sb.Append("<div class=\"performance-metric-items-wrapper\">");
                sb.Append("<div class=\"performance-metric-image\">");
                sb.Append($"{PeopleCount}");
                sb.Append("</div>");
                sb.Append("<div class=\"performance-metric-details\">");
                sb.Append($"<p>{_localizer[MessageKeys.TotalPeopleCount]}</p>");
                sb.Append($"<strong>{keyPerformanceMetrics.TotalPeopleCount?.ToString("N0")}</strong>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append("<div class=\"performance-metric-items-wrapper\">");
                sb.Append("<div class=\"performance-metric-image\">");
                sb.Append($"{PeopleOccupancy}");
                sb.Append("</div>");
                sb.Append("<div class=\"performance-metric-details\">");
                sb.Append($"<p>{_localizer[MessageKeys.AveragePeopleOccupancyRate]}</p>");
                sb.Append($"<strong>{keyPerformanceMetrics.AveragePeopleOccupancyRate.ToString()}%</strong>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</td>");
            }


            // Total Vehicle Count
            if (showVehicle)
            {
                sb.Append("<td>");
                sb.Append("<div class=\"performance-metric-items-wrapper\">");
                sb.Append("<div class=\"performance-metric-image\">");
                sb.Append($"{VehicleCount}");
                sb.Append("</div>");
                sb.Append("<div class=\"performance-metric-details\">");
                sb.Append($"<p>{_localizer[MessageKeys.TotalVehicleCount]}</p>");
                sb.Append($"<strong>{keyPerformanceMetrics.TotalVehicleCount?.ToString("N0")}</strong>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</td>");

                // Average Vehicle Occupancy Rate
                sb.Append("<td>");
                sb.Append("<div class=\"performance-metric-items-wrapper\">");
                sb.Append("<div class=\"performance-metric-image\">");
                sb.Append($"{VehicleOccupancy}");
                sb.Append("</div>");
                sb.Append("<div class=\"performance-metric-details\">");
                sb.Append($"<p>{_localizer[MessageKeys.AverageVehicleOccupancyRate]}</p>");
                sb.Append($"<strong>{keyPerformanceMetrics.AverageVehicleOccupancyRate.ToString()}%</strong>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</td>");
            }


            sb.Append("</tr>");
            sb.Append("</table>");


            return sb.ToString();
        }

        public string GenerateTableHeaderHtml(IEnumerable<string> reportComperisionType)
        {
            var sb = new System.Text.StringBuilder();

            var showPeople = reportComperisionType == null || reportComperisionType.Count() == 0 || reportComperisionType.Contains("people");
            var showVehicle = reportComperisionType == null || reportComperisionType.Count() == 0 || reportComperisionType.Contains("vehicle");
            if (showPeople)
            {
                sb.Append($"<th scope=\"col\">{_localizer[MessageKeys.PeopleCount]}</th>");
                sb.Append($"<th scope=\"col\">{_localizer[MessageKeys.PeopleOccupancy]}</th>");
            }
            if (showVehicle)
            {
                sb.Append($"<th scope=\"col\">{_localizer[MessageKeys.VehicleCount]}</th>");
                sb.Append($"<th scope=\"col\">{_localizer[MessageKeys.VehicleOccupancy]}</th>");
            }
            return sb.ToString();
        }
        public string GenerateTableRowsHtml(List<PerformanceComparisonTable> tableData, IEnumerable<string> reportComperisionType)
        {
            var sb = new System.Text.StringBuilder();
            var averageData = tableData.Where((item) => item.SiteZoneName == "Average").FirstOrDefault();

            for (int i = 0; i < tableData.Count; i++)
            {
                var item = tableData[i];
                string rowStyle = (i == tableData.Count - 1) ? " style=\"background-color: #eaf2fd !important;\"" : "";
                sb.Append($"<tr {rowStyle}>");
                sb.Append($"<td>{item.SiteZoneName}</td>");
                string peopleCountClass = (item.PeopleCount < averageData?.PeopleCount) ? "text-red" : "text-green";
                string vehicleCountClass = (item.VehicleCount < averageData?.VehicleCount) ? "text-red" : "text-green";
                if (reportComperisionType == null || reportComperisionType.Contains("people"))
                {
                    sb.Append($"<td class=\"{peopleCountClass}\">{item.PeopleCount:N0}</td>");
                    sb.Append($"<td>{item.PeopleUtilization:N2} ({item.PeopleOccupancy:N2}%)</td>");
                }
                if (reportComperisionType == null || reportComperisionType.Contains("vehicle"))
                {
                    sb.Append($"<td class=\"{vehicleCountClass}\">{item.VehicleCount:N0}</td>");
                    sb.Append($"<td>{item.VehicleUtilization:N2} ({item.VehicleOccupancy:N2}%)</td>");
                }
                sb.Append("</tr>");
            }
            return sb.ToString();
        }
        public string GenerateSummaryInsightsHtml(
            List<PerformanceComparisonTable> data,
            IEnumerable<string> reportComperisionType)
        {
            if (data == null || data.Count == 0)
                return $"<ul><li>{_localizer[MessageKeys.NoDataAvailableForInsights]}.</li></ul>";

            var types = reportComperisionType?.ToList() ?? new List<string> { "vehicle", "people" };

            // -----------------------------
            // Step 1: Filter valid rows
            // -----------------------------
            var filteredData = data.Where(site =>
            {
                // must have zone name and not "average"
                if (string.IsNullOrWhiteSpace(site.SiteZoneName) ||
                    site.SiteZoneName.Equals("average", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // values (safe)
                var peopleCount = site.PeopleCount;
                var peopleOcc = site.PeopleOccupancy;
                var vehicleCount = site.VehicleCount;
                var vehicleOcc = site.VehicleOccupancy;

                // ignore row if all 4 are 0
                bool allZero =
                    peopleCount <= 0 &&
                    peopleOcc <= 0 &&
                    vehicleCount <= 0 &&
                    vehicleOcc <= 0;

                if (allZero) return false;

                // category checks
                bool checkVehicle =
                    types.Contains("vehicle") &&
                    vehicleCount > 0 &&
                    vehicleOcc > 0;

                bool checkPeople =
                    types.Contains("people") &&
                    peopleCount > 0 &&
                    peopleOcc > 0;

                // If both selected → allow row if either is valid
                if (types.Contains("vehicle") && types.Contains("people"))
                {
                    return checkVehicle || checkPeople;
                }

                // If only vehicle selected
                if (types.Contains("vehicle"))
                {
                    return checkVehicle;
                }

                // If only people selected
                if (types.Contains("people"))
                {
                    return checkPeople;
                }

                return false;

            }).ToList();

            if (filteredData.Count == 0)
                return $"<ul><li>{_localizer[MessageKeys.NoValidDataAvailableForInsights]}</li></ul>";

            var insights = new List<string>();

            // =========================================================
            // VEHICLE INSIGHTS (ignore 0 values in comparison)
            // =========================================================
            if (types.Contains("vehicle"))
            {
                var vehicleOccData = filteredData.Where(s => s.VehicleOccupancy > 0).ToList();
                var vehicleCountData = filteredData.Where(s => s.VehicleCount > 0).ToList();

                // Highest Vehicle Occupancy
                if (vehicleOccData.Any())
                {
                    var maxVehicleOccupancySite = vehicleOccData
                        .OrderByDescending(s => s.VehicleOccupancy)
                        .First();

                    insights.Add(
                        $"{maxVehicleOccupancySite.SiteZoneName} {_localizer[MessageKeys.HasHighestVehicleOccupancyRate]} ({maxVehicleOccupancySite.VehicleOccupancy:N2}%)."
                    );

                    // Avg comparison (only >0 values)
                    var avgVehicleOccupancy = vehicleOccData.Average(s => s.VehicleOccupancy);

                    var busierSites = vehicleOccData
                        .Where(s => s.VehicleOccupancy > avgVehicleOccupancy)
                        .Select(s => s.SiteZoneName)
                        .ToList();

                    var lessBusySites = vehicleOccData
                        .Where(s => s.VehicleOccupancy <= avgVehicleOccupancy)
                        .Select(s => s.SiteZoneName)
                        .ToList();

                    if (busierSites.Any() && lessBusySites.Any())
                    {
                        insights.Add(
                            $"{_localizer[MessageKeys.VehicleOccupancyLevelsSuggest]} {string.Join(", ", busierSites)} {_localizer[MessageKeys.AreBusierThan]} {string.Join(", ", lessBusySites)}."
                        );
                    }
                }

                // Highest Vehicle Count
                if (vehicleCountData.Any())
                {
                    var maxVehicleCountSite = vehicleCountData
                        .OrderByDescending(s => s.VehicleCount)
                        .First();

                    insights.Add(
                        $"{maxVehicleCountSite.SiteZoneName} {_localizer[MessageKeys.HasHighestVehicleCount]} ({maxVehicleCountSite.VehicleCount:N0})."
                    );
                }
            }

            // =========================================================
            // PEOPLE INSIGHTS (ignore 0 values in comparison)
            // =========================================================
            if (types.Contains("people"))
            {
                var peopleOccData = filteredData.Where(s => s.PeopleOccupancy > 0).ToList();
                var peopleCountData = filteredData.Where(s => s.PeopleCount > 0).ToList();

                // Highest People Occupancy
                if (peopleOccData.Any())
                {
                    var maxPeopleOccupancySite = peopleOccData
                        .OrderByDescending(s => s.PeopleOccupancy)
                        .First();

                    insights.Add(
                        $"{maxPeopleOccupancySite.SiteZoneName} {_localizer[MessageKeys.HasHighestPeopleOccupancyRate]} ({maxPeopleOccupancySite.PeopleOccupancy:N2}%)."
                    );

                    // Avg comparison (only >0 values)
                    var avgPeopleOccupancy = peopleOccData.Average(s => s.PeopleOccupancy);

                    var busierSites = peopleOccData
                        .Where(s => s.PeopleOccupancy > avgPeopleOccupancy)
                        .Select(s => s.SiteZoneName)
                        .ToList();

                    var lessBusySites = peopleOccData
                        .Where(s => s.PeopleOccupancy <= avgPeopleOccupancy)
                        .Select(s => s.SiteZoneName)
                        .ToList();

                    if (busierSites.Any() && lessBusySites.Any())
                    {
                        insights.Add(
                            $"{_localizer[MessageKeys.PeopleOccupancyLevelsSuggest]} {string.Join(", ", busierSites)} {_localizer[MessageKeys.AreBusierThan]} {string.Join(", ", lessBusySites)}."
                        );
                    }
                }

                // Highest People Count
                if (peopleCountData.Any())
                {
                    var maxPeopleCountSite = peopleCountData
                        .OrderByDescending(s => s.PeopleCount)
                        .First();

                    insights.Add(
                        $"{maxPeopleCountSite.SiteZoneName} {_localizer[MessageKeys.HasHighestPeopleCount]} ({maxPeopleCountSite.PeopleCount:N0})."
                    );
                }
            }

            // =========================================================
            // COMBINED INSIGHTS (vehicle + people) ignore 0
            // =========================================================
            if (types.Contains("vehicle") && types.Contains("people"))
            {
                var vehicleOccData = filteredData.Where(s => s.VehicleOccupancy > 0).ToList();
                var peopleOccData = filteredData.Where(s => s.PeopleOccupancy > 0).ToList();

                if (vehicleOccData.Any() && peopleOccData.Any())
                {
                    var maxVehicleOccupancySite = vehicleOccData
                        .OrderByDescending(s => s.VehicleOccupancy)
                        .First();

                    var maxPeopleOccupancySite = peopleOccData
                        .OrderByDescending(s => s.PeopleOccupancy)
                        .First();

                    if (maxVehicleOccupancySite.SiteZoneName == maxPeopleOccupancySite.SiteZoneName)
                    {
                        insights.Add(
                            $"{maxPeopleOccupancySite.SiteZoneName} {_localizer[MessageKeys.HasHighestPeopleOccupancyRate]} ({maxPeopleOccupancySite.PeopleOccupancy:N2}%) {_localizer[MessageKeys.AndVehicleOccupancyRate]} ({maxVehicleOccupancySite.VehicleOccupancy:N2}%)."
                        );
                    }
                }
            }

            if (!insights.Any())
            {
                insights.Add($"{_localizer[MessageKeys.NoValidInsightsBasedOnSelection]}");
            }

            // -----------------------------
            // Build HTML output
            // -----------------------------
            var html = new System.Text.StringBuilder();
            html.Append("<ul style='list-style-type: disc; padding-left: 20px;'>");

            foreach (var insight in insights)
            {
                html.Append($"<li style='color: orangered;'>{insight}</li>");
            }

            html.Append("</ul>");

            return html.ToString();

            //if (data == null || data.Count == 0)
            //    return "<ul><li>No data available to generate insights.</li></ul>";

            //var insights = new List<string>();

            //var maxVehicleOccupancySite = data.OrderByDescending(s => s.VehicleOccupancy).First();
            //var maxPeopleOccupancySite = data.OrderByDescending(s => s.PeopleOccupancy).First();
            //var maxPeopleCountSite = data.OrderByDescending(s => s.PeopleCount).First();

            //if (maxVehicleOccupancySite.SiteZoneName == maxPeopleOccupancySite.SiteZoneName &&
            //    maxVehicleOccupancySite.VehicleOccupancy == maxPeopleOccupancySite.PeopleOccupancy)
            //{
            //    insights.Add($"{maxPeopleOccupancySite.SiteZoneName} has the highest people occupancy rate ({maxPeopleOccupancySite.PeopleOccupancy:N2}%) and vehicle occupancy rate.");
            //}
            //else if (maxVehicleOccupancySite.SiteZoneName == maxPeopleOccupancySite.SiteZoneName)
            //{
            //    insights.Add($"{maxPeopleOccupancySite.SiteZoneName} has the highest people occupancy rate ({maxPeopleOccupancySite.PeopleOccupancy:N2}%) and vehicle occupancy rate ({maxVehicleOccupancySite.VehicleOccupancy:N2}%).");
            //}
            //else
            //{
            //    insights.Add($"{maxVehicleOccupancySite.SiteZoneName} has the highest vehicle occupancy rate ({maxVehicleOccupancySite.VehicleOccupancy:N2}%).");
            //    insights.Add($"{maxPeopleOccupancySite.SiteZoneName} has the highest people occupancy.");
            //}

            //insights.Add($"{maxPeopleCountSite.SiteZoneName} has the highest number of people counted.");

            //var avgOccupancy = data.Average(s => s.VehicleOccupancy);
            //var busierSites = data.Where(s => s.VehicleOccupancy > avgOccupancy).Select(s => s.SiteZoneName).ToList();
            //var lessBusySites = data.Where(s => s.VehicleOccupancy <= avgOccupancy).Select(s => s.SiteZoneName).ToList();

            //if (busierSites.Any() && lessBusySites.Any())
            //{
            //    insights.Add($"Occupancy levels suggest {string.Join(" and ", busierSites)} are busier than {string.Join(" and ", lessBusySites)}.");
            //}

            //// Build HTML string with custom colors (orangered for first 3 items)
            //var html = new System.Text.StringBuilder();
            //html.Append("<ul style='list-style-type: disc; padding-left: 20px;'>");

            //for (int i = 0; i < insights.Count; i++)
            //{

            //    html.Append($"<li class='colored-bullet'><span style='color: inherit;' > {insights[i]}</span></li>");

            //}

            //html.Append("</ul>");

            //return html.ToString();
        }
        public string GenerateFloorZoneHtml(string floors, string zones)
        {
            var sb = new System.Text.StringBuilder();

            sb.Append($"<div class=\"report-type-box\"><p>Floors</p><span>{floors}</span></div>");
            sb.Append($"<div class=\"report-type-box\"><p>Zones</p><span>{zones}</span></div>");

            return sb.ToString();
        }
        public async Task<IEnumerable<FloorZoneByPermissionDto>> GetAllZoneBySiteAndFloorId(string siteId, IEnumerable<string> floorIds)
        {
            // Get site data from repository
            var siteData = await _siteRepository.GetAsync(siteId);

            if (siteData == null)
            {
                throw new KeyNotFoundException($"Site with ID {siteId} not found");
            }

            // Extract connection parameters (adjust property names as needed)
            var apiBaseUrl = siteData.HostingAddress;
            var username = siteData.Username;
            var password = siteData.Password;

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            // 1. First call the login API to get the token
            var loginResponse = await GetAuthToken(httpClient, username, password);

            if (string.IsNullOrEmpty(loginResponse?.Data.AccessToken))
            {
                throw new UnauthorizedAccessException("Failed to authenticate with the API");
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

            var widgetJson = JsonConvert.SerializeObject(floorIds);
            var widgetContent = new StringContent(widgetJson, Encoding.UTF8, "application/json");

            // People In/Out Total
            var floorZoneResponse = await httpClient.PostAsync($"{apiBaseUrl}/api/Floor/FloorZoneByPermission", widgetContent);
            var floorZoneResponseContent = await floorZoneResponse.Content.ReadAsStringAsync();
            var floorZoneData = JsonConvert.DeserializeObject<StandardAPIResponse<IEnumerable<FloorZoneByPermissionDto>>>(floorZoneResponseContent);

            return floorZoneData == null ? Enumerable.Empty<FloorZoneByPermissionDto>() : floorZoneData.Data;

        }

        public async Task<StandardAPIResponse<TokenResponseModel>> GetAuthToken(HttpClient httpClient, string username, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json");

                var loginResponse = await httpClient.PostAsync("api/Auth/login", content);
                loginResponse.EnsureSuccessStatusCode();

                var jsonString = await loginResponse.Content.ReadAsStringAsync();
                var loginResult = JsonConvert.DeserializeObject<StandardAPIResponse<TokenResponseModel>>(jsonString);
                return loginResult;
            }
            catch (Exception ex)
            {
                // Log error if needed
                // _logger.LogError(ex, "Error during authentication");
                throw new UnauthorizedAccessException("Authentication failed", ex);
            }
        }


    }

}
