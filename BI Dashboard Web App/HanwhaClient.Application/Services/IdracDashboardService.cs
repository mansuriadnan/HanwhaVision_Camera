using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class IdracDashboardService : IIdracDashboardService
    {
        public readonly IIDracManagementRepository _iDracManagementRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IIdracEventLogsRepository _idracEventLogsRepository;
        private readonly IIdracClientService _idracClientService;
        private readonly IIdracSystemLogsRepository _idracSystemLogsRepository;
        private readonly ILogger<IdracDashboardService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public IdracDashboardService(IIDracManagementRepository iDracManagementRepository,
            ISiteRepository siteRepository,
            IUsersRepository userRepository,
            IIdracEventLogsRepository idracEventLogsRepository,
            IIdracClientService idracClientService,
            IIdracSystemLogsRepository idracSystemLogsRepository,
            ILogger<IdracDashboardService> logger,
            IServiceProvider serviceProvider)
        {
            _iDracManagementRepository = iDracManagementRepository;
            _siteRepository = siteRepository;
            _idracEventLogsRepository = idracEventLogsRepository;
            _idracClientService = idracClientService;
            _idracSystemLogsRepository = idracSystemLogsRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task<(IEnumerable<IdracServerListDashboardResponse> data, Dictionary<string, object> referenceData)> GetIdracServerListAsync(string parentSiteId)
        {
            var servers = await _iDracManagementRepository.GetServersByParentSiteIdAsync(parentSiteId);

            Dictionary<string, object> referenceData = new();

            var parentSiteIds = servers.Where(x => x.ParentSiteId != null).Select(x => x.ParentSiteId).Distinct();
            var parentSiteReferenceData = await GetIDracMasterParentSiteReferenceDataAsync(parentSiteIds);
            referenceData.Add("parentSite", parentSiteReferenceData);

            var childSiteIds = servers.Where(x => x.ParentSiteId != null).Select(x => x.ChildSiteId).Distinct();
            var childSiteReferenceData = await GetChildSiteReferenceDataAsync(childSiteIds);
            referenceData.Add("childSite", childSiteReferenceData);

            return await Task.FromResult((servers, referenceData));
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
        public async Task<IdracEventLogsListResponse>GetIdracEventLogsAsync(IdracEventLogsListRequest request)
        {
            var result = await _idracEventLogsRepository.GetIdracEventLogsAsync(request);

            return new IdracEventLogsListResponse
            {
                EventLogsDetails = result.logs.Select(x => new IdracEventLogsListRes
                {
                        Id = x.Id,
                        Description = x.Message,
                        EventTimestamp = x.EventTimestamp,
                        Severity = x.Severity
                }).ToList(),

                TotalCount = result.totalCount
            };
        }
        public async Task<bool> UpdateIdracLedIndicatorAsync(IdracLedIndicatorRequest request)
        {

            var credential = await _iDracManagementRepository.GetCredentialsByIpAddressAsync(request.IpAddress);
            var login = await _idracClientService.IdracLoginAsync(request.IpAddress, credential.UserName, credential.Password);

            if (!login)
            {
                throw new Exception("iDRAC login failed.");
            }

            var body = new
            {
                LocationIndicatorActive = request.LedState
            };

            // Call actual Redfish API
            await _idracClientService.PatchIdracAsync<IdracEnableEventServiceResponse>(IdracApiConstant.IdracLedStateChange, body, request.IpAddress);

            // Update MongoDB
            await _iDracManagementRepository.UpdateIdracLedIndicatorAsync(request.ServerId, request.LedState);

            return true;

        }
        public async Task<bool> IdracPowerActionAsync(IdracPowerActionRequest request)
        {
            var credentials = await _iDracManagementRepository.GetCredentialsByIpAddressAsync(request.IpAddress);

            if (credentials.UserName == null || credentials.Password == null)
            {
                throw new Exception("iDRAC credentials not found.");
            }

            // Login
            var login =
                await _idracClientService.IdracLoginAsync(request.IpAddress, credentials.UserName, credentials.Password);

            if (!login)
            {
                throw new Exception("iDRAC login failed.");
            }

            // Redfish request body
            var body = new
            {
                ResetType = request.ResetType
            };

            // Call Redfish API
            await _idracClientService.PostIdracAsync<object>(IdracApiConstant.IdracPowerAction, body, request.IpAddress);

            // Update DB
            await _iDracManagementRepository.UpdatePowerStateAsync(request.ServerId, request.ResetType);

            return true;
        }
        public async Task<IdracSystemInformationResponse>GetSystemInformationAsync(IdracSystemInformationRequest request)
        {
            var credential = await _iDracManagementRepository.GetCredentialsByIpAddressAsync(request.IpAddress);

            var login =
                await _idracClientService.IdracLoginAsync(request.IpAddress, credential.UserName, credential.Password);

            if (!login)
            {
                throw new Exception("iDRAC login failed.");
            }
                        
            // PARALLEL API CALLS            

            var systemTask =
                _idracClientService.GetIdracAsync<RedfishSystemResponse>(
                    IdracApiConstant.SystemInformation,
                    request.IpAddress);

            var osTask =
                _idracClientService.GetIdracAsync<RedfishAttributesResponse>(
                    IdracApiConstant.ServerOsAttributes,
                    request.IpAddress);

            var ipTask =
                _idracClientService.GetIdracAsync<RedfishAttributesResponse>(
                    IdracApiConstant.IPv4Attributes,
                    request.IpAddress);

            var nicTask =
                _idracClientService.GetIdracAsync<RedfishAttributesResponse>(
                    IdracApiConstant.NicAttributes,
                    request.IpAddress);

            var infoTask =
                _idracClientService.GetIdracAsync<RedfishAttributesResponse>(
                    IdracApiConstant.IdracInfoAttributes,
                    request.IpAddress);

            var biosTask =
                _idracClientService.GetIdracAsync<BiosResponse>(
                    IdracApiConstant.BiosInformation,
                    request.IpAddress);

            var licenseCollectionTask =
                _idracClientService.GetIdracAsync<LicenseCollectionResponse>(
                    IdracApiConstant.LicenseCollection,
                    request.IpAddress);

            await Task.WhenAll(
                systemTask,
                osTask,
                ipTask,
                nicTask,
                infoTask,
                biosTask,
                licenseCollectionTask);

            var systemResponse = await systemTask;

            var osResponse = await osTask;

            var ipResponse = await ipTask;

            var nicResponse = await nicTask;

            var infoResponse = await infoTask;

            var biosResponse = await biosTask;

            var licenseCollectionResponse = await licenseCollectionTask;

            // LICENSE API CALLS
            var licenseDescriptions = new List<string>();

            if (licenseCollectionResponse?.Members != null
                && licenseCollectionResponse.Members.Any())
            {
                var licenseTasks =
                    licenseCollectionResponse.Members
                        .Where(x => !string.IsNullOrEmpty(x.OdataId))
                        .Select(x =>
                            _idracClientService.GetIdracAsync<LicenseDetailResponse>(
                                x.OdataId!,
                                request.IpAddress))
                        .ToList();

                var licenseResponses =
                    await Task.WhenAll(licenseTasks);

                licenseDescriptions =
                    licenseResponses
                        .Where(x => !string.IsNullOrWhiteSpace(x?.Description))
                        .Select(x => x!.Description!)
                        .Distinct()
                        .ToList();
            }
            // FINAL RESPONSE
            var response = new IdracSystemInformationResponse
            {
                PowerState =
                    systemResponse?.PowerState,

                Model =
                    biosResponse?.Attributes?.SystemModelName
                    ?? systemResponse?.Model,

                HostName =
                    GetAttributeValue(
                        osResponse,
                        "ServerOS.1.HostName"),

                OperatingSystem =
                    GetAttributeValue(
                        osResponse,
                        "ServerOS.1.OSName"),

                OperatingSystemVersion =
                    GetAttributeValue(
                        osResponse,
                        "ServerOS.1.OSVersion"),

                ServiceTag =
                    systemResponse?.Oem?.Dell?.DellSystem?.ChassisServiceTag,

                BiosVersion =
                    biosResponse?.Attributes?.SystemBiosVersion
                    ?? systemResponse?.BiosVersion,

                IdracFirmwareVersion =
                    GetAttributeValue(
                        infoResponse,
                        "Info.1.Version"),

                IPAddress =
                    GetAttributeValue(
                        ipResponse,
                        "IPv4.1.Address"),

                IdracMacAddress =
                    GetAttributeValue(
                        nicResponse,
                        "NIC.1.MACAddress"),

                License =
                    licenseDescriptions.Any()
                        ? string.Join(", ", licenseDescriptions)
                        : null
            };
            await _idracClientService.IdracLogoutAsync(request.IpAddress);
            return response;
        }
        private string? GetAttributeValue(RedfishAttributesResponse? response, string key)
        {
            if (response?.Attributes == null)
                return null;

            if (!response.Attributes.TryGetValue(key, out var value))
                return null;

            return value?.ToString();
        }
        public async Task<IdracSystemtLogsListResponse> GetIdracSystemLogsAsync(IdracSystemLogsListRequest request)
        {
            var result = await _idracSystemLogsRepository.GetIdracSystemLogsAsync(request);

            return new IdracSystemtLogsListResponse
            {
                SystemLogsDetails = result.logs.Select(x => new IdracSystemLogsListRes
                {
                    Id = x.Id,
                    Description = x.Description,
                    SystemTimestamp = x.Datetime,
                    Severity = x.Severity
                }).ToList(),

                TotalCount = result.totalCount
            };
        }
    }
}
