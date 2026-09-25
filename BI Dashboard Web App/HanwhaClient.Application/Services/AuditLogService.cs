using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Vml.Office;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static HanwhaClient.Application.Services.AuditLogService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HanwhaClient.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUsersRepository _userMasterRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IScreenMasterRepository _screenMasterRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IClientTimeZoneRepository _clientTimeZoneRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;

        public AuditLogService(IAuditLogRepository auditLogRepository, IUsersRepository userMasterRepository, IRoleRepository roleRepository
            , IScreenMasterRepository screenMasterRepository, IDeviceMasterRepository deviceMasterRepository, IVehicleOwnerRepository vehicleOwnerRepository, ICountryRepository countryRepository
            , IClientTimeZoneRepository clientTimeZoneRepository,
            ISiteRepository siteRepository, IUsersService usersService, IDateConvert dateConvert)
        {
            this._auditLogRepository = auditLogRepository;
            _userMasterRepository = userMasterRepository;
            _roleRepository = roleRepository;
            _screenMasterRepository = screenMasterRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _vehicleOwnerRepository = vehicleOwnerRepository;
            _countryRepository = countryRepository;
            _clientTimeZoneRepository = clientTimeZoneRepository;
            _siteRepository = siteRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
        }

        public async Task<(AuditLogResponse auditLogDetail, Dictionary<string, object> referenceData)> GetAuditLogDetail(AuditLogRequest auditLogRequest)
        {
            var data = await _auditLogRepository.GetAuditLogDetailByFilter(auditLogRequest);
            AuditLogResponse auditLogResponse = new AuditLogResponse();
            auditLogResponse.AuditLogDetails = data.auditLog.Select(x => new AuditLogDetail
            {
                Id = x.Id,
                CollectionName = x.CollectionName,
                OperationType = x.OperationType,
                RemovedFields = x.RemovedFields,
                DocumentKey = x.DocumentKey != null ? x.DocumentKey["_id"].ToString() : "",
                OperationData = x.FullDocument != null ? x.FullDocument.ToJson() : x.UpdateDescription?.ToJson(),
            }
            ).ToList();
            auditLogResponse.TotalCount = data.totalCount;

            List<BsonDocument> OperationData = data.auditLog.Select(x => x.FullDocument != null ? x.FullDocument : x.UpdateDescription).ToList();
            Dictionary<string, object> referenceData = new();
            if (auditLogRequest.CollectioName == AppDBConstants.UserMaster)
            {
                var createdByIds = OperationData.Where(x => x != null && x.Contains("createdBy") && !x["createdBy"].IsBsonNull).Select(x => x["createdBy"].ToString()).ToList();
                var updatedByIds = OperationData.Where(x => x != null && x.Contains("updatedBy") && !x["updatedBy"].IsBsonNull).Select(x => x["updatedBy"].ToString()).ToList();
                var roleIds = OperationData.Where(x => x != null && x.Contains("roleIds") && !x["roleIds"].IsBsonNull).SelectMany(x => x["roleIds"].AsBsonArray.Select(r => r.ToString())).ToList();
                referenceData.Add("createdBy", await GetUserMasterReferenceData(createdByIds));
                referenceData.Add("updatedBy", await GetUserMasterReferenceData(updatedByIds));
                referenceData.Add("roleIds", await GetRoleMasterReferenceData(roleIds));
            }
            else if (auditLogRequest.CollectioName == AppDBConstants.RoleMaster)
            {
                var createdByIds = OperationData.Where(x => x != null && x.Contains("createdBy") && !x["createdBy"].IsBsonNull).Select(x => x["createdBy"].ToString()).ToList();
                var updatedByIds = OperationData.Where(x => x != null && x.Contains("updatedBy") && !x["updatedBy"].IsBsonNull).Select(x => x["updatedBy"].ToString()).ToList();
                referenceData.Add("createdBy", await GetUserMasterReferenceData(createdByIds));
                referenceData.Add("updatedBy", await GetUserMasterReferenceData(updatedByIds));
            }
            else if (auditLogRequest.CollectioName == AppDBConstants.RoleScreenMapping)
            {
                var createdByIds = OperationData.Where(x => x != null && x.Contains("createdBy") && !x["createdBy"].IsBsonNull).Select(x => x["createdBy"].ToString()).ToList();
                var updatedByIds = OperationData.Where(x => x != null && x.Contains("updatedBy") && !x["updatedBy"].IsBsonNull).Select(x => x["updatedBy"].ToString()).ToList();
                var screenIds = OperationData.Where(x => x != null && x.Contains("screenMappings") && !x["screenMappings"].IsBsonNull).SelectMany(x => x["screenMappings"].AsBsonArray.Select(y => y["screenId"].ToString())).ToList();
                referenceData.Add("createdBy", await GetUserMasterReferenceData(createdByIds));
                referenceData.Add("updatedBy", await GetUserMasterReferenceData(updatedByIds));
                referenceData.Add("screenIds", await GetScrennMasterReferenceData(screenIds));
            }
            return (auditLogResponse, referenceData);
        }

        public static List<string> ignoreKeys = new List<string>
                                                {
                                                     "createdBy",
                                                     "updatedBy",
                                                     "createdOn",
                                                     "updatedOn",
                                                     "_id",
                                                     "isDeleted",
                                                     "deletedOn",
                                                     "password",
                                                     "dataAccessPermission",
                                                     "profileImage",
                                                     "isPasswordReset",
                                                     "series",
                                                     "vehicleNumber",
                                                };

        public async Task<List<string>> GetAuditLogCollectionName()
        {
            return await _auditLogRepository.GetAuditLogCollectionName();
        }

        private async Task<List<OptionModel<string, string>>> GetUserMasterReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<UserMaster> projection = Builders<UserMaster>.Projection
            .Include("username")
            .Include("_id");

            var users = await _userMasterRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.Username)).ToList();
            return options;
        }
        private async Task<List<OptionModel<string, string>>> GetRoleMasterReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<RoleMaster> projection = Builders<RoleMaster>.Projection
            .Include("_id")
            .Include("roleName");
            var roles = await _roleRepository.GetManyAsync(ids);
            options = roles.Select(x => new OptionModel<string, string>(x.Id, x.RoleName)).ToList();
            return options;
        }
        private async Task<List<OptionModel<string, string>>> GetScrennMasterReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<ScreenMaster> projection = Builders<ScreenMaster>.Projection
            .Include("screen_name")
            .Include("_id");

            var users = await _screenMasterRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.ScreenName)).ToList();
            return options;
        }

        public async Task<(AuditLogsResponse auditLogsDetail, Dictionary<string, object> referenceData)> GetAuditLogsDetail(AuditLogsRequest auditLogRequest)
        {
            Dictionary<string, object> referenceData = new();
            var data = await _auditLogRepository.GetAuditLogsDetailByFilter(auditLogRequest);
            AuditLogsResponse auditLogsResponse = new AuditLogsResponse();
            auditLogsResponse.AuditLogsDetails = data.auditLog.Select(x => new AuditLogsDetail
            {
                Id = x.Id,
                CollectionName = x.CollectionName,
                OperationType = x.OperationType,
                DocumentKey = x.DocumentKey != null ? x.DocumentKey["_id"].ToString() : "",
                OperationData = x.OperationType == "Update"
                                ? (x.UpdateDescription != null ? x.UpdateDescription.ToJson() : null)
                                : (x.FullDocument != null ? x.FullDocument.ToJson() : null),
                DocumentBeforeChange = x.OperationType == "Update" && x.DocumentBeforeChange != null ? x.DocumentBeforeChange.ToJson() : null,
                CreatedBy = x.CreatedBy,
                CreatedOn = x.CreatedOn
            }
            ).ToList();
            var userIds = auditLogsResponse.AuditLogsDetails
                .Where(x => x != null && !string.IsNullOrEmpty(x.CreatedBy))
                .Select(x => x.CreatedBy.ToString())
                .Distinct()
                .ToList();

            referenceData.Add("createdBy", await GetUserMasterReferenceData(userIds));
            auditLogsResponse.TotalCount = data.totalCount;

            List<BsonDocument> OperationData = data.auditLog.Select(x => x.FullDocument != null ? x.FullDocument : x.UpdateDescription).ToList();
            OperationData.AddRange(data.auditLog.Select(x => x.DocumentBeforeChange).ToList());

            if (OperationData != null)
            {

                if (auditLogRequest.CollectionName == AppDBConstants.UserMaster)
                {
                    var roleIds = OperationData.Where(x => x != null && x.Contains("roleIds") && !x["roleIds"].IsBsonNull).SelectMany(x => x["roleIds"].AsBsonArray.Select(r => r.ToString())).Distinct().ToList();
                    var timezoneIds = OperationData.Where(x => x != null && x.Contains("userPreferences") && x["userPreferences"].IsBsonDocument && x["userPreferences"].AsBsonDocument.Contains("timezoneId") && !x["userPreferences"]["timezoneId"].IsBsonNull)
                                        .Select(x => x["userPreferences"]["timezoneId"].ToString())
                                        .Distinct()
                                        .ToList();

                    if (timezoneIds.Any())
                    {
                        referenceData.Add("timezoneId", await GetTimezoneMasterReferenceData(timezoneIds));
                    }
                    if (roleIds.Any())
                    {
                        referenceData.Add("roleIds", await GetRoleMasterReferenceData(roleIds));
                    }
                }
                else if (auditLogRequest.CollectionName == AppDBConstants.VehicleOwner)
                {
                    var deviceIds = OperationData.Where(x => x != null && x.Contains("allowedGates") && !x["allowedGates"].IsBsonNull).SelectMany(x => x["allowedGates"].AsBsonArray.Select(r => r.ToString())).Distinct().ToList();
                    if (deviceIds.Any())
                    {
                        referenceData.Add("deviceIds", await GetDeviceMasterReferenceData(deviceIds));
                    }
                }
                else if (auditLogRequest.CollectionName == AppDBConstants.ANPRVehicle)
                {
                    var ownerIds = OperationData.Where(x => x != null && x.Contains("vehicleOwnerId") && !x["vehicleOwnerId"].IsBsonNull).Select(x => x["vehicleOwnerId"].ToString()).Distinct().ToList();
                    var countryIds = OperationData.Where(x => x != null && x.Contains("country") && !x["country"].IsBsonNull).Select(x => x["country"].ToString()).Distinct().ToList();
                    if (ownerIds.Any())
                    {
                        referenceData.Add("ownerIds", await GetVehicleOwnerReferenceData(ownerIds));
                    }
                    if (countryIds.Any())
                    {
                        referenceData.Add("countryIds", await GetCountryReferenceData(countryIds));
                    }
                }
                else if (auditLogRequest.CollectionName == AppDBConstants.IDracmaster || auditLogRequest.CollectionName == AppDBConstants.SsmSiteMapping)
                {
                    var childSiteIds = OperationData.Where(x => x != null && x.Contains("childSiteId") && !x["childSiteId"].IsBsonNull).Select(x => x["childSiteId"].ToString()).Distinct().ToList();
                    var parentSiteIds = OperationData.Where(x => x != null && x.Contains("parentSiteId") && !x["parentSiteId"].IsBsonNull).Select(x => x["parentSiteId"].ToString()).Distinct().ToList();
                    if (childSiteIds.Any())
                    {
                        referenceData.Add("childSite", await GetChildSiteReferenceDataAsync(childSiteIds));
                    }
                    if (parentSiteIds.Any())
                    {
                        referenceData.Add("parentSite", await GetIDracMasterParentSiteReferenceDataAsync(parentSiteIds));
                    }
                }
            }
            return (auditLogsResponse, referenceData);
        }
        private async Task<List<OptionModel<string, string>>> GetDeviceMasterReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<DeviceMaster> projection = Builders<DeviceMaster>.Projection
            .Include("_id")
            .Include("deviceName");
            var devices = await _deviceMasterRepository.GetManyAsync(ids);
            options = devices.Select(x => new OptionModel<string, string>(x.Id, x.DeviceName)).ToList();
            return options;
        }
        private async Task<List<OptionModel<string, string>>> GetVehicleOwnerReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<VehicleOwner> projection = Builders<VehicleOwner>.Projection
            .Include("_id")
            .Include("ownerName");
            var devices = await _vehicleOwnerRepository.GetManyAsync(ids);
            options = devices.Select(x => new OptionModel<string, string>(x.Id, x.OwnerName)).ToList();
            return options;
        }
        private async Task<List<OptionModel<string, string>>> GetCountryReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<Country> projection = Builders<Country>.Projection
            .Include("_id")
            .Include("Name");
            var devices = await _countryRepository.GetManyAsync(ids);
            options = devices.Select(x => new OptionModel<string, string>(x.Id, x.Name)).ToList();
            return options;
        }
        private async Task<List<OptionModel<string, string>>> GetTimezoneMasterReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<ClientTimezones> projection = Builders<ClientTimezones>.Projection
            .Include("_id")
            .Include("timeZoneName");
            var devices = await _clientTimeZoneRepository.GetManyAsync(ids);
            options = devices.Select(x => new OptionModel<string, string>(x.Id, x.TimeZoneName)).ToList();
            return options;
        }
        public async Task<List<OptionModel<string, string>>> GetIDracMasterParentSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<SiteMaster> projection = Builders<SiteMaster>.Projection
            .Include("siteName")
            .Include("_id");
            var sites = await _siteRepository.GetManyAsync(ids, projection);
            options = sites.Select(x => new OptionModel<string, string>(x.Id, x.SiteName)).ToList();
            return options;
        }

        public async Task<List<OptionModel<string, string>>> GetChildSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var objectIds = ids.Select(id => (id)).ToList();

            var filter = Builders<SiteMaster>.Filter.ElemMatch(
                x => x.ChildSites,
                c => objectIds.Contains(c.Id)
            );

            var projection = Builders<SiteMaster>.Projection.Include("childSites");

            var sites = await _siteRepository.GetByFilterAsync(filter, projection);

            return sites
                .SelectMany(x => x.ChildSites ?? new List<ChildSite>())
                .Where(c => objectIds.Contains(c.Id))
                .Select(c => new OptionModel<string, string>(
                    c.Id.ToString(),
                    c.SiteName
                ))
                .DistinctBy(x => x.label)
                .ToList();
        }

        public async Task<StringBuilder> ExportAuditLogsCSVAsync(AuditLogsRequest auditLogRequest, string userId)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // ✅ Add CSV Header
            auditLogRequest.IsExport = true;
            auditLogRequest.PageNumber = auditLogRequest.PageNumber == 0 ? 1 : auditLogRequest.PageNumber;

            var auditLogData = await GetAuditLogsDetail(auditLogRequest);

            if (auditLogData.auditLogsDetail.AuditLogsDetails == null ||
                !auditLogData.auditLogsDetail.AuditLogsDetails.Any())
            {
                return stringBuilder;
            }

            stringBuilder.AppendLine($"\"Exported On: {DateTime.Now:MMM dd, yyyy hh:mm tt}\"");
            stringBuilder.AppendLine($"Screen Name: {GetTitleById(auditLogRequest.CollectionName)}");
            stringBuilder.AppendLine("");

            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            stringBuilder.AppendLine("Document Id,User,Type,Date,Details");

            List<string> data = new List<string>();
            try
            {
                foreach (var item in auditLogData.auditLogsDetail.AuditLogsDetails)
                {
                    var newData = ParseJson(item.OperationData);
                    var oldData = ParseJson(item.DocumentBeforeChange);

                    var userName = "";

                    var referenceData = auditLogData.referenceData;

                    if (item.OperationType.ToLower() == "delete")
                    {
                        continue;
                    }

                    else if (item.OperationType.ToLower() == "insert")
                    {
                        data = new List<string>();
                        var insertData = FlattenObject(newData);
                        var displayStrings = new List<string>();

                        foreach (var insertItem in insertData)
                        {
                            string key = insertItem.Key;
                            object value = insertItem.Value;
                            string cleanValue;

                            if (value is Array arrayValue)
                            {
                                var parts = new List<string>();

                                foreach (var v in arrayValue)
                                {
                                    if (v != null && v.GetType().IsClass && !(v is string))
                                    {
                                        // Check for special properties
                                        var idProp = v.GetType().GetProperty("_id");
                                        if (idProp != null)
                                        {
                                            parts.Add(FormatDisplayValue(key, idProp.GetValue(v), referenceData, ignoreKeys));
                                            continue;
                                        }

                                        var oidProp = v.GetType().GetProperty("$oid");
                                        if (oidProp != null)
                                        {
                                            parts.Add(FormatDisplayValue(key, oidProp.GetValue(v), referenceData, ignoreKeys));
                                            continue;
                                        }

                                        var eventSourceProp = v.GetType().GetProperty("eventSource");
                                        if (eventSourceProp != null)
                                        {
                                            parts.Add(eventSourceProp.GetValue(v).ToString());
                                            continue;
                                        }

                                        parts.Add(Newtonsoft.Json.JsonConvert.SerializeObject(v));
                                    }
                                    else
                                    {
                                        parts.Add(FormatDisplayValue(key, v, referenceData, ignoreKeys));
                                    }
                                }

                                cleanValue = string.Join(", ", parts);
                            }
                            else
                            {
                                cleanValue = FormatDisplayValue(key, value, referenceData, ignoreKeys);
                            }

                            if (key == "allowedFromTime.date" || key == "allowedToTime.date")
                            {

                                DateTime date = DateTime.ParseExact(cleanValue, "MMM dd, yyyy HH:mm", CultureInfo.InvariantCulture);

                                DateTime result = date.Add(offsetTimeStamp);

                                cleanValue = result.ToString("hh:mm tt");
                            }

                            data.Add($"{FormatNestedKey(key)}: inserted with \"{cleanValue}\"\n");
                        }
                        userName = GetUserName(item.CreatedBy, referenceData);

                    }

                    else if (item.OperationType.ToLower() == "update")
                    {
                        var obj = newData as JObject ?? new JObject();
                        data = new List<string>();

                        bool isDeleted = obj["isDeleted"]?.Value<bool>() == true;

                        if (isDeleted)
                        {
                            item.OperationType = "Delete";
                        }

                        var differences = GetDifferences(oldData, newData, "");

                        bool isOsSyncTimeZone = false;

                        // ✅ Get OS Sync flag
                        //var oldObj = oldData as JObject;
                        //if (oldObj != null &&
                        //    oldObj["userPreferences"]?["isOsSyncTimeZone"] != null)
                        //{
                        //    isOsSyncTimeZone =
                        //        oldObj["userPreferences"]["isOsSyncTimeZone"].Value<bool>();
                        //}

                        var oldObj = oldData as JObject;
                        var userPrefs = oldObj?["userPreferences"] as JObject;
                        isOsSyncTimeZone = userPrefs?["isOsSyncTimeZone"]?.Value<bool>() ?? false;


                        foreach (var diff in differences)
                        {
                            // Skip timezoneId when OS sync is ON
                            if (isOsSyncTimeZone &&
                                diff.Key == "userPreferences.timezoneId")
                                continue;

                            string cleanOld;
                            if (diff.OldValue is Array oldArray)
                            {
                                List<string> oldList = new List<string>();
                                foreach (var v in oldArray)
                                {
                                    oldList.Add(FormatDisplayValue(
                                        diff.Key, v, referenceData, ignoreKeys));
                                }
                                cleanOld = string.Join(", ", oldList);
                            }
                            else
                            {
                                cleanOld = FormatDisplayValue(
                                    diff.Key, diff.OldValue, referenceData, ignoreKeys);
                            }

                            string cleanNew;
                            if (diff.NewValue is Array newArray)
                            {
                                List<string> newList = new List<string>();
                                foreach (var v in newArray)
                                {
                                    newList.Add(FormatDisplayValue(
                                        diff.Key, v, referenceData, ignoreKeys));
                                }
                                cleanNew = string.Join(", ", newList);
                            }
                            else
                            {
                                cleanNew = FormatDisplayValue(
                                    diff.Key, diff.NewValue, referenceData, ignoreKeys);
                            }


                            if (cleanOld != "-" && (diff.Key == "allowedFromTime.date" || diff.Key == "allowedToTime.date"))
                            {

                                DateTime date = DateTime.ParseExact(cleanOld, "MMM dd, yyyy HH:mm", CultureInfo.InvariantCulture);

                                DateTime result = date.Add(offsetTimeStamp);

                                cleanOld = result.ToString("hh:mm tt");
                            }

                            if (cleanNew != "-" && (diff.Key == "allowedFromTime.date" || diff.Key == "allowedToTime.date"))
                            {

                                DateTime date = DateTime.ParseExact(cleanNew, "MMM dd, yyyy HH:mm", CultureInfo.InvariantCulture);

                                DateTime result = date.Add(offsetTimeStamp);

                                cleanNew = result.ToString("hh:mm tt");
                            }

                            data.Add($"{FormatNestedKey(diff.Key)}: changed from \"{cleanOld}\" to \"{cleanNew}\"\n");
                        }
                        userName = GetUserName(item.CreatedBy, referenceData);
                    }

                    // ✅ Convert details list into single string
                    string detailsText = string.Join("", data).TrimEnd(',');

                    // ✅ Escape CSV values
                    string Escape(string value)
                    {
                        if (string.IsNullOrEmpty(value)) return "";
                        return "\"" + value.Replace("\"", "\"\"") + "\"";
                    }

                    //var createdOnDate = FormatDate(item.CreatedOn, offsetTimeStamp);

                    string createdOnDate = item.CreatedOn.Value
                                       .Add(offsetTimeStamp)
                                       .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                    //return $"=\"{formatted}\"";

                    // ✅ Append row to CSV
                    stringBuilder.AppendLine(
                        $"{Escape(item.DocumentKey)}," +
                        $"{Escape(userName)}," +
                        $"{Escape(item.OperationType)}," +
                        $"{Escape(createdOnDate)}," +
                        $"{Escape(detailsText)}"
                    );
                }
            }
            catch (Exception ex)
            {
                var exMsg = ex.Message;
                throw;
            }

            return stringBuilder;
        }

        public static string FormatDate(DateTime? date, TimeSpan offset)
        {
            if (date == null)
                return "";

            string formatted = date.Value
                .Add(offset)
                .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            return EscapeText(formatted);
        }

        public static string EscapeText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            value = value.Replace("\"", "\"\"");

            return $"=\"{value}\"";
        }

        private string GetUserName(string userId, Dictionary<string, object> referenceData)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return string.Empty;

            if (!referenceData.TryGetValue("createdBy", out var users) || users == null)
                return userId;

            var userArray = JArray.FromObject(users);

            var user = userArray.FirstOrDefault(x =>
                x["value"]?.ToString() == userId);

            return user?["label"]?.ToString() ?? userId;
        }

        public static List<DifferenceAuditDto> GetDifferences(object? oldVal, object? newVal, string parentKey = "")
        {
            var diffs = new List<DifferenceAuditDto>();

            // roleIds
            if (parentKey == "roleIds")
            {
                var oldIds = NormalizeRoleIds(oldVal);
                var newIds = NormalizeRoleIds(newVal);

                var oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    oldIds.OrderBy(x => x));

                var newJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    newIds.OrderBy(x => x));

                if (oldJson != newJson)
                {
                    diffs.Add(new DifferenceAuditDto
                    {
                        Key = "roleIds",
                        OldValue = oldIds,
                        NewValue = newIds
                    });
                }

                return diffs;
            }

            // allowedGates
            if (parentKey == "allowedGates")
            {
                var oldIds = NormalizeIds(oldVal);
                var newIds = NormalizeIds(newVal);

                var oldJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    oldIds.OrderBy(x => x));

                var newJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    newIds.OrderBy(x => x));

                if (oldJson != newJson)
                {
                    diffs.Add(new DifferenceAuditDto
                    {
                        Key = "allowedGates",
                        OldValue = oldIds,
                        NewValue = newIds
                    });
                }

                return diffs;
            }

            // country
            if (parentKey == "country")
            {
                object? oldId = oldVal;
                object? newId = newVal;

                if (oldVal is JObject oldObj && oldObj["_id"] != null)
                    oldId = oldObj["_id"]?.ToString();

                if (newVal is JObject newObj && newObj["_id"] != null)
                    newId = newObj["_id"]?.ToString();

                if (!Equals(oldId, newId))
                {
                    diffs.Add(new DifferenceAuditDto
                    {
                        Key = "country",
                        OldValue = oldId,
                        NewValue = newId
                    });
                }

                return diffs;
            }

            // ARRAY HANDLING
            if (oldVal is object[] || newVal is object[])
            {
                var oldArr = oldVal as object[] ?? Array.Empty<object>();
                var newArr = newVal as object[] ?? Array.Empty<object>();

                int maxLength = Math.Max(oldArr.Length, newArr.Length);

                for (int i = 0; i < maxLength; i++)
                {
                    string keyPath = $"{parentKey}[{i}]";

                    var oldItem = i < oldArr.Length ? oldArr[i] : null;
                    var newItem = i < newArr.Length ? newArr[i] : null;

                    diffs.AddRange(GetDifferences(oldItem, newItem, keyPath));
                }

                return diffs;
            }

            // OBJECT HANDLING
            if (IsPlainObject(oldVal) || IsPlainObject(newVal))
            {
                var oldObj = oldVal as JObject ?? new JObject();
                var newObj = newVal as JObject ?? new JObject();

                var allKeys = oldObj.Properties()
                    .Select(x => x.Name)
                    .Union(
                        newObj.Properties()
                        .Select(x => x.Name));

                foreach (var key in allKeys)
                {
                    if (ignoreKeys.Contains(key))
                        continue;

                    string fullKey = string.IsNullOrEmpty(parentKey)
                        ? key
                        : $"{parentKey}.{key}";

                    diffs.AddRange(
                        GetDifferences(
                            oldObj[key],
                            newObj[key],
                            fullKey));
                }

                return diffs;
            }

            // PRIMITIVE COMPARISON
            var oldJsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(oldVal);
            var newJsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(newVal);

            if (oldJsonValue != newJsonValue)
            {
                diffs.Add(new DifferenceAuditDto
                {
                    Key = parentKey,
                    OldValue = oldVal,
                    NewValue = newVal
                });
            }

            return diffs;
        }

        private static List<object?> NormalizeRoleIds(object? value)
        {
            var result = new List<object?>();

            if (value is JArray array)
            {
                foreach (var item in array)
                {
                    if (item is JObject obj && obj["_id"] != null)
                        result.Add(obj["_id"]?.ToString());
                    else
                        result.Add(item?.ToObject<object>());
                }
            }

            return result;
        }

        private static List<object?> NormalizeIds(object? value)
        {
            var result = new List<object?>();

            if (value is JArray array)
            {
                foreach (var item in array)
                {
                    if (item is JObject obj && obj["_id"] != null)
                        result.Add(obj["_id"]?.ToString());
                    else
                        result.Add(item?.ToObject<object>());
                }
            }

            return result;
        }

        private static bool IsPlainObject(object? value)
        {
            return value is JObject;
        }

        private static string FormatDisplayValue(string key, object? value, Dictionary<string, object> referenceData, List<string> ignoreKeys)
        {
            if (value is bool boolValue)
                return boolValue ? "Yes" : "No";

            if (value == null)
                return "-";

            if (value is JValue jValue)
            {
                if (jValue.Type == JTokenType.Boolean)
                    return jValue.Value<bool>() ? "Yes" : "No";
            }

            return ReplaceIdWithLabel(
                key,
                value,
                referenceData,
                ignoreKeys)?.ToString() ?? "-";
        }

        private static object? ReplaceIdWithLabel(string key, object? value, Dictionary<string, object> referenceData, List<string> ignoreKeys)
        {
            if (ignoreKeys.Contains(key))
                return value;

            if (value == null)
                return "-";

            // createdBy / updatedBy
            if (key == "createdBy" || key == "updatedBy")
            {
                var user = GetLookupLabel(referenceData, "createdBy", value?.ToString());
                return user ?? value;
            }

            // roleIds
            if (key == "roleIds")
            {
                var ids = NormalizeIdCollection(value, referenceData);

                return string.Join(", ",
                    ids.Select(id =>
                        GetLookupLabel(referenceData, "roleIds", id) ?? id));
            }

            // country
            if (key.StartsWith("country"))
            {
                var id = ExtractId(value);

                return GetLookupLabel(referenceData, "countryIds", id) ?? id;
            }

            // vehicle owner
            if (key.StartsWith("vehicleOwnerId"))
            {
                var id = ExtractId(value);

                return GetLookupLabel(referenceData, "ownerIds", id) ?? id;
            }

            // timezone
            if (key == "userPreferences.timezoneId")
            {
                var id = ExtractId(value);

                return GetLookupLabel(referenceData, "timezoneId", id) ?? id;
            }

            if (key == "userPreferences.language")
            {
                var id = ExtractId(value);

                var dataLanguage = LanguageOptionConstant.LANGUAGE_OPTIONS.Find(x => x.Id == id);
                if (dataLanguage != null)
                {
                    return dataLanguage.Title;
                }
                return "English";
            }

            // allowed gates
            if (key == "allowedGates")
            {
                var ids = NormalizeIdCollection(value, referenceData);

                return string.Join(", ",
                    ids.Select(id =>
                        GetLookupLabel(referenceData, "deviceIds", id) ?? id));
            }

            // child site
            if (key == "childSiteId")
            {
                var id = ExtractId(value);

                return GetLookupLabel(referenceData, "childSite", id) ?? id;
            }

            // parent site
            if (key == "parentSiteId")
            {
                var id = ExtractId(value);

                return GetLookupLabel(referenceData, "parentSite", id) ?? id;
            }

            // Date fields
            if (key == "visitorValidFrom" ||
                key == "visitorValidTo" ||
                key == "visitorValidFrom.date" ||
                key == "visitorValidTo.date" ||
                key == "allowedFromTime.date" ||
                key == "allowedToTime.date")
            {
                return FormatDateValue(value);
            }

            return value;
        }

        private static string? GetLookupLabel(Dictionary<string, object> referenceData, string dictionaryKey, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!referenceData.TryGetValue(dictionaryKey, out var lookupData))
                return null;

            // Try to handle IEnumerable of objects (not necessarily Dictionary)
            if (lookupData is IEnumerable<object> items)
            {
                foreach (var item in items)
                {
                    // Try to get value and label using reflection or dynamic
                    var props = item.GetType().GetProperties();
                    var valueProp = props.FirstOrDefault(p => p.Name.Equals("value", StringComparison.OrdinalIgnoreCase));
                    var labelProp = props.FirstOrDefault(p => p.Name.Equals("label", StringComparison.OrdinalIgnoreCase));

                    if (valueProp != null && labelProp != null)
                    {
                        var itemValue = valueProp.GetValue(item)?.ToString();
                        if (itemValue == value)
                        {
                            return labelProp.GetValue(item)?.ToString();
                        }
                    }

                    // Alternative: Try to convert to Dictionary if it's a JObject or similar
                    if (item is Dictionary<string, object> dict)
                    {
                        if (dict.TryGetValue("value", out var val) && val?.ToString() == value)
                        {
                            return dict.TryGetValue("label", out var label) ? label?.ToString() : null;
                        }
                    }
                }
            }

            return null;
        }

        private static string? ExtractId(object? value)
        {
            if (value == null)
                return null;

            if (value is JObject jObj)
                return jObj["_id"]?.ToString();

            if (value is Dictionary<string, object> dict &&
                dict.TryGetValue("_id", out var id))
            {
                return id?.ToString();
            }

            return value.ToString();
        }

        private static string FormatDateValue(object? value)
        {
            if (value == null)
                return "-";

            if (DateTime.TryParse(value.ToString(), out var date))
            {
                return date.ToString("MMM dd, yyyy HH:mm");
            }

            return value.ToString() ?? "-";
        }

        private Dictionary<string, string> keyRenameMap;

        public string FormatNestedKey(string key)
        {
            // Replace [index] with " → #index"
            string replaced = Regex.Replace(key, @"\[(\d+)\]", " → #$1");

            return string.Join(" → ",
                replaced
                    .Split('.')
                    .Select(k => FormatKey(k))
            );
        }

        public string FormatKey(string key)
        {
            keyRenameMap = new();

            string renamed = keyRenameMap.ContainsKey(key)
                ? keyRenameMap[key]
                : key;

            if (string.IsNullOrEmpty(renamed))
                return renamed;

            return char.ToUpper(renamed[0]) + renamed.Substring(1);
        }

        public static List<string> NormalizeIdCollection(object value, Dictionary<string, object> referenceData)
        {
            // Step 1: Convert value to list
            List<object> values = new List<object>();

            if (value is IEnumerable<object> list && value is not string)
            {
                values.AddRange(list);
            }
            else
            {
                values.Add(value);
            }

            // Step 2: Extract IDs
            List<string> ids = new List<string>();

            foreach (var v in values)
            {
                if (v is JObject obj && obj["_id"] != null)
                {
                    ids.Add(obj["_id"].ToString());
                }
                else
                {
                    ids.Add(v?.ToString());
                }
            }

            // Step 3: Get deviceIds from referenceData
            List<string> result = new List<string>();

            if (referenceData != null && referenceData.ContainsKey("deviceIds"))
            {
                var deviceList = referenceData["deviceIds"] as IEnumerable<object>;

                foreach (var id in ids)
                {
                    string label = id;

                    if (deviceList != null)
                    {

                        foreach (var item in deviceList)
                        {
                            var valueProp = item.GetType().GetProperty("value");
                            var labelProp = item.GetType().GetProperty("label");

                            if (valueProp != null &&
                                valueProp.GetValue(item)?.ToString() == id)
                            {
                                label = labelProp?.GetValue(item)?.ToString();
                                break;
                            }
                        }

                    }

                    result.Add(label);
                }
            }
            else
            {
                // If no reference data → return IDs
                result.AddRange(ids);
            }

            // Step 4: Join result
            return result;
        }

        public object ParseJson(string jsonString)
        {
            // Step 1: Check null or empty
            if (string.IsNullOrEmpty(jsonString))
                return null;

            try
            {
                // Step 2: Replace $oid → _id and $date → date
                string cleaned = Regex.Replace(
                    jsonString,
                    @"\$oid|\$date",
                    match => match.Value == "$oid" ? "_id" : "date"
                );

                // Step 3: Parse JSON
                return JsonConvert.DeserializeObject(cleaned);
            }
            catch
            {
                // Step 4: Return null if error
                return null;
            }
        }

        //Insert function

        private Dictionary<string, object> FlattenObject(object obj,string parentKey = "",Dictionary<string, object>? result = null)
        {
            result ??= new Dictionary<string, object>();

            if (obj is not JObject jObject)
                return result;

            foreach (var property in jObject.Properties())
            {
                if (ignoreKeys.Contains(property.Name))
                    continue;

                var fullKey = string.IsNullOrEmpty(parentKey)
                    ? property.Name
                    : $"{parentKey}.{property.Name}";

                var value = property.Value;


                if (value is JObject childObject)
                {
                    if (childObject.ContainsKey("_id") &&
                        childObject.Count == 1)
                    {
                        result[fullKey] = childObject["_id"]?.ToString() ?? "";
                    }
                    else
                    {
                        FlattenObject(childObject, fullKey, result);
                    }
                }
                else
                {
                    result[fullKey] = value.ToObject<object>()!;
                }
            }

            return result;
        }

        public static string GetTitleById(string id)
        {
            var item = auditLogsCollections.FirstOrDefault(x => x.Id == id);
            return item != null ? item.Title : null; // or "Unknown Title"
        }

        private static List<AuditLogCollectionDto> auditLogsCollections = new List<AuditLogCollectionDto>
        {
            new AuditLogCollectionDto { Id = "userMaster", Title = "User Master" },
            new AuditLogCollectionDto { Id = "vehicleOwner", Title = "Vehicle Owner" },
            new AuditLogCollectionDto { Id = "ANPRvehicle", Title = "ANPR Vehicle" },
            new AuditLogCollectionDto { Id = "deviceMaster", Title = "Device Master" },
            new AuditLogCollectionDto { Id = "viMultiServerManagement", Title = "Multi Server Management" },
            new AuditLogCollectionDto { Id = "ssmSiteMapping", Title = "SSM Management" },
            new AuditLogCollectionDto { Id = "iDRACMaster", Title = "iDRAC Management" }
        };

    }
}
